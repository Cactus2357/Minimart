using MinimartApi.Db.Models;

namespace MinimartApi.Services
{
    public interface IFileService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}
