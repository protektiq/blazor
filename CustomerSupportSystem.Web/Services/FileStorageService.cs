using CustomerSupportSystem.Domain.Entities;
using System.Security.Cryptography;

namespace CustomerSupportSystem.Web.Services;

public interface IFileStorageService
{
    Task<FileStorageResult> StoreFileAsync(Stream fileStream, string originalFileName, string contentType, string uploadedById);
    Task<Stream?> GetFileAsync(string storedFileName);
    Task<bool> DeleteFileAsync(string storedFileName);
    string GenerateDownloadToken();
    string GetSecureDownloadUrl(string token, int attachmentId);
}

public class FileStorageResult
{
    public bool Success { get; set; }
    public string StoredFileName { get; set; } = string.Empty;
    public string DownloadToken { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _storagePath;

    public FileStorageService(ILogger<FileStorageService> logger, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
        
        // Store files outside webroot for security
        _storagePath = Path.Combine(_environment.ContentRootPath, "..", "Attachments");
        
        // Ensure directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<FileStorageResult> StoreFileAsync(Stream fileStream, string originalFileName, string contentType, string uploadedById)
    {
        try
        {
            // Generate secure random filename
            var fileExtension = Path.GetExtension(originalFileName);
            var randomFileName = GenerateSecureFileName() + fileExtension;
            var filePath = Path.Combine(_storagePath, randomFileName);

            // Ensure file doesn't already exist (extremely unlikely but safe)
            while (File.Exists(filePath))
            {
                randomFileName = GenerateSecureFileName() + fileExtension;
                filePath = Path.Combine(_storagePath, randomFileName);
            }

            // Write file to disk
            using (var fileStreamWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamWriter);
            }

            var downloadToken = GenerateDownloadToken();

            return new FileStorageResult
            {
                Success = true,
                StoredFileName = randomFileName,
                DownloadToken = downloadToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing file {FileName}", originalFileName);
            return new FileStorageResult
            {
                Success = false,
                ErrorMessage = "Error storing file."
            };
        }
    }

    public Task<Stream?> GetFileAsync(string storedFileName)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, storedFileName);
            
            if (!File.Exists(filePath))
            {
                return Task.FromResult<Stream?>(null);
            }

            // Return stream without exposing file path
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<Stream?>(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FileName}", storedFileName);
            return Task.FromResult<Stream?>(null);
        }
    }

    public Task<bool> DeleteFileAsync(string storedFileName)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, storedFileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", storedFileName);
            return Task.FromResult(false);
        }
    }

    public string GenerateDownloadToken()
    {
        // Generate a cryptographically secure random token
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public string GetSecureDownloadUrl(string token, int attachmentId)
    {
        return $"/api/attachments/download/{attachmentId}?token={token}";
    }

    private static string GenerateSecureFileName()
    {
        // Generate a secure random filename using GUID and timestamp
        var guid = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"{timestamp}_{guid}";
    }
}
