using CustomerSupportSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportSystem.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<EmailIngestion> EmailIngestions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Ticket entity
        builder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Priority).HasConversion<int>();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.AssigneeId);

            // Configure relationships
            entity.HasOne(e => e.Customer)
                  .WithMany(u => u.CreatedTickets)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Assignee)
                  .WithMany(u => u.AssignedTickets)
                  .HasForeignKey(e => e.AssigneeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Comments)
                  .WithOne(c => c.Ticket)
                  .HasForeignKey(c => c.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Attachments)
                  .WithOne(a => a.Ticket)
                  .HasForeignKey(a => a.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TicketComment entity
        builder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TicketId).IsRequired();
            entity.Property(e => e.AuthorId).IsRequired();
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsInternal).HasDefaultValue(false);

            // Configure relationships
            entity.HasOne(e => e.Author)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Ticket)
                  .WithMany(t => t.Comments)
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ApplicationUser entity
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(e => e.UploadedAttachments)
                  .WithOne(a => a.UploadedBy)
                  .HasForeignKey(a => a.UploadedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TicketAttachment entity
        builder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TicketId).IsRequired();
            entity.Property(e => e.UploadedById).IsRequired();
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileSizeBytes).IsRequired();
            entity.Property(e => e.UploadedAt).IsRequired();
            entity.Property(e => e.DownloadToken).IsRequired().HasMaxLength(64);

            entity.HasIndex(e => e.DownloadToken).IsUnique();
            entity.HasIndex(e => e.TicketId);
        });

        // Configure EmailIngestion entity
        builder.Entity<EmailIngestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OriginalBody).IsRequired();
            entity.Property(e => e.ProcessedBody).IsRequired();
            entity.Property(e => e.FromEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FromName).HasMaxLength(255);
            entity.Property(e => e.MessageId).IsRequired();
            entity.Property(e => e.ReceivedAt).IsRequired();

            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => e.ReceivedAt);
            entity.HasIndex(e => e.IsProcessed);

            entity.HasOne(e => e.CreatedTicket)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedTicketId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
