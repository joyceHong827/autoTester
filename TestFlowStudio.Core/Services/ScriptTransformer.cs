using TestFlowStudio.Core.Helpers;
using TestFlowStudio.Core.Interfaces;
using TestFlowStudio.Core.Models;

namespace TestFlowStudio.Core.Services;

public class ScriptTransformer
{
    private readonly IAIService _ai;
    private readonly AppSettings _settings;

    public ScriptTransformer(IAIService ai, AppSettings settings)
    {
        _ai = ai;
        _settings = settings;
    }

    /// <summary>
    /// Transforms a raw Codegen JS snippet into a full TypeScript .spec.ts file,
    /// guided by the test-case Markdown.
    /// Saves the result to disk and updates tc.PlaywrightScript.
    /// </summary>
    public async Task<string> TransformAndSaveAsync(
        string codegenScript,
        TestCase tc,
        Action<string>? onChunk = null,
        CancellationToken ct = default)
    {
        var dir = _settings.Output.ScriptsDirectory;
        Directory.CreateDirectory(dir);

        var result = new System.Text.StringBuilder();
        Action<string> handler = chunk =>
        {
            result.Append(chunk);
            onChunk?.Invoke(chunk);
        };

        await _ai.TransformScriptStreamAsync(
            codegenScript, tc.MarkdownBody, handler, ct);

        var script = result.ToString().Trim();

        // Derive filename from TestCase file (TC-1234_xxx.md -> TC-1234_xxx.spec.ts)
        var baseName = Path.GetFileNameWithoutExtension(tc.FilePath);
        var outPath  = Path.Combine(dir, baseName + ".spec.ts");

        await File.WriteAllTextAsync(outPath, script, System.Text.Encoding.UTF8, ct);

        // Update TestCase to point at the new script
        tc.PlaywrightScript = outPath;
        if (!string.IsNullOrEmpty(tc.FilePath))
            MarkdownHelper.SaveFile(tc);

        return outPath;
    }
}
