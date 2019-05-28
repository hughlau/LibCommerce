using System;
using System.Collections.Generic;
using System.Text;

/****************************************************************
*   Author：L
*   Time：2019/5/28 14:35:31
*   FrameVersion：2.2
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Infrastructure.Helpers
{
    public static class SwfHelper
    {

        #region =============属性============



        #endregion

        #region ===========构造函数==========



        #endregion

        #region ===========基本方法==========



        #endregion

        #region =============方法============

        public static void Pdf2Img(string pdfPath,string imgFloderPath)
        {
            string fileName = Environment.CurrentDirectory + "/wwwroot/lib/tools/mudraw.exe";
            var psi = new System.Diagnostics.ProcessStartInfo(fileName, "-r100 -o " + imgFloderPath + "pdf_%d.png " + pdfPath);
            System.Diagnostics.Process.Start(psi);
        }

        #endregion
    }
}
