using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rihla.Application.Interfaces;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FilesController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("driver/{driverId}/document")]
        public async Task<IActionResult> UploadDriverDocument(int driverId, IFormFile file, [FromForm] string documentType)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                var filePath = await _fileUploadService.UploadDriverDocumentAsync(driverId, file, documentType);
                var fileUrl = await _fileUploadService.GetFileUrlAsync(filePath);

                return Ok(new { FilePath = filePath, FileUrl = fileUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpPost("vehicle/{vehicleId}/document")]
        public async Task<IActionResult> UploadVehicleDocument(int vehicleId, IFormFile file, [FromForm] string documentType)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                var filePath = await _fileUploadService.UploadVehicleDocumentAsync(vehicleId, file, documentType);
                var fileUrl = await _fileUploadService.GetFileUrlAsync(filePath);

                return Ok(new { FilePath = filePath, FileUrl = fileUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpPost("student/{studentId}/photo")]
        public async Task<IActionResult> UploadStudentPhoto(int studentId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                var filePath = await _fileUploadService.UploadStudentPhotoAsync(studentId, file);
                var fileUrl = await _fileUploadService.GetFileUrlAsync(filePath);

                return Ok(new { FilePath = filePath, FileUrl = fileUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpGet("{*filePath}")]
        public async Task<IActionResult> GetFile(string filePath)
        {
            try
            {
                var fileBytes = await _fileUploadService.GetFileAsync(filePath);
                var contentType = GetContentType(filePath);
                return File(fileBytes, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving file: {ex.Message}");
            }
        }

        [HttpDelete("{*filePath}")]
        public async Task<IActionResult> DeleteFile(string filePath)
        {
            try
            {
                var success = await _fileUploadService.DeleteFileAsync(filePath);
                if (success)
                    return Ok("File deleted successfully");
                else
                    return NotFound("File not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }
    }
}
