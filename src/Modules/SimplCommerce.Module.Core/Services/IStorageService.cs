using System.IO;
using System.Threading.Tasks;

namespace SimplCommerce.Module.Core.Services
{
    public interface IStorageService
    {
        string GetMediaUrl(string fileName);

        Task SaveMediaAsync(Stream mediaBinaryStream,string category, string fileName, string mimeType = null);

        Task DeleteMediaAsync(string fileName);

        string GetMediaUrlUpload(string fileName, string category);

        string GetMediaUrlImg(string fileName, string category);
    }
}
