namespace RTracking.Api.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    private const string UploadsFolder = "uploads";

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public string GetUploadsDirectory()
    {
        // For API projects, use ContentRootPath if WebRootPath is null
        var basePath = _environment.WebRootPath ?? _environment.ContentRootPath;
        var uploadsPath = Path.Combine(basePath, UploadsFolder);
        return uploadsPath;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subfolder = "receipts")
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        var uploadsPath = GetUploadsDirectory();
        var folderPath = Path.Combine(uploadsPath, subfolder);
        
        // Ensure directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path from wwwroot or absolute path
        var relativePath = Path.Combine(UploadsFolder, subfolder, uniqueFileName).Replace('\\', '/');
        
        _logger.LogInformation("File saved: {FilePath}", relativePath);
        
        return relativePath;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        try
        {
            // Handle both relative and absolute paths
            string fullPath;
            if (Path.IsPathRooted(filePath))
            {
                fullPath = filePath;
            }
            else
            {
                var uploadsPath = GetUploadsDirectory();
                fullPath = Path.Combine(uploadsPath, filePath.TrimStart('/', '\\'));
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", fullPath);
                return await Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }
}
