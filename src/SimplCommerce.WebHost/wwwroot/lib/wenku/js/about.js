var id="";
if(getQueryString("id")) {
	id = getQueryString("id");

}
var app = new Vue({
	el: "#app",
	data: {
		data: "",
		inter_url: inter_url,
		inter_img: inter_img,
		id: 0,
		iflogin: true,
		wenname:"",
		gold:"",count:"",collect:"",
		nic:"",
		datas:"",
		dataslist:[],
		keyword:"",
		aboutList:""
		
		
	},
	created: function() {
		this.ajax();
		if(getCookie('wenname')) {
			this.iflogin = false;
			this.wenname=getCookie('wenname');
			this.gold=getCookie('gold');
			this.count=getCookie('count');
			this.collect=getCookie('collect');
		} else {
			this.iflogin = true;
			
		}
	},
	methods: {
		searches:function(){
			var that=this;
			if(this.keyword==""){
				wenku_alert("danger","请输入搜索关键词", 1000, "");
				return false;
			}
			window.location.href="http://www.apicool.cn/wenku/js/search.html?wd="+this.keyword;
		},
		search:function(){
			var that=this;
			if(this.keyword==""){
				wenku_alert("danger","请输入搜索关键词", 1000, "");
				return false;
			}
			ajaxsGet(inter_url + 'interface/search.ashx?f_word='+this.keyword, {
				

			}, function(rt) {
				if(rt.code == 0) {
					
				}
			})
		},
		
		ajax: function() {
			var that = this;
			ajaxs(inter_url + 'Interface/index.ashx ', {}, function(data) {
                  
				that.data = data;
				if(typeof(that.data) == 'string' ){
					that.data = JSON.parse(data) ;
				}
				setTimeout(function() {
					var mySwiper = new Swiper('.swiper-container', {
						autoplay: 5000, //可选选项，自动滑动

						pagination: '.swiper-pagination',
						prevButton: '.swiper-button-prev',
						nextButton: '.swiper-button-next',
						loop: true,
					})
				})

			});
			ajaxs(inter_url + 'interface/Category.ashx ', {}, function(data) {

				that.datas = data;
				if(typeof(that.datas) == 'string' ){
					that.datas = JSON.parse(data) ;
				}
				console.log(that.datas.catalog)
				setTimeout(function(){
					that.dataslist= that.doTwoNav(that.datas.catalog[0].f_catid);
				},500)
				

			})
			ajaxsGet(inter_url + 'interface/about.ashx?id='+id, {}, function(data) {
                 that.aboutList = data.about ;
				if(typeof(that.datas) == 'string' ){
					that.aboutList = JSON.parse(data.about) ;
				}
				
				
				

			})

		},
		ifwhere:function(){
			if(id==1){
				return "关于我们";
			}else if(id==2){
				return "文库协议";
			}else if(id==3){
				return "版本更新";
			}else if(id==4){
				return "免责声明";
			}else if(id==5){
				return "联系我们";
			}
		},
		 indexSelect(event){
           var that=this;
            console.log(event.target.value)
            that.dataslist= that.doTwoNav(event.target.value);
            

     },
		doTwoNav: function(index) {
			var arr = [];
			this.data.catadetail.forEach(function(t, i) {
				if(t.f_catid == index) {
					arr.push(t)
				}
			})
			
			return arr;
		},
		doThreeNav: function(index) {
			var arr = [];
			this.data.content.forEach(function(t, i) {
				if(t.f_catid == index) {
					arr.push(t)
				}
			})

			return arr.slice(0, 5);
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