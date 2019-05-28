using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Infrastructure.Helpers;
using SimplCommerce.Infrastructure.Web.SmartTable;
using SimplCommerce.Module.Catalog.Areas.Catalog.ViewModels;
using SimplCommerce.Module.Catalog.Models;
using SimplCommerce.Module.Catalog.Models.ProductDetail;
using SimplCommerce.Module.Catalog.Services;
using SimplCommerce.Module.Core.Extensions;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Core.Services;

namespace SimplCommerce.Module.Catalog.Areas.Catalog.Controllers
{
    [Area("Catalog")]
    [Authorize(Roles = "admin, vendor")]
    [Route("api/products")]
    public class ProductApiController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductLink> _productLinkRepository;
        private readonly IRepository<ProductOptionValue> _productOptionValueRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IProductService _productService;
        private readonly IRepository<ProductMedia> _productMediaRepository;
        private readonly IWorkContext _workContext;

        public ProductApiController(
            IRepository<Product> productRepository,
            IMediaService mediaService,
            IProductService productService,
            IRepository<ProductLink> productLinkRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductOptionValue> productOptionValueRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IRepository<ProductMedia> productMediaRepository,
            IWorkContext workContext)
        {
            _productRepository = productRepository;
            _mediaService = mediaService;
            _productService = productService;
            _productLinkRepository = productLinkRepository;
            _productCategoryRepository = productCategoryRepository;
            _productOptionValueRepository = productOptionValueRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
            _productMediaRepository = productMediaRepository;
            _workContext = workContext;
        }

