using Aspose.Words;
using Aspose.Words.Saving;
using System.IO;

/****************************************************************
*   Author：L
*   Time：2019/5/28 11:25:59
*   FrameVersion：2.2
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Infrastructure.Helpers
{
    public static class OfficeHelper
    {

        #region =============属性============



        #endregion

        #region ===========构造函数==========



        #endregion

        #region ===========基本方法==========



        #endregion

        #region =============方法============

        public static void Word2PDF(string srcDocPath, string dstPdfPath)
        {
            try
            {
                Document srcDoc = new Document(srcDocPath);
                srcDoc.Save(dstPdfPath, SaveFormat.Pdf);
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }

        public static void Word2Jpg(string strDocPath,string imgPath)
        {
            try
            {
                Document srcDoc = new Document(strDocPath);
                ImageSaveOptions iso = new ImageSaveOptions(SaveFormat.Jpeg);
                iso.Resolution = 128;
                iso.PrettyFormat = true;
                iso.UseAntiAliasing = true;
                for (int i = 0; i < srcDoc.PageCount; i++)
                {
                    iso.PageIndex = i;
                    srcDoc.Save(Path.Combine(imgPath,  i + ".jpg"), iso);
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }
       


        #endregion
    }
}
