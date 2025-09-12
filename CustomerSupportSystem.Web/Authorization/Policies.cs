using Microsoft.AspNetCore.Authorization;

namespace CustomerSupportSystem.Web.Authorization;

public static class Policies
{
    public const string CanTransitionTicket = "CanTransitionTicket";
    public const string CanEditOwnTicket = "CanEditOwnTicket";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanViewAllTickets = "CanViewAllTickets";
    public const string CanManageAttachments = "CanManageAttachments";
    public const string CanManageEmailIngestion = "CanManageEmailIngestion";
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Agent = "Agent";
    public const string Customer = "Customer";
}
