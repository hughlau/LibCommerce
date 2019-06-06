using SimplCommerce.Infrastructure;
using SimplCommerce.Infrastructure.Helpers;
using SimplCommerce.Module.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/****************************************************************
*   Author：L
*   Time：2019/5/29 14:21:19
*   FrameVersion：2.2
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Module.TransferFile
{
    public partial class TransferFileService : ITransferFileService
    {

        #region =============属性============



        #endregion

        #region ===========构造函数==========



        #endregion

        #region ===========基本方法==========



        #endregion

        #region =============方法============

        public int FileToImage(string filePath,string categoryId)
        {
            int back = 0;
            if (!string.IsNullOrEmpty(filePath))
            {
                string imagefilePath = Path.Combine(GlobalConfiguration.WebRootPath, "imgfile",categoryId);
                int lastSplitX = filePath.LastIndexOf("/");
                int lastSplitP = filePath.LastIndexOf(".");
                string mediaName = filePath.Substring(lastSplitX + 1, (lastSplitP - lastSplitX - 1));
                //string pdfpath = imagefilePath+"\\" + mediaName + ".pdf";
                try
                {
                    string mediaPath = Path.Combine(imagefilePath, mediaName)+"\\";
                    if (Path.GetExtension(filePath).ToLower() == ".doc"
                        || Path.GetExtension(filePath).ToLower() == ".docx")
                    {
                        //OfficeHelper.Word2PDF(filePath, pdfpath);
                        //if (!Directory.Exists(mediaPath))
                        //{
                        //    Directory.CreateDirectory(mediaPath);
                        //}
                        //SwfHelper.Pdf2Img(pdfpath, mediaPath);
                        //FileInfo file = new FileInfo(pdfpath);
                        //file.Delete();

                        if (!Directory.Exists(mediaPath))
                        {
                            Directory.CreateDirectory(mediaPath);
                        }
                        OfficeHelper.Word2Jpg(filePath, mediaPath);
                    }
                    else if (Path.GetExtension(filePath).ToLower() == ".pdf")
                    {
                        if (!Directory.Exists(mediaPath))
                        {
                            Directory.CreateDirectory(mediaPath);
                        }
                        SwfHelper.Pdf2Img(filePath, mediaPath);
                    }
                    back = Directory.GetFiles(mediaPath).Length;
                }
                catch(Exception ex)
                {
                   // TODO 记录日志
                }
            }
            return back;
        }

        #endregion

    }
}
