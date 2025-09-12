using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CustomerSupportSystem.Tests;

[TestFixture]
public class NavigationTests : BaseTest
{
    [Test]
    public async Task Navigation_Menu_Should_Be_Visible()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("navigation-menu");

        // Assert - Check for navigation menu
        await Expect(Page.Locator(".navbar")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Customer Support System")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navigation_Menu_Should_Have_All_Required_Links()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();

        // Assert - Check for all navigation links
        await Expect(Page.Locator("a[href='']").Filter(new() { HasText = "Dashboard" })).ToBeVisibleAsync(); // Dashboard
        await Expect(Page.Locator("a[href='tickets']")).ToBeVisibleAsync(); // Tickets
        await Expect(Page.Locator("a[href='tickets/new']")).ToBeVisibleAsync(); // New Ticket
        await Expect(Page.Locator("a[href='users']")).ToBeVisibleAsync(); // Users
        await Expect(Page.Locator("a[href='reports']")).ToBeVisibleAsync(); // Reports
    }

    [Test]
    public async Task Navigation_Should_Work_Between_Pages()
    {
        // Arrange
        await WaitForBlazorToLoadAsync();

        // Act & Assert - Test navigation to Tickets page
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await TakeScreenshotAsync("navigated-to-tickets");

        // Navigate back to Dashboard
        await Page.ClickAsync("a:has-text('Dashboard')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(TestConfiguration.BaseUrl);
        await TakeScreenshotAsync("navigated-back-to-dashboard");
    }

    [Test]
    public async Task Navigation_Should_Highlight_Active_Page()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();

        // Assert - Dashboard should be active
        var dashboardLink = Page.Locator("a:has-text('Dashboard')");
        await Expect(dashboardLink).ToHaveClassAsync(new Regex("nav-link.*active|active.*nav-link"));

        // Navigate to Tickets
        await Page.ClickAsync("a[href='tickets']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Tickets should be active
        var ticketsLink = Page.Locator("a[href='tickets']").Filter(new() { HasText = "Tickets" }).First;
        await Expect(ticketsLink).ToHaveClassAsync(new Regex("nav-link.*active|active.*nav-link"));
    }

    [Test]
    public async Task Navigation_Should_Work_With_Browser_Back_Forward()
    {
        // Arrange
        await WaitForBlazorToLoadAsync();

        // Act - Navigate to Tickets
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");

        // Use browser back button
        await Page.GoBackAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(TestConfiguration.BaseUrl);

        // Use browser forward button
        await Page.GoForwardAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
    }

    [Test]
    public async Task Navigation_Should_Handle_Direct_URL_Access()
    {
        // Arrange & Act - Access tickets page directly
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("direct-tickets-access");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Tickets");

        // Act - Access ticket detail directly
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("direct-ticket-detail-access");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");
    }

    [Test]
    public async Task Navigation_Should_Handle_Invalid_Routes_Gracefully()
    {
        // Arrange & Act - Try to access a non-existent route
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/nonexistent");
        
        // Wait for page to load, but don't wait for Blazor on invalid routes
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("invalid-route");

        // Assert - Should show 404 or appropriate error
        // Note: This might show a Blazor error page or redirect to home
        
        // Check for various error indicators
        var hasPageNotFound = await Page.Locator("text=Page Not Found").IsVisibleAsync();
        var hasNotFound = await Page.Locator("text=Not Found").IsVisibleAsync();
        var has404 = await Page.Locator("text=404").IsVisibleAsync();
        var hasErrorText = await Page.Locator("text=Error").IsVisibleAsync();
        
        // Also check if the page is empty (which is also a valid error scenario)
        var pageContent = await Page.ContentAsync();
        var isEmpty = pageContent.Contains("<html><head></head><body></body></html>");
        
        var hasError = hasPageNotFound || hasNotFound || has404 || hasErrorText || isEmpty;
        Assert.That(hasError, Is.True, "Should show an error for invalid routes");
    }

    [Test]
    public async Task Navigation_Should_Maintain_State_Across_Pages()
    {
        // Arrange
        await WaitForBlazorToLoadAsync();

        // Act - Navigate to tickets and note some data
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var ticketCount = await Page.Locator("tbody tr").CountAsync();
        
        // Navigate back to dashboard
        await Page.ClickAsync("a:has-text('Dashboard')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Navigate back to tickets
        await Page.ClickAsync("a:has-text('Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Data should be consistent
        var newTicketCount = await Page.Locator("tbody tr").CountAsync();
        Assert.That(newTicketCount, Is.EqualTo(ticketCount), "Ticket count should remain consistent");
    }
}
