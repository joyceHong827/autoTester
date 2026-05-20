using System.Diagnostics;
using Newtonsoft.Json.Linq;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

public class PlaywrightService : IPlaywrightService
{
    private readonly AppSettings _settings;
    private Process? _codegenProcess;

    public PlaywrightService(AppSettings settings) => _settings = settings;

    // ── Environment check ──────────────────────────────────────────────────

    public async Task<(bool nodeOk, string nodeVersion, bool playwrightOk)> CheckEnvironmentAsync()
    {
        var nodeVer = await RunCommandAsync(_settings.Playwright.NodePath, "--version");
        var nodeOk  = nodeVer.TrimStart().StartsWith("v", StringComparison.Ordinal);
        var pwVer   = await RunCommandAsync("npx", "playwright --version");
        var pwOk    = pwVer.Contains("Version", StringComparison.OrdinalIgnoreCase)
                   || pwVer.Contains("playwright", StringComparison.OrdinalIgnoreCase);
        return (nodeOk, nodeVer.Trim(), pwOk);
    }

    public async Task InstallPlaywrightAsync(Action<string> onOutput)
    {
        await RunCommandStreamAsync("npm", "install -g @playwright/test", onOutput);
        await RunCommandStreamAsync("npx", "playwright install chromium", onOutput);
    }

    // ── Codegen ────────────────────────────────────────────────────────────

