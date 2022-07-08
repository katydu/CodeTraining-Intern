
var bookDataFromLocalStorage = [];

$(function(){
    loadBookData();
    var data = [
        {text:"資料庫",value:"image/database.jpg"},
        {text:"網際網路",value:"image/internet.jpg"},
        {text:"應用系統整合",value:"image/system.jpg"},
        {text:"家庭保健",value:"image/home.jpg"},
        {text:"語言",value:"image/language.jpg"}
    ]

    
    $("#book_category").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        dataSource: data,
        index: 0,
        change: changeBookImg
    });
    //驗證date
    $("#addBookForm").kendoValidator({
        rules: {
            dateRule1: function(input){
                var pattern = /[a-zA-Z]/g;
                //判斷是否有包含字串，去驗證dateInput，要加上指定是datepicker不然書籍目錄也會被檢查到，因為他也在addBookForm底下
                if(input.is("[name = datePicker]")&&pattern.test(input.val())==true){
                    return false;//包含字串就報錯
                }
                return true;
            },
            dateRule2:  function(input){
                var today = new Date();
                var inputDate = new Date(input.val());
                if(input.is("[name = datePicker]")){
                    return today >= inputDate;
                }
                return true;
            },
        },
        messages: {
            required: "This Field can not be empty！",
            dateRule1:" You have some Fields need to enter",
            dateRule2: "This day is in the future"
        }
    });

    $("#bought_datepicker").kendoDatePicker({
        format: "yyyy-MM-dd",
        dateInput: true,//會去限制user輸入日期
        value: new Date()
    });

    $("#book_grid").kendoGrid({
        dataSource: {
            data: bookDataFromLocalStorage,
            schema: {
                model: {
                    fields: {
                        BookId: {type:"int"},
                        BookName: { type: "string" },
                        BookCategory: { type: "string" },
                        BookAuthor: { type: "string" },
                        BookBoughtDate: { type: "string" }
                    }
                }
            },
            pageSize: 20,
        },
        toolbar: kendo.template("<div class='book-grid-toolbar'><input class='book-grid-search' placeholder='我想要找......' type='text'></input></div><select id='searchType'></select><button class='k-button k-primary'>搜尋</button>"),
        height: 550,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            { field: "BookId", title: "書籍編號",width:"10%"},
            { field: "BookName", title: "書籍名稱", width: "50%" },
            { field: "BookCategory", title: "書籍種類", width: "10%" },
            { field: "BookAuthor", title: "作者", width: "15%" },
            { field: "BookBoughtDate", title: "購買日期", width: "15%" },
            { command: { text: "刪除", click: deleteBook }, title: " ", width: "120px" }
        ]
        
    });

    $("#searchType").kendoDropDownList({
        dataSource: 
        [
            {text: "書籍編號", value: "BookId"},
            {text: "書籍名稱", value: "BookName"},
            {text: "書籍種類", value: "BookCategory"},
            {text: "作者", value: "BookAuthor"},
            {text: "購買日期", value: "BookBoughtDate"},
        ],
        dataTextField: "text",
        dataValueField: "value",
        change: changeSearchTypeWithContent
    });

    //從keyup()改成用input就可以不用離開searchbox就可以做到像autocomplete的效果
    $(".book-grid-search").on("input",function(){
        changeSearchTypeWithContent();
    })

    //kendo window
    var myWindow = $("#fieldlist");
    debugger;

    $("#open_window").click(function() {
        myWindow.data("kendoWindow").open();
    });

    $("#add_book_btn").click(function(e) {
        addBook(e);
    });

    myWindow.kendoWindow({
        width: "400px",
        title: "新增書籍",
        visible: false,
        draggable: false,
        resizable: false,
        modal: true,//背景不能點
        actions: [//window右上角的縮小放大關閉功能
            "Pin",
            //"Minimize",
            //"Maximize",
            "Close"
        ],
        close:refreshColumn
    }).data("kendoWindow").center();

    
})
//清除欄位內容
//注意不用加上close()會關不掉
function refreshColumn(){
    $("#book_category").data("kendoDropDownList").select(0);
    $("#book_name").val("");
    $("#book_author").val("");
    $("#bought_datepicker").data("kendoDatePicker").value(new Date());
    $("#book_publisher").val("");
    changeBookImg() //更換圖書類別為預設值也要一起更改圖片
}
function loadBookData(){
    bookDataFromLocalStorage = JSON.parse(localStorage.getItem("bookData"));
    if(bookDataFromLocalStorage == null){
        bookDataFromLocalStorage = bookData;
        //序列化
        localStorage.setItem("bookData",JSON.stringify(bookDataFromLocalStorage));//localStorage更新
    }
}

