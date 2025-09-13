using System.ComponentModel.DataAnnotations;

namespace CustomerSupportSystem.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public string? CustomerId { get; set; }
    
    public string? AssigneeId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser? Customer { get; set; }
    public virtual ApplicationUser? Assignee { get; set; }
    public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3,
    Reopened = 4
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
