using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MvcProject.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<int> UploadAsync(IFormFile file, int taskId, CancellationToken ct = default);
        Task DeleteAsync(int attachmentId, CancellationToken ct = default);
        Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(int attachmentId, CancellationToken ct = default);
    }
}