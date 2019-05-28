using Aspose.Words;

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

        #endregion
    }
}
