using System;
using System.Collections.Generic;
using System.Text;

/****************************************************************
*   Author：L
*   Time：2019/5/29 14:22:58
*   FrameVersion：2.2
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Module.Core.Services
{
    public interface ITransferFileService
    {

        #region =============方法============

        int FileToImage(string filePath,string categoryId);

        #endregion
    }
}
