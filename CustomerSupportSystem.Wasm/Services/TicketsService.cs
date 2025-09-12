using CustomerSupportSystem.Domain.Entities;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;

namespace CustomerSupportSystem.Wasm.Services;

public class TicketsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LocalStorageService _localStorage;
    private readonly string _localStorageKey = "customer_support_tickets";
    private readonly string _commentsStorageKey = "customer_support_comments";

    public TicketsService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
    }

    public async Task<List<Ticket>> GetTicketsAsync()
    {
        try
        {
            // Try to get tickets from API first
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync("api/tickets");
            
            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadFromJsonAsync<List<Ticket>>();
                if (tickets != null && tickets.Any())
                {
                    // Save to local storage as backup
                    await SaveTicketsToLocalStorage(tickets);
                    return tickets;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Fallback to local storage
        var localTickets = await GetTicketsFromLocalStorage();
        if (localTickets.Any())
        {
            return localTickets;
        }

        // Final fallback to mock data
        return GetMockTickets();
    }

    public async Task<Ticket?> GetTicketAsync(int id)
    {
        try
        {
            // Try API first
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"api/tickets/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Ticket>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Fallback to local storage
        var tickets = await GetTicketsFromLocalStorage();
        return tickets.FirstOrDefault(t => t.Id == id) ?? GetMockTickets().FirstOrDefault(t => t.Id == id);
    }

    public async Task<List<TicketComment>> GetTicketCommentsAsync(int ticketId)
    {
        try
        {
            // Try API first
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"api/tickets/{ticketId}/comments");
            
            if (response.IsSuccessStatusCode)
            {
                var comments = await response.Content.ReadFromJsonAsync<List<TicketComment>>();
                if (comments != null && comments.Any())
                {
                    await SaveCommentsToLocalStorage(ticketId, comments);
                    return comments;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Fallback to local storage
        var localComments = await GetCommentsFromLocalStorage(ticketId);
        if (localComments.Any())
        {
            return localComments;
        }

        // Final fallback to mock data
        return GetMockComments(ticketId);
    }

    public async Task<Ticket?> CreateTicketAsync(Ticket ticket)
    {
        try
        {
            // Try API first
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.PostAsJsonAsync("api/tickets", ticket);
            
            if (response.IsSuccessStatusCode)
            {
                var createdTicket = await response.Content.ReadFromJsonAsync<Ticket>();
                if (createdTicket != null)
                {
                    // Add to local storage
                    var tickets = await GetTicketsFromLocalStorage();
                    tickets.Add(createdTicket);
                    await SaveTicketsToLocalStorage(tickets);
                    return createdTicket;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Fallback to local storage
        var localTickets = await GetTicketsFromLocalStorage();
        ticket.Id = localTickets.Count > 0 ? localTickets.Max(t => t.Id) + 1 : 1;
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.Open;
        
        localTickets.Add(ticket);
        await SaveTicketsToLocalStorage(localTickets);
        
        return ticket;
    }

    public async Task<bool> UpdateTicketAsync(Ticket ticket)
    {
        try
        {
            // Try API first
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.PutAsJsonAsync($"api/tickets/{ticket.Id}", ticket);
            
            if (response.IsSuccessStatusCode)
            {
                // Update local storage
                var tickets = await GetTicketsFromLocalStorage();
                var index = tickets.FindIndex(t => t.Id == ticket.Id);
                if (index >= 0)
                {
                    tickets[index] = ticket;
                    await SaveTicketsToLocalStorage(tickets);
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Fallback to local storage
        var localTickets = await GetTicketsFromLocalStorage();
        var localIndex = localTickets.FindIndex(t => t.Id == ticket.Id);
        if (localIndex >= 0)
        {
            localTickets[localIndex] = ticket;
            await SaveTicketsToLocalStorage(localTickets);
            return true;
        }

        return false;
    }

    public async Task<TicketComment?> AddCommentAsync(int ticketId, TicketComment comment)
    {
        try
        {
            // Try API first
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.PostAsJsonAsync($"api/tickets/{ticketId}/comments", comment);
            
            if (response.IsSuccessStatusCode)
            {
                var createdComment = await response.Content.ReadFromJsonAsync<TicketComment>();
                if (createdComment != null)
                {
                    // Add to local storage
                    var comments = await GetCommentsFromLocalStorage(ticketId);
                    comments.Add(createdComment);
                    await SaveCommentsToLocalStorage(ticketId, comments);
                    return createdComment;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
        }

        // Fallback to local storage
        var localComments = await GetCommentsFromLocalStorage(ticketId);
        comment.Id = localComments.Count > 0 ? localComments.Max(c => c.Id) + 1 : 1;
        comment.TicketId = ticketId;
        comment.CreatedAt = DateTime.UtcNow;
        
        localComments.Add(comment);
        await SaveCommentsToLocalStorage(ticketId, localComments);
        
        return comment;
    }

    // Local Storage Methods
    private async Task<List<Ticket>> GetTicketsFromLocalStorage()
    {
        try
        {
            var json = await GetFromLocalStorage(_localStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<Ticket>>(json) ?? new List<Ticket>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Local storage read failed: {ex.Message}");
        }
        return new List<Ticket>();
    }

    private async Task SaveTicketsToLocalStorage(List<Ticket> tickets)
    {
        try
        {
            var json = JsonSerializer.Serialize(tickets);
            await SetInLocalStorage(_localStorageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Local storage write failed: {ex.Message}");
        }
    }

    private async Task<List<TicketComment>> GetCommentsFromLocalStorage(int ticketId)
    {
        try
        {
            var key = $"{_commentsStorageKey}_{ticketId}";
            var json = await GetFromLocalStorage(key);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<TicketComment>>(json) ?? new List<TicketComment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Local storage read failed: {ex.Message}");
        }
        return new List<TicketComment>();
    }

    private async Task SaveCommentsToLocalStorage(int ticketId, List<TicketComment> comments)
    {
        try
        {
            var key = $"{_commentsStorageKey}_{ticketId}";
            var json = JsonSerializer.Serialize(comments);
            await SetInLocalStorage(key, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Local storage write failed: {ex.Message}");
        }
    }

    private async Task<string> GetFromLocalStorage(string key)
    {
        return await _localStorage.GetItemAsync(key) ?? "";
    }

    private async Task SetInLocalStorage(string key, string value)
    {
        await _localStorage.SetItemAsync(key, value);
    }

    // Mock Data Methods
    private List<Ticket> GetMockTickets()
    {
        return new List<Ticket>
        {
            new Ticket
            {
                Id = 1,
                Title = "Login Issues",
                Description = "I'm unable to log into my account. I keep getting an error message.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Customer = new ApplicationUser { FirstName = "Jane", LastName = "Customer", Email = "jane@example.com" },
                Assignee = new ApplicationUser { FirstName = "John", LastName = "Agent", Email = "john@company.com" }
            },
            new Ticket
            {
                Id = 2,
                Title = "Password Reset Request",
                Description = "I forgot my password and need to reset it. The reset link in my email is not working.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Customer = new ApplicationUser { FirstName = "Bob", LastName = "Smith", Email = "bob@example.com" },
                Assignee = new ApplicationUser { FirstName = "John", LastName = "Agent", Email = "john@company.com" }
            },
            new Ticket
            {
                Id = 3,
                Title = "Feature Request",
                Description = "It would be great if you could add dark mode to the application.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Low,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                Customer = new ApplicationUser { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com" }
            }
        };
    }

    private List<TicketComment> GetMockComments(int ticketId)
    {
        if (ticketId == 1)
        {
            return new List<TicketComment>
            {
                new TicketComment
                {
                    Id = 1,
                    TicketId = 1,
                    Body = "This is happening on both Chrome and Firefox browsers.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(1),
                    Author = new ApplicationUser { FirstName = "Jane", LastName = "Customer" }
                },
                new TicketComment
                {
                    Id = 2,
                    TicketId = 1,
                    Body = "I've reproduced the issue. Looking into it now.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Author = new ApplicationUser { FirstName = "John", LastName = "Agent" }
                }
            };
        }
        
        return new List<TicketComment>();
    }
}