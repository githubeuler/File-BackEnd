
using AutoMapper;
using Files.Application.Interfaces;
using Files.Application.Wrappers;
using MediatR;

namespace Files.Application.Features.Files.Command.DeleteFiles
{
    public class DeleteFilesCommand : IRequest<Response<string>>
    {
        public string FilePath { get; set; }
    }
    public class DeleteFilesCommandHandler : IRequestHandler<DeleteFilesCommand, Response<string>>
    {
        private readonly IUploadFilesAsync _uploadFilesAsync;
        private readonly IMapper _mapper;

        public DeleteFilesCommandHandler(IUploadFilesAsync uploadFilesAsync, IMapper mapper)
        {
            _uploadFilesAsync = uploadFilesAsync;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(DeleteFilesCommand request, CancellationToken cancellationToken)
        {

            var res = await _uploadFilesAsync.DeleteFile(request.FilePath);
            return res;
        }
    }
}
