var inter_url = "https://localhost:49206";
//var inter_url = "";
var inter_img = "https://localhost:49206";

function getQueryString(name) {
	var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
	var r = window.location.search.substr(1).match(reg);
	if(r != null) return r[2];
	return '';
}
function getQueryStrings(name) {
    var reg = new RegExp("(^|&)"+ name +"=([^&]*)(&|$)");
        var URL =  decodeURI(window.location.search);
        var r = URL.substr(1).match(reg);
        if(r!=null){
            //decodeURI() 函数可对 encodeURI() 函数编码过的 URI 进行解码
            return  decodeURI(r[2]);
        };
        return null;


 }
function setCookie(name, value) {
	var Days = 30;
	var exp = new Date();
	exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
	document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
}

function getCookie(name) {
	var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");
	if(arr = document.cookie.match(reg))
		return unescape(arr[2]);
	else
		return null;
}

function delCookie(name) {
	var exp = new Date();
	exp.setTime(exp.getTime() - 1);
	var cval = getCookie(name);
	if(cval != null)
		document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString();
}

function ajaxs(url, data, cb) {

	$.ajax({
		url: url,
		type: "post",
		data: JSON.stringify(data),
		dataType: 'json',

		contentType: 'application/x-www-form-urlencoded',
		success: function(data) {

			//console.log(data);
			cb(data);
		},
		error: function(data) {

			//console.log(JSON.stringify(data))

			var yg = new Ygtoast();
			yg.toast("网络错误", 1500);
		}
	})
}

function ajaxsGet(url, data, cb) {

	$.ajax({
		url: url,
		type: "get",
		data: JSON.stringify(data),
		dataType: 'json',
		contentType: 'application/json;charset=UTF-8',
		success: function(data) {

			//console.log(data);
			cb(data);
		},
		error: function(data) {

			//console.log(JSON.stringify(data))

			var yg = new Ygtoast();
			yg.toast("网络错误", 1500);
		}
	})
}
//立即执行函数
(function(window, undefined) {
	var Ygtoast = function() { //构造函数大驼峰命名法
	};
	Ygtoast.prototype = { //prototype 属性使您有能力向对象添加属性和方法。
		create: function(str, duration) {
			let self = this;
			var toastHtml = '';
			var toastText = '<div class="yg-toast-text">' + str + '</div>';
			toastHtml = '<div class="yg-toast">' + toastText + '</div>';
			if(document.querySelector(".yg-toast")) return; //当还没hide时禁止重复点击
			document.body.insertAdjacentHTML('beforeend', toastHtml);
			duration == null ? duration = 2000 : ''; //如果toast没有加上时间，默认2000毫秒；
			self.show();
			setTimeout(function() {
				self.hide();
			}, duration);
		},
		show: function() {
			let self = this;
			document.querySelector(".yg-toast").style.display = "block";
			document.querySelector(".yg-toast").style.marginTop = "-" + Math.round(document.querySelector(".yg-toast").offsetHeight / 2) + "px";
			if(document.querySelector(".yg-toast")) return;
		},
		hide: function() {
			var self = this;
			if(document.querySelector(".yg-toast")) {
				document.querySelector(".yg-toast").parentNode.removeChild(document.querySelector(".yg-toast"));
			}
		},
		toast: function(str, duration) {
			var self = this;
			return self.create(str, duration);
		}
	};
	window.Ygtoast = Ygtoast;
}(window));