    public async Task<string> RecordAsync(string url, CancellationToken ct = default)
    {
        var tempFile = Path.Combine(
            Path.GetTempPath(), $"tfstudio_{Guid.NewGuid():N}.js");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName        = "npx",
                Arguments       = $"playwright codegen --target javascript --output \"{tempFile}\" \"{url}\"",
                UseShellExecute = true
            };
            _codegenProcess = Process.Start(psi)
                ?? throw new InvalidOperationException("無法啟動 Playwright Codegen");
            await _codegenProcess.WaitForExitAsync(ct);
        }
        finally { _codegenProcess = null; }

        if (!File.Exists(tempFile)) return "";
        var script = await File.ReadAllTextAsync(tempFile, System.Text.Encoding.UTF8, ct);
        File.Delete(tempFile);
        return script;
    }

    public void StopRecording()
    {
        try { _codegenProcess?.Kill(entireProcessTree: true); } catch { }
    }

    // ── Test runner ────────────────────────────────────────────────────────

    public async Task<TestRunResult> RunTestAsync(
        string specFilePath,
        Action<string>? onOutput = null,
        CancellationToken ct = default)
    {
        var testDir = FindPlaywrightTestsDir();
        var rawOutput = new System.Text.StringBuilder();

        onOutput?.Invoke($"[系統] Playwright 執行目錄：{testDir}");
        onOutput?.Invoke($"[系統] 測試腳本：{specFilePath}");

        // 如果 spec 檔案不在 testDir 下，改用 spec 所在目錄作為 testDir
        var relativeSpecPath = Path.GetRelativePath(testDir, specFilePath).Replace('\\', '/');
        if (relativeSpecPath.StartsWith(".."))
        {
            testDir = Path.GetDirectoryName(specFilePath)!;
            relativeSpecPath = Path.GetFileName(specFilePath);
        }

        // Playwright test 命令引數是正則表達式，需要轉義特殊字元
        // 同時對含有 & 等特殊字元的檔名，改用不含特殊字元的前綴匹配
        var specPattern = relativeSpecPath;
        if (specPattern.Contains('&') || specPattern.Contains('$') || specPattern.Contains('*'))
        {
            // 使用檔名中 & 之前的部分作為正則匹配模式
            var fileName = Path.GetFileNameWithoutExtension(specFilePath);
            var safePrefix = fileName.Split('&', '$', '*')[0];
            specPattern = safePrefix;
        }

        var headedArg = _settings.Playwright.HeadedMode ? "--headed" : "";

        // ① JSON 報告路徑 — 使用命令列 --reporter 確保 JSON 一定輸出
        var jsonReportPath = Path.Combine(testDir, "playwright-report", "results.json");
        Directory.CreateDirectory(Path.GetDirectoryName(jsonReportPath)!);

        // 清除舊 JSON 避免誤用上次結果
        if (File.Exists(jsonReportPath))
            try { File.Delete(jsonReportPath); } catch { }

        // ② 執行 Playwright 測試：同時用 list（顯示進度）和 json（輸出到檔案）
        var psi = new ProcessStartInfo
        {
            FileName               = "cmd.exe",
            Arguments              = $"/c \"chcp 65001 > nul && npx playwright test \"{specPattern}\" {headedArg} --reporter=list,json\"",
            WorkingDirectory       = testDir,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding  = System.Text.Encoding.UTF8
        };
        // PLAYWRIGHT_JSON_OUTPUT_NAME 環境變數讓 json reporter 寫入檔案而非 stdout
        psi.Environment["PLAYWRIGHT_JSON_OUTPUT_NAME"] = jsonReportPath;

        var jsonStdout = new System.Text.StringBuilder();
        using var proc = new Process { StartInfo = psi };
        proc.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            var line = StripAnsiCodes(e.Data);
            // JSON reporter 輸出到 stdout，收集到 jsonStdout 備用
            jsonStdout.AppendLine(line);
            rawOutput.AppendLine(line);
        };
        proc.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            var line = StripAnsiCodes(e.Data);
            rawOutput.AppendLine(line);
            onOutput?.Invoke(line);
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        await proc.WaitForExitAsync(ct);

        // 等待 JSON 檔案寫入完成
        await Task.Delay(800, ct);

        // ③ 讀取 JSON report 檔（由 PLAYWRIGHT_JSON_OUTPUT_NAME 環境變數寫入）
        string? jsonContent = null;
        if (File.Exists(jsonReportPath))
        {
            try
            {
                var fileText = await File.ReadAllTextAsync(jsonReportPath, System.Text.Encoding.UTF8, ct);
                var trimmed  = fileText.TrimStart();
                if (trimmed.StartsWith("{"))
                {
                    jsonContent = trimmed;
                    onOutput?.Invoke($"[系統] 已讀取測試結果（{fileText.Length:#,0} 位元組）");
                }
            }
            catch (Exception ex)
            {
                onOutput?.Invoke($"[系統] 讀取 results.json 失敗：{ex.Message}");
            }
        }

        // Fallback: 如果檔案不存在或無效，嘗試使用 stdout 中的 JSON
        if (jsonContent == null)
        {
            var stdoutText = jsonStdout.ToString().TrimStart();
            if (stdoutText.StartsWith("{"))
            {
                jsonContent = stdoutText;
                onOutput?.Invoke($"[系統] 已從 stdout 取得 JSON 測試結果（{stdoutText.Length:#,0} 位元組）");
            }
            else
            {
                onOutput?.Invoke("[系統] 找不到 JSON 測試結果，改用輸出日誌解析");
            }
        }

        var result = ParseResult(jsonContent ?? rawOutput.ToString(), proc.ExitCode);

        // 補充診斷資訊
        onOutput?.Invoke($"[系統] 解析結果 — 測試數：{result.TotalTests}，步驟數：{result.TestSteps.Count}，狀態：{result.Status}");

        result.RawOutput       = rawOutput.ToString();
        result.HtmlReportPath  = Path.Combine(testDir, "playwright-report", "index.html");

        CollectTestAttachments(testDir, result);
        return result;
    }

    /// <summary>
    /// 確保 testDir 下有 playwright.config.ts，並且包含 JSON reporter 輸出設定。
    /// 若已存在但沒有 JSON reporter，會追加設定。
    /// </summary>
    private static void EnsurePlaywrightConfig(string testDir, string jsonReportPath)
    {
        var configPath = Path.Combine(testDir, "playwright.config.ts");
        var relJsonPath = Path.GetRelativePath(testDir, jsonReportPath).Replace('\\', '/');

        // 判斷 tests 子目錄是否存在，不存在時 testDir 設為 '.'
        var testsSubDir = Path.Combine(testDir, "tests");
        var configTestDir = Directory.Exists(testsSubDir) ? "./tests" : ".";

        if (!File.Exists(configPath))
        {
            // 建立最小化 config，包含 list + JSON reporter
            File.WriteAllText(configPath, $@"import {{ defineConfig }} from '@playwright/test';

export default defineConfig({{
  testDir: '{configTestDir}',
  fullyParallel: false,
  retries: 0,
  reporter: [
    ['list'],
    ['json', {{ outputFile: '{relJsonPath}' }}],
    ['html', {{ open: 'never' }}],
  ],
  use: {{
    headless: true,
  }},
}});
", System.Text.Encoding.UTF8);
            return;
        }

        // 已存在：確認是否有 json reporter 的 outputFile 設定
        var content = File.ReadAllText(configPath, System.Text.Encoding.UTF8);
        if (!content.Contains("outputFile") && !content.Contains("results.json"))
        {
            // 在 reporter 陣列中補上 json reporter（在最後一個 ] 前插入）
            var needle = "['html'";
            if (!content.Contains(needle))
                needle = "reporter:";

            var insertPos = content.IndexOf(needle, StringComparison.Ordinal);
            if (insertPos > 0)
            {
                var insertion = $"\n    ['json', {{ outputFile: '{relJsonPath}' }}],\n    ";
                content = content.Insert(insertPos, insertion);
                File.WriteAllText(configPath, content, System.Text.Encoding.UTF8);
            }
        }
    }

    /// <summary>
    /// Save TypeScript test to configured directory
    /// </summary>
    public async Task<string> SaveTestScriptAsync(
        string testName,
        string script,
        CancellationToken ct = default)
    {
        var outputDir = _settings.Playwright.TestsOutputDirectory;
        Directory.CreateDirectory(outputDir);

        var fileName = $"{SanitizeFileName(testName)}.spec.ts";
        var filePath = Path.Combine(outputDir, fileName);

        await File.WriteAllTextAsync(filePath, script, System.Text.Encoding.UTF8, ct);
        return filePath;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalids = Path.GetInvalidFileNameChars();

        // 將空格和無效字元都替換為底線
        var invalidCharsAndSpace = new List<char>(invalids) { ' ' };
        var sanitized = string.Join("_", fileName.Split(invalidCharsAndSpace.ToArray(), 
            StringSplitOptions.RemoveEmptyEntries));

        return sanitized.TrimEnd('.');
    }

    /// <summary>
    /// 移除 ANSI 顏色代碼和控制序列
    /// </summary>
    private static string StripAnsiCodes(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // ANSI escape sequences 的正則模式: ESC[ + 參數 + 字母
        return System.Text.RegularExpressions.Regex.Replace(
            text, 
            @"\x1B\[[0-9;]*[a-zA-Z]", 
            string.Empty);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// 收集測試執行產生的附件（截圖、影片、追蹤檔案）
    /// </summary>
    private static void CollectTestAttachments(string testDir, TestRunResult result)
    {
        try
        {
            var testResultsDir = Path.Combine(testDir, "test-results");
            if (!Directory.Exists(testResultsDir))
                return;

            // 收集截圖
            var screenshots = Directory.GetFiles(testResultsDir, "*.png", SearchOption.AllDirectories);
            foreach (var screenshot in screenshots)
            {
                result.Attachments.Add(new TestAttachment
                {
                    Type = "screenshot",
                    FilePath = screenshot,
                    FileName = Path.GetFileName(screenshot),
                    Description = GetScreenshotDescription(screenshot),
                    CreatedAt = File.GetCreationTime(screenshot)
                });
            }

            // 收集影片
            var videos = Directory.GetFiles(testResultsDir, "*.webm", SearchOption.AllDirectories);
            foreach (var video in videos)
            {
                result.Attachments.Add(new TestAttachment
                {
                    Type = "video",
                    FilePath = video,
                    FileName = Path.GetFileName(video),
                    Description = "測試執行錄影",
                    CreatedAt = File.GetCreationTime(video)
                });
            }

            // 收集追蹤檔案
            var traces = Directory.GetFiles(testResultsDir, "*.zip", SearchOption.AllDirectories)
                .Where(f => f.Contains("trace"));
            foreach (var trace in traces)
            {
                result.Attachments.Add(new TestAttachment
                {
                    Type = "trace",
                    FilePath = trace,
                    FileName = Path.GetFileName(trace),
                    Description = "Playwright Trace",
                    CreatedAt = File.GetCreationTime(trace)
                });
            }
        }
        catch (Exception ex)
        {
            // 收集附件失敗不影響測試結果
            Console.WriteLine($"收集測試附件時發生錯誤：{ex.Message}");
        }
    }

    /// <summary>
    /// 從截圖檔案名稱推斷描述
    /// </summary>
    private static string GetScreenshotDescription(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        // Playwright 截圖命名格式通常包含測試名稱和狀態
        if (fileName.Contains("test-failed"))
            return "測試失敗時的截圖";
        if (fileName.Contains("test-finished"))
            return "測試完成時的截圖";
        if (fileName.Contains("before"))
            return "執行前的截圖";
        if (fileName.Contains("after"))
            return "執行後的截圖";

        return $"測試截圖 - {fileName}";
    }

    private static TestRunResult ParseResult(string raw, int exitCode)
    {
        var result = new TestRunResult { RawOutput = raw, RunTime = DateTime.Now };
        try
        {
            var start = raw.IndexOf('{');
            if (start >= 0)
            {
                var jobj = JObject.Parse(raw[start..]);

                // 解析測試統計
                var stats = jobj["stats"];
                if (stats != null)
                {
                    result.TotalTests  = stats["expected"]   != null ? (int)stats["expected"]!   : 0;
                    result.FailedTests = stats["unexpected"] != null ? (int)stats["unexpected"]! : 0;
                    result.PassedTests = result.TotalTests - result.FailedTests;

                    // 解析執行時間
                    if (stats["duration"] != null)
                    {
                        result.Duration = TimeSpan.FromMilliseconds((double)stats["duration"]!);
                    }
                }

                // 解析測試套件和步驟（遞迴處理巢狀 suites）
                var suites = jobj["suites"] as JArray ?? new JArray();
                ParseSuites(suites, result);
            }
        }
        catch { /* fallback to exit-code */ }

        result.FailedTests = result.FailureMessages.Count;
        result.PassedTests = result.TotalTests - result.FailedTests;
        result.Status      = exitCode == 0 ? "passed" : "failed";
        result.Success     = exitCode == 0;
        return result;
    }

    /// <summary>
    /// 遞迴解析 Playwright JSON reporter 的巢狀 suites 結構。
    /// </summary>
    private static void ParseSuites(JArray suites, TestRunResult result)
    {
        foreach (var suite in suites)
        {
            // 遞迴處理巢狀 suites
            var childSuites = suite["suites"] as JArray;
            if (childSuites != null && childSuites.Count > 0)
                ParseSuites(childSuites, result);

            var specs = suite["specs"] as JArray ?? new JArray();
            foreach (var spec in specs)
            {
                var specTitle = spec["title"]?.ToString() ?? "";
                var tests = spec["tests"] as JArray ?? new JArray();

                foreach (var test in tests)
                {
                    var testTitle = test["title"]?.ToString() ?? "";
                    var status = test["status"]?.ToString();
                    var results = test["results"] as JArray;

                    if (results != null && results.Count > 0)
                    {
                        var testResult = results[0];
                        var duration = testResult["duration"] != null
                            ? TimeSpan.FromMilliseconds((double)testResult["duration"]!)
                            : TimeSpan.Zero;

                        var testPassed = status == "expected" || status == "passed";

                        var stepsArr = testResult["steps"] as JArray;
                        var extractedSteps = new List<TestStepResult>();
                        if (stepsArr != null)
                        {
                            ExtractSteps(stepsArr, extractedSteps, parentName: null);
                        }

                        if (extractedSteps.Count > 0)
                        {
                            result.TestSteps.AddRange(extractedSteps);
                            foreach (var s in extractedSteps.Where(s => !s.Passed && !string.IsNullOrEmpty(s.ErrorMessage)))
                                result.FailureMessages.Add($"{s.StepName}: {s.ErrorMessage}");
                        }
                        else
                        {
                            var stepResult = new TestStepResult
                            {
                                StepName = string.IsNullOrEmpty(testTitle) ? specTitle : testTitle,
                                Action = "測試案例",
                                Passed = testPassed,
                                Duration = duration,
                                Expected = string.IsNullOrEmpty(testTitle) ? specTitle : testTitle,
                                Actual = testPassed ? "符合預期" : "未符合預期"
                            };

                            if (status is "failed" or "unexpected")
                            {
                                var error = testResult["error"];
                                if (error != null)
                                {
                                    var errMsg = error["message"]?.ToString() ?? "";
                                    stepResult.ErrorMessage = errMsg;
                                    stepResult.Actual = "測試失敗";
                                    result.FailureMessages.Add($"{specTitle}: {errMsg}");
                                }
                            }

                            result.TestSteps.Add(stepResult);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 遞迴從 Playwright JSON reporter 的 steps 陣列中取出每一個動作步驟。
    /// 過濾掉 hooks/before/after 等噪音，只保留實際 test.step 與斷言。
    /// </summary>
    private static void ExtractSteps(JArray stepsArr, List<TestStepResult> output, string? parentName)
    {
        foreach (var step in stepsArr)
        {
            var category = step["category"]?.ToString() ?? "";
            var title = step["title"]?.ToString() ?? "";

            // 跳過 hook 類型（before/after each / fixture 等）但保留其子步驟
            bool skipSelf = category == "hook" || category == "fixture";

            if (!skipSelf && !string.IsNullOrWhiteSpace(title))
            {
                var dur = step["duration"] != null
                    ? TimeSpan.FromMilliseconds((double)step["duration"]!)
                    : TimeSpan.Zero;

                var error = step["error"];
                var errMsg = error?["message"]?.ToString() ?? "";
                var passed = string.IsNullOrEmpty(errMsg);

                // 標籤化常見類別（純中文，不使用英文）
                var actionLabel = category switch
                {
                    "test.step" => "自訂步驟",
                    "expect"    => "斷言驗證",
                    "pw:api"    => "頁面動作",
                    "hook"      => "前置/後置",
                    "fixture"   => "測試夾具",
                    _           => "步驟"
                };

                var stepName = string.IsNullOrEmpty(parentName)
                    ? title
                    : $"{parentName} → {title}";

                output.Add(new TestStepResult
                {
                    StepName     = stepName,
                    Action       = actionLabel,
                    Expected     = title,
                    Actual       = passed ? "符合預期" : (errMsg.Length > 200 ? errMsg[..200] + "..." : errMsg),
                    ErrorMessage = errMsg,
                    Passed       = passed,
                    Duration     = dur
                });
            }

            // 遞迴處理子步驟（巢狀的 test.step）
            if (step["steps"] is JArray childSteps && childSteps.Count > 0)
            {
                var newParent = skipSelf ? parentName : (string.IsNullOrEmpty(title) ? parentName : title);
                ExtractSteps(childSteps, output, newParent);
            }
        }
    }

    private string FindPlaywrightTestsDir()
    {
        // 優先使用設定值
        var configured = _settings.Playwright.PlaywrightTestsDirectory;
        if (!string.IsNullOrWhiteSpace(configured) && Directory.Exists(configured)
            && File.Exists(Path.Combine(configured, "package.json")))
        {
            return configured;
        }

        // 備用：從設定的 TestsOutputDirectory 向上找
        var testOutputDir = _settings.Playwright.TestsOutputDirectory;
        if (Directory.Exists(testOutputDir))
        {
            var playwrightRoot = Path.GetDirectoryName(testOutputDir);
            if (playwrightRoot != null && File.Exists(Path.Combine(playwrightRoot, "package.json")))
                return playwrightRoot;
        }

        // 備用：從應用程式目錄向上搜尋
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            var pwTestsDir = Path.Combine(dir.FullName, "playwright-tests");
            if (Directory.Exists(pwTestsDir) && File.Exists(Path.Combine(pwTestsDir, "package.json")))
                return pwTestsDir;

            if (dir.GetFiles("*.sln").Length > 0)
            {
                pwTestsDir = Path.Combine(dir.FullName, "playwright-tests");
                if (Directory.Exists(pwTestsDir))
                    return pwTestsDir;
            }
            dir = dir.Parent!;
        }

        // 最後備用：硬編碼路徑
        return @"D:\autoTester\playwright-tests";
    }

    private static async Task<string> RunCommandAsync(string cmd, string args)
    {
        try
        {
            // Windows 需要透過 cmd.exe 執行 npm/npx
            var isNpmOrNpx = cmd.Equals("npm", StringComparison.OrdinalIgnoreCase) 
                          || cmd.Equals("npx", StringComparison.OrdinalIgnoreCase);

            var psi = new ProcessStartInfo
            {
                FileName               = isNpmOrNpx ? "cmd.exe" : cmd,
                Arguments              = isNpmOrNpx ? $"/c chcp 65001 > nul && {cmd} {args}" : args,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding  = System.Text.Encoding.UTF8
            };
            using var p = Process.Start(psi)!;
            var output  = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();
            return output;
        }
        catch { return ""; }
    }

    private static async Task RunCommandStreamAsync(string cmd, string args, Action<string> onOutput)
    {
        // Windows 需要透過 cmd.exe 執行 npm/npx
        var isNpmOrNpx = cmd.Equals("npm", StringComparison.OrdinalIgnoreCase) 
                      || cmd.Equals("npx", StringComparison.OrdinalIgnoreCase);

        var psi = new ProcessStartInfo
        {
            FileName               = isNpmOrNpx ? "cmd.exe" : cmd,
            Arguments              = isNpmOrNpx ? $"/c chcp 65001 > nul && {cmd} {args}" : args,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow         = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding  = System.Text.Encoding.UTF8
        };
        using var p = new Process { StartInfo = psi };
        p.OutputDataReceived += (_, e) => { if (e.Data is not null) onOutput(StripAnsiCodes(e.Data)); };
        p.ErrorDataReceived  += (_, e) => { if (e.Data is not null) onOutput(StripAnsiCodes(e.Data)); };
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        await p.WaitForExitAsync();
    }
}
