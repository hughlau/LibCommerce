using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimplCommerce.Infrastructure.Data;
using SimplCommerce.Infrastructure.Helpers;
using SimplCommerce.Module.Core.Models;
using SimplCommerce.Module.Core.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimplCommerce.Module.Core.Areas.Core.Controllers
{
    [Area("Core")]
    [Route("api/captcha")]
    public class CaptchaApiController : Controller
    {

        private readonly ICacheService _cacheService;

        public CaptchaApiController(
            ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        // GET: /<controller>/
        [HttpGet("get/{id}")]
        public IActionResult Get(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _cacheService.Remove(id);
            }
            string code = CaptchaHelper.GetSingleObj().CreateCode();
            string captoken = "V_"+Guid.NewGuid().ToString();
            TimeSpan timeSpan = new TimeSpan(0, 0, 30);
            _cacheService.Add(captoken,code, timeSpan);
            var bitmap = CaptchaHelper.GetSingleObj().CreateBitmapByImgVerifyCode(code, 100, 40);
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Gif);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            String strbaser64 = Convert.ToBase64String(arr);
            var send = new CacheM
            {
                Token = captoken,
                Img = strbaser64
            };
            return Json(send);
        }

        [HttpGet("yz")]
        public IActionResult Yz(string token,string code,string id)
        {
            object ocode= _cacheService.Get(token);
            var send = new CacheM
            {
                Success = false
            };
            if (ocode!=null && ocode.ToString()==code)
            {
                TimeSpan timeSpan = new TimeSpan(0, 0, 30);
                string dtoken = "D_" + Guid.NewGuid().ToString();
                _cacheService.Add(dtoken, id, timeSpan);
                send.Success = true;
                send.DownToken = dtoken;
            }

                return Json(send);
            
            
        }
    }
}
