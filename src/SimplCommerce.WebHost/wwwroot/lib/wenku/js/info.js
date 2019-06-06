var id = "";
var f_catid = "";
var f_cattid = "";
var type = "";

var ua = window.navigator.userAgent.toLowerCase();

var isPC = function () {
	var ua = navigator.userAgent,
		isWindowsPhone = /(?:Windows Phone)/.test(ua),
		isSymbian = /(?:SymbianOS)/.test(ua) || isWindowsPhone,
		isAndroid = /(?:Android)/.test(ua),
		isFireFox = /(?:Firefox)/.test(ua),
		isChrome = /(?:Chrome|CriOS)/.test(ua),
		isTablet = /(?:iPad|PlayBook)/.test(ua) || (isAndroid && !/(?:Mobile)/.test(ua)) || (isFireFox && /(?:Tablet)/.test(ua)),
		isPhone = /(?:iPhone)/.test(ua) && !isTablet,
		isPc = !isPhone && !isAndroid && !isSymbian;
	return {
		isTablet: isTablet,
		isPhone: isPhone,
		isAndroid: isAndroid,
		isPc: isPc
	};
}
var os = isPC();
id = getLastString();
//if(getQueryString("id")) {
//	id = getQueryString("id");
//}
$(".bdsharebuttonbox").share();

