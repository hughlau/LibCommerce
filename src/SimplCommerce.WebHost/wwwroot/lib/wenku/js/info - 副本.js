var id = "";
var f_catid = "";
var f_cattid = "";
var type = "";

var ua = window.navigator.userAgent.toLowerCase();
var isPC = function() {
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
if(getQueryString("id")) {
	id = getQueryString("id");

}
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
		docsnum:1
	},
	created: function() {
		this.ajax();
		if(getCookie('wenname')) {
			this.iflogin = false;
			this.wenname = getCookie('wenname');
		} else {
			this.iflogin = true;

		}
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
		download: function() {
			var that = this;

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
						usercode: getCookie('wenname'),
						id: this.datas.content[0].f_id
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
			ajaxs(inter_url + 'Interface/view.ashx', {
				id: id,
				usercode: wenname
			}, function(data) {

				that.datas = data;
               // console.log(that.datas.content[0].doc.length)
                if(that.datas.content[0].doc.length>5){
                	that.doc = that.datas.content[0].doc.slice(0,5);
                	that.docstate=true;
                }else{
                	that.doc = that.datas.content[0].doc;
                	that.docstate=false;
                }
				ajaxs(inter_url + 'Interface/index.ashx ', {}, function(data) {
					that.data = data;
					if(typeof(that.data) == 'string') {
						that.data = JSON.parse(data);
						var data = JSON.parse(data);
					}
					$("#title").html(that.datas.content[0].f_title);
					$("#keywords").attr('content', that.datas.content[0].f_title);
					$("#description").attr('content', that.datas.content[0].f_desc);

					that.fe();
					setTimeout(function() {
						$(".wenku-lazy").lazyload({
							effect: "fadeIn",
						});
					}, 100)

				})
			})
			ajaxs(inter_url + 'Interface/readAmount.ashx', {
				id: id
			}, function(data) {

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