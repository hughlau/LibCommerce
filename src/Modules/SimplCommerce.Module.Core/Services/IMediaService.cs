using System.IO;
using System.Threading.Tasks;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Core.Services
{
    public interface IMediaService
    {
        string GetMediaUrl(Media media);

        string GetMediaUrl(string fileName);

        string GetMediaUrlByUpload(Media media, string category);

        string GetMediaUrlByUpload(string fileName, string category);

        string GetMediaUrlByImg(Media media, string category);

        string GetMediaUrlByImg(string fileName, string category);

        string GetThumbnailUrl(Media media);

        Task SaveMediaAsync(Stream mediaBinaryStream,string category, string fileName, string mimeType = null);

        Task DeleteMediaAsync(Media media);

        Task DeleteMediaAsync(string fileName);

        int FileToImage(string filePath, string categoryId);
    }
}
