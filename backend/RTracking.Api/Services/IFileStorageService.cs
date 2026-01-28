namespace RTracking.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder = "receipts");
    Task<bool> DeleteFileAsync(string filePath);
    string GetUploadsDirectory();
}
