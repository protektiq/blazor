using CustomerSupportSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CustomerSupportSystem.Web.Services;

public interface IEmailIngestionService
{
    Task<EmailProcessingResult> ProcessEmailAsync(string subject, string body, string fromEmail, string fromName, string messageId);
    Task<EmailIngestion> CreateEmailIngestionRecordAsync(string subject, string originalBody, string fromEmail, string fromName, string messageId);
}

public class EmailProcessingResult
{
    public bool Success { get; set; }
    public int? CreatedTicketId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ProcessedBody { get; set; } = string.Empty;
}

public class EmailIngestionService : IEmailIngestionService
{
    private readonly ILogger<EmailIngestionService> _logger;
    private readonly CustomerSupportSystem.Data.Context.ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // PII patterns for redaction
    private static readonly List<Regex> PiiPatterns = new()
    {
        // Credit card numbers
        new Regex(@"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", RegexOptions.Compiled),
        
        // SSN patterns
        new Regex(@"\b\d{3}[-\s]?\d{2}[-\s]?\d{4}\b", RegexOptions.Compiled),
        
        // Phone numbers
        new Regex(@"\b\d{3}[-\s]?\d{3}[-\s]?\d{4}\b", RegexOptions.Compiled),
        
        // Email addresses (keep the sender's email)
        new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled),
        
        // IP addresses
        new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled)
    };

    public EmailIngestionService(
        ILogger<EmailIngestionService> logger,
        CustomerSupportSystem.Data.Context.ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<EmailProcessingResult> ProcessEmailAsync(string subject, string body, string fromEmail, string fromName, string messageId)
    {
        try
        {
            // Check if email already processed
            var existingIngestion = await _context.EmailIngestions
                .FirstOrDefaultAsync(e => e.MessageId == messageId);

            if (existingIngestion != null)
            {
                return new EmailProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Email already processed."
                };
            }

            // Redact PII from body
            var processedBody = RedactPii(body, fromEmail);

            // Create email ingestion record
            var emailIngestion = new EmailIngestion
            {
                Subject = subject,
                OriginalBody = body,
                ProcessedBody = processedBody,
                FromEmail = fromEmail,
                FromName = fromName,
                MessageId = messageId,
                ReceivedAt = DateTime.UtcNow
            };

            _context.EmailIngestions.Add(emailIngestion);
            await _context.SaveChangesAsync();

            // Try to create ticket
            var ticketResult = await CreateTicketFromEmailAsync(emailIngestion);
            
            if (ticketResult.Success)
            {
                emailIngestion.CreatedTicketId = ticketResult.CreatedTicketId;
                emailIngestion.IsProcessed = true;
                emailIngestion.ProcessedAt = DateTime.UtcNow;
            }
            else
            {
                emailIngestion.ProcessingError = ticketResult.ErrorMessage;
            }

            await _context.SaveChangesAsync();

            return new EmailProcessingResult
            {
                Success = true,
                CreatedTicketId = ticketResult.CreatedTicketId,
                ProcessedBody = processedBody
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email from {FromEmail}", fromEmail);
            return new EmailProcessingResult
            {
                Success = false,
                ErrorMessage = "Error processing email."
            };
        }
    }

    public async Task<EmailIngestion> CreateEmailIngestionRecordAsync(string subject, string originalBody, string fromEmail, string fromName, string messageId)
    {
        var emailIngestion = new EmailIngestion
        {
            Subject = subject,
            OriginalBody = originalBody,
            ProcessedBody = RedactPii(originalBody, fromEmail),
            FromEmail = fromEmail,
            FromName = fromName,
            MessageId = messageId,
            ReceivedAt = DateTime.UtcNow
        };

        _context.EmailIngestions.Add(emailIngestion);
        await _context.SaveChangesAsync();

        return emailIngestion;
    }

    private string RedactPii(string text, string senderEmail)
    {
        var redactedText = text;

        foreach (var pattern in PiiPatterns)
        {
            if (pattern.ToString().Contains("email"))
            {
                // For email patterns, only redact if it's not the sender's email
                redactedText = pattern.Replace(redactedText, match =>
                {
                    if (string.Equals(match.Value, senderEmail, StringComparison.OrdinalIgnoreCase))
                        return match.Value; // Keep sender's email
                    return "[EMAIL_REDACTED]";
                });
            }
            else
            {
                redactedText = pattern.Replace(redactedText, "[REDACTED]");
            }
        }

        return redactedText;
    }

    private async Task<TicketCreationResult> CreateTicketFromEmailAsync(EmailIngestion emailIngestion)
    {
        try
        {
            // Find or create customer user
            var customer = await _userManager.FindByEmailAsync(emailIngestion.FromEmail);
            
            if (customer == null)
            {
                // Create new customer user
                customer = new ApplicationUser
                {
                    UserName = emailIngestion.FromEmail,
                    Email = emailIngestion.FromEmail,
                    FirstName = emailIngestion.FromName ?? "Customer",
                    LastName = "User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(customer);
                if (!result.Succeeded)
                {
                    return new TicketCreationResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to create customer user."
                    };
                }

                await _userManager.AddToRoleAsync(customer, "Customer");
            }

            // Create ticket
            var ticket = new Ticket
            {
                Title = emailIngestion.Subject,
                Description = emailIngestion.ProcessedBody,
                CustomerId = customer.Id,
                Status = TicketStatus.Open,
                Priority = TicketPriority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return new TicketCreationResult
            {
                Success = true,
                CreatedTicketId = ticket.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket from email");
            return new TicketCreationResult
            {
                Success = false,
                ErrorMessage = "Error creating ticket from email."
            };
        }
    }

    private class TicketCreationResult
    {
        public bool Success { get; set; }
        public int? CreatedTicketId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
