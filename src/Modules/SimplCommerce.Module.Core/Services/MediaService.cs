using System.IO;
using System.Threading.Tasks;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Core.Services
{
    public class MediaService : IMediaService
    {
        private readonly IRepository<Media> _mediaRepository;
        private readonly IStorageService _storageService;
        private readonly ITransferFileService _transferFileService;

        public MediaService(IRepository<Media> mediaRepository, IStorageService storageService
            ,ITransferFileService transferFileService)
        {
            _mediaRepository = mediaRepository;
            _storageService = storageService;
            _transferFileService = transferFileService;
        }

        public string GetMediaUrl(Media media)
        {
            if(media == null)
            {
                return GetMediaUrl("no-image.png");
            }

            return GetMediaUrl(media.FileName);
        }

        public string GetMediaUrl(string fileName)
        {
            return _storageService.GetMediaUrl(fileName);
        }

        public string GetThumbnailUrl(Media media)
        {
            return GetMediaUrl(media);
        }

        public Task SaveMediaAsync(Stream mediaBinaryStream, string category, string fileName, string mimeType = null)
        {
            return _storageService.SaveMediaAsync(mediaBinaryStream, category, fileName, mimeType);
        }

        public Task DeleteMediaAsync(Media media)
        {
            _mediaRepository.Remove(media);
            return DeleteMediaAsync(media.FileName);
        }

        public Task DeleteMediaAsync(string fileName)
        {
            return _storageService.DeleteMediaAsync(fileName);
        }

        public int FileToImage(string filePath,string categoryId)
        {
            return _transferFileService.FileToImage(filePath,categoryId);
        }

        public string GetMediaUrlByUpload(Media media,string category)
        {
            if (media == null)
            {
                return GetMediaUrl("no-image.png");
            }
            return _storageService.GetMediaUrlUpload(media.FileName, category);
        }

        public string GetMediaUrlByUpload(string fileName, string category)
        {
            return _storageService.GetMediaUrlUpload(fileName, category);
        }

        public string GetMediaUrlByImg(Media media, string category)
        {
            if (media == null)
            {
                return GetMediaUrl("no-image.png");
            }
            return _storageService.GetMediaUrlImg(media.FileName, category);
        }

        public string GetMediaUrlByImg(string fileName, string category)
        {
            return _storageService.GetMediaUrlImg(fileName, category);
        }
    }
}
