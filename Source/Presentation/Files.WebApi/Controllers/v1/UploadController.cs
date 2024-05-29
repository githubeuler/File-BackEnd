using Files.Application.Features.Files.Command.DeleteFiles;
using Files.Application.Features.Files.Command.UploadFiles;
using Files.Infraestructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace Files.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class UploadController : BaseApiController
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        public UploadController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;

        }


        [HttpPost("upload-single-file")]
        public async Task<IActionResult> Post([FromForm] UploadFilesCommand command)
        {
            return Ok(await Mediator.Send(command));
        }


        [HttpPost("download-file")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            byte[] fileArray = new byte[1000000];
            string contentType = string.Empty;
            try
            {

                if (_configuration["Ambiente"].ToString() == "1")
                {
                    fileArray = await System.IO.File.ReadAllBytesAsync(_configuration["ServerApiFiles"].ToString() + fileName);
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

                            //var Origen = _configuration["ServerApiFiles"].ToString() + fileName;
                            //var Fin = Directory.GetCurrentDirectory() + fileName;

                            //System.IO.File.Copy(_configuration["ServerApiFiles"].ToString() + fileName, Directory.GetCurrentDirectory() + fileName);

                            var path = _configuration["ServerApiFiles"].ToString() + fileName;
                            //using (var fileStream = new FileStream(path, FileMode.Create))
                            //{
                            //    for (int i = 0; i < fileArray.Length; i++)
                            //    {
                            //        fileStream.WriteByte(fileArray[i]);
                            //    }
                            //    fileStream.Seek(0, SeekOrigin.Begin);
                            //}

                            //var fileStream = System.IO.File.Open(_configuration["ServerApiFiles"].ToString() + fileName, FileMode.Open);

                            //var memoryStream = new MemoryStream();

                            //fileStream.CopyTo(memoryStream);

                            //fileArray = memoryStream.ToArray();
                            //fileStream.Close();
                            //System.IO.File.Delete(Directory.GetCurrentDirectory() + fileName);

                            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                            byte[] ImageData = new byte[fs.Length];
                            fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));
                            fs.Close();
                            fileArray = ImageData;
                        }
                        );
                    }
                }


                string[] words = fileName.Split(@"\");

                for (int i = 1; i < words.Length; i++)
                {
                    if (i == words.Length - 1)
                    {
                        fileName = words[i].ToString();
                    }
                }
                contentType = GetContentType(fileName);

                return File(fileArray, contentType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        [HttpGet("base64-file")]
        public async Task<IActionResult> GetFileBase64(string fileName)
        {
            var fileArray = await System.IO.File.ReadAllBytesAsync(Path.Combine(_environment.ContentRootPath, _configuration["DirectoryApiFiles"].ToString(), $"{fileName}"));
            return Ok(Convert.ToBase64String(fileArray));
        }

        private string GetContentType(string filename)
        {
            string extension = filename.Split(".")[1];
            switch (extension)
            {
                case "pdf":
                    return "application/pdf";
                case "PDF":
                    return "image/png";
                case "png":
                    return "image/png";
                case "PNG":
                    return "image/png";
                case "xml":
                    return "text/xml";
                case "XML":
                    return "TEXT/XML";
                case "jpg":
                    return "image/jpg";
                case "JPG":
                    return "image/jpg";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "DOCX":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xls":
                    return "application/vnd.ms-excel";
                default:
                    return "error";

            }
        }


        [HttpPost("delete-file")]
        public async Task<IActionResult> DeleteFile([FromForm] DeleteFilesCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
