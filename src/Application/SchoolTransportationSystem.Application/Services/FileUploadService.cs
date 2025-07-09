using Microsoft.Extensions.Configuration;
using Rihla.Application.Interfaces;

namespace Rihla.Application.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };

        public FileUploadService(IConfiguration configuration)
        {
            _uploadPath = configuration["FileUpload:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            _maxFileSize = long.TryParse(configuration["FileUpload:MaxFileSize"], out var maxSize) ? maxSize : 5 * 1024 * 1024; // 5MB default
            
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadDriverDocumentAsync(int driverId, Stream fileStream, string fileName, string documentType)
        {
            if (!IsValidDocumentFile(fileName, fileStream.Length))
                throw new ArgumentException("Invalid file type or size");

            var newFileName = $"driver_{driverId}_{documentType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}";
            var driverPath = Path.Combine(_uploadPath, "drivers", driverId.ToString());
            
            if (!Directory.Exists(driverPath))
                Directory.CreateDirectory(driverPath);

            var filePath = Path.Combine(driverPath, newFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            return Path.Combine("drivers", driverId.ToString(), newFileName);
        }

        public async Task<string> UploadVehicleDocumentAsync(int vehicleId, Stream fileStream, string fileName, string documentType)
        {
            if (!IsValidDocumentFile(fileName, fileStream.Length))
                throw new ArgumentException("Invalid file type or size");

            var newFileName = $"vehicle_{vehicleId}_{documentType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}";
            var vehiclePath = Path.Combine(_uploadPath, "vehicles", vehicleId.ToString());
            
            if (!Directory.Exists(vehiclePath))
                Directory.CreateDirectory(vehiclePath);

            var filePath = Path.Combine(vehiclePath, newFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            return Path.Combine("vehicles", vehicleId.ToString(), newFileName);
        }

        public async Task<string> UploadStudentPhotoAsync(int studentId, Stream fileStream, string fileName)
        {
            if (!IsValidImageFile(fileName, fileStream.Length))
                throw new ArgumentException("Invalid image file type or size");

            var newFileName = $"student_{studentId}_photo_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}";
            var studentPath = Path.Combine(_uploadPath, "students", studentId.ToString());
            
            if (!Directory.Exists(studentPath))
                Directory.CreateDirectory(studentPath);

            var filePath = Path.Combine(studentPath, newFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            return Path.Combine("students", studentId.ToString(), newFileName);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_uploadPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            if (File.Exists(fullPath))
            {
                return await File.ReadAllBytesAsync(fullPath);
            }
            throw new FileNotFoundException("File not found");
        }

        public async Task<string> GetFileUrlAsync(string filePath)
        {
            return $"/api/files/{filePath.Replace('\\', '/')}";
        }

        public bool IsValidImageFile(string fileName, long fileSize)
        {
            if (string.IsNullOrEmpty(fileName) || fileSize == 0 || fileSize > _maxFileSize)
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension);
        }

        public bool IsValidDocumentFile(string fileName, long fileSize)
        {
            if (string.IsNullOrEmpty(fileName) || fileSize == 0 || fileSize > _maxFileSize)
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedDocumentExtensions.Contains(extension);
        }
    }
}
