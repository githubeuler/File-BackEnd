using Microsoft.AspNetCore.Http;

namespace Files.Application.DTOs.Uploads
{
    public class FileModelEntryDto
    {
        public string? Filename { get; set; }
        public IFormFile? Attachment { get; set; }
    }


    public class FileModelDownloadDto
    {
        public string? Filename { get; set; }
        public string? ContentType { get; set; }
    }
}
