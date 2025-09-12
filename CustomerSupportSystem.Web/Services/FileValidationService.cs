using System.Security.Cryptography;

namespace CustomerSupportSystem.Web.Services;

public interface IFileValidationService
{
    Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType, long fileSize);
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string DetectedContentType { get; set; } = string.Empty;
}

public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;
    private readonly long _maxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    // Magic bytes for file types
    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { "image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { "application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/jpg"
    };

    public FileValidationService(ILogger<FileValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType, long fileSize)
    {
        var result = new FileValidationResult();

        // Check file size
        if (fileSize > _maxFileSizeBytes)
        {
            result.ErrorMessage = $"File size {fileSize:N0} bytes exceeds maximum allowed size of {_maxFileSizeBytes:N0} bytes.";
            return result;
        }

        // Check file extension
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            result.ErrorMessage = $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", AllowedExtensions)}";
            return result;
        }

        // Check content type
        if (!AllowedContentTypes.Contains(contentType))
        {
            result.ErrorMessage = $"Content type '{contentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}";
            return result;
        }

        // Read magic bytes
        var buffer = new byte[8];
        var originalPosition = fileStream.Position;
        fileStream.Position = 0;
        
        try
        {
            var bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
            fileStream.Position = originalPosition;

            if (bytesRead < 3)
            {
                result.ErrorMessage = "File is too small to determine type.";
                return result;
            }

            // Check magic bytes
            var detectedType = DetectContentType(buffer);
            if (string.IsNullOrEmpty(detectedType))
            {
                result.ErrorMessage = "File type could not be determined from content.";
                return result;
            }

            // Verify content type matches detected type
            if (!string.Equals(contentType, detectedType, StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = $"Content type mismatch. Expected: {contentType}, Detected: {detectedType}";
                return result;
            }

            result.DetectedContentType = detectedType;
            result.IsValid = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file {FileName}", fileName);
            result.ErrorMessage = "Error reading file content.";
            return result;
        }
    }

    private static string DetectContentType(byte[] buffer)
    {
        foreach (var kvp in MagicBytes)
        {
            if (buffer.Length >= kvp.Value.Length)
            {
                bool matches = true;
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    if (buffer[i] != kvp.Value[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    return kvp.Key;
                }
            }
        }

        return string.Empty;
    }
}
