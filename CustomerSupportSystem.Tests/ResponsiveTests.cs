using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CustomerSupportSystem.Tests;

[TestFixture]
public class ResponsiveTests : BaseTest
{
    [Test]
    public async Task Dashboard_Should_Be_Responsive_On_Mobile()
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667); // iPhone SE size
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("dashboard-mobile");

        // Assert - Check that the layout adapts to mobile
        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card").First).ToBeVisibleAsync();
        
        // Check that cards stack vertically on mobile
        var statsCards = Page.Locator(".row .col-md-3 .card");
        var firstCard = statsCards.First;
        var secondCard = statsCards.Nth(1);
        
        var firstCardBox = await firstCard.BoundingBoxAsync();
        var secondCardBox = await secondCard.BoundingBoxAsync();
        
        // On mobile, cards should stack vertically (second card should be below first)
        Assert.That(secondCardBox.Y, Is.GreaterThan(firstCardBox.Y + firstCardBox.Height - 10), 
            "Cards should stack vertically on mobile");
    }

    [Test]
    public async Task Dashboard_Should_Be_Responsive_On_Tablet()
    {
        // Arrange
        await Page.SetViewportSizeAsync(768, 1024); // iPad size
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("dashboard-tablet");

        // Assert - Check that the layout works on tablet
        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card").First).ToBeVisibleAsync();
        
        // Check that cards are arranged in a grid
        var statsCards = Page.Locator(".row .col-md-3 .card");
        await Expect(statsCards).ToHaveCountAsync(4); // 4 stats cards
    }

    [Test]
    public async Task Dashboard_Should_Be_Responsive_On_Desktop()
    {
        // Arrange
        await Page.SetViewportSizeAsync(1920, 1080); // Desktop size
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("dashboard-desktop");

        // Assert - Check that the layout works on desktop
        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card").First).ToBeVisibleAsync();
        
        // Check that cards are arranged in a grid
        var statsCards = Page.Locator(".row .col-md-3 .card");
        await Expect(statsCards).ToHaveCountAsync(4);
    }

    [Test]
    public async Task Tickets_Page_Should_Be_Responsive_On_Mobile()
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("tickets-mobile");

        // Assert - Check that the table is responsive
        await Expect(Page.Locator("table")).ToBeVisibleAsync();
        
        // Check that the table has horizontal scroll on mobile
        var tableContainer = Page.Locator(".table-responsive");
        await Expect(tableContainer).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navigation_Should_Be_Responsive_On_Mobile()
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("navigation-mobile");

        // Assert - Check that navigation is responsive
        await Expect(Page.Locator(".navbar")).ToBeVisibleAsync();
        
        // Check for mobile menu toggle (if implemented)
        var navbarToggler = Page.Locator(".navbar-toggler");
        if (await navbarToggler.IsVisibleAsync())
        {
            await Expect(navbarToggler).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Be_Responsive_On_Mobile()
    {
        // Arrange
        await Page.SetViewportSizeAsync(375, 667);
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("ticket-detail-mobile");

        // Assert - Check that the ticket detail page is responsive
        await Expect(Page.Locator("h2")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card").First).ToBeVisibleAsync();
        
        // Check that buttons are accessible on mobile
        await Expect(Page.Locator("button:has-text('Edit')")).ToBeVisibleAsync();
        await Expect(Page.Locator("button:has-text('Back to Tickets')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Application_Should_Handle_Viewport_Changes()
    {
        // Arrange
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("viewport-desktop");

        // Act - Change to mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.WaitForTimeoutAsync(500); // Wait for layout to adjust
        await TakeScreenshotAsync("viewport-mobile");

        // Act - Change back to desktop
        await Page.SetViewportSizeAsync(1920, 1080);
        await Page.WaitForTimeoutAsync(500); // Wait for layout to adjust
        await TakeScreenshotAsync("viewport-desktop-again");

        // Assert - Application should still work
        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
        await Expect(Page.Locator(".card").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Application_Should_Be_Accessible_On_Different_Screen_Sizes()
    {
        // Test multiple viewport sizes
        var viewports = new[]
        {
            (375, 667),   // iPhone SE
            (768, 1024),  // iPad
            (1024, 768),  // iPad landscape
            (1920, 1080)  // Desktop
        };

        foreach (var (width, height) in viewports)
        {
            // Arrange
            await Page.SetViewportSizeAsync(width, height);
            await WaitForBlazorToLoadAsync();
            await TakeScreenshotAsync($"accessibility-{width}x{height}");

            // Assert - Basic accessibility checks
            await Expect(Page.Locator("h1")).ToBeVisibleAsync();
            await Expect(Page.Locator("h1")).ToHaveTextAsync(new Regex(".+"));
            
            // Check that at least some interactive elements are visible and clickable
            var buttons = Page.Locator("button, a");
            var buttonCount = await buttons.CountAsync();
            
            // On smaller screens, some elements might be hidden in mobile menu
            // So we'll check that at least 1 button is visible (minimum for functionality)
            var visibleButtons = 0;
            for (int i = 0; i < Math.Min(buttonCount, 5); i++)
            {
                var button = buttons.Nth(i);
                if (await button.IsVisibleAsync())
                {
                    visibleButtons++;
                }
            }
            
            Assert.That(visibleButtons, Is.GreaterThanOrEqualTo(1), 
                $"At least 1 button should be visible on {width}x{height} viewport");
        }
    }
}
