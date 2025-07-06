using Microsoft.AspNetCore.Http;
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
            _maxFileSize = configuration.GetValue<long>("FileUpload:MaxFileSize", 5 * 1024 * 1024); // 5MB default
            
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadDriverDocumentAsync(int driverId, IFormFile file, string documentType)
        {
            if (!IsValidDocumentFile(file))
                throw new ArgumentException("Invalid file type or size");

            var fileName = $"driver_{driverId}_{documentType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(file.FileName)}";
            var driverPath = Path.Combine(_uploadPath, "drivers", driverId.ToString());
            
            if (!Directory.Exists(driverPath))
                Directory.CreateDirectory(driverPath);

            var filePath = Path.Combine(driverPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("drivers", driverId.ToString(), fileName);
        }

        public async Task<string> UploadVehicleDocumentAsync(int vehicleId, IFormFile file, string documentType)
        {
            if (!IsValidDocumentFile(file))
                throw new ArgumentException("Invalid file type or size");

            var fileName = $"vehicle_{vehicleId}_{documentType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(file.FileName)}";
            var vehiclePath = Path.Combine(_uploadPath, "vehicles", vehicleId.ToString());
            
            if (!Directory.Exists(vehiclePath))
                Directory.CreateDirectory(vehiclePath);

            var filePath = Path.Combine(vehiclePath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("vehicles", vehicleId.ToString(), fileName);
        }

        public async Task<string> UploadStudentPhotoAsync(int studentId, IFormFile file)
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("Invalid image file type or size");

            var fileName = $"student_{studentId}_photo_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(file.FileName)}";
            var studentPath = Path.Combine(_uploadPath, "students", studentId.ToString());
            
            if (!Directory.Exists(studentPath))
                Directory.CreateDirectory(studentPath);

            var filePath = Path.Combine(studentPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("students", studentId.ToString(), fileName);
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

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension);
        }

        public bool IsValidDocumentFile(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedDocumentExtensions.Contains(extension);
        }
    }
}
