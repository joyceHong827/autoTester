namespace TestFlowStudio.Core.Models;

/// <summary>
/// 單一驗證條件：描述要驗證的 UI 元素 / 欄位 / 屬性，以及期望的結果。
/// 這些條件會在 AI 轉換腳本時，附加為 expect() assertion 的依據。
/// </summary>
public class AssertionRule
{
    /// <summary>識別用名稱，例如「帳號欄位」「錯誤訊息」</summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 定位方式：CSS / XPath / Text / Role / TestId / Label
    /// </summary>
    public string LocatorType { get; set; } = "CSS";

    /// <summary>定位值，例如 #username / //input[@id='email'] </summary>
    public string LocatorValue { get; set; } = "";

    /// <summary>
    /// 驗證類型：
    ///   TextEquals     — 文字內容等於
    ///   TextContains   — 文字內容包含
    ///   ValueEquals    — input value 等於
    ///   IsVisible      — 元素可見
    ///   IsHidden       — 元素隱藏
    ///   IsEnabled      — 元素啟用
    ///   IsDisabled     — 元素停用
    ///   IsChecked      — checkbox 已勾選
    ///   HasAttribute   — 有指定屬性
    ///   CountEquals    — 元素數量等於
    ///   UrlContains    — 頁面 URL 包含
    ///   TitleEquals    — 頁面標題等於
    ///   Custom         — 自訂 expect() 片段
    /// </summary>
    public string AssertionType { get; set; } = "TextEquals";

    /// <summary>期望值（IsVisible / IsHidden 等不需要）</summary>
    public string ExpectedValue { get; set; } = "";

    /// <summary>備註說明，會放入腳本的 // 註解</summary>
    public string Note { get; set; } = "";

    /// <summary>是否啟用（取消勾選則 AI 不會產生對應 assertion）</summary>
    public bool Enabled { get; set; } = true;

    // ── Helpers ───────────────────────────────────────────────────────────

    public static readonly string[] LocatorTypes =
    {
        "CSS", "XPath", "Text", "Role", "TestId", "Label", "Placeholder"
    };

    public static readonly string[] AssertionTypes =
    {
        "TextEquals", "TextContains", "ValueEquals",
        "IsVisible", "IsHidden", "IsEnabled", "IsDisabled", "IsChecked",
        "HasAttribute", "CountEquals",
        "UrlContains", "TitleEquals",
        "Custom"
    };

    /// <summary>
    /// 判斷此 AssertionType 是否需要填寫 ExpectedValue
    /// </summary>
    public bool NeedsExpectedValue => AssertionType is not (
        "IsVisible" or "IsHidden" or "IsEnabled" or "IsDisabled" or "IsChecked");

    /// <summary>產生給 AI 閱讀的自然語言描述</summary>
    public string ToPromptLine()
    {
        var locator = $"[{LocatorType}] {LocatorValue}".Trim();
        return AssertionType switch
        {
            "TextEquals"   => $"驗證 {locator} 的文字內容等於「{ExpectedValue}」",
            "TextContains" => $"驗證 {locator} 的文字內容包含「{ExpectedValue}」",
            "ValueEquals"  => $"驗證 {locator} 的 input value 等於「{ExpectedValue}」",
            "IsVisible"    => $"驗證 {locator} 可見（toBeVisible）",
            "IsHidden"     => $"驗證 {locator} 不可見（toBeHidden）",
            "IsEnabled"    => $"驗證 {locator} 為啟用狀態（toBeEnabled）",
            "IsDisabled"   => $"驗證 {locator} 為停用狀態（toBeDisabled）",
            "IsChecked"    => $"驗證 {locator} 已被勾選（toBeChecked）",
            "HasAttribute" => $"驗證 {locator} 具有屬性「{ExpectedValue}」",
            "CountEquals"  => $"驗證 {locator} 的元素數量等於 {ExpectedValue}",
            "UrlContains"  => $"驗證頁面 URL 包含「{ExpectedValue}」",
            "TitleEquals"  => $"驗證頁面標題等於「{ExpectedValue}」",
            "Custom"       => $"自訂 assertion：{ExpectedValue}",
            _              => $"{AssertionType}: {locator} → {ExpectedValue}"
        };
    }

    /// <summary>產生對應的 Playwright TypeScript expect() 程式碼</summary>
    public string ToPlaywrightCode()
    {
        var loc = LocatorType switch
        {
            "CSS"         => $"page.locator('{LocatorValue}')",
            "XPath"       => $"page.locator('{LocatorValue}')",
            "Text"        => $"page.getByText('{LocatorValue}')",
            "Role"        => $"page.getByRole('{LocatorValue}')",
            "TestId"      => $"page.getByTestId('{LocatorValue}')",
            "Label"       => $"page.getByLabel('{LocatorValue}')",
            "Placeholder" => $"page.getByPlaceholder('{LocatorValue}')",
            _             => $"page.locator('{LocatorValue}')"
        };

        var noteStr = string.IsNullOrWhiteSpace(Note) ? "" : $" // {Note}";

        return AssertionType switch
        {
            "TextEquals"   => $"await expect({loc}).toHaveText('{ExpectedValue}');{noteStr}",
            "TextContains" => $"await expect({loc}).toContainText('{ExpectedValue}');{noteStr}",
            "ValueEquals"  => $"await expect({loc}).toHaveValue('{ExpectedValue}');{noteStr}",
            "IsVisible"    => $"await expect({loc}).toBeVisible();{noteStr}",
            "IsHidden"     => $"await expect({loc}).toBeHidden();{noteStr}",
            "IsEnabled"    => $"await expect({loc}).toBeEnabled();{noteStr}",
            "IsDisabled"   => $"await expect({loc}).toBeDisabled();{noteStr}",
            "IsChecked"    => $"await expect({loc}).toBeChecked();{noteStr}",
            "HasAttribute" => $"await expect({loc}).toHaveAttribute({ExpectedValue});{noteStr}",
            "CountEquals"  => $"await expect({loc}).toHaveCount({ExpectedValue});{noteStr}",
            "UrlContains"  => $"await expect(page).toHaveURL(/{ExpectedValue}/);{noteStr}",
            "TitleEquals"  => $"await expect(page).toHaveTitle('{ExpectedValue}');{noteStr}",
            "Custom"       => $"{ExpectedValue}{noteStr}",
            _              => $"// TODO: {ToPromptLine()}"
        };
    }
}
