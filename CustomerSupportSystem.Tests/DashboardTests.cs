using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CustomerSupportSystem.Tests;

[TestFixture]
public class DashboardTests : BaseTest
{
    [Test]
    public async Task Dashboard_Should_Load_Successfully()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("dashboard-loaded");

        // Assert
        await Expect(Page).ToHaveTitleAsync("Dashboard");
        
        // Check for main dashboard elements
        await Expect(Page.Locator("h1")).ToContainTextAsync("Dashboard");
        
        // Check for statistics cards
        await Expect(Page.Locator(".card.bg-primary")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card.bg-success")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card.bg-warning")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card.bg-info")).ToBeVisibleAsync();
        
        // Check for statistics labels
        await Expect(Page.Locator("text=Total Tickets")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Open Tickets")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=In Progress")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Resolved")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_Should_Display_Recent_Tickets()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();

        // Assert
        await Expect(Page.Locator("text=Recent Tickets")).ToBeVisibleAsync();
        
        // Check if there's a table for recent tickets
        var recentTicketsSection = Page.Locator(".card:has-text('Recent Tickets')");
        await Expect(recentTicketsSection).ToBeVisibleAsync();
        
        // Check for table headers
        await Expect(Page.Locator("th:has-text('ID')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Title')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Status')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Priority')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Customer')")).ToBeVisibleAsync();
        await Expect(Page.Locator("th:has-text('Created')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_Should_Have_Quick_Actions()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();

        // Assert
        await Expect(Page.Locator("text=Quick Actions")).ToBeVisibleAsync();
        
        // Check for quick action buttons
        await Expect(Page.Locator("a:has-text('Create New Ticket')")).ToBeVisibleAsync();
        await Expect(Page.Locator("a:has-text('View All Tickets')")).ToBeVisibleAsync();
        await Expect(Page.Locator("a:has-text('Manage Users')")).ToBeVisibleAsync();
        await Expect(Page.Locator("a:has-text('View Reports')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_Quick_Actions_Should_Navigate_Correctly()
    {
        // Arrange
        await WaitForBlazorToLoadAsync();

        // Act & Assert - Test "View All Tickets" navigation
        await Page.ClickAsync("a:has-text('View All Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await TakeScreenshotAsync("navigated-to-tickets");

        // Navigate back to dashboard
        await Page.GoBackAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(TestConfiguration.BaseUrl);
    }

    [Test]
    public async Task Dashboard_Should_Display_Ticket_Statistics()
    {
        // Arrange & Act
        await WaitForBlazorToLoadAsync();

        // Assert - Check that statistics are displayed (numbers should be visible)
        var totalTicketsCard = Page.Locator(".card.bg-primary .card-title");
        var openTicketsCard = Page.Locator(".card.bg-success .card-title");
        var inProgressCard = Page.Locator(".card.bg-warning .card-title");
        var resolvedCard = Page.Locator(".card.bg-info .card-title");

        await Expect(totalTicketsCard).ToBeVisibleAsync();
        await Expect(openTicketsCard).ToBeVisibleAsync();
        await Expect(inProgressCard).ToBeVisibleAsync();
        await Expect(resolvedCard).ToBeVisibleAsync();

        // Verify that the statistics contain numeric values
        var totalTicketsText = await totalTicketsCard.TextContentAsync();
        var openTicketsText = await openTicketsCard.TextContentAsync();
        var inProgressText = await inProgressCard.TextContentAsync();
        var resolvedText = await resolvedCard.TextContentAsync();

        Assert.That(int.TryParse(totalTicketsText, out _), Is.True, "Total tickets should be a number");
        Assert.That(int.TryParse(openTicketsText, out _), Is.True, "Open tickets should be a number");
        Assert.That(int.TryParse(inProgressText, out _), Is.True, "In progress tickets should be a number");
        Assert.That(int.TryParse(resolvedText, out _), Is.True, "Resolved tickets should be a number");
    }
}
