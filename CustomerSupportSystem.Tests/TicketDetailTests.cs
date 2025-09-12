using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CustomerSupportSystem.Tests;

[TestFixture]
public class TicketDetailTests : BaseTest
{
    [Test]
    public async Task Ticket_Detail_Page_Should_Load_Successfully()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();
        await TakeScreenshotAsync("ticket-detail-loaded");

        // Assert
        await Expect(Page).ToHaveTitleAsync("Ticket 1");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Display_Ticket_Information()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Wait for ticket data to load
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");
        
        // Assert - Check for ticket details
        await Expect(Page.Locator("text=Login Issues")).ToBeVisibleAsync(); // Title
        await Expect(Page.Locator("text=Open")).ToBeVisibleAsync(); // Status
        await Expect(Page.Locator("text=High")).ToBeVisibleAsync(); // Priority
        
        // Check for ticket description
        await Expect(Page.Locator("text=I'm unable to log into my account. I keep getting an error message.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Display_Customer_Information()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Wait for ticket data to load
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");

        // Assert - Check for customer information
        await Expect(Page.Locator("text=Customer:")).ToBeVisibleAsync();
        await Expect(Page.Locator("p.mb-0").Filter(new() { HasText = "Jane Customer" })).ToBeVisibleAsync(); // Seeded customer name
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Display_Comments_Section()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Wait for ticket data to load
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");

        // Assert - Check for comments section
        await Expect(Page.Locator("text=Comments")).ToBeVisibleAsync();
        
        // Check for existing comments (we seeded some)
        var comments = Page.Locator(".comment");
        await Expect(comments).ToHaveCountAsync(3); // Three seeded comments
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Have_Action_Buttons()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Assert - Check for action buttons
        await Expect(Page.Locator("button:has-text('Edit')")).ToBeVisibleAsync();
        await Expect(Page.Locator("button:has-text('Back to Tickets')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Ticket_Detail_Page_Back_Button_Should_Navigate_To_Tickets_List()
    {
        // Arrange
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Act
        await Page.ClickAsync("button:has-text('Back to Tickets')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TakeScreenshotAsync("navigated-back-to-tickets");

        // Assert
        await Expect(Page).ToHaveURLAsync($"{TestConfiguration.BaseUrl}/tickets");
        await Expect(Page.Locator("h2")).ToContainTextAsync("Tickets");
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Display_Creation_Date()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Wait for ticket data to load
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");

        // Assert - Check for creation date
        await Expect(Page.Locator("text=Created:")).ToBeVisibleAsync();
        
        // The date should be in a readable format (it's in a p element, not span)
        var dateElement = Page.Locator("text=Created:").Locator("..").Locator("p");
        await Expect(dateElement).ToBeVisibleAsync();
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Show_Assignee_Information()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Wait for ticket data to load
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");

        // Assert - Check for assignee information
        await Expect(Page.Locator("text=Assignee:")).ToBeVisibleAsync();
        
        // The first ticket should be assigned to an agent (target the assignee section specifically)
        var assigneeSection = Page.Locator("text=Assignee:").Locator("..");
        await Expect(assigneeSection.Locator("text=John Agent")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Display_Comment_Details()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/1");
        await WaitForBlazorToLoadAsync();

        // Wait for ticket data to load
        await Expect(Page.Locator("h2")).ToContainTextAsync("Ticket #1");

        // Assert - Check for comment details (first comment is oldest)
        var comment = Page.Locator(".comment").First;
        await Expect(comment).ToContainTextAsync("This is happening on both Chrome and Firefox browsers.");
        await Expect(comment).ToContainTextAsync("Jane Customer");
    }

    [Test]
    public async Task Ticket_Detail_Page_Should_Handle_Non_Existent_Ticket()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestConfiguration.BaseUrl}/tickets/999");
        await WaitForBlazorToLoadAsync();
        
        // Wait for the page to fully load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Take a screenshot to debug what's actually displayed
        await TakeScreenshotAsync("non-existent-ticket-debug");
        
        // Check what's actually on the page
        var pageTitle = await Page.TitleAsync();
        Console.WriteLine($"Page title: {pageTitle}");
        
        // Try to find any error-related text
        var errorElements = await Page.Locator("text='not found', text='error', text='404'").AllAsync();
        Console.WriteLine($"Found {errorElements.Count} potential error elements");
        
        // Assert - Should show appropriate message
        await Expect(Page.Locator("text=Ticket Not Found")).ToBeVisibleAsync();
    }
}
