using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CustomerSupportSystem.Tests;

[TestFixture]
public class EndToEndTests : BaseTest
{
    [Test]
    public async Task Complete_User_Journey_Should_Work()
    {
        // Arrange & Act - Start at dashboard
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("e2e-start-dashboard");

        // Verify dashboard loads correctly
        await Expect(Page.Locator("h1")).ToContainTextAsync("Dashboard");
        await Expect(Page.Locator(".card").First).ToBeVisibleAsync();

        // Navigate to tickets page
        await Page.ClickAsync("a[href='tickets']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("e2e-tickets-page");

        // Verify tickets page loads
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Tickets");
        await Expect(Page.Locator("table")).ToBeVisibleAsync();

        // Click on first ticket to view details
        var firstTicketLink = Page.Locator("tbody tr").First.Locator("a").First;
        await firstTicketLink.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("e2e-ticket-details");

        // Verify ticket details page loads
        await Expect(Page).ToHaveURLAsync(new Regex($"{TestConfiguration.BaseUrl}/tickets/\\d+"));
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #");

        // Navigate back to tickets
        await Page.ClickAsync("button:has-text('Back to Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("e2e-back-to-tickets");

        // Verify we're back on tickets page
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");

        // Navigate back to dashboard
        await Page.ClickAsync("a:has-text('Dashboard')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("e2e-back-to-dashboard");

        // Verify we're back on dashboard
        await Expect(Page).ToHaveURLAsync(TestConfiguration.BaseUrl);
        await Expect(Page.Locator("h1")).ToContainTextAsync("Dashboard");
    }

    [Test]
    public async Task Application_Should_Handle_Multiple_Navigation_Scenarios()
    {
        // Test various navigation patterns
        var navigationTests = new[]
        {
            ("Dashboard", TestConfiguration.BaseUrl),
            ("Tickets", $"{TestConfiguration.BaseUrl}/tickets"),
            ("Dashboard", TestConfiguration.BaseUrl),
            ("Tickets", $"{TestConfiguration.BaseUrl}/tickets")
        };

        foreach (var (linkText, expectedUrl) in navigationTests)
        {
            // Act
            await Page.ClickAsync($"a:has-text('{linkText}')");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert
            await Expect(Page).ToHaveURLAsync(expectedUrl);
        }
    }

    [Test]
    public async Task Application_Should_Maintain_Data_Consistency()
    {
        // Arrange - Start at dashboard
        await WaitForBlazorToLoadAsync();

        // Get initial ticket count from dashboard
        var totalTicketsElement = Page.Locator(".card.bg-primary .card-title");
        var totalTicketsText = await totalTicketsElement.TextContentAsync();
        var initialTicketCount = int.Parse(totalTicketsText);

        // Navigate to tickets page
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Count tickets on tickets page
        var ticketRows = Page.Locator("tbody tr");
        var ticketsPageCount = await ticketRows.CountAsync();

        // Navigate back to dashboard
        await Page.ClickAsync("a:has-text('Dashboard')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get ticket count again
        var totalTicketsText2 = await totalTicketsElement.TextContentAsync();
        var finalTicketCount = int.Parse(totalTicketsText2);

        // Assert - Data should be consistent
        Assert.That(initialTicketCount, Is.EqualTo(finalTicketCount), "Ticket count should remain consistent");
        Assert.That(ticketsPageCount, Is.EqualTo(initialTicketCount), "Ticket count should match between pages");
    }

    [Test]
    public async Task Application_Should_Handle_Refresh_Scenarios()
    {
        // Arrange - Navigate to tickets page
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("before-refresh");

        // Act - Refresh the page
        await Page.ReloadAsync();
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("after-refresh");

        // Assert - Page should still work correctly
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Tickets");
        await Expect(Page.Locator("table")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Application_Should_Handle_Direct_URL_Access_And_Navigation()
    {
        // Arrange & Act - Access ticket detail directly
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("direct-ticket-access");

        // Assert - Should load correctly
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");

        // Navigate to tickets list
        await Page.ClickAsync("button:has-text('Back to Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be on tickets page
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");

        // Navigate to dashboard
        await Page.ClickAsync("a:has-text('Dashboard')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be on dashboard
        await Expect(Page).ToHaveURLAsync(TestConfiguration.BaseUrl);
    }

    [Test]
    public async Task Application_Should_Handle_Error_Scenarios_Gracefully()
    {
        // Test non-existent ticket
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/999");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("non-existent-ticket-error");

        // Should show error message
        var hasError = await Page.Locator("text=Ticket Not Found").IsVisibleAsync() ||
                      await Page.Locator("text=Error").IsVisibleAsync();
        Assert.That(hasError, Is.True, "Should show error for non-existent ticket");

        // Navigate back to working page
        await Page.GotoAsync(TestConfiguration.BaseUrl);
        await WaitForBlazorToLoadAsync();
        await Expect(Page.Locator("h1")).ToContainTextAsync("Dashboard");
    }

    [Test]
    public async Task Application_Should_Work_With_Different_Browser_Actions()
    {
        // Test browser back/forward
        await WaitForBlazorToLoadAsync();
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Use browser back
        await Page.GoBackAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(TestConfiguration.BaseUrl);

        // Use browser forward
        await Page.GoForwardAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");

        // Test page reload
        await Page.ReloadAsync();
        await WaitForBlazorToLoadAsync();
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Tickets");
    }
}
