using System.ComponentModel.DataAnnotations;

namespace CustomerSupportSystem.Domain.Entities;

public class TicketComment
{
    public int Id { get; set; }
    
    [Required]
    public int TicketId { get; set; }
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsInternal { get; set; } = false; // For internal agent notes
    
    // Navigation properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual ApplicationUser Author { get; set; } = null!;
}
