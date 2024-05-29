using Files.Application.Wrappers;
using Microsoft.AspNetCore.Http;

namespace Files.Application.Interfaces
{
    public interface IUploadFilesAsync
    {
        void UploadFile(List<IFormFile> files, string subDirectory);
        (string fileType, byte[] archiveData, string archiveName) DownloadFiles(string subDirectory);
        string SizeConverter(long bytes);
        bool CheckFileExtension(IFormFile file);
        Task<Response<string>> WriteFile(IFormFile file, string fileName, string NombreSeccion);
        Task<Response<string>> DeleteFile(string filePath);
    }
}