//刪除書籍
function deleteBook(e){
    e.preventDefault();
    //e.target是使用者按的那顆刪除按鈕
    //closest是去抓取離按鈕最近的tr
    //tr是列td是欄
    var tr = $(e.target).closest("tr"),
        grid = $("#book_grid").data("kendoGrid"),//將book_grid的資料存進grid
        dataItem = grid.dataItem(tr);//依照tr去抓取grid裡的那一列資料
    //用confirm去確認使用者是否真的要刪除資料
    var yes = confirm('Are you sure you want to delete'+dataItem.BookName+'?');
    if (yes) {//yes改用!return代替(?)
        //程式繼續跑
        grid.dataSource.remove(dataItem);//刪除grid之dataSource
        localStorage.setItem("bookData", JSON.stringify(grid.dataSource.data()));//localStorage更新
    } else {
        return;
    }
}

//變換圖片
function changeBookImg() {
    //先抓到下拉式選單
    var dropdownlist = $("#book_category").data("kendoDropDownList");
    var num = dropdownlist.value();//data裡的value
    $(".book-image").attr("src", num);
}

//新增書籍
function addBook(e) {
    e.preventDefault();//避免kendo做的超連結(刪除)按鈕的#連結顯示在網址
    var book_name = $('#book_name').val();
    var book_category = $('#book_category').data("kendoDropDownList").text();//用value比較好
    var book_author = $('#book_author').val();
    var book_date = $('#bought_datepicker').data("kendoDatePicker").value();
    var boughtDay = kendo.toString(book_date, 'yyyy-MM-dd');
    var book_publish = $('#book_publisher').val();

    var max = 0;
    //找book_id的最大值
    var max = Math.max.apply(Math, bookDataFromLocalStorage.map(function (o) { return o.BookId; }));

    //新增加的值    
    var new_data = {
        "BookId": max + 1,//下一個id值為目前資料id最大值+1
        "BookCategory": book_category,
        "BookName": book_name,
        "BookAuthor": book_author,
        "BookBoughtDate": boughtDay,
        "BookPublisher": book_publish
    };
    //第一行程式有宣告bookDataFromLocalStorage,直接用它當作localstorage目前之資料,不用再新增一個陣列變數"book"
    const grid = $("#book_grid").data("kendoGrid");
    const window = $("#fieldlist").data("kendoWindow");
    const validator = $("#addBookForm").data("kendoValidator");

    if(validator.validate()){
        grid.dataSource.add(new_data);
        bookDataFromLocalStorage.push(new_data);
        localStorage.setItem("bookData", JSON.stringify(bookDataFromLocalStorage));
        window.close();
    }else{
        alert("You must fill THE EMPTY FIELD to add book data！");
    }
}

//搜尋
function changeSearchTypeWithContent(){

    const inputValue = $(".book-grid-search").val();
    const grid = $("#book_grid").data("kendoGrid");
    //新增在右邊的下拉式選單為searchType
    const searchType = $("#searchType").data("kendoDropDownList").value();

    if (inputValue) {
        grid.dataSource.filter({field: searchType, operator: "contains", value: inputValue});
    } else {
        grid.dataSource.filter({});
    }
}