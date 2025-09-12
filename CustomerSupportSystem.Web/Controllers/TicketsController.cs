using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerSupportSystem.Data.Context;
using CustomerSupportSystem.Domain.Entities;
using CustomerSupportSystem.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportSystem.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ILogger<TicketsController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthorizationService _authorizationService;

    public TicketsController(
        ILogger<TicketsController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuthorizationService authorizationService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    [HttpPut("{ticketId}/status")]
    public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var ticket = await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            // Check authorization for ticket transition
            var authResult = await _authorizationService.AuthorizeAsync(
                User, ticket, new TicketTransitionRequirement());

            if (!authResult.Succeeded)
            {
                return Forbid("You don't have permission to transition this ticket.");
            }

            // Validate status transition
            if (!IsValidStatusTransition(ticket.Status, request.Status))
            {
                return BadRequest($"Invalid status transition from {ticket.Status} to {request.Status}.");
            }

            var oldStatus = ticket.Status;
            ticket.Status = request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} status changed from {OldStatus} to {NewStatus} by user {UserId}",
                ticketId, oldStatus, request.Status, User.Identity?.Name);

            return Ok(new
            {
                ticketId = ticket.Id,
                oldStatus,
                newStatus = ticket.Status,
                updatedAt = ticket.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket status for ticket {TicketId}", ticketId);
            return StatusCode(500, "Error updating ticket status.");
        }
    }

    [HttpPut("{ticketId}")]
    public async Task<IActionResult> UpdateTicket(int ticketId, [FromBody] UpdateTicketRequest request)
    {
        try
        {
            var ticket = await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Check if user can edit this ticket
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var canEdit = false;

            if (userRoles.Contains("Admin") || userRoles.Contains("Agent"))
            {
                // Admins and agents can edit any ticket
                canEdit = true;
            }
            else if (userRoles.Contains("Customer") && ticket.CustomerId == currentUser.Id)
            {
                // Customers can edit their own tickets within time limit
                var authResult = await _authorizationService.AuthorizeAsync(
                    User, ticket, new OwnTicketEditRequirement(TimeSpan.FromHours(24))); // 24 hour limit

                canEdit = authResult.Succeeded;
            }

            if (!canEdit)
            {
                return Forbid("You don't have permission to edit this ticket.");
            }

            // Update ticket fields
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                ticket.Title = request.Title;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                ticket.Description = request.Description;
            }

            // Only admins and agents can change priority and assignee
            if (userRoles.Contains("Admin") || userRoles.Contains("Agent"))
            {
                if (request.Priority.HasValue)
                {
                    ticket.Priority = request.Priority.Value;
                }

                if (request.AssigneeId != null)
                {
                    ticket.AssigneeId = request.AssigneeId;
                }
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} updated by user {UserId}", ticketId, User.Identity?.Name);

            return Ok(new
            {
                ticketId = ticket.Id,
                title = ticket.Title,
                description = ticket.Description,
                status = ticket.Status,
                priority = ticket.Priority,
                assigneeId = ticket.AssigneeId,
                updatedAt = ticket.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket {TicketId}", ticketId);
            return StatusCode(500, "Error updating ticket.");
        }
    }

    private static bool IsValidStatusTransition(TicketStatus currentStatus, TicketStatus newStatus)
    {
        // Define valid status transitions
        return currentStatus switch
        {
            TicketStatus.Open => newStatus == TicketStatus.InProgress || 
                                newStatus == TicketStatus.Closed,
            TicketStatus.InProgress => newStatus == TicketStatus.Resolved || 
                                     newStatus == TicketStatus.Closed ||
                                     newStatus == TicketStatus.Open,
            TicketStatus.Resolved => newStatus == TicketStatus.Closed || 
                                   newStatus == TicketStatus.Reopened,
            TicketStatus.Closed => newStatus == TicketStatus.Reopened,
            TicketStatus.Reopened => newStatus == TicketStatus.InProgress || 
                                   newStatus == TicketStatus.Closed,
            _ => false
        };
    }
}

public class UpdateStatusRequest
{
    public TicketStatus Status { get; set; }
}

public class UpdateTicketRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? AssigneeId { get; set; }
}
