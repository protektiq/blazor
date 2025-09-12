using CustomerSupportSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CustomerSupportSystem.Web.Authorization;

public class TicketTransitionRequirement : IAuthorizationRequirement
{
}

public class OwnTicketEditRequirement : IAuthorizationRequirement
{
    public TimeSpan MaxEditTime { get; }

    public OwnTicketEditRequirement(TimeSpan maxEditTime)
    {
        MaxEditTime = maxEditTime;
    }
}

public class TicketTransitionHandler : AuthorizationHandler<TicketTransitionRequirement, Ticket>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketTransitionHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TicketTransitionRequirement requirement,
        Ticket ticket)
    {
        var user = await _userManager.GetUserAsync(context.User);
        if (user == null)
        {
            context.Fail();
            return;
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        // Admin can always transition tickets
        if (userRoles.Contains(Roles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        // Assignee can transition tickets they are assigned to
        if (userRoles.Contains(Roles.Agent) && ticket.AssigneeId == user.Id)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}

public class OwnTicketEditHandler : AuthorizationHandler<OwnTicketEditRequirement, Ticket>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public OwnTicketEditHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnTicketEditRequirement requirement,
        Ticket ticket)
    {
        var user = await _userManager.GetUserAsync(context.User);
        if (user == null)
        {
            context.Fail();
            return;
        }

        // Customer can only edit their own tickets
        if (ticket.CustomerId != user.Id)
        {
            context.Fail();
            return;
        }

        // Check if the ticket is within the editable time window
        var timeSinceCreation = DateTime.UtcNow - ticket.CreatedAt;
        if (timeSinceCreation > requirement.MaxEditTime)
        {
            context.Fail();
            return;
        }

        // Check if ticket is in a state that allows customer editing
        if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Resolved)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