        [HttpGet("quick-search")]
        public async Task<IActionResult> QuickSearch(string name)
        {
            var query = _productRepository.Query()
                .Where(x => !x.IsDeleted && !x.HasOptions && x.IsAllowToOrder);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            var products = await query.Take(5).Select(x => new
            {
                x.Id,
                x.Name
            }).ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var product = _productRepository.Query()
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .Include(x => x.ProductLinks).ThenInclude(p => p.LinkedProduct)
                .Include(x => x.OptionValues).ThenInclude(o => o.Option)
                .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute).ThenInclude(g => g.Group)
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }

            var productVm = new ProductVm
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                MetaTitle = product.MetaTitle,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                Sku = product.Sku,
                Gtin = product.Gtin,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Specification = product.Specification,
                OldPrice = product.OldPrice,
                Price = product.Price,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStart = product.SpecialPriceStart,
                SpecialPriceEnd = product.SpecialPriceEnd,
                IsFeatured = product.IsFeatured,
                IsPublished = product.IsPublished,
                IsCallForPricing =  product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                CategoryIds = product.Categories.Select(x => x.CategoryId).ToList(),
                ThumbnailImageUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage),
                BrandId = product.BrandId,
                TaxClassId = product.TaxClassId,
                StockTrackingIsEnabled = product.StockTrackingIsEnabled
            };

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.Image))
            {
                productVm.ProductImages.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    MediaUrl = _mediaService.GetThumbnailUrl(productMedia.Media)
                });
            }

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.File))
            {
                productVm.ProductDocuments.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    Caption = productMedia.Media.Caption,
                    MediaUrl = _mediaService.GetMediaUrl(productMedia.Media)
                });
            }


            productVm.Options = product.OptionValues.OrderBy(x => x.SortIndex).Select(x =>
                new ProductOptionVm
                {
                    Id = x.OptionId,
                    Name = x.Option.Name,
                    DisplayType = x.DisplayType,
                    Values = JsonConvert.DeserializeObject<IList<ProductOptionValueVm>>(x.Value)
                }).ToList();

            foreach (var variation in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.Variations.Add(new ProductVariationVm
                {
                    Id = variation.Id,
                    Name = variation.Name,
                    Sku = variation.Sku,
                    Gtin = variation.Gtin,
                    Price = variation.Price,
                    OldPrice = variation.OldPrice,
                    NormalizedName = variation.NormalizedName,
                    OptionCombinations = variation.OptionCombinations.Select(x => new ProductOptionCombinationVm
                    {
                        OptionId = x.OptionId,
                        OptionName = x.Option.Name,
                        Value = x.Value,
                        SortIndex = x.SortIndex
                    }).OrderBy(x => x.SortIndex).ToList()
                });
            }

            foreach (var relatedProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.RelatedProducts.Add(new ProductLinkVm
                {
                    Id = relatedProduct.Id,
                    Name = relatedProduct.Name,
                    IsPublished = relatedProduct.IsPublished
                });
            }

            foreach (var crossSellProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.CrossSellProducts.Add(new ProductLinkVm
                {
                    Id = crossSellProduct.Id,
                    Name = crossSellProduct.Name,
                    IsPublished = crossSellProduct.IsPublished
                });
            }

            productVm.Attributes = product.AttributeValues.Select(x => new ProductAttributeVm
            {
                AttributeValueId = x.Id,
                Id = x.AttributeId,
                Name = x.Attribute.Name,
                GroupName = x.Attribute.Group.Name,
                Value = x.Value
            }).ToList();
            //string back = "{\"content\":[{\"f_id\":\"79\",\"f_catid\":\"1\",\"f_onelevel\":\"ERP系统\",\"f_cattid\":\"7\",\"f_towlevel\":\"需求文档\",\"f_title\":\"测试生成缩略图\",\"f_code\":\"文库用户\",\"f_gold\":\"0\",\"f_view\":\"49\",\"f_page\":\"99\",\"f_date\":\"2019/05/07 22:33:06\",\"f_img\":\"images/pdf.jpg\",\"f_desc\":\"测试生成缩略图\",\"doc\":[{\"pic\":\"/user-content/bffb6f2c-8a3f-4fdd-817d-09a9f18cd190.jpg\"},{\"pic\":\"/user-content/bffb6f2c-8a3f-4fdd-817d-09a9f18cd190.jpg\"},{\"pic\":\"/user-content/25d3da45-b57b-40b6-8f41-2fc5170cb6b7.jpg\"},{\"pic\":\"/user-content/7da07700-9a17-498b-ba58-526559343878.jpg\"},{\"pic\":\"/user-content/636928651863010247/6369286518630102475.Png\"},{\"pic\":\"/user-content/7da07700-9a17-498b-ba58-526559343878.jpg\"},{\"pic\":\"/user-content/636928651863010247/6369286518630102477.Png\"},{\"pic\":\"/user-content/636928651863010247/6369286518630102478.Png\"},{\"pic\":\"/user-content/636928651863010247/6369286518630102479.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024710.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024711.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024712.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024713.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024714.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024715.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024716.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024717.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024718.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024719.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024720.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024721.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024722.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024723.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024724.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024725.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024726.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024727.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024728.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024729.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024730.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024731.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024732.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024733.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024734.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024735.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024736.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024737.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024738.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024739.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024740.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024741.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024742.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024743.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024744.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024745.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024746.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024747.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024748.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024749.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024750.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024751.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024752.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024753.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024754.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024755.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024756.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024757.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024758.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024759.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024760.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024761.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024762.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024763.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024764.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024765.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024766.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024767.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024768.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024769.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024770.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024771.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024772.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024773.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024774.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024775.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024776.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024777.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024778.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024779.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024780.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024781.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024782.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024783.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024784.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024785.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024786.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024787.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024788.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024789.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024790.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024791.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024792.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024793.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024794.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024795.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024796.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024797.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024798.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024799.Png\"}],\"indexComment\":[{\"f_docid\":\"79\",\"f_user\":\"wenku\",\"f_avatar\":\"images/avatar/636889357094325704.jpg\",\"f_content\":\"test\",\"f_datetime\":\"2019-05-09 11:35:37\"}]}],\"Hot\":[{\"f_id\":\"1\",\"f_title\":\"文库系统操作手册\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"2\",\"f_title\":\"权限测试文档\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"3\",\"f_title\":\"文库系统测试问题汇总\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"4\",\"f_title\":\"PMI网上申请考试指导\",\"f_img\":\"images/pdf.jpg\"},{\"f_id\":\"5\",\"f_title\":\"返工返修统计记录表\",\"f_img\":\"images/excel.jpg\"},{\"f_id\":\"6\",\"f_title\":\"顾客满意度调查统计表\",\"f_img\":\"images/excel.jpg\"},{\"f_id\":\"7\",\"f_title\":\"Excel2010操作与技巧\",\"f_img\":\"images/pdf.jpg\"},{\"f_id\":\"8\",\"f_title\":\"路由器毕业论文范文\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"9\",\"f_title\":\"二分查找法的实现和应用汇总\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"10\",\"f_title\":\"【上海交通大学(上海交大)计算机组成与系统结构】【习题试卷】10\",\"f_img\":\"images/word.jpg\"}],\"New\":[{\"f_id\":\"1\",\"f_title\":\"文库系统操作手册\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"2\",\"f_title\":\"权限测试文档\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"3\",\"f_title\":\"文库系统测试问题汇总\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"4\",\"f_title\":\"PMI网上申请考试指导\",\"f_img\":\"images/pdf.jpg\"},{\"f_id\":\"5\",\"f_title\":\"返工返修统计记录表\",\"f_img\":\"images/excel.jpg\"}]}";
            //ContentResult br = new ContentResult();
            //br.Content = back;
            //br.ContentType = "application/json";
            //return br;
            return Json(productVm);
        }

        [HttpPost("trnasfer/{id}")]
        public async Task<IActionResult> TrnasferImage(long id)
        {
            var product = _productRepository.Query()
               .Include(x => x.ThumbnailImage)
               .Include(x => x.Medias).ThenInclude(m => m.Media)
               .Include(x => x.ProductLinks).ThenInclude(p => p.LinkedProduct)
               .Include(x => x.OptionValues).ThenInclude(o => o.Option)
               .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute).ThenInclude(g => g.Group)
               .Include(x => x.Categories)
               .FirstOrDefault(x => x.Id == id);

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }

            var productVm = new ProductVm
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                MetaTitle = product.MetaTitle,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                Sku = product.Sku,
                Gtin = product.Gtin,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Specification = product.Specification,
                OldPrice = product.OldPrice,
                Price = product.Price,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStart = product.SpecialPriceStart,
                SpecialPriceEnd = product.SpecialPriceEnd,
                IsFeatured = product.IsFeatured,
                IsPublished = product.IsPublished,
                IsCallForPricing = product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                CategoryIds = product.Categories.Select(x => x.CategoryId).ToList(),
                ThumbnailImageUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage),
                BrandId = product.BrandId,
                TaxClassId = product.TaxClassId,
                StockTrackingIsEnabled = product.StockTrackingIsEnabled
            };

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.Image))
            {
                productVm.ProductImages.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    MediaUrl = _mediaService.GetThumbnailUrl(productMedia.Media)
                });
            }

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.File))
            {
                productVm.ProductDocuments.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    Caption = productMedia.Media.Caption,
                    MediaUrl = _mediaService.GetMediaUrl(productMedia.Media)
                });
            }


            productVm.Options = product.OptionValues.OrderBy(x => x.SortIndex).Select(x =>
                new ProductOptionVm
                {
                    Id = x.OptionId,
                    Name = x.Option.Name,
                    DisplayType = x.DisplayType,
                    Values = JsonConvert.DeserializeObject<IList<ProductOptionValueVm>>(x.Value)
                }).ToList();

            foreach (var variation in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.Variations.Add(new ProductVariationVm
                {
                    Id = variation.Id,
                    Name = variation.Name,
                    Sku = variation.Sku,
                    Gtin = variation.Gtin,
                    Price = variation.Price,
                    OldPrice = variation.OldPrice,
                    NormalizedName = variation.NormalizedName,
                    OptionCombinations = variation.OptionCombinations.Select(x => new ProductOptionCombinationVm
                    {
                        OptionId = x.OptionId,
                        OptionName = x.Option.Name,
                        Value = x.Value,
                        SortIndex = x.SortIndex
                    }).OrderBy(x => x.SortIndex).ToList()
                });
            }

            foreach (var relatedProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.RelatedProducts.Add(new ProductLinkVm
                {
                    Id = relatedProduct.Id,
                    Name = relatedProduct.Name,
                    IsPublished = relatedProduct.IsPublished
                });
            }

            foreach (var crossSellProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.CrossSellProducts.Add(new ProductLinkVm
                {
                    Id = crossSellProduct.Id,
                    Name = crossSellProduct.Name,
                    IsPublished = crossSellProduct.IsPublished
                });
            }

            productVm.Attributes = product.AttributeValues.Select(x => new ProductAttributeVm
            {
                AttributeValueId = x.Id,
                Id = x.AttributeId,
                Name = x.Attribute.Name,
                GroupName = x.Attribute.Group.Name,
                Value = x.Value
            }).ToList();
            string path = Environment.CurrentDirectory+ "\\wwwroot"+ productVm.ProductDocuments[0].MediaUrl;
            string pdfpath = Environment.CurrentDirectory + "/wwwroot/user-content/pdf/1.pdf";
            string back = "成功";
            try
            {
                OfficeHelper.Word2PDF(path, pdfpath);
                if (!Directory.Exists(Environment.CurrentDirectory + "/wwwroot/user-content/pdf/1/"))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "/wwwroot/user-content/pdf/1/");
                }
                SwfHelper.Pdf2Img(pdfpath, Environment.CurrentDirectory + "/wwwroot/user-content/pdf/1/");
            }
            catch(Exception ex)
            {
                back = "失败"+ex.Message;
            }
            ContentResult br = new ContentResult();
            br.Content = "{\"data\":\""+back+"\"}";
            br.ContentType = "application/json";
            return br;
        }

        [HttpGet("show/{id}")]
        public async Task<IActionResult> Show(long id)
        {
            var product = _productRepository.Query()
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .Include(x => x.ProductLinks).ThenInclude(p => p.LinkedProduct)
                .Include(x => x.OptionValues).ThenInclude(o => o.Option)
                .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute).ThenInclude(g => g.Group)
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }

            var productVm = new ProductVm
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                MetaTitle = product.MetaTitle,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                Sku = product.Sku,
                Gtin = product.Gtin,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Specification = product.Specification,
                OldPrice = product.OldPrice,
                Price = product.Price,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStart = product.SpecialPriceStart,
                SpecialPriceEnd = product.SpecialPriceEnd,
                IsFeatured = product.IsFeatured,
                IsPublished = product.IsPublished,
                IsCallForPricing = product.IsCallForPricing,
                IsAllowToOrder = product.IsAllowToOrder,
                CategoryIds = product.Categories.Select(x => x.CategoryId).ToList(),
                ThumbnailImageUrl = _mediaService.GetThumbnailUrl(product.ThumbnailImage),
                BrandId = product.BrandId,
                TaxClassId = product.TaxClassId,
                StockTrackingIsEnabled = product.StockTrackingIsEnabled
            };

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.Image))
            {
                productVm.ProductImages.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    MediaUrl = _mediaService.GetThumbnailUrl(productMedia.Media)
                });
            }

            foreach (var productMedia in product.Medias.Where(x => x.Media.MediaType == MediaType.File))
            {
                productVm.ProductDocuments.Add(new ProductMediaVm
                {
                    Id = productMedia.Id,
                    Caption = productMedia.Media.Caption,
                    MediaUrl = _mediaService.GetMediaUrl(productMedia.Media)
                });
            }


            productVm.Options = product.OptionValues.OrderBy(x => x.SortIndex).Select(x =>
                new ProductOptionVm
                {
                    Id = x.OptionId,
                    Name = x.Option.Name,
                    DisplayType = x.DisplayType,
                    Values = JsonConvert.DeserializeObject<IList<ProductOptionValueVm>>(x.Value)
                }).ToList();

            foreach (var variation in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.Variations.Add(new ProductVariationVm
                {
                    Id = variation.Id,
                    Name = variation.Name,
                    Sku = variation.Sku,
                    Gtin = variation.Gtin,
                    Price = variation.Price,
                    OldPrice = variation.OldPrice,
                    NormalizedName = variation.NormalizedName,
                    OptionCombinations = variation.OptionCombinations.Select(x => new ProductOptionCombinationVm
                    {
                        OptionId = x.OptionId,
                        OptionName = x.Option.Name,
                        Value = x.Value,
                        SortIndex = x.SortIndex
                    }).OrderBy(x => x.SortIndex).ToList()
                });
            }

            foreach (var relatedProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.RelatedProducts.Add(new ProductLinkVm
                {
                    Id = relatedProduct.Id,
                    Name = relatedProduct.Name,
                    IsPublished = relatedProduct.IsPublished
                });
            }

            foreach (var crossSellProduct in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell).Select(x => x.LinkedProduct).Where(x => !x.IsDeleted).OrderBy(x => x.Id))
            {
                productVm.CrossSellProducts.Add(new ProductLinkVm
                {
                    Id = crossSellProduct.Id,
                    Name = crossSellProduct.Name,
                    IsPublished = crossSellProduct.IsPublished
                });
            }

            productVm.Attributes = product.AttributeValues.Select(x => new ProductAttributeVm
            {
                AttributeValueId = x.Id,
                Id = x.AttributeId,
                Name = x.Attribute.Name,
                GroupName = x.Attribute.Group.Name,
                Value = x.Value
            }).ToList();
            //string back = "{\"content\":[{\"f_id\":\"79\",\"f_catid\":\"1\",\"f_onelevel\":\"ERP系统\",\"f_cattid\":\"7\",\"f_towlevel\":\"需求文档\",\"f_title\":\"测试生成缩略图\",\"f_code\":\"文库用户\",\"f_gold\":\"0\",\"f_view\":\"49\",\"f_page\":\"99\",\"f_date\":\"2019/05/07 22:33:06\",\"f_img\":\"images/pdf.jpg\",\"f_desc\":\"测试生成缩略图\",\"doc\":[{\"pic\":\"/user-content/bffb6f2c-8a3f-4fdd-817d-09a9f18cd190.jpg\"},{\"pic\":\"/user-content/bffb6f2c-8a3f-4fdd-817d-09a9f18cd190.jpg\"},{\"pic\":\"/user-content/25d3da45-b57b-40b6-8f41-2fc5170cb6b7.jpg\"},{\"pic\":\"/user-content/7da07700-9a17-498b-ba58-526559343878.jpg\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.jpg\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"},{\"pic\":\"/user-content/636928651863010247/63692865186301024798.Png\"},{\"pic\":\"/user-content/bc68637c-420f-4951-9130-b9fca36484e6.Png\"}],\"indexComment\":[{\"f_docid\":\"79\",\"f_user\":\"wenku\",\"f_avatar\":\"images/avatar/636889357094325704.jpg\",\"f_content\":\"test\",\"f_datetime\":\"2019-05-09 11:35:37\"}]}],\"Hot\":[{\"f_id\":\"1\",\"f_title\":\"文库系统操作手册\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"2\",\"f_title\":\"权限测试文档\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"3\",\"f_title\":\"文库系统测试问题汇总\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"4\",\"f_title\":\"PMI网上申请考试指导\",\"f_img\":\"images/pdf.jpg\"},{\"f_id\":\"5\",\"f_title\":\"返工返修统计记录表\",\"f_img\":\"images/excel.jpg\"},{\"f_id\":\"6\",\"f_title\":\"顾客满意度调查统计表\",\"f_img\":\"images/excel.jpg\"},{\"f_id\":\"7\",\"f_title\":\"Excel2010操作与技巧\",\"f_img\":\"images/pdf.jpg\"},{\"f_id\":\"8\",\"f_title\":\"路由器毕业论文范文\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"9\",\"f_title\":\"二分查找法的实现和应用汇总\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"10\",\"f_title\":\"【上海交通大学(上海交大)计算机组成与系统结构】【习题试卷】10\",\"f_img\":\"images/word.jpg\"}],\"New\":[{\"f_id\":\"1\",\"f_title\":\"文库系统操作手册\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"2\",\"f_title\":\"权限测试文档\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"3\",\"f_title\":\"文库系统测试问题汇总\",\"f_img\":\"images/word.jpg\"},{\"f_id\":\"4\",\"f_title\":\"PMI网上申请考试指导\",\"f_img\":\"images/pdf.jpg\"},{\"f_id\":\"5\",\"f_title\":\"返工返修统计记录表\",\"f_img\":\"images/excel.jpg\"}]}";
            //ContentResult br = new ContentResult();
            //br.Content = back;
            //br.ContentType = "application/json";
            //return br;
            ProductShow productShow = new ProductShow();
            productShow.content = new List<Models.ProductDetail.Content>();
            productShow.content.Add(new Models.ProductDetail.Content());
            productShow.content[0].f_id="79";
            productShow.content[0].f_catid="1";
            productShow.content[0].f_onelevel="ERP系统";
            productShow.content[0].f_cattid="7";
            productShow.content[0].f_towlevel="需求文档";
            productShow.content[0].f_title="测试生成缩略图";
            productShow.content[0].f_code="文库用户";
            productShow.content[0].f_gold="0";
            productShow.content[0].f_view="5";
            productShow.content[0].f_page="5";
            productShow.content[0].f_date="2019/05/07 22:33:06";
            productShow.content[0].f_img="images/pdf.jpg";
            productShow.content[0].f_desc="测试生成缩略图";
            productShow.content[0].doc = new List<docItem>();
            productShow.content[0].indexComment = new IndexComment();
            for (int i = 1; i < 6; i++)
            {
                docItem item = new docItem();
                item.pic = "/user-content/pdf/1/pdf_" + i + ".png";
;               productShow.content[0].doc.Add(item);
            }
            productShow.Hot = new List<HotItem>();
            productShow.New = new List<HotItem>();
            return Json(productShow);
        }

        [HttpPost("grid")]
        public async Task<IActionResult> List([FromBody] SmartTableParam param)
        {
            var query = _productRepository.Query().Where(x => !x.IsDeleted);
            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin"))
            {
                query = query.Where(x => x.VendorId == currentUser.VendorId);
            }

            if (param.Search.PredicateObject != null)
            {
                dynamic search = param.Search.PredicateObject;
                if (search.Name != null)
                {
                    string name = search.Name;
                    query = query.Where(x => x.Name.Contains(name));
                }

                if (search.HasOptions != null)
                {
                    bool hasOptions = search.HasOptions;
                    query = query.Where(x => x.HasOptions == hasOptions);
                }

                if (search.IsVisibleIndividually != null)
                {
                    bool isVisibleIndividually = search.IsVisibleIndividually;
                    query = query.Where(x => x.IsVisibleIndividually == isVisibleIndividually);
                }

                if (search.IsPublished != null)
                {
                    bool isPublished = search.IsPublished;
                    query = query.Where(x => x.IsPublished == isPublished);
                }

                if (search.CreatedOn != null)
                {
                    if (search.CreatedOn.before != null)
                    {
                        DateTimeOffset before = search.CreatedOn.before;
                        query = query.Where(x => x.CreatedOn <= before);
                    }

                    if (search.CreatedOn.after != null)
                    {
                        DateTimeOffset after = search.CreatedOn.after;
                        query = query.Where(x => x.CreatedOn >= after);
                    }
                }
            }

            var gridData = query.ToSmartTableResult(
                param,
                x => new ProductListItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    HasOptions = x.HasOptions,
                    IsVisibleIndividually = x.IsVisibleIndividually,
                    IsFeatured = x.IsFeatured,
                    IsAllowToOrder = x.IsAllowToOrder,
                    IsCallForPricing = x.IsCallForPricing,
                    StockQuantity = x.StockQuantity,
                    CreatedOn = x.CreatedOn,
                    IsPublished = x.IsPublished
                });

            return Json(gridData);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ProductForm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _workContext.GetCurrentUser();

            var product = new Product
            {
                Name = model.Product.Name,
                Slug = model.Product.Slug,
                MetaTitle = model.Product.MetaTitle,
                MetaKeywords = model.Product.MetaKeywords,
                MetaDescription = model.Product.MetaDescription,
                Sku = model.Product.Sku,
                Gtin = model.Product.Gtin,
                ShortDescription = model.Product.ShortDescription,
                Description = model.Product.Description,
                Specification = model.Product.Specification,
                Price = model.Product.Price,
                OldPrice = model.Product.OldPrice,
                SpecialPrice = model.Product.SpecialPrice,
                SpecialPriceStart = model.Product.SpecialPriceStart,
                SpecialPriceEnd = model.Product.SpecialPriceEnd,
                IsPublished = model.Product.IsPublished,
                IsFeatured = model.Product.IsFeatured,
                IsCallForPricing = model.Product.IsCallForPricing,
                IsAllowToOrder = model.Product.IsAllowToOrder,
                BrandId = model.Product.BrandId,
                TaxClassId = model.Product.TaxClassId,
                StockTrackingIsEnabled = model.Product.StockTrackingIsEnabled,
                HasOptions = model.Product.Variations.Any() ? true : false,
                IsVisibleIndividually = true,
                CreatedBy = currentUser,
                LatestUpdatedBy = currentUser
            };

            if (!User.IsInRole("admin"))
            {
                product.VendorId = currentUser.VendorId;
            }

            var optionIndex = 0;
            foreach (var option in model.Product.Options)
            {
                product.AddOptionValue(new ProductOptionValue
                {
                    OptionId = option.Id,
                    DisplayType = option.DisplayType,
                    Value = JsonConvert.SerializeObject(option.Values),
                    SortIndex = optionIndex
                });

                optionIndex++;
            }

            foreach (var attribute in model.Product.Attributes)
            {
                var attributeValue = new ProductAttributeValue
                {
                    AttributeId = attribute.Id,
                    Value = attribute.Value
                };

                product.AddAttributeValue(attributeValue);
            }

            foreach (var categoryId in model.Product.CategoryIds)
            {
                var productCategory = new ProductCategory
                {
                    CategoryId = categoryId
                };
                product.AddCategory(productCategory);
            }

            await SaveProductMedias(model, product);

            MapProductVariationVmToProduct(currentUser, model, product);
            MapProductLinkVmToProduct(model, product);

            var productPriceHistory = CreatePriceHistory(currentUser, product);
            product.PriceHistories.Add(productPriceHistory);

            _productService.Create(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, ProductForm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = _productRepository.Query()
                .Include(x => x.ThumbnailImage)
                .Include(x => x.Medias).ThenInclude(m => m.Media)
                .Include(x => x.ProductLinks).ThenInclude(x => x.LinkedProduct)
                .Include(x => x.OptionValues).ThenInclude(o => o.Option)
                .Include(x => x.AttributeValues).ThenInclude(a => a.Attribute).ThenInclude(g => g.Group)
                .Include(x => x.Categories)
                .FirstOrDefault(x => x.Id == id);

            if(product == null)
            {
                return NotFound();
            }

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }

            var isPriceChanged = false;
            if (product.Price != model.Product.Price ||
                product.OldPrice != model.Product.OldPrice ||
                product.SpecialPrice != model.Product.SpecialPrice ||
                product.SpecialPriceStart != model.Product.SpecialPriceStart ||
                product.SpecialPriceEnd != model.Product.SpecialPriceEnd)
            {
                isPriceChanged = true;
            }


            product.Name = model.Product.Name;
            product.Slug = model.Product.Slug;
            product.MetaTitle = model.Product.MetaTitle;
            product.MetaKeywords = model.Product.MetaKeywords;
            product.MetaDescription = model.Product.MetaDescription;
            product.Sku = model.Product.Sku;
            product.Gtin = model.Product.Gtin;
            product.ShortDescription = model.Product.ShortDescription;
            product.Description = model.Product.Description;
            product.Specification = model.Product.Specification;
            product.Price = model.Product.Price;
            product.OldPrice = model.Product.OldPrice;
            product.SpecialPrice = model.Product.SpecialPrice;
            product.SpecialPriceStart = model.Product.SpecialPriceStart;
            product.SpecialPriceEnd = model.Product.SpecialPriceEnd;
            product.BrandId = model.Product.BrandId;
            product.TaxClassId = model.Product.TaxClassId;
            product.IsFeatured = model.Product.IsFeatured;
            product.IsPublished = model.Product.IsPublished;
            product.IsCallForPricing = model.Product.IsCallForPricing;
            product.IsAllowToOrder = model.Product.IsAllowToOrder;
            product.StockTrackingIsEnabled = model.Product.StockTrackingIsEnabled;
            product.LatestUpdatedBy = currentUser;

            if (isPriceChanged)
            {
                var productPriceHistory = CreatePriceHistory(currentUser, product);
                product.PriceHistories.Add(productPriceHistory);
            }

            await SaveProductMedias(model, product);

            foreach (var productMediaId in model.Product.DeletedMediaIds)
            {
                var productMedia = product.Medias.First(x => x.Id == productMediaId);
                _productMediaRepository.Remove(productMedia);
                await _mediaService.DeleteMediaAsync(productMedia.Media);
            }

            AddOrDeleteProductOption(model, product);
            AddOrDeleteProductAttribute(model, product);
            AddOrDeleteCategories(model, product);
            AddOrDeleteProductVariation(currentUser, model, product);
            AddOrDeleteProductLinks(model, product);

            _productService.Update(product);

            return Accepted();
        }

        [HttpPost("change-status/{id}")]
        public async Task<IActionResult> ChangeStatus(long id)
        {
            var product = _productRepository.Query().FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }

            product.IsPublished = !product.IsPublished;
            await _productRepository.SaveChangesAsync();

            return Accepted();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var product = _productRepository.Query().FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var currentUser = await _workContext.GetCurrentUser();
            if (!User.IsInRole("admin") && product.VendorId != currentUser.VendorId)
            {
                return BadRequest(new { error = "You don't have permission to manage this product" });
            }

            await _productService.Delete(product);

            return NoContent();
        }

        private static void MapProductVariationVmToProduct(User loginUser, ProductForm model, Product product)
        {
            foreach (var variationVm in model.Product.Variations)
            {
                var productLink = new ProductLink
                {
                    LinkType = ProductLinkType.Super,
                    Product = product,
                    LinkedProduct = product.Clone()
                };

                productLink.LinkedProduct.CreatedById = loginUser.Id;
                productLink.LinkedProduct.LatestUpdatedById = loginUser.Id;
                productLink.LinkedProduct.Name = variationVm.Name;
                productLink.LinkedProduct.Slug = variationVm.Name.ToUrlFriendly();
                productLink.LinkedProduct.Sku = variationVm.Sku;
                productLink.LinkedProduct.Gtin = variationVm.Gtin;
                productLink.LinkedProduct.Price = variationVm.Price;
                productLink.LinkedProduct.OldPrice = variationVm.OldPrice;
                productLink.LinkedProduct.NormalizedName = variationVm.NormalizedName;
                productLink.LinkedProduct.HasOptions = false;
                productLink.LinkedProduct.IsVisibleIndividually = false;

                foreach (var combinationVm in variationVm.OptionCombinations)
                {
                    productLink.LinkedProduct.AddOptionCombination(new ProductOptionCombination
                    {
                        OptionId = combinationVm.OptionId,
                        Value = combinationVm.Value,
                        SortIndex = combinationVm.SortIndex
                    });
                }

                productLink.LinkedProduct.ThumbnailImage = product.ThumbnailImage;

                var productPriceHistory = CreatePriceHistory(loginUser, productLink.LinkedProduct);
                product.PriceHistories.Add(productPriceHistory);

                product.AddProductLinks(productLink);
            }
        }

        private static ProductPriceHistory CreatePriceHistory(User loginUser, Product product)
        {
            return new ProductPriceHistory
            {
                CreatedBy = loginUser,
                Product = product,
                Price = product.Price,
                OldPrice = product.OldPrice,
                SpecialPrice = product.SpecialPrice,
                SpecialPriceStart = product.SpecialPriceStart,
                SpecialPriceEnd = product.SpecialPriceEnd
            };
        }

        private static void MapProductLinkVmToProduct(ProductForm model, Product product)
        {
            foreach (var relatedProductVm in model.Product.RelatedProducts)
            {
                var productLink = new ProductLink
                {
                    LinkType = ProductLinkType.Related,
                    Product = product,
                    LinkedProductId = relatedProductVm.Id
                };

                product.AddProductLinks(productLink);
            }

            foreach (var crossSellProductVm in model.Product.CrossSellProducts)
            {
                var productLink = new ProductLink
                {
                    LinkType = ProductLinkType.CrossSell,
                    Product = product,
                    LinkedProductId = crossSellProductVm.Id
                };

                product.AddProductLinks(productLink);
            }
        }

        private void AddOrDeleteCategories(ProductForm model, Product product)
        {
            foreach (var categoryId in model.Product.CategoryIds)
            {
                if (product.Categories.Any(x => x.CategoryId == categoryId))
                {
                    continue;
                }

                var productCategory = new ProductCategory
                {
                    CategoryId = categoryId
                };
                product.AddCategory(productCategory);
            }

            var deletedProductCategories =
                product.Categories.Where(productCategory => !model.Product.CategoryIds.Contains(productCategory.CategoryId))
                    .ToList();

            foreach (var deletedProductCategory in deletedProductCategories)
            {
                deletedProductCategory.Product = null;
                product.Categories.Remove(deletedProductCategory);
                _productCategoryRepository.Remove(deletedProductCategory);
            }
        }

        private void AddOrDeleteProductOption(ProductForm model, Product product)
        {
            var optionIndex = 0;
            foreach (var optionVm in model.Product.Options)
            {
                var optionValue = product.OptionValues.FirstOrDefault(x => x.OptionId == optionVm.Id);
                if (optionValue == null)
                {
                    product.AddOptionValue(new ProductOptionValue
                    {
                        OptionId = optionVm.Id,
                        DisplayType = optionVm.DisplayType,
                        Value = JsonConvert.SerializeObject(optionVm.Values),
                        SortIndex = optionIndex
                    });
                }
                else
                {
                    optionValue.Value = JsonConvert.SerializeObject(optionVm.Values);
                    optionValue.DisplayType = optionVm.DisplayType;
                    optionValue.SortIndex = optionIndex;
                }

                optionIndex++;
            }

            var deletedProductOptionValues = product.OptionValues.Where(x => model.Product.Options.All(vm => vm.Id != x.OptionId)).ToList();

            foreach (var productOptionValue in deletedProductOptionValues)
            {
                product.OptionValues.Remove(productOptionValue);
                _productOptionValueRepository.Remove(productOptionValue);
            }
        }

        private void AddOrDeleteProductVariation(User loginUser,  ProductForm model, Product product)
        {
            foreach (var productVariationVm in model.Product.Variations)
            {
                var productLink = product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super).FirstOrDefault(x => x.LinkedProduct.Name == productVariationVm.Name);
                if (productLink == null)
                {
                    productLink = new ProductLink
                    {
                        LinkType = ProductLinkType.Super,
                        Product = product,
                        LinkedProduct = product.Clone()
                    };

                    productLink.LinkedProduct.CreatedById = loginUser.Id;
                    productLink.LinkedProduct.LatestUpdatedById = loginUser.Id;
                    productLink.LinkedProduct.Name = productVariationVm.Name;
                    productLink.LinkedProduct.Slug = StringHelper.ToUrlFriendly(productVariationVm.Name);
                    productLink.LinkedProduct.Sku = productVariationVm.Sku;
                    productLink.LinkedProduct.Gtin = productVariationVm.Gtin;
                    productLink.LinkedProduct.Price = productVariationVm.Price;
                    productLink.LinkedProduct.OldPrice = productVariationVm.OldPrice;
                    productLink.LinkedProduct.NormalizedName = productVariationVm.NormalizedName;
                    productLink.LinkedProduct.HasOptions = false;
                    productLink.LinkedProduct.IsVisibleIndividually = false;
                    productLink.LinkedProduct.ThumbnailImage = product.ThumbnailImage;

                    foreach (var combinationVm in productVariationVm.OptionCombinations)
                    {
                        productLink.LinkedProduct.AddOptionCombination(new ProductOptionCombination
                        {
                            OptionId = combinationVm.OptionId,
                            Value = combinationVm.Value,
                            SortIndex = combinationVm.SortIndex
                        });
                    }

                    var productPriceHistory = CreatePriceHistory(loginUser, productLink.LinkedProduct);
                    productLink.LinkedProduct.PriceHistories.Add(productPriceHistory);

                    product.AddProductLinks(productLink);
                }
                else
                {
                    var isPriceChanged = false;
                    if(productLink.LinkedProduct.Price != productVariationVm.Price ||
                        productLink.LinkedProduct.OldPrice != productVariationVm.OldPrice)
                    {
                        isPriceChanged = true;
                    }

                    productLink.LinkedProduct.LatestUpdatedById = loginUser.Id;
                    productLink.LinkedProduct.LatestUpdatedOn = DateTimeOffset.Now;
                    productLink.LinkedProduct.Sku = productVariationVm.Sku;
                    productLink.LinkedProduct.Gtin = productVariationVm.Gtin;
                    productLink.LinkedProduct.Price = productVariationVm.Price;
                    productLink.LinkedProduct.OldPrice = productVariationVm.OldPrice;
                    productLink.LinkedProduct.IsDeleted = false;
                    productLink.LinkedProduct.StockTrackingIsEnabled = product.StockTrackingIsEnabled;

                    if (isPriceChanged)
                    {
                        var productPriceHistory = CreatePriceHistory(loginUser, productLink.LinkedProduct);
                        productLink.LinkedProduct.PriceHistories.Add(productPriceHistory);
                    }
                }
            }

            foreach (var productLink in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Super))
            {
                if (model.Product.Variations.All(x => x.Name != productLink.LinkedProduct.Name))
                {
                    _productLinkRepository.Remove(productLink);
                    productLink.LinkedProduct.IsDeleted = true;
                }
            }
        }

        // Due to some issue with EF Core, we have to use _productLinkRepository in this case.
        private void AddOrDeleteProductLinks(ProductForm model, Product product)
        {
            foreach (var relatedProductVm in model.Product.RelatedProducts)
            {
                var productLink = product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related).FirstOrDefault(x => x.LinkedProductId == relatedProductVm.Id);
                if (productLink == null)
                {
                    productLink = new ProductLink
                    {
                        LinkType = ProductLinkType.Related,
                        Product = product,
                        LinkedProductId = relatedProductVm.Id,
                    };

                    _productLinkRepository.Add(productLink);
                }
            }

            foreach (var productLink in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.Related))
            {
                if (model.Product.RelatedProducts.All(x => x.Id != productLink.LinkedProductId))
                {
                    _productLinkRepository.Remove(productLink);
                }
            }

            foreach (var crossSellProductVm in model.Product.CrossSellProducts)
            {
                var productLink = product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell).FirstOrDefault(x => x.LinkedProductId == crossSellProductVm.Id);
                if (productLink == null)
                {
                    productLink = new ProductLink
                    {
                        LinkType = ProductLinkType.CrossSell,
                        Product = product,
                        LinkedProductId = crossSellProductVm.Id,
                    };

                    _productLinkRepository.Add(productLink);
                }
            }

            foreach (var productLink in product.ProductLinks.Where(x => x.LinkType == ProductLinkType.CrossSell))
            {
                if (model.Product.CrossSellProducts.All(x => x.Id != productLink.LinkedProductId))
                {
                    _productLinkRepository.Remove(productLink);
                }
            }
        }

        private void AddOrDeleteProductAttribute(ProductForm model, Product product)
        {
            foreach (var productAttributeVm in model.Product.Attributes)
            {
                var productAttrValue =
                    product.AttributeValues.FirstOrDefault(x => x.AttributeId == productAttributeVm.Id);
                if (productAttrValue == null)
                {
                    productAttrValue = new ProductAttributeValue
                    {
                        AttributeId = productAttributeVm.Id,
                        Value = productAttributeVm.Value
                    };
                    product.AddAttributeValue(productAttrValue);
                }
                else
                {
                    productAttrValue.Value = productAttributeVm.Value;
                }
            }

            var deletedAttrValues =
                product.AttributeValues.Where(attrValue => model.Product.Attributes.All(x => x.Id != attrValue.AttributeId))
                    .ToList();

            foreach (var deletedAttrValue in deletedAttrValues)
            {
                deletedAttrValue.Product = null;
                product.AttributeValues.Remove(deletedAttrValue);
                _productAttributeValueRepository.Remove(deletedAttrValue);
            }
        }

        private async Task SaveProductMedias(ProductForm model, Product product)
        {
            if (model.ThumbnailImage != null)
            {
                var fileName = await SaveFile(model.ThumbnailImage);
                if (product.ThumbnailImage != null)
                {
                    product.ThumbnailImage.FileName = fileName;
                }
                else
                {
                    product.ThumbnailImage = new Media {FileName = fileName};
                }
            }

            // Currently model binder cannot map the collection of file productImages[0], productImages[1]
            foreach (var file in Request.Form.Files)
            {
                if (file.ContentDisposition.Contains("productImages"))
                {
                    model.ProductImages.Add(file);
                }
                else if (file.ContentDisposition.Contains("productDocuments"))
                {
                    model.ProductDocuments.Add(file);
                }
            }

            foreach (var file in model.ProductImages)
            {
                var fileName = await SaveFile(file);
                var productMedia = new ProductMedia
                {
                    Product = product,
                    Media = new Media {FileName = fileName, MediaType = MediaType.Image}
                };
                product.AddMedia(productMedia);
            }

            foreach (var file in model.ProductDocuments)
            {
                var fileName = await SaveFile(file);
                var productMedia = new ProductMedia
                {
                    Product = product,
                    Media = new Media { FileName = fileName, MediaType = MediaType.File, Caption = file.FileName }
                };
                product.AddMedia(productMedia);
            }
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Value.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _mediaService.SaveMediaAsync(file.OpenReadStream(), fileName, file.ContentType);
            return fileName;
        }
    }
}
