using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace API.IRepository
{
    public interface IFileRepository
    
    {
        Task<string> CreateFileAsync(IFormFile file, string path);
        Task<string> CreateFileFromBase64Async(string base64String, string path);
        public void DeleteFile(string path);

    }
}