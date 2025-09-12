using System.ComponentModel.DataAnnotations;

namespace CustomerSupportSystem.Domain.Entities;

public class TicketAttachment
{
    public int Id { get; set; }
    
    [Required]
    public int TicketId { get; set; }
    
    [Required]
    public string UploadedById { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    [Required]
    public long FileSizeBytes { get; set; }
    
    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [StringLength(64)]
    public string DownloadToken { get; set; } = string.Empty;
    
    public DateTime? TokenExpiresAt { get; set; }
    
    // Navigation properties
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual ApplicationUser UploadedBy { get; set; } = null!;
}
