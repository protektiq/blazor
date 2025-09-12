using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace CustomerSupportSystem.Tests;

public class BaseTest : PageTest
{
    [SetUp]
    public async Task SetUpAsync()
    {
        // Set default timeouts
        Page.SetDefaultTimeout(TestConfiguration.Timeout);
        Page.SetDefaultNavigationTimeout(TestConfiguration.NavigationTimeout);
        
        // Navigate to the application
        await Page.GotoAsync(TestConfiguration.BaseUrl);
        
        // Wait for the application to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected async Task WaitForBlazorToLoadAsync()
    {
        // Wait for Blazor Server to be ready
        await Page.WaitForFunctionAsync("() => window.Blazor !== undefined");
        await Page.WaitForFunctionAsync("() => window.Blazor._internal.navigationManager !== undefined");
    }

    protected async Task TakeScreenshotAsync(string name)
    {
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = $"screenshots/{name}.png",
            FullPage = true
        });
    }
}