var app = new Vue({
	el: "#app",
	data: {
		src: "",
		data: "",
		datas: "",
		inter_img: inter_img,
		inter_url: inter_url,
		id: id,
		f_catidName: "",
		f_cattidName: [],
		twoindex: -1,
		iflogin: false,
		keyword: "",
		wenname: "",
		comment: "",
		doc:"",
		docstate:false,
        docsnum: 1,
        catpchatoken:"1"
    },
    mounted: function () {
        $('#ModalDownload').on('show.bs.modal',
            function () {
                alert("1");
                app.captcha();
            }
        );
    },
	created: function() {
		this.ajax();
	},
	methods: {
		zhankai:function(){
		
			var that=this;
			var arr=that.datas.content[0].doc.slice(that.docsnum*5,that.docsnum*5+5);
			
			if(arr.length==5){
				that.docsnum++;
			 that.doc=	that.doc.concat(arr);
			}else{
				that.docsnum++;
			that.doc=	that.doc.concat(arr);
				that.docstate=false;
			}
			console.log(that.doc)
		},
		subcomment: function() {
			var that = this;
			if(this.comment == "") {
				wenku_alert("danger", "请输入评论内容", 1000, "");
				return false;
			}
			ajaxs(inter_url + 'Interface/viewComment.ashx', {
				id: id,
				usercode: that.wenname,
				comment: that.comment
			}, function(data) {
				if(data.code == '-1') {
					wenku_alert("danger", "评论失败", 1000, "");
				} else {
					that.comment="";
					wenku_alert("success", "评论成功", 1000, "");
					that.ajax();
				}
			})
		},
		searches: function() {
			var that = this;
			if(this.keyword == "") {
				wenku_alert("danger", "请输入搜索关键词", 1000, "");
				return false;
			}
			window.location.href = "search.html?wd=" + this.keyword;
        },
        captcha: function () {
            ajaxsGet('https://localhost:49206/api/captcha/get/' + this.catpchatoken, {
            }, function (data) {
                $("#imgcaptcha").attr("src", 'data:image/gif;base64,' + data.img);
                $("#hidCapToken").val(data.token);
            });
        },
        download: function () {
            if ($.trim($("#captchacode").val()) == "") {
                wenku_alert("danger","验证码错误", 3000, "");
                return;
            }
            var that = this;
            ajaxsGet('https://localhost:49206/api/captcha/yz?token=' + $("#hidCapToken").val()
                + "&code=" + $.trim($("#captchacode").val()) + "&id=" + that.id, {
            }, function (data) {
                if (!data.success) {
                    wenku_alert("danger","验证码错误", data.desc, 3000, "");
                } else {
                    window.location.href = 'https://localhost:49206/api/products/down/' + data.downToken;
                    //$.download('https://localhost:49206/api/products/down/' + data.downtoken);
                    //ajaxsGet('https://localhost:49206/api/products/down/' + data.downtoken, {
                    //}, function (data) {
                    //})
                }
            });
			if(this.datas.content[0].f_gold > 0 && this.iflogin) {

				if(os.isAndroid || os.isPhone || ua.match(/MicroMessenger/i) == 'micromessenger') {

					that.src = "PayAPP.html?usercode=" + getCookie('wenname') + "&gold=" + that.datas.content[0].f_gold + "&id=" + id;
					window.location.href = that.src;
				} else {
					that.src = "PayIndex.html?usercode=" + getCookie('wenname') + "&gold=" + that.datas.content[0].f_gold + "&id=" + id;
					$("#ModalDownload").modal('hide');
					$("#ModalUserEdits").modal('show');
				}

			} else {
				if(this.iflogin) {

					window.location.href = "reg.html";
					return false;
				} else {
					ajaxs(inter_url + 'Interface/downLoad.ashx ', {
                        usercode: id
					}, function(data) {
						if(data.code == '-1') {

							wenku_alert("danger", data.desc, 3000, "");
							setTimeout(function() {

								if(os.isAndroid || os.isPhone || ua.match(/MicroMessenger/i) == 'micromessenger') {

									that.src = "PayAPP.html?usercode=" + getCookie('wenname') + "&gold=" + that.datas.content[0].f_gold + "&id=" + id;
									window.location.href = that.src;
								} else {
									that.src = "PayIndex.html?usercode=" + getCookie('wenname') + "&gold=" + that.datas.content[0].f_gold + "&id=" + id;
									$("#ModalDownload").modal('hide');
									$("#ModalUserEdits").modal('show');
								}

							}, 3000)

						} else {
							wenku_alert("success", "即将跳转下载页面", 3000, "");
							setTimeout(function() {
								window.location.href = inter_url + data.desc;
								//window.open(inter_url+data.desc)
							}, 3000)
						}
					})
				}

			}

		},
		ajax: function() {
			var that = this;
			var wenname = "";
			if(getCookie('wenname')) {
				wenname = getCookie('wenname');
			}
			//var dd = '{"content":[{"f_id":"79","f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档","f_title":"测试生成缩略图","f_code":"文库用户","f_gold":"0","f_view":"49","f_page":"99","f_date":"2019/05/07 22:33:06","f_img":"images/pdf.jpg","f_desc":"测试生成缩略图","doc":[{"pic":"FileImage/636928651863010247/6369286518630102471.Png"},{"pic":"FileImage/636928651863010247/6369286518630102472.Png"},{"pic":"FileImage/636928651863010247/6369286518630102473.Png"},{"pic":"FileImage/636928651863010247/6369286518630102474.Png"},{"pic":"FileImage/636928651863010247/6369286518630102475.Png"},{"pic":"FileImage/636928651863010247/6369286518630102476.Png"},{"pic":"FileImage/636928651863010247/6369286518630102477.Png"},{"pic":"FileImage/636928651863010247/6369286518630102478.Png"},{"pic":"FileImage/636928651863010247/6369286518630102479.Png"},{"pic":"FileImage/636928651863010247/63692865186301024710.Png"},{"pic":"FileImage/636928651863010247/63692865186301024711.Png"},{"pic":"FileImage/636928651863010247/63692865186301024712.Png"},{"pic":"FileImage/636928651863010247/63692865186301024713.Png"},{"pic":"FileImage/636928651863010247/63692865186301024714.Png"},{"pic":"FileImage/636928651863010247/63692865186301024715.Png"},{"pic":"FileImage/636928651863010247/63692865186301024716.Png"},{"pic":"FileImage/636928651863010247/63692865186301024717.Png"},{"pic":"FileImage/636928651863010247/63692865186301024718.Png"},{"pic":"FileImage/636928651863010247/63692865186301024719.Png"},{"pic":"FileImage/636928651863010247/63692865186301024720.Png"},{"pic":"FileImage/636928651863010247/63692865186301024721.Png"},{"pic":"FileImage/636928651863010247/63692865186301024722.Png"},{"pic":"FileImage/636928651863010247/63692865186301024723.Png"},{"pic":"FileImage/636928651863010247/63692865186301024724.Png"},{"pic":"FileImage/636928651863010247/63692865186301024725.Png"},{"pic":"FileImage/636928651863010247/63692865186301024726.Png"},{"pic":"FileImage/636928651863010247/63692865186301024727.Png"},{"pic":"FileImage/636928651863010247/63692865186301024728.Png"},{"pic":"FileImage/636928651863010247/63692865186301024729.Png"},{"pic":"FileImage/636928651863010247/63692865186301024730.Png"},{"pic":"FileImage/636928651863010247/63692865186301024731.Png"},{"pic":"FileImage/636928651863010247/63692865186301024732.Png"},{"pic":"FileImage/636928651863010247/63692865186301024733.Png"},{"pic":"FileImage/636928651863010247/63692865186301024734.Png"},{"pic":"FileImage/636928651863010247/63692865186301024735.Png"},{"pic":"FileImage/636928651863010247/63692865186301024736.Png"},{"pic":"FileImage/636928651863010247/63692865186301024737.Png"},{"pic":"FileImage/636928651863010247/63692865186301024738.Png"},{"pic":"FileImage/636928651863010247/63692865186301024739.Png"},{"pic":"FileImage/636928651863010247/63692865186301024740.Png"},{"pic":"FileImage/636928651863010247/63692865186301024741.Png"},{"pic":"FileImage/636928651863010247/63692865186301024742.Png"},{"pic":"FileImage/636928651863010247/63692865186301024743.Png"},{"pic":"FileImage/636928651863010247/63692865186301024744.Png"},{"pic":"FileImage/636928651863010247/63692865186301024745.Png"},{"pic":"FileImage/636928651863010247/63692865186301024746.Png"},{"pic":"FileImage/636928651863010247/63692865186301024747.Png"},{"pic":"FileImage/636928651863010247/63692865186301024748.Png"},{"pic":"FileImage/636928651863010247/63692865186301024749.Png"},{"pic":"FileImage/636928651863010247/63692865186301024750.Png"},{"pic":"FileImage/636928651863010247/63692865186301024751.Png"},{"pic":"FileImage/636928651863010247/63692865186301024752.Png"},{"pic":"FileImage/636928651863010247/63692865186301024753.Png"},{"pic":"FileImage/636928651863010247/63692865186301024754.Png"},{"pic":"FileImage/636928651863010247/63692865186301024755.Png"},{"pic":"FileImage/636928651863010247/63692865186301024756.Png"},{"pic":"FileImage/636928651863010247/63692865186301024757.Png"},{"pic":"FileImage/636928651863010247/63692865186301024758.Png"},{"pic":"FileImage/636928651863010247/63692865186301024759.Png"},{"pic":"FileImage/636928651863010247/63692865186301024760.Png"},{"pic":"FileImage/636928651863010247/63692865186301024761.Png"},{"pic":"FileImage/636928651863010247/63692865186301024762.Png"},{"pic":"FileImage/636928651863010247/63692865186301024763.Png"},{"pic":"FileImage/636928651863010247/63692865186301024764.Png"},{"pic":"FileImage/636928651863010247/63692865186301024765.Png"},{"pic":"FileImage/636928651863010247/63692865186301024766.Png"},{"pic":"FileImage/636928651863010247/63692865186301024767.Png"},{"pic":"FileImage/636928651863010247/63692865186301024768.Png"},{"pic":"FileImage/636928651863010247/63692865186301024769.Png"},{"pic":"FileImage/636928651863010247/63692865186301024770.Png"},{"pic":"FileImage/636928651863010247/63692865186301024771.Png"},{"pic":"FileImage/636928651863010247/63692865186301024772.Png"},{"pic":"FileImage/636928651863010247/63692865186301024773.Png"},{"pic":"FileImage/636928651863010247/63692865186301024774.Png"},{"pic":"FileImage/636928651863010247/63692865186301024775.Png"},{"pic":"FileImage/636928651863010247/63692865186301024776.Png"},{"pic":"FileImage/636928651863010247/63692865186301024777.Png"},{"pic":"FileImage/636928651863010247/63692865186301024778.Png"},{"pic":"FileImage/636928651863010247/63692865186301024779.Png"},{"pic":"FileImage/636928651863010247/63692865186301024780.Png"},{"pic":"FileImage/636928651863010247/63692865186301024781.Png"},{"pic":"FileImage/636928651863010247/63692865186301024782.Png"},{"pic":"FileImage/636928651863010247/63692865186301024783.Png"},{"pic":"FileImage/636928651863010247/63692865186301024784.Png"},{"pic":"FileImage/636928651863010247/63692865186301024785.Png"},{"pic":"FileImage/636928651863010247/63692865186301024786.Png"},{"pic":"FileImage/636928651863010247/63692865186301024787.Png"},{"pic":"FileImage/636928651863010247/63692865186301024788.Png"},{"pic":"FileImage/636928651863010247/63692865186301024789.Png"},{"pic":"FileImage/636928651863010247/63692865186301024790.Png"},{"pic":"FileImage/636928651863010247/63692865186301024791.Png"},{"pic":"FileImage/636928651863010247/63692865186301024792.Png"},{"pic":"FileImage/636928651863010247/63692865186301024793.Png"},{"pic":"FileImage/636928651863010247/63692865186301024794.Png"},{"pic":"FileImage/636928651863010247/63692865186301024795.Png"},{"pic":"FileImage/636928651863010247/63692865186301024796.Png"},{"pic":"FileImage/636928651863010247/63692865186301024797.Png"},{"pic":"FileImage/636928651863010247/63692865186301024798.Png"},{"pic":"FileImage/636928651863010247/63692865186301024799.Png"}],"indexComment":[{"f_docid":"79","f_user":"wenku","f_avatar":"images/avatar/636889357094325704.jpg","f_content":"test","f_datetime":"2019-05-09 11:35:37"}]}],"Hot":[{"f_id":"1","f_title":"文库系统操作手册","f_img":"images/word.jpg"},{"f_id":"2","f_title":"权限测试文档","f_img":"images/word.jpg"},{"f_id":"3","f_title":"文库系统测试问题汇总","f_img":"images/word.jpg"},{"f_id":"4","f_title":"PMI网上申请考试指导","f_img":"images/pdf.jpg"},{"f_id":"5","f_title":"返工返修统计记录表","f_img":"images/excel.jpg"},{"f_id":"6","f_title":"顾客满意度调查统计表","f_img":"images/excel.jpg"},{"f_id":"7","f_title":"Excel2010操作与技巧","f_img":"images/pdf.jpg"},{"f_id":"8","f_title":"路由器毕业论文范文","f_img":"images/word.jpg"},{"f_id":"9","f_title":"二分查找法的实现和应用汇总","f_img":"images/word.jpg"},{"f_id":"10","f_title":"【上海交通大学(上海交大)计算机组成与系统结构】【习题试卷】10","f_img":"images/word.jpg"}],"New":[{"f_id":"1","f_title":"文库系统操作手册","f_img":"images/word.jpg"},{"f_id":"2","f_title":"权限测试文档","f_img":"images/word.jpg"},{"f_id":"3","f_title":"文库系统测试问题汇总","f_img":"images/word.jpg"},{"f_id":"4","f_title":"PMI网上申请考试指导","f_img":"images/pdf.jpg"},{"f_id":"5","f_title":"返工返修统计记录表","f_img":"images/excel.jpg"}]}';
   //            // console.log(that.datas.content[0].doc.length)
			//   that.datas =JSON.parse(dd);
   //             if(that.datas.content[0].doc.length>5){
   //             	that.doc = that.datas.content[0].doc.slice(0,5);
   //             	that.docstate=true;
   //             }else{
   //             	that.doc = that.datas.content[0].doc;
   //             	that.docstate=false;
   //             }

            ajaxsGet('https://localhost:49206/api/products/show/' + this.id, {
            }, function (data) {

                that.datas = data;
                // console.log(that.datas.content[0].doc.length)
                if (that.datas.content[0].doc.length > 5) {
                    that.doc = that.datas.content[0].doc.slice(0, 5);
                    that.docstate = true;
                } else {
                    that.doc = that.datas.content[0].doc;
                    that.docstate = false;
                }
                var ad = '{"usersum":"17","docsum":"79","banner":[{"f_id":"1","f_title":"首面横幅","f_link":"http: //www.baidu.com","f_img":"images/banner1.png"},{"f_id":"2","f_title":"首面横幅2","f_link":"http: //www.163.com","f_img":"images/banner2.png"},{"f_id":"3","f_title":"首面横幅3","f_link":"http: //www.google.com","f_img":"images/banner3.png"}],"link":[{"f_id":"1","f_title":"官方博客","f_link":"http: //www.apierp.cn/"},{"f_id":"2","f_title":"淘宝店铺","f_link":"https: //czfeixiang.taobao.com/"},{"f_id":"3","f_title":"联系我们","f_link":"http: //www.officesee.com"}],"catalog":[{"f_catid":"1","f_onelevel":"ERP系统","f_img":"/images/1.png","f_view":"Y"},{"f_catid":"2","f_onelevel":"SAP系统","f_img":"/images/2.png","f_view":"Y"},{"f_catid":"3","f_onelevel":"WMS系统","f_img":"/images/3.png","f_view":"Y"},{"f_catid":"4","f_onelevel":"SRM系统","f_img":"/images/4.png","f_view":"Y"},{"f_catid":"5","f_onelevel":"MES系统","f_img":"/images/5.png","f_view":"Y"},{"f_catid":"6","f_onelevel":"通知公告","f_img":"/images/6.png","f_view":"Y"}],"catadetail":[{"f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档"},{"f_catid":"1","f_onelevel":"ERP系统","f_cattid":"8","f_towlevel":"流程文档"},{"f_catid":"1","f_onelevel":"ERP系统","f_cattid":"9","f_towlevel":"操作手册"},{"f_catid":"1","f_onelevel":"ERP系统","f_cattid":"10","f_towlevel":"帮助说明"},{"f_catid":"2","f_onelevel":"SAP系统","f_cattid":"27","f_towlevel":"需求分析"},{"f_catid":"2","f_onelevel":"SAP系统","f_cattid":"28","f_towlevel":"方案制定"},{"f_catid":"2","f_onelevel":"SAP系统","f_cattid":"29","f_towlevel":"操作流程"},{"f_catid":"2","f_onelevel":"SAP系统","f_cattid":"30","f_towlevel":"帮助说明"},{"f_catid":"3","f_onelevel":"WMS系统","f_cattid":"13","f_towlevel":"项目需求"},{"f_catid":"3","f_onelevel":"WMS系统","f_cattid":"14","f_towlevel":"方案约定"},{"f_catid":"3","f_onelevel":"WMS系统","f_cattid":"15","f_towlevel":"操作流程"},{"f_catid":"3","f_onelevel":"WMS系统","f_cattid":"16","f_towlevel":"帮助说明"},{"f_catid":"4","f_onelevel":"SRM系统","f_cattid":"36","f_towlevel":"IT"},{"f_catid":"4","f_onelevel":"SRM系统","f_cattid":"37","f_towlevel":"人力资源"},{"f_catid":"4","f_onelevel":"SRM系统","f_cattid":"38","f_towlevel":"项目管理"},{"f_catid":"4","f_onelevel":"SRM系统","f_cattid":"39","f_towlevel":"合同评审"},{"f_catid":"5","f_onelevel":"MES系统","f_cattid":"45","f_towlevel":"需求文档"},{"f_catid":"5","f_onelevel":"MES系统","f_cattid":"46","f_towlevel":"提交方案"},{"f_catid":"5","f_onelevel":"MES系统","f_cattid":"47","f_towlevel":"操作流程"},{"f_catid":"5","f_onelevel":"MES系统","f_cattid":"48","f_towlevel":"帮助文档"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"12","f_towlevel":"帮助说明"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"13","f_towlevel":"需求分析"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"14","f_towlevel":"通知公告"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"14","f_towlevel":"通知公告"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"14","f_towlevel":"通知公告"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"14","f_towlevel":"通知公告"},{"f_catid":"6","f_onelevel":"通知公告","f_cattid":"15","f_towlevel":"外来通知"}],"content":[{"f_id":"75","f_catid":"6","f_onelevel":"通知公告","f_cattid":"12","f_towlevel":"帮助说明","f_title":"20190425150150.docx","f_view":"1"},{"f_id":"74","f_catid":"6","f_onelevel":"通知公告","f_cattid":"12","f_towlevel":"帮助说明","f_title":"636928628307312939.docx","f_view":"0"},{"f_id":"73","f_catid":"6","f_onelevel":"通知公告","f_cattid":"12","f_towlevel":"帮助说明","f_title":"636928628307312939.docx","f_view":"2"},{"f_id":"72","f_catid":"6","f_onelevel":"通知公告","f_cattid":"12","f_towlevel":"帮助说明","f_title":"rw.docx","f_view":"0"},{"f_id":"68","f_catid":"6","f_onelevel":"通知公告","f_cattid":"12","f_towlevel":"帮助说明","f_title":"rw.docx","f_view":"0"},{"f_id":"55","f_catid":"5","f_onelevel":"MES系统","f_cattid":"45","f_towlevel":"需求文档","f_title":"0111395319743_3156.ppt","f_view":"3"},{"f_id":"54","f_catid":"2","f_onelevel":"SAP系统","f_cattid":"30","f_towlevel":"帮助说明","f_title":"0111395319743_3156.pptx","f_view":"3"},{"f_id":"52","f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档","f_title":"悬赏功能上线测试","f_view":"50"},{"f_id":"48","f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档","f_title":"[已读]BMS2.0培训.pptx","f_view":"76"},{"f_id":"39","f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档","f_title":"飞翔文库分享系统操作手册v1.5","f_view":"123"},{"f_id":"38","f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档","f_title":"测试手机端在线支付","f_view":"113"},{"f_id":"32","f_catid":"5","f_onelevel":"MES系统","f_cattid":"46","f_towlevel":"提交方案","f_title":"ERP实施散记-书本复制","f_view":"20"},{"f_id":"31","f_catid":"5","f_onelevel":"MES系统","f_cattid":"46","f_towlevel":"提交方案","f_title":"小说书本复制-想当年","f_view":"7"},{"f_id":"30","f_catid":"5","f_onelevel":"MES系统","f_cattid":"48","f_towlevel":"帮助文档","f_title":"word编排报纸设计","f_view":"6"},{"f_id":"29","f_catid":"4","f_onelevel":"SRM系统","f_cattid":"36","f_towlevel":"IT","f_title":"计算机发展史","f_view":"4"},{"f_id":"28","f_catid":"4","f_onelevel":"SRM系统","f_cattid":"36","f_towlevel":"IT","f_title":"逻辑学基础-书本复制","f_view":"6"},{"f_id":"27","f_catid":"4","f_onelevel":"SRM系统","f_cattid":"37","f_towlevel":"人力资源","f_title":"我眼中的网络时代","f_view":"5"},{"f_id":"26","f_catid":"3","f_onelevel":"WMS系统","f_cattid":"16","f_towlevel":"帮助说明","f_title":"五笔学习建议-图文介绍","f_view":"29"},{"f_id":"25","f_catid":"3","f_onelevel":"WMS系统","f_cattid":"13","f_towlevel":"项目需求","f_title":"职业人格MBTI测评报告","f_view":"8"},{"f_id":"24","f_catid":"3","f_onelevel":"WMS系统","f_cattid":"14","f_towlevel":"方案约定","f_title":"365个事-8月份","f_view":"6"},{"f_id":"23","f_catid":"3","f_onelevel":"WMS系统","f_cattid":"13","f_towlevel":"项目需求","f_title":"365个事-9月份","f_view":"3"},{"f_id":"22","f_catid":"2","f_onelevel":"SAP系统","f_cattid":"29","f_towlevel":"操作流程","f_title":"SAP-CO-PCA-SAP利润中心会计配置及操作手册-V1.0","f_view":"16"},{"f_id":"21","f_catid":"2","f_onelevel":"SAP系统","f_cattid":"27","f_towlevel":"需求分析","f_title":"理解不断提高党的执政能力五个方面的基本要求","f_view":"21"},{"f_id":"20","f_catid":"4","f_onelevel":"SRM系统","f_cattid":"36","f_towlevel":"IT","f_title":"CRM教程","f_view":"2"},{"f_id":"18","f_catid":"2","f_onelevel":"SAP系统","f_cattid":"27","f_towlevel":"需求分析","f_title":"2015会计继续教育课后习题","f_view":"4"},{"f_id":"15","f_catid":"2","f_onelevel":"SAP系统","f_cattid":"27","f_towlevel":"需求分析","f_title":"论文摘要范文","f_view":"3"},{"f_id":"13","f_catid":"4","f_onelevel":"SRM系统","f_cattid":"36","f_towlevel":"IT","f_title":"离散数学教材","f_view":"3"},{"f_id":"11","f_catid":"5","f_onelevel":"MES系统","f_cattid":"45","f_towlevel":"需求文档","f_title":"第二章-以太网","f_view":"2"},{"f_id":"3","f_catid":"3","f_onelevel":"WMS系统","f_cattid":"14","f_towlevel":"方案约定","f_title":"文库系统测试问题汇总","f_view":"13"},{"f_id":"1","f_catid":"1","f_onelevel":"ERP系统","f_cattid":"7","f_towlevel":"需求文档","f_title":"文库系统操作手册","f_view":"83"}],"hotdoc":[{"f_id":"80","f_title":"文库管理系统功能手册v1.8.pdf","f_view":"13","f_gold":"免费","f_img":"FileThumbnail/636932523540932739.jpg","f_ico":"images/pdf_24.png"},{"f_id":"79","f_title":"测试生成缩略图","f_view":"50","f_gold":"免费","f_img":"FileThumbnail/636928651863010247.jpg","f_ico":"images/pdf_24.png"},{"f_id":"78","f_title":"测试缩略图2","f_view":"4","f_gold":"免费","f_img":"FileThumbnail/636928650155642592.jpg","f_ico":"images/word_24.png"},{"f_id":"77","f_title":"测试缩略图","f_view":"0","f_gold":"免费","f_img":"FileThumbnail/636928649231429730.jpg","f_ico":"images/word_24.png"},{"f_id":"76","f_title":"测试缩略图2","f_view":"1","f_gold":"免费","f_img":"FileThumbnail/636928646375996408.jpg","f_ico":"images/word_24.png"},{"f_id":"75","f_title":"20190425150150.docx","f_view":"1","f_gold":"免费","f_img":"FileThumbnail/636928637872840055.jpg","f_ico":"images/word_24.png"},{"f_id":"74","f_title":"636928628307312939.docx","f_view":"0","f_gold":"免费","f_img":"FileThumbnail/636928636438828035.jpg","f_ico":"images/word_24.png"},{"f_id":"73","f_title":"636928628307312939.docx","f_view":"2","f_gold":"免费","f_img":"FileThumbnail/636928632074818428.jpg","f_ico":"images/word_24.png"},{"f_id":"72","f_title":"rw.docx","f_view":"0","f_gold":"免费","f_img":"FileThumbnail/636928630081654425.jpg","f_ico":"images/word_24.png"},{"f_id":"71","f_title":"测试缩略图1","f_view":"0","f_gold":"免费","f_img":"FileThumbnail/636928628307312939.jpg","f_ico":"images/word_24.png"}]}';
                that.data = JSON.parse(ad);
                if (typeof (that.data) == 'string') {
                    that.data = JSON.parse(data);
                    var data = JSON.parse(data);
                }
                $("#title").html(that.datas.content[0].f_title);
                $("#keywords").attr('content', that.datas.content[0].f_title);
                $("#description").attr('content', that.datas.content[0].f_desc);

                that.fe();
                setTimeout(function () {
                    $(".wenku-lazy").lazyload({
                        effect: "fadeIn",
                    });
                }, 100)
            })
					

				

		},
		fe: function() {
			var that = this;

			this.data.catadetail.forEach(function(t, i) {
				if(t.f_cattid == that.datas.content[0].f_cattid) {
					that.f_catidName = t;

				}
			})
			console.log(that.f_catidName)

		},

		doThreeNav: function(index) {
			var arr = [];
			this.data.content.forEach(function(t, i) {
				if(t.f_catid == index) {
					arr.push(t)
				}
			})
			return arr;
		}
	},
})
//cls：success/(error|danger)
//msg:message
//timeout:超时刷新和跳转时间
//url:有url链接的话，跳转url链接
function wenku_alert(cls, msg, timeout, url) {
	var t = timeout > 0 ? parseInt(timeout) : 3000;
	if(cls == "error" || cls == "danger") {
		cls = "error";
	} else {
		cls = "success";
		// position="mid-center";
		close = false;
	}
	$.toast({
		text: msg, // Text that is to be shown in the toast
		// heading: 'Note', // Optional heading to be shown on the toast
		icon: cls, // Type of toast icon
		showHideTransition: 'slide', // fade, slide or plain
		allowToastClose: cls == "success" ? false : true, // Boolean value true or false
		hideAfter: t, // false to make it sticky or number representing the miliseconds as time after which toast needs to be hidden
		stack: 8, // false if there should be only one toast at a time or a number representing the maximum number of toasts to be shown at a time
		position: "top-center", // bottom-left or bottom-right or bottom-center or top-left or top-right or top-center or mid-center or an object representing the left, right, top, bottom values

		textAlign: 'left', // Text alignment i.e. left, right or center
		loader: true, // Whether to show loader or not. True by default
		loaderBg: '#c0f201', // Background color of the toast loader
		beforeShow: function() {}, // will be triggered before the toast is shown
		afterShown: function() {}, // will be triggered after the toat has been shown
		beforeHide: function() {}, // will be triggered before the toast gets hidden
		afterHidden: function() {} // will be triggered after the toast has been hidden
	});

	if(url) {
		setTimeout(function() {
			location.href = url
		}, t - 500);
	}
}




