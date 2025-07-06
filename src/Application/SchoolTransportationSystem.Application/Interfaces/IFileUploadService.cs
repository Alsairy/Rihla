namespace Rihla.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadDriverDocumentAsync(int driverId, Stream fileStream, string fileName, string documentType);
        Task<string> UploadVehicleDocumentAsync(int vehicleId, Stream fileStream, string fileName, string documentType);
        Task<string> UploadStudentPhotoAsync(int studentId, Stream fileStream, string fileName);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath);
        bool IsValidImageFile(string fileName, long fileSize);
        bool IsValidDocumentFile(string fileName, long fileSize);
    }
}
