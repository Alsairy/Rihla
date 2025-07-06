using Microsoft.AspNetCore.Http;

namespace Rihla.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadDriverDocumentAsync(int driverId, IFormFile file, string documentType);
        Task<string> UploadVehicleDocumentAsync(int vehicleId, IFormFile file, string documentType);
        Task<string> UploadStudentPhotoAsync(int studentId, IFormFile file);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath);
        bool IsValidImageFile(IFormFile file);
        bool IsValidDocumentFile(IFormFile file);
    }
}
