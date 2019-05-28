var app = new Vue({
	el: "#app",
	data: {
		data: "",
		inter_url: inter_url,
		inter_img: inter_img,
		id: 0,
		iflogin: true,
		wenname: "",
		gold: "",
		count: "",
		collect: "",
		nic: "",
		datas: "",
		dataslist: [],
		keyword: "",
		depart: "",
		depart1: "",
		depart2: "",
		depart3: "",
		f_id1: "",
		f_id2: "",
		f_id3: "",
		nikname: "",
		avatar: "",
		datalist: [],
		datalist1: [],
		datalist2: [],
		datalist3: [],
		datalist4: [],
		hotdoc:""

	},
	created: function() {
		this.ajax();
		if(getCookie('wenname')) {
			this.avatar = getCookie('avatar');
			this.nikname = getCookie('nikname');
			this.iflogin = false;
			this.wenname = getCookie('wenname');
			this.gold = getCookie('gold');
			this.count = getCookie('count');
			this.collect = getCookie('collect');
		} else {
			this.iflogin = true;

		}
	},
	methods: {
		getCouponSelected1: function(e) {
			this.depart2 = this.depart1[e.target.options.selectedIndex].depart2;
			this.f_id2 = this.depart1[e.target.options.selectedIndex].depart2[0].f_id2;
			this.depart3 = this.depart2[0].depart3;
			this.f_id3 = this.depart2[0].depart3[0].f_id3;

		},
		getCouponSelected2: function(e) {
			this.depart3 = this.depart2[e.target.options.selectedIndex].depart3;
			this.f_id3 = this.depart2[e.target.options.selectedIndex].depart3[0].f_id3;

		},
		searches: function() {
			var that = this;
			if(this.keyword == "") {
				wenku_alert("danger", "请输入搜索关键词", 1000, "");
				return false;
			}
			window.location.href = "http://www.apicool.cn/wenku/js/search.html?wd=" + this.keyword;
		},
		search: function() {
			var that = this;
			if(this.keyword == "") {
				wenku_alert("danger", "请输入搜索关键词", 1000, "");
				return false;
			}
			ajaxsGet(inter_url + 'interface/search.ashx?f_word=' + this.keyword, {

			}, function(rt) {
				if(rt.code == 0) {

				}
			})
		},
		login: function() {
			var _this = $(this),
				form = $("form"),
				inputs = form.find("input[required=required]"),
				usercode = form.find("input[name=usercode]"),

				userpwd = form.find("input[name=userpwd]")

			;
			console.log(usercode)
			ajaxs(inter_url + 'interface/login.ashx', {
				"usercode": usercode.val(),
				"userpwd": userpwd.val(),

			}, function(rt) {

				if(rt.code == 0) {
					//					console.log(rt.code)
					//gold金币数量，count文档数量，collect收藏数量
					setCookie("gold", rt.gold);
					setCookie("count", rt.count);
					setCookie("collect", rt.collect);
					setCookie("nikname", rt.f_nikname);
					setCookie("avatar", rt.f_avatar);
					setCookie("wenname", usercode.val());

					window.location.reload();
					//wenku_alert("success", rt.desc, 3000, form.attr("data-redirect"));
				} else {
					wenku_alert("danger", rt.desc, 3000, "");
				}

			});
		},
		logout: function() {
			delCookie('wenname');
			window.location = 'http://www.apicool.cn/wenku/js/index.html';
		},
		ajax: function() {
			var that = this;
			ajaxs(inter_url + 'Interface/index.ashx ', {}, function(data) {

				that.data = data;
				if(typeof(that.data) == 'string') {
					that.data = JSON.parse(data);
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
				if(typeof(that.datas) == 'string') {
					that.datas = JSON.parse(data);
				}
				console.log(that.datas.catalog)
				setTimeout(function() {
					that.dataslist = that.doTwoNav(that.datas.catalog[0].f_catid);
				}, 1000)

			})
			ajaxs(inter_url + 'interface/classify.ashx ', {}, function(data) {

				that.datalist = data;
				that.datalist1 = data;
				that.datalist2 = data[0].tow;
				that.datalist3 = data[0].tow[0].three;
				that.datalist4 = data[0].tow[0].three[0].four;

			})
			ajaxs(inter_url + 'interface/depart.ashx ', {}, function(data) {
				that.depart1 = data;
				that.depart2 = data[0].depart2;
				that.depart3 = data[0].depart2[0].depart3;
				that.f_id1 = data[0].f_id1
				that.f_id2 = data[0].depart2[0].f_id2;
				that.f_id3 = data[0].depart2[0].depart3[0].f_id3;
				if(typeof(that.depart) == 'string') {
					that.depart = JSON.parse(data);
				}

			})
		},
		indexSelect1: function(event) {
			var that = this;

			var index = 0;

			for(var x = 0; x < that.datalist1.length; x++) {

				if(event.target.value == that.datalist1[x].f_catid) {

					index = x;
				}
			}
			that.datalist2 = that.datalist1[index].tow;
			that.datalist3 = that.datalist2[0].three;
			that.datalist4 = that.datalist3[0].four;
			//console.log(index)

		},
		indexSelect2: function(event) {
			var that = this;
			//console.log(event.target.value)
			var index = 0;

			for(var x = 0; x < that.datalist2.length; x++) {
				if(event.target.value == that.datalist2[x].f_cattid) {
					index = x;
				}
			}

			that.datalist3 = that.datalist2[index].three;
			that.datalist4 = that.datalist3[0].four;

		},
		indexSelect3: function(event) {
			var that = this;
			//console.log(event.target.value)
			var index = 0;

			for(var x = 0; x < that.datalist3.length; x++) {
				if(event.target.value == that.datalist3[x].f_catid3) {
					index = x;
				}
			}

			that.datalist4 = that.datalist3[index].four;

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