using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerSupportSystem.Web.Services;
using CustomerSupportSystem.Data.Context;
using CustomerSupportSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportSystem.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly ILogger<AttachmentsController> _logger;
    private readonly IFileValidationService _fileValidationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AttachmentsController(
        ILogger<AttachmentsController> logger,
        IFileValidationService fileValidationService,
        IFileStorageService fileStorageService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _fileValidationService = fileValidationService;
        _fileStorageService = fileStorageService;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("upload/{ticketId}")]
    public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Check if ticket exists and user has access
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

            // Check authorization - customer can upload to own tickets, agents/admins to any ticket
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var canUpload = userRoles.Contains("Admin") || 
                           userRoles.Contains("Agent") || 
                           ticket.CustomerId == currentUser.Id;

            if (!canUpload)
            {
                return Forbid("You don't have permission to upload files to this ticket.");
            }

            // Validate file
            var validationResult = await _fileValidationService.ValidateFileAsync(
                file.OpenReadStream(), 
                file.FileName, 
                file.ContentType, 
                file.Length);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            // Store file
            var storageResult = await _fileStorageService.StoreFileAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                currentUser.Id);

            if (!storageResult.Success)
            {
                return StatusCode(500, storageResult.ErrorMessage);
            }

            // Create attachment record
            var attachment = new TicketAttachment
            {
                TicketId = ticketId,
                UploadedById = currentUser.Id,
                OriginalFileName = file.FileName,
                StoredFileName = storageResult.StoredFileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow,
                DownloadToken = storageResult.DownloadToken,
                TokenExpiresAt = DateTime.UtcNow.AddDays(7) // Token expires in 7 days
            };

            _context.TicketAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                attachmentId = attachment.Id,
                fileName = attachment.OriginalFileName,
                fileSize = attachment.FileSizeBytes,
                contentType = attachment.ContentType,
                uploadedAt = attachment.UploadedAt,
                downloadUrl = _fileStorageService.GetSecureDownloadUrl(attachment.DownloadToken, attachment.Id)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for ticket {TicketId}", ticketId);
            return StatusCode(500, "Error uploading file.");
        }
    }

    [HttpGet("download/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId, [FromQuery] string token)
    {
        try
        {
            var attachment = await _context.TicketAttachments
                .Include(a => a.Ticket)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
            {
                return NotFound("Attachment not found.");
            }

            // Validate download token
            if (attachment.DownloadToken != token)
            {
                return Forbid("Invalid download token.");
            }

            // Check if token is expired
            if (attachment.TokenExpiresAt.HasValue && attachment.TokenExpiresAt < DateTime.UtcNow)
            {
                return Forbid("Download token has expired.");
            }

            // Check if user has access to the ticket
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var canDownload = userRoles.Contains("Admin") || 
                            userRoles.Contains("Agent") || 
                            attachment.Ticket.CustomerId == currentUser.Id;

            if (!canDownload)
            {
                return Forbid("You don't have permission to download this file.");
            }

            // Get file stream
            var fileStream = await _fileStorageService.GetFileAsync(attachment.StoredFileName);
            if (fileStream == null)
            {
                return NotFound("File not found on disk.");
            }

            return File(fileStream, attachment.ContentType, attachment.OriginalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(500, "Error downloading file.");
        }
    }

    [HttpDelete("{attachmentId}")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
    {
        try
        {
            var attachment = await _context.TicketAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null)
            {
                return NotFound("Attachment not found.");
            }

            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(attachment.StoredFileName);

            // Remove from database
            _context.TicketAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return StatusCode(500, "Error deleting file.");
        }
    }

    [HttpGet("ticket/{ticketId}")]
    public async Task<IActionResult> GetTicketAttachments(int ticketId)
    {
        try
        {
            // Check if ticket exists and user has access
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

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var canView = userRoles.Contains("Admin") || 
                         userRoles.Contains("Agent") || 
                         ticket.CustomerId == currentUser.Id;

            if (!canView)
            {
                return Forbid("You don't have permission to view attachments for this ticket.");
            }

            var attachments = await _context.TicketAttachments
                .Include(a => a.UploadedBy)
                .Where(a => a.TicketId == ticketId)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new
                {
                    a.Id,
                    a.OriginalFileName,
                    a.FileSizeBytes,
                    a.ContentType,
                    a.UploadedAt,
                    uploadedBy = a.UploadedBy.FullName,
                    downloadUrl = _fileStorageService.GetSecureDownloadUrl(a.DownloadToken, a.Id)
                })
                .ToListAsync();

            return Ok(attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attachments for ticket {TicketId}", ticketId);
            return StatusCode(500, "Error retrieving attachments.");
        }
    }
}
