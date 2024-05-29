using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Files.Application.Interfaces;
using Files.Application.Wrappers;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace Files.Infraestructure.Repositories
{
    public class UploadFilesAsync : IUploadFilesAsync
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);
        public UploadFilesAsync(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }


        #region Upload File  
        public void UploadFile(List<IFormFile> files, string subDirectory)
        {
            subDirectory = subDirectory ?? string.Empty;
            var target = Path.Combine(_hostingEnvironment.ContentRootPath, subDirectory);

            Directory.CreateDirectory(target);

            files.ForEach(async file =>
            {
                if (file.Length <= 0) return;
                var filePath = Path.Combine(target, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            });
        }
        #endregion

        #region Download File  
        public (string fileType, byte[] archiveData, string archiveName) DownloadFiles(string subDirectory)
        {
            var zipName = $"archive-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.zip";

            var files = Directory.GetFiles(Path.Combine(_hostingEnvironment.ContentRootPath, subDirectory)).ToList();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    files.ForEach(file =>
                    {
                        var theFile = archive.CreateEntry(file);

                        using (var streamWriter = new StreamWriter(theFile.Open()))
                        {
                            streamWriter.Write(File.ReadAllText(file));
                        }
                    });
                }

                return ("application/zip", memoryStream.ToArray(), zipName);
            }

        }
        #endregion

        #region Size Converter  
        public string SizeConverter(long bytes)
        {
            var fileSize = new decimal(bytes);
            var kilobyte = new decimal(1024);
            var megabyte = new decimal(1024 * 1024);
            var gigabyte = new decimal(1024 * 1024 * 1024);

            switch (fileSize)
            {
                case var _ when fileSize < kilobyte:
                    return $"Less then 1KB";
                case var _ when fileSize < megabyte:
                    return $"{Math.Round(fileSize / kilobyte, 0, MidpointRounding.AwayFromZero):##,###.##}KB";
                case var _ when fileSize < gigabyte:
                    return $"{Math.Round(fileSize / megabyte, 2, MidpointRounding.AwayFromZero):##,###.##}MB";
                case var _ when fileSize >= gigabyte:
                    return $"{Math.Round(fileSize / gigabyte, 2, MidpointRounding.AwayFromZero):##,###.##}GB";
                default:
                    return "n/a";
            }
        }
        #endregion



        public bool CheckFileExtension(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".docx" || extension == ".pdf" || extension == ".xml" || extension == ".png" || extension == ".jpg"); // Change the extension based on your need
        }

        public async Task<Response<string>> WriteFile(IFormFile file, string fileName, string NombreSeccion)
        {
            bool isSaveSuccess = false;
            byte[] fileArray = new byte[1000000];
            Response<string> response = new Response<string>();
            try
            {

                fileName = fileName.Split('.')[0].ToString();

                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = fileName + extension; //Create a new Name for the file due to security reasons.

                var path = _configuration["ServerApiFiles"].ToString();
                if (NombreSeccion != null)
                {
                    if (_configuration["Ambiente"].ToString() == "1")
                    {
                        string[] words = NombreSeccion.Split(@"\");
                        for (int i = 1; i < words.Length; i++)
                        {
                            if (words[i] != null)
                            {
                                path = path + @"\" + words[i].ToString();
                                if (!Directory.Exists(path))
                                    System.IO.Directory.CreateDirectory(path);
                            }
                        }
                        path = path + @"\" + fileName;
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        string usuario = _configuration["UserDirectoryApiFiles"].ToString();
                        string dominio = _configuration["ServerApiFiles"].ToString();
                        string clave = _configuration["PasswordDirectoryApiFiles"].ToString();

                        using (WindowsLogin wi = new WindowsLogin(usuario, dominio, clave))
                        {
                            WindowsIdentity.RunImpersonated(wi.Identity.AccessToken, async () =>
                            {
                                WindowsIdentity useri = WindowsIdentity.GetCurrent();
                                string[] words = NombreSeccion.Split(@"\");
                                for (int i = 1; i < words.Length; i++)
                                {
                                    if (words[i] != null)
                                    {
                                        if (words[i] != string.Empty)
                                        {
                                            path = path + @"\" + words[i].ToString();
                                            if (!Directory.Exists(path))
                                                System.IO.Directory.CreateDirectory(path);
                                        }
                                    }
                                }
                                path = path + @"\" + fileName;


                                using (var ms = new MemoryStream())
                                {
                                    file.CopyTo(ms);
                                    fileArray = ms.ToArray();
                                }
                                Int32 offset = 0;
                                Int32 sizeOfBuffer = 999999;
                                FileStream fileStream = null;

                                try
                                {
                                    fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: sizeOfBuffer, useAsync: true);
                                    await fileStream.WriteAsync(fileArray, offset, fileArray.Length);
                                }
                                catch
                                {
                                    //Write code here to handle exceptions.

                                }
                                finally
                                {
                                    if (fileStream != null)
                                        fileStream.Dispose();
                                }

                            }
                            );
                        }
                    }
                }
                else
                {
                    path = _configuration["ServerApiFiles"].ToString() + @"\" + _configuration["DirectoryApiFiles"].ToString() + fileName;
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyToAsync(stream);
                    }
                }

            }
            catch (Exception ex)
            {
                return new Response<string>(ex.Message.ToString());
            }

            return new Response<string>("correcto", "Archivo adjuntado exitosamente.");
        }

        public async Task<Response<string>> DeleteFile(string filePath)
        {

            string contentType = string.Empty;
            try
            {

                if (_configuration["Ambiente"].ToString() == "1")
                {
                    System.IO.File.Delete(_configuration["ServerApiFiles"].ToString() + filePath);
                }
                else
                {
                    string usuario = _configuration["UserDirectoryApiFiles"].ToString();
                    string dominio = _configuration["ServerApiFiles"].ToString();
                    string clave = _configuration["PasswordDirectoryApiFiles"].ToString();

                    using (WindowsLogin wi = new WindowsLogin(usuario, dominio, clave))
                    {
                        WindowsIdentity.RunImpersonated(wi.Identity.AccessToken, async () =>
                        {
                            WindowsIdentity useri = WindowsIdentity.GetCurrent();
                            var path = _configuration["ServerApiFiles"].ToString() + filePath;
                            System.IO.File.Delete(path);
                        }
                        );
                    }
                }

                return new Response<string>("correcto", "Archivo se elimino exitosamente.");
            }
            catch (Exception ex)
            {
                return new Response<string>(ex.Message.ToString());
            }
        }
    }
}
