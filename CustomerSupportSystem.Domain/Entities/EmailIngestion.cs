using System.ComponentModel.DataAnnotations;

namespace CustomerSupportSystem.Domain.Entities;

public class EmailIngestion
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string OriginalBody { get; set; } = string.Empty;
    
    [Required]
    public string ProcessedBody { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string FromEmail { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? FromName { get; set; }
    
    [Required]
    public string MessageId { get; set; } = string.Empty;
    
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsProcessed { get; set; } = false;
    
    public int? CreatedTicketId { get; set; }
    
    public string? ProcessingError { get; set; }
    
    // Navigation properties
    public virtual Ticket? CreatedTicket { get; set; }
}
