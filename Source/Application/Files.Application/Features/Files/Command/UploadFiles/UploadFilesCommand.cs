using AutoMapper;
using Files.Application.Interfaces;
using Files.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Files.Application.Features.Files.Command.UploadFiles
{
    public class UploadFilesCommand : IRequest<Response<string>>
    {
        public string? Filename { get; set; }
        public string? NombreSeccion { get; set; }
        public IFormFile? Attachment { get; set; }

    }

    public class UploadFilesCommandHandler : IRequestHandler<UploadFilesCommand, Response<string>>
    {
        private readonly IUploadFilesAsync _uploadFilesAsync;
        private readonly IMapper _mapper;

        public UploadFilesCommandHandler(IUploadFilesAsync uploadFilesAsync, IMapper mapper)
        {
            _uploadFilesAsync = uploadFilesAsync;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(UploadFilesCommand request, CancellationToken cancellationToken)
        {

            var res = await _uploadFilesAsync.WriteFile(request.Attachment, request.Filename, request.NombreSeccion);
            return res;
        }
    }
}
