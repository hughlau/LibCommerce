using System.IO;
using System.Threading.Tasks;
using SimplCommerce.Infrastructure;
using SimplCommerce.Module.Core.Services;

namespace SimplCommerce.Module.StorageLocal
{
    public class LocalStorageService : IStorageService
    {
        private const string MediaRootFoler = "user-content";

        private const string UploadRootFoler = "upfile";

        private const string ImgRootFloer = "imgfile";

        public string GetMediaUrl(string fileName)
        {
            return $"/{MediaRootFoler}/{fileName}";
        }

        public async Task SaveMediaAsync(Stream mediaBinaryStream,string category ,string fileName, string mimeType = null)
        {
            if (!Directory.Exists(Path.Combine(GlobalConfiguration.WebRootPath, UploadRootFoler, category)))
            {
                Directory.CreateDirectory(Path.Combine(GlobalConfiguration.WebRootPath, UploadRootFoler, category));
            }
            var filePath = Path.Combine(GlobalConfiguration.WebRootPath, UploadRootFoler, category, fileName);
            using (var output = new FileStream(filePath, FileMode.Create))
            {
                await mediaBinaryStream.CopyToAsync(output);
            }
        }

        public async Task DeleteMediaAsync(string fileName)
        {
            var filePath = Path.Combine(GlobalConfiguration.WebRootPath, UploadRootFoler, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }

        public string GetMediaUrlUpload(string fileName,string category)
        {
            return $"/{UploadRootFoler}/{category}/{fileName}";
        }

        public string GetMediaUrlImg(string fileName,string category)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            return $"/{ImgRootFloer}/{category}/{fileName}/";
        }
    }
}
