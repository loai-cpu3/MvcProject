using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MvcProject.Data;
using MvcProject.Models.Domain;

namespace MvcProject.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AttachmentService> _logger;

        private static readonly string[] AllowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg", ".txt" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public AttachmentService(ApplicationDbContext db, IWebHostEnvironment env, ILogger<AttachmentService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public async Task<int> UploadAsync(IFormFile file, int taskId, CancellationToken ct = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.Length == 0) throw new InvalidOperationException("Empty file.");
            if (file.Length > MaxFileSize) throw new InvalidOperationException("File exceeds maximum allowed size.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("File type not allowed.");

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "tasks", taskId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(fs, ct);
            }

            var attachment = new Attachment
            {
                ProjectTaskId = taskId,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                Size = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Uploaded attachment {AttachmentId} for task {TaskId}", attachment.Id, taskId);

            return attachment.Id;
        }

        public async Task DeleteAsync(int attachmentId, CancellationToken ct = default)
        {
            var attach = await _db.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
            if (attach == null) return;

            var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "tasks", attach.ProjectTaskId.ToString(), attach.StoredFileName);
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {FilePath}", filePath);
            }

            _db.Attachments.Remove(attach);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted attachment {AttachmentId}", attachmentId);
        }

        public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(int attachmentId, CancellationToken ct = default)
        {
            var attach = await _db.Attachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
            if (attach == null) throw new FileNotFoundException();

            var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "tasks", attach.ProjectTaskId.ToString(), attach.StoredFileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, attach.ContentType ?? "application/octet-stream", attach.OriginalFileName);
        }
    }
}