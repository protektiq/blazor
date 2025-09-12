using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerSupportSystem.Web.Services;
using CustomerSupportSystem.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportSystem.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Agent")]
public class EmailIngestionController : ControllerBase
{
    private readonly ILogger<EmailIngestionController> _logger;
    private readonly IEmailIngestionService _emailIngestionService;
    private readonly ApplicationDbContext _context;

    public EmailIngestionController(
        ILogger<EmailIngestionController> logger,
        IEmailIngestionService emailIngestionService,
        ApplicationDbContext context)
    {
        _logger = logger;
        _emailIngestionService = emailIngestionService;
        _context = context;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessEmail([FromBody] ProcessEmailRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Subject) || 
                string.IsNullOrWhiteSpace(request.Body) || 
                string.IsNullOrWhiteSpace(request.FromEmail) ||
                string.IsNullOrWhiteSpace(request.MessageId))
            {
                return BadRequest("Missing required fields.");
            }

            var result = await _emailIngestionService.ProcessEmailAsync(
                request.Subject,
                request.Body,
                request.FromEmail,
                request.FromName ?? string.Empty,
                request.MessageId);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new
            {
                success = true,
                createdTicketId = result.CreatedTicketId,
                processedBody = result.ProcessedBody
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email from {FromEmail}", request.FromEmail);
            return StatusCode(500, "Error processing email.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmailIngestions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.EmailIngestions
                .OrderByDescending(e => e.ReceivedAt);

            var totalCount = await query.CountAsync();
            
            var emails = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.Subject,
                    e.FromEmail,
                    e.FromName,
                    e.ReceivedAt,
                    e.IsProcessed,
                    e.ProcessedAt,
                    e.CreatedTicketId,
                    e.ProcessingError
                })
                .ToListAsync();

            return Ok(new
            {
                emails,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email ingestions");
            return StatusCode(500, "Error retrieving email ingestions.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmailIngestion(int id)
    {
        try
        {
            var email = await _context.EmailIngestions
                .FirstOrDefaultAsync(e => e.Id == id);

            if (email == null)
            {
                return NotFound("Email ingestion not found.");
            }

            return Ok(new
            {
                email.Id,
                email.Subject,
                email.OriginalBody,
                email.ProcessedBody,
                email.FromEmail,
                email.FromName,
                email.MessageId,
                email.ReceivedAt,
                email.IsProcessed,
                email.ProcessedAt,
                email.CreatedTicketId,
                email.ProcessingError
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email ingestion {Id}", id);
            return StatusCode(500, "Error retrieving email ingestion.");
        }
    }

    [HttpPost("{id}/retry")]
    public async Task<IActionResult> RetryEmailProcessing(int id)
    {
        try
        {
            var email = await _context.EmailIngestions
                .FirstOrDefaultAsync(e => e.Id == id);

            if (email == null)
            {
                return NotFound("Email ingestion not found.");
            }

            if (email.IsProcessed && email.CreatedTicketId.HasValue)
            {
                return BadRequest("Email has already been processed successfully.");
            }

            // Reset processing state
            email.IsProcessed = false;
            email.ProcessingError = null;
            email.CreatedTicketId = null;

            await _context.SaveChangesAsync();

            // Retry processing
            var result = await _emailIngestionService.ProcessEmailAsync(
                email.Subject,
                email.OriginalBody,
                email.FromEmail,
                email.FromName ?? string.Empty,
                email.MessageId);

            if (result.Success)
            {
                email.IsProcessed = true;
                email.ProcessedAt = DateTime.UtcNow;
                email.CreatedTicketId = result.CreatedTicketId;
                email.ProcessingError = null;
            }
            else
            {
                email.ProcessingError = result.ErrorMessage;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = result.Success,
                createdTicketId = result.CreatedTicketId,
                error = result.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying email processing for {Id}", id);
            return StatusCode(500, "Error retrying email processing.");
        }
    }
}

public class ProcessEmailRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string MessageId { get; set; } = string.Empty;
}
