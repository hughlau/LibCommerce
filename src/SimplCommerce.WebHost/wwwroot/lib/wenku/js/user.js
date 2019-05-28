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
		pageindex :1,
		shareList:[],
		pagenow:"",
		pagesum:"",
		tabindex:0,
		keyword:"",
		nikname:"",
	    avatar:"",
	    shareLists:[],
		pagenows:"",
		pagesums:"",
		pageindexs :1,
		
		
	},
	created: function() {
	
		if(getCookie('wenname')) {
			this.iflogin = false;
			this.avatar =getCookie('avatar');
			this.nikname =getCookie('nikname');
			this.wenname=getCookie('wenname');
			this.gold=getCookie('gold');
			this.count=getCookie('count');
			this.collect=getCookie('collect');
		} else {
			this.iflogin = true;
			window.location.href='http://www.apicool.cn/wenku/js/reg.html';
			
		}
		this.ajax();
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
		open:function(url){
			window.location.href=url;
		},
		logout: function(){
			delCookie('wenname');
			window.location='http://www.apicool.cn/wenku/js/index.html';
		},
		ajax: function() {
			var that = this;
			ajaxs(inter_url + 'Interface/index.ashx ', {}, function(data) {

				that.data = data;
				setTimeout(function() {
					var mySwiper = new Swiper('.swiper-container', {
						autoplay: 5000, //可选选项，自动滑动

						pagination: '.swiper-pagination',
						prevButton: '.swiper-button-prev',
						nextButton: '.swiper-button-next',
						loop: true,
					})
				})

			})
			ajaxs(inter_url + 'interface/shareList.ashx ', {
				usercode: getCookie('wenname'),  //
				pageindex:this.pageindex
			}, function(data) {

				that.shareList = data.share;
				that.pagenow=Number(data.pagenow) ;
				that.pagesum=data.pagesum;
				
			})
			ajaxs(inter_url + 'interface/offerList.ashx', {
				usercode: getCookie('wenname'),  //
				pageindex:this.pageindexs
			}, function(data) {

				that.shareLists = data.offer;
				that.pagenows=Number(data.pagenow) ;
				that.pagesums=data.pagesum;
				
			})

		},
		page:function(index){
			
		    var index=Number(index);
			var that=this;
			that.pageindex=index;
			ajaxs(inter_url + 'interface/shareList.ashx ', {
				usercode: getCookie('wenname'),  //getCookie('wenname')
				pageindex:index
			}, function(data) {

				that.shareList = data.share;
				that.pagenow=Number(data.pagenow) ;
				that.pagesum=data.pagesum;
				
			})
		},
		pages:function(index){
			
		    var index=Number(index);
			var that=this;
			that.pageindexs=index;
			ajaxs(inter_url + 'interface/offerList.ashx ', {
				usercode: getCookie('wenname'),  //getCookie('wenname')
				pageindex:index
			}, function(data) {

				that.shareLists = data.offer;
				that.pagenows=Number(data.pagenow) ;
				that.pagesums=data.pagesum;
				
			})
		},
		imgs:function(index){
			//doc,docx,ppt,pptx,xls,xlsx,pdf,txt
			if(index==".docx"||index==".doc"){
				return "images/word_24.png"/*tpa=http://www.apicool.cn/wenku/js/images/word_24.png*/
			}else if(index==".ppt"||index==".pptx"){
				return "images/ppt_24.png"/*tpa=http://www.apicool.cn/wenku/js/images/ppt_24.png*/
			}else if(index==".xls"||index==".xlsx"){
				return "images/excel_24.png"/*tpa=http://www.apicool.cn/wenku/js/images/excel_24.png*/
			}else if(index==".pdf"){
				return "images/pdf_24.png"/*tpa=http://www.apicool.cn/wenku/js/images/pdf_24.png*/
			}else if(index==".txt"){
				return "images/txt_24.png"/*tpa=http://www.apicool.cn/wenku/js/images/txt_24.png*/
			}else{
				return "images/epub_24.png"/*tpa=http://www.apicool.cn/wenku/js/images/epub_24.png*/
			}
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