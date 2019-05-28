using System;
using System.Collections.Generic;
using System.Text;

/****************************************************************
*   Author：L
*   Time：2019/5/28 15:55:22
*   FrameVersion：2.2
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Module.Catalog.Models.ProductDetail
{
    public partial class Content
    {

        #region =============属性============

        public string f_id { get; set; }
        public string f_catid { get; set; }
        public string f_onelevel { get; set; }
        public string f_cattid { get; set; }
        public string f_towlevel { get; set; }
        public string f_title { get; set; }
        public string f_code { get; set; }
        public string f_gold { get; set; }
        public string f_view { get; set; }
        public string f_page { get; set; }
        public string f_date { get; set; }
        public string f_img { get; set; }
        public string f_desc { get; set; }
        public List<docItem> doc { get; set; }
        public IndexComment indexComment { get; set; }
        #endregion

        #region ===========构造函数==========



        #endregion

        #region ===========基本方法==========



        #endregion

        #region =============方法============



        #endregion
    }
}
