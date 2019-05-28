var f_catid = "";
var f_cattid = "";
var type = "";
if(getQueryString("f_catid")) {
	f_catid = getQueryString("f_catid");
	type = "f_catid";
}
if(getQueryString("f_cattid")) {
	f_cattid = getQueryString("f_cattid");
	type = "f_cattid";
}

var app = new Vue({
	el: "#app",
	data: {
		data: "",
		datas: "",
		inter_url: inter_url,
		inter_img: inter_img,
		id: f_catid,
		f_catidName: "",
		f_cattidName: [],
		twoindex: -1,
		iflogin: false,
		pageindex: 1,
		pagenow: "",
		pagesum: "",
		keyword:"",
		wenname:""
	},
	created: function() {
		this.ajax();
		if(getCookie('wenname')) {
			this.iflogin = false;
            this.wenname=getCookie('wenname');
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
		ajax: function() {
			var that = this;
			var id = "";

			if(type == "f_catid") {

				ajaxsGet(inter_url + 'Interface/list.ashx?catid=' + f_catid + '&page=' + that.pageindex, {

				}, function(data) {

					that.datas = data.list;
					that.pagenow = Number(data.pagenow);
					that.pagesum = Number(data.pagesum);

				})
			} else {

				ajaxsGet(inter_url + 'Interface/list.ashx?cattid=' + f_cattid + '&catid=' + f_catid + '&page=' + that.pageindex, {

				}, function(data) {

					that.datas = data.list;
                     that.pagenow = Number(data.pagenow);
					that.pagesum = Number(data.pagesum);
				})
			}

			ajaxs(inter_url + 'Interface/index.ashx ', {}, function(data) {

				that.data = data;
				
				if(typeof(that.data) == 'string' ){
					that.data = JSON.parse(data) ;
				}
				that.fe();

			})

		},
		page: function(index) {

			var index = Number(index);

			var that = this;
			var id = "";
			that.pageindex = index;

			if(type == "f_catid") {

				ajaxsGet(inter_url + 'Interface/list.ashx?catid=' + f_catid + '&page=' + that.pageindex, {

				}, function(data) {
                      
					that.datas = data.list;
					that.pagenow = Number(data.pagenow);
					that.pagesum = Number(data.pagesum);
					
				})
			} else {

				ajaxsGet(inter_url + 'Interface/list.ashx?cattid=' + f_cattid + '&catid=' + f_catid + '&page=' + that.pageindex, {

				}, function(data) {

					that.datas = data.list;
					that.pagenow = Number(data.pagenow);
					that.pagesum = Number(data.pagesum);
					

				})
			}

		},
		fe: function() {
			var that = this;
			that.data.catalog.forEach(function(t, i) {
				if(t.f_catid == f_catid) {
					that.f_catidName = t.f_onelevel;
					$("title").html(t.f_onelevel)
					$("#keywords").attr('content', t.f_onelevel);
					$("#description").attr('content', t.f_onelevel)
				}
			})

			this.data.catadetail.forEach(function(t, i) {
				if(t.f_catid == f_catid) {
					that.f_cattidName.push(t)
				}
			})

			if(type == "f_catid") {
				this.twoindex = -1;
			} else {

				this.twoindex = f_cattid;
				that.f_cattidName.forEach(function(t, i) {
					if(t.f_cattid == f_cattid) {

						$("title").html(t.f_towlevel);
						$("#keywords").attr('content', t.f_towlevel);
						$("#description").attr('content', t.f_towlevel)
					}
				})

			}
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