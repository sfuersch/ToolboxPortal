using Microsoft.Playwright;
using ToolboxPortal.Models;
using System.Text.Json;
using System.Globalization;
using System.Text;

namespace ToolboxPortal.Services;

public class ThgScrapingService
{
    private readonly IConfiguration _configuration;

    public ThgScrapingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        value = value.Trim().ToLowerInvariant();

        var normalized = value.Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("ß", "ss")
            .Replace("  ", " ")
            .Trim();
    }

    public async Task<List<ThgReferralEntry>> GetReferralEntriesAsync()
    {
        var loginUrl = _configuration["ThgScraping:LoginUrl"];
        var referralUrl = _configuration["ThgScraping:ReferralUrl"];
        var username = _configuration["ThgScraping:Username"];
        var password = _configuration["ThgScraping:Password"];
        var headless = bool.Parse(_configuration["ThgScraping:Headless"] ?? "true");

        if (string.IsNullOrWhiteSpace(loginUrl))
            throw new InvalidOperationException("LoginUrl ist nicht konfiguriert.");

        if (string.IsNullOrWhiteSpace(referralUrl))
            throw new InvalidOperationException("ReferralUrl ist nicht konfiguriert.");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Benutzername oder Passwort ist nicht konfiguriert.");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = headless
            });

        var page = await browser.NewPageAsync();

        await page.GotoAsync(loginUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });

        await AcceptCookiesIfVisible(page);

        var contentBeforeLogin = await page.ContentAsync();

        if (ContainsSecurityChallenge(contentBeforeLogin))
            throw new InvalidOperationException("Sicherheitsabfrage oder Captcha erkannt.");

        await page.FillAsync("[data-testid='loginEmail']", username);
        await page.FillAsync("#password", password);
        await page.ClickAsync("button[type='submit']");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(6000);

        if (page.Url.Contains("login", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Login fehlgeschlagen oder weiterhin auf Login-Seite.");

        await page.GotoAsync(referralUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });

        await page.WaitForTimeoutAsync(3000);

        var referralContent = await page.ContentAsync();

        if (ContainsSecurityChallenge(referralContent))
            throw new InvalidOperationException("Sicherheitsabfrage auf Referral-Seite erkannt.");


        var entriesJson = await page.EvaluateAsync<string>(
    """
    () => {
        const result = [];

        const rows = Array.from(document.querySelectorAll('tbody tr'));

        for (const row of rows) {
            const cells = Array.from(row.querySelectorAll('td'));

            if (cells.length < 4) {
                continue;
            }

            const name = (cells[0].innerText || "").trim();
            const registrationDate = (cells[1].innerText || "").trim();
            const status = (cells[2].innerText || "").trim();
            const amount = (cells[3].innerText || "").trim();

            if (!name) {
                continue;
            }

            result.push({
                name,
                registrationDate,
                status,
                amount
            });
        }

        return JSON.stringify(result);
    }
    """);

        var entries = JsonSerializer.Deserialize<List<ThgReferralEntry>>(
            entriesJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ThgReferralEntry>();

        return entries;
    }

    public async Task<ThgScrapingResult> RunHealthCheckAsync()
    {
        var loginUrl = _configuration["ThgScraping:LoginUrl"];
        var username = _configuration["ThgScraping:Username"];
        var password = _configuration["ThgScraping:Password"];
        var headless = bool.Parse(_configuration["ThgScraping:Headless"] ?? "true");

        if (string.IsNullOrWhiteSpace(loginUrl))
        {
            return ThgScrapingResult.Fail("LoginUrl ist nicht konfiguriert.");
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return ThgScrapingResult.Fail("Benutzername oder Passwort ist nicht konfiguriert.");
        }

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = headless
            });

        var page = await browser.NewPageAsync();

        await page.GotoAsync(loginUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });
        await AcceptCookiesIfVisible(page);

        var contentBeforeLogin = await page.ContentAsync();

        if (ContainsSecurityChallenge(contentBeforeLogin))
        {
            return ThgScrapingResult.Fail("Sicherheitsabfrage oder Captcha erkannt.");
        }

        await page.FillAsync("[data-testid='loginEmail']", username);

        await page.FillAsync("#password", password);

        await page.ClickAsync("button[type='submit']");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Task.Delay(3000);

        var currentUrl = page.Url;

        var contentAfterLogin = await page.ContentAsync();

        if (ContainsSecurityChallenge(contentAfterLogin))
        {
            return ThgScrapingResult.Fail("Nach Login wurde eine Sicherheitsabfrage erkannt.");
        }

        if (currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            return ThgScrapingResult.Fail("Login fehlgeschlagen oder weiterhin auf Login-Seite.");
        }

        var referralUrl = _configuration["ThgScraping:ReferralUrl"];

        if (string.IsNullOrWhiteSpace(referralUrl))
        {
            return ThgScrapingResult.Fail("ReferralUrl ist nicht konfiguriert.");
        }

        await page.GotoAsync(referralUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });

        await page.WaitForTimeoutAsync(6000);

        var referralContent = await page.ContentAsync();

        if (ContainsSecurityChallenge(referralContent))
        {
            return ThgScrapingResult.Fail("Sicherheitsabfrage auf Referral-Seite erkannt.");
        }

        var referralTitle = await page.TitleAsync();

        return ThgScrapingResult.Ok(
            $"Login erfolgreich und Referral-Seite erreichbar. Titel: {referralTitle}, URL: {page.Url}");

        
    }

    private static async Task AcceptCookiesIfVisible(IPage page)
    {
        var selectors = new[]
        {
        "#CybotCookiebotDialogBodyLevelButtonLevelOptinAllowAll",
        "#CybotCookiebotDialogBodyButtonAccept",
        "button:has-text('Alle akzeptieren')",
        "button:has-text('Akzeptieren')",
        "button:has-text('OK')"
    };

        foreach (var selector in selectors)
        {
            try
            {
                var button = page.Locator(selector);

                if (await button.CountAsync() > 0 && await button.First.IsVisibleAsync())
                {
                    await button.First.ClickAsync(new LocatorClickOptions
                    {
                        Timeout = 5000
                    });

                    await page.WaitForTimeoutAsync(1000);

                    return;
                }
            }
            catch
            {
            }
        }
    }

    private static bool ContainsSecurityChallenge(string html)
    {
        var value = html.ToLowerInvariant();

        return value.Contains("captcha")
            || value.Contains("cloudflare")
            || value.Contains("sicherheitsabfrage")
            || value.Contains("ich bin kein roboter")
            || value.Contains("verify you are human");
    }
}

public class ThgScrapingResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = "";

    public static ThgScrapingResult Ok(string message)
    {
        return new ThgScrapingResult
        {
            Success = true,
            Message = message
        };
    }

    public static ThgScrapingResult Fail(string message)
    {
        return new ThgScrapingResult
        {
            Success = false,
            Message = message
        };
    }
}