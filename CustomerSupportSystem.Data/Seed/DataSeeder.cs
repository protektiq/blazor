using CustomerSupportSystem.Data.Context;
using CustomerSupportSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportSystem.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed users
        await SeedUsersAsync(userManager);

        // Seed tickets
        await SeedTicketsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Agent", "Customer" };

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin user
        var adminUser = await userManager.FindByEmailAsync("admin@customersupport.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@customersupport.com",
                Email = "admin@customersupport.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Agent user
        var agentUser = await userManager.FindByEmailAsync("agent@customersupport.com");
        if (agentUser == null)
        {
            agentUser = new ApplicationUser
            {
                UserName = "agent@customersupport.com",
                Email = "agent@customersupport.com",
                FirstName = "John",
                LastName = "Agent",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(agentUser, "Agent123!");
            await userManager.AddToRoleAsync(agentUser, "Agent");
        }

        // Customer user
        var customerUser = await userManager.FindByEmailAsync("customer@example.com");
        if (customerUser == null)
        {
            customerUser = new ApplicationUser
            {
                UserName = "customer@example.com",
                Email = "customer@example.com",
                FirstName = "Jane",
                LastName = "Customer",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(customerUser, "Customer123!");
            await userManager.AddToRoleAsync(customerUser, "Customer");
        }
    }

    private static async Task SeedTicketsAsync(ApplicationDbContext context)
    {
        if (!await context.Tickets.AnyAsync())
        {
            var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@example.com");
            var agent = await context.Users.FirstOrDefaultAsync(u => u.Email == "agent@customersupport.com");

            if (customer != null && agent != null)
            {
                var tickets = new List<Ticket>
                {
                    new Ticket
                    {
                        Title = "Login Issues",
                        Description = "I'm unable to log into my account. I keep getting an error message.",
                        Status = TicketStatus.Open,
                        Priority = TicketPriority.High,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        CustomerId = customer.Id,
                        AssigneeId = agent.Id
                    },
                    new Ticket
                    {
                        Title = "Password Reset Request",
                        Description = "I forgot my password and need to reset it. The reset link in my email is not working.",
                        Status = TicketStatus.InProgress,
                        Priority = TicketPriority.Medium,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        CustomerId = customer.Id,
                        AssigneeId = agent.Id
                    },
                    new Ticket
                    {
                        Title = "Feature Request",
                        Description = "It would be great if you could add dark mode to the application.",
                        Status = TicketStatus.Open,
                        Priority = TicketPriority.Low,
                        CreatedAt = DateTime.UtcNow.AddHours(-3),
                        CustomerId = customer.Id
                    }
                };

                context.Tickets.AddRange(tickets);
                await context.SaveChangesAsync();

                // Add some comments to the first ticket
                var firstTicket = tickets.First();
                var comments = new List<TicketComment>
                {
                    new TicketComment
                    {
                        TicketId = firstTicket.Id,
                        AuthorId = customer.Id,
                        Body = "This is happening on both Chrome and Firefox browsers.",
                        CreatedAt = DateTime.UtcNow.AddDays(-2).AddHours(1)
                    },
                    new TicketComment
                    {
                        TicketId = firstTicket.Id,
                        AuthorId = agent.Id,
                        Body = "I've reproduced the issue. Looking into it now.",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        IsInternal = false
                    },
                    new TicketComment
                    {
                        TicketId = firstTicket.Id,
                        AuthorId = agent.Id,
                        Body = "Found the issue - it's related to session timeout. Working on a fix.",
                        CreatedAt = DateTime.UtcNow.AddHours(-2),
                        IsInternal = true
                    }
                };

                context.TicketComments.AddRange(comments);
                await context.SaveChangesAsync();
            }
        }
    }
}
