$(function () {
    /*** 初始化各個網頁物件及註冊事件 ***/

    /*** 彈出視窗 ***/
    $("#BookDataWindow").kendoWindow({
        width: "400px",
        //size: "auto",
        visible: false,
        resizable: false,
        draggable: false,
        modal: true,
        position: {
            left: "40%"
        }
    });
    
    /*** 視窗上的各個物件 ***/
    $("#BtnAddBook").kendoButton();
    $("#BtnUpdateBook").kendoButton();
    $("#BtnDeleteBook").kendoButton();
    $("#WindowBookName").kendoTextBox();
    $("#WindowBookAuthor").kendoTextBox();
    $("#WindowBookPublisher").kendoTextBox();
    $("#WindowBookNote").kendoTextArea({
        rows: 10,
        maxLength: 1100
    });
    $("#WindowBoughtDate").kendoDatePicker({
        format: "yyyy/MM/dd",
        dateInput: true,
        value: new Date()
    });
    //借閱人，靠id或class去區分
    MakeKendoDropDownList("#WindowBooKeeper", "/BookData/GetSearchUserTable", { "ReturnType": "WithChName" });

    /*** 查詢畫面上的各個物件 ***/
    $("#BookName").kendoAutoComplete({
        //contains是只要搜尋裡面包含就算
        filter: "contains",
        dataSource: {
            transport: {
                read: {
                    url: "/BookData/GetSearchResult",
                    dataType: "json",
                    type: "post"
                }
            }
        },
        //This is the text that will be displayed in the list of matched results
        dataTextField: "BookName",
        //widget automatically adjusts the width of the popup element and does not wrap up the item label
        autoWidth: true
    });
    //search頁面的下拉選單
    MakeKendoDropDownList(".BookClass", "/BookData/GetBookClassTable", "");
    MakeKendoDropDownList("#BooKeeper", "/BookData/GetSearchUserTable", { "ReturnType": "OnlyEngName" });
    MakeKendoDropDownList(".BookStatus", "/BookData/GetBookStatusTable", "")

    /*** 顯示查詢結果的Grid ***/
    $("#SerachGrid").kendoGrid({
        dataSource: {
            transport: {
                read: {
                    url: "/BookData/GetSearchResult",
                    dataType: "json",
                    type: "post",
                    data: function () {
                        return {
                            //將Index上的四個條件當作參數傳進去給後端做搜尋
                            //因BookName沒有設定kendo textbox所以用auto complete
                            "BookName": $("#BookName").data("kendoAutoComplete").value(),
                            "BookClass": $("#BookClass").data("kendoDropDownList").value(),
                            "BookStatus": $("#BookStatus").data("kendoDropDownList").value(),
                            "BooKeeper": $("#BooKeeper").data("kendoDropDownList").value(),
                        }
                    }
                }
            },
            //used to parse the remote service response,fields裡面是搜尋結果後的grid欄位
            schema: {
                model: {
                    fields: {
                        ClassId: { type: "string" },
                        BookName: { type: "string" },
                        BoughtDate: { type: "string" },
                        BookStatus: { type: "string" },
                        BooKeeper: { type: "string" },
                    }
                }
            },
            //一頁塞幾個內容結果
            pageSize: 20,
        },
        height: 550,
        width: 1200,
        //user could sort the grid by clicking the column header cells
        sortable: true,
        pageable: {
            //grid下方有textbox可以輸入頁碼
            input: true,
            //預設是true,下方沒有個欄位頁碼的按鈕可以選
            numeric: false
        },
        columns: [
            {
                field: "ClassId",
                title: "書籍種類",
                width: "15%"
            },
            {
                field: "BookName",
                title: "書籍名稱",
                width: "45%",
                template:
                    //書名超連結至明細
                    function (dataItem) { 
                        return '<a id = "' + dataItem.BookId + '" onclick = "ShowBookDetail(' + dataItem.BookId + ')">' + kendo.htmlEncode(dataItem.BookName) + '</a>';
                    }
            },
            {
                field: "BoughtDate",
                title: "購買日期",
                width: "15%"
            },
            {
                field: "BookStatus",
                title: "借閱狀態",
                width: "15%"
            },
            {
                field: "BooKeeper",
                title: "借閱人",
                width: "15%"
            },
            {
                command: { text: "借閱紀錄", click: ShowBookLendRecord },
                title: " ",
                width: "120px"
            },
            {
                command: { text: "編輯", click: ShowUpdateBookData },
                title: " ",
                width: "100px"
            },
            {
                command: { text: "刪除", click: DeleteBookDataInGrid },
                title: " ",
                width: "100px"
            }
        ]
    });

    /*** 各按鈕事件註冊 ***/
    MakeButtonAndBindClickEvent("#BtnSearch", function () { $("#SerachGrid").data("kendoGrid").dataSource.read() });
    MakeButtonAndBindClickEvent("#BtnClear", function () { ClearSearchCondition() });
    MakeButtonAndBindClickEvent("#BtnShowAddWindow", function () { RefreshAndOpenTheWindow("AddBook") });
    MakeButtonAndBindClickEvent("#BtnAddBook", function () { AddBookData() });
    MakeButtonAndBindClickEvent("#BtnUpdateBook", function () { UpdateBookData() });
    MakeButtonAndBindClickEvent("#BtnDeleteBook", function () { DeleteBookDataInUpdateWindow() });

    /*** 借閱紀錄之彈出視窗 ***/
    $("#LendRecordWindow").kendoWindow({
        size: "auto",
        visible: false,
        resizable: false,
        draggable: false,
        position: {
            top: "20%",
            left: "23%"
        }
    });

    /*** 新增及修改之驗證器 ***/
    $("#Validator").kendoValidator({
        rules: {
            dateRule: function (input) {
                var today = new Date();
                var inputDate = new Date(input.val());
                if (input.is("[name = BoughtDatePicker]")) {
                    return today >= inputDate;
                }
                return true;
            }
        },
        messages: {
            required: "This Field can not be empty！",
            dateRule: "This day is in the future"
        }
    });
})


/**
 * 依照傳入的ButtonId綁定傳入的點擊事件
 * @param {any} ButtonId
 * @param {any} ClickTriggerMethod
 */
function MakeButtonAndBindClickEvent(ButtonId, ClickTriggerMethod) {
    $(ButtonId).click(function () {
        ClickTriggerMethod();
    });
}

/**
 * 依照傳入的下拉選單Id以Data當作傳入參數取得Url中的選項
 * @param {any} DropDownListId
 * @param {any} Url
 * @param {any} Data
 */
function MakeKendoDropDownList(DropDownListId, Url, Data) {
    $(DropDownListId).kendoDropDownList({
        dataSource: {
            transport: {
                read: {
                    url: Url,
                    dataType: "json",
                    type: "post",
                    //傳給下一個函式(GetSearchUserTable的ReturnType)的資料
                    data: function () {
                        return Data
                    }
                }
            },
        },
        //不加下面兩行會導致下拉選單的選項呈現:object Object
        dataTextField: "Text",
        dataValueField: "Value",
        optionLabel: "請選擇..."
    });
}

/*** 取得並回傳Kendo Window上的其他物件 ***/
function GetWindowBookComponents() {
    return {
        BookName: $("#WindowBookName").data("kendoTextBox"),
        BookAuthor: $("#WindowBookAuthor").data("kendoTextBox"),
        BookPublisher: $("#WindowBookPublisher").data("kendoTextBox"),
        BookNote: $("#WindowBookNote").data("kendoTextArea"),
        BoughtDate: $("#WindowBoughtDate").data("kendoDatePicker"),
        BookClass: $("#WindowBookClass").data("kendoDropDownList"),
        BookStatus: $("#WindowBookStatus").data("kendoDropDownList"),
        BooKeeper: $("#WindowBooKeeper").data("kendoDropDownList"),
        BookId: $("#WindowBookId")
    }
}

/**
 * 刷新Kendo Window上的所有物件並依照傳入參數開啟相應的Window
 * @param {any} WindowType 可為AddBook、UpdateBook、BookDetails三種
 * 依據參數的不同來決定Window上的差異
 * LendDetail()是控制
 */
function RefreshAndOpenTheWindow(WindowType) {
    SetWindowComponentValue("");
    //ControlWindowComponentsReadStatus(false);
    switch (WindowType) {
        case "AddBook":
            ChangeWindowTitle(WindowType);
            LendDetail("Hide");
            $("#BtnAddBook").show();
            break;
        case "UpdateBook":
            ChangeWindowTitle(WindowType);
            LendDetail("Show");
            $(".UpdateButtonGroup").show();
            break;
        case "BookDetails":
            ChangeWindowTitle(WindowType);
            LendDetail("Show");
            break;
    }

    $("#Validator").data("kendoValidator").reset();
    $("#BookDataWindow").data("kendoWindow").open();
}

/**
 * 根據傳入參數決定Window上的借閱狀態及借閱人的下拉式選單如何顯示
 * @param {any} ViewType 有Show以及Hide兩種，分別用來顯示及隱藏
 */
function LendDetail(ViewType) {
    const StatusDropDownList = $("#BookStatusListItem");
    const KeeperDropDownList = $("#BooKeeperListItem");

    if (ViewType === "Show") {
        StatusDropDownList.show();
        KeeperDropDownList.show();
    } else if (ViewType === "Hide") {
        StatusDropDownList.hide();
        KeeperDropDownList.hide();
    }
}

/**
 * 依照參數判斷要給予彈出視窗欄位的值
 * @param {any} ThisBookData
 */
function SetWindowComponentValue(ThisBookData) {
    const WindowComponents = GetWindowBookComponents();
    if (ThisBookData != "") {
        WindowComponents.BookName.value(ThisBookData.BookName);
        WindowComponents.BookAuthor.value(ThisBookData.BookAuthor);
        WindowComponents.BookPublisher.value(ThisBookData.Publisher);
        WindowComponents.BookNote.value(ThisBookData.BookNote);
        WindowComponents.BoughtDate.value(new Date(ThisBookData.BoughtDate));
        WindowComponents.BookClass.value(ThisBookData.ClassCode);
        WindowComponents.BookStatus.value(ThisBookData.StatusCode);
        WindowComponents.BooKeeper.value(ThisBookData.KeeperId);
        WindowComponents.BookId.val(ThisBookData.BookId);
    } else {
        WindowComponents.BookName.value("");
        WindowComponents.BookAuthor.value("");
        WindowComponents.BookPublisher.value("");
        WindowComponents.BookNote.value("");
        WindowComponents.BoughtDate.value(new Date());
        WindowComponents.BookClass.value("");
        WindowComponents.BookStatus.value("");
        WindowComponents.BooKeeper.value("");
        WindowComponents.BookId.val("");
    }
}

/**
 * 藉由傳入參數判斷彈出視窗內的欄位是否唯讀
 * @param {any} ReadStatus 空白或false兩種
 */
function ControlWindowComponentsReadStatus(ReadStatus) {
    $.each($("input.WindowTextBox"), function (IndexInArray, ValueOfElement) {
        $(ValueOfElement).data("kendoTextBox").readonly(ReadStatus);
    });

    $("#WindowBookNote").data("kendoTextArea").readonly(ReadStatus);
    $("#WindowBoughtDate").data("kendoDatePicker").readonly(ReadStatus);

    $.each($("select.WindowDropDownList"), function (IndexInArray, ValueOfElement) {
        $(ValueOfElement).data("kendoDropDownList").readonly(ReadStatus);
    });
}

/**
 * 整合Ajax的工具
 * @param {any} Url API位置
 * @param {any} Data 傳入的資料
 * @param {any} SucceseCallBack 成功後的執行動作
 */
function AjaxTool(Url, Data, SucceseCallBack) {
    $.ajax({
        type: "POST",
        url: Url,
        data: Data,
        dataType: "json",
        success: function (Response) {
            SucceseCallBack(Response);
        }, error: function (error) {
            alert("系統發生錯誤");
        }
    });
}

/**
 * 藉由書籍狀態判斷借閱人下拉式選單是否可選，以及使用者誤用書籍狀態下拉式選單
 * 的補救措施
 * @param {any} KeeperId
 */
function BindBookStatusAndKeeper(KeeperId) {
    const BookStatus = $("#WindowBookStatus").data("kendoDropDownList");
    const BooKeeper = $("#WindowBooKeeper").data("kendoDropDownList");

    //進場判斷
    if (BookStatus.value() === "B" || BookStatus.value() === "C") {
        BooKeeper.readonly(false);
        BooKeeper.value(KeeperId.toString());
    } else {
        BooKeeper.readonly();
        BooKeeper.select(0);
    }

    //後續判斷
    BookStatus.bind("change", function (BookStatusDropDownList) {
        if (BookStatus.value() === "B" || BookStatus.value() === "C") {
            BooKeeper.readonly(false);
            BooKeeper.value(KeeperId.toString());
        } else {
            BooKeeper.readonly();
            BooKeeper.select(0);
        }
    });
}

/*** 搜尋畫面之清除按鈕註冊事件，按下後清除搜尋欄位並讀一次Search Result Grid ***/
function ClearSearchCondition() {
    $("#BookName").data("kendoAutoComplete").value("");
    $("#BookClass").data("kendoDropDownList").select(0);
    $("#BookStatus").data("kendoDropDownList").select(0);
    $("#BooKeeper").data("kendoDropDownList").select(0);
    $("#SerachGrid").data("kendoGrid").dataSource.read();
}

/*** 將Window上填入的資料帶入物件中，再以Ajax傳遞並新增書籍 ***/
function AddBookData() {
    const WindowComponents = GetWindowBookComponents();
    const AddedData = {
        "BookName": WindowComponents.BookName.value(),
        "BookAuthor": WindowComponents.BookAuthor.value(),
        "Publisher": WindowComponents.BookPublisher.value(),
        "BookNote": WindowComponents.BookNote.value(),
        "BoughtDate": kendo.toString(WindowComponents.BoughtDate.value(), "yyyy/MM/dd"),
        "ClassId": WindowComponents.BookClass.value()
    }
    const AddedWindow = $("#BookDataWindow").data("kendoWindow");
    const Grid = $("#SerachGrid").data("kendoGrid");
    const Validator = $("#Validator").data("kendoValidator");
    const BookNameAutoComplete = $("#BookName").data("kendoAutoComplete");
    debugger;
    if (Validator.validate()) {
        var AfterAdd = function (Response) {
            if (Response) {
                alert("書籍 (" + WindowComponents.BookName.value() + ") 新增成功");
                AddedWindow.close();
                Grid.dataSource.read();
                debugger;
                //我猜是要把新增的書也加進去auto complete裡
                BookNameAutoComplete.dataSource.read();
            } else {
                alert("書籍新增失敗");
            }
        }
        debugger;
        AjaxTool("/BookData/AddBook", AddedData, AfterAdd);
        debugger;
    } else {
        alert("You must fill THE EMPTY FIELD to add book data！");
    }
}

/**
 * 藉由Grid上的Command按鈕觸發，從該按鈕的Row上取得要刪除之BookId並使用Ajax進行刪除
 * @param {any} BtnDelete 觸發事件的按鈕
 */
function DeleteBookDataInGrid(BtnDelete) {
    const Grid = $("#SerachGrid").data("kendoGrid");
    const ThisTableRow = $(BtnDelete.target).closest("tr");
    const ThisBookData = Grid.dataItem(ThisTableRow);
    const BookNameAutoComplete = $("#BookName").data("kendoAutoComplete");

    if (confirm("是否刪除這本書 (" + ThisBookData.BookName + ") ?")) {
        var AfterDelete = function (Response) {
            if (Response) {
                alert("書籍 (" + ThisBookData.BookName + ") 刪除成功");
                Grid.dataSource.read();
                BookNameAutoComplete.dataSource.read();
            } else {
                alert("書籍 (" + ThisBookData.BookName + ") 出借中, 無法刪除");
            }
        }
        AjaxTool("/BookData/DeleteBookData", "BookId=" + ThisBookData.BookId.toString(), AfterDelete);
    } else {
        alert("取消刪除");
    }
}

/*** 藉由更新畫面上的刪除按鈕觸發，取得Window上書籍資料的BookId並使用Ajax進行刪除 ***/
function DeleteBookDataInUpdateWindow() {
    const BookId = $("#WindowBookId").val();
    const BookName = $("#WindowBookName").data("kendoTextBox").value();

    const Grid = $("#SerachGrid").data("kendoGrid");
    const BookDataWindow = $("#BookDataWindow").data("kendoWindow");
    const BookNameAutoComplete = $("#BookName").data("kendoAutoComplete");

    if (confirm("是否刪除這本書 (" + BookName + ") ?")) {
        var AfterDelete = function (Response) {
            if (Response) {
                alert("書籍 (" + BookName + ") 刪除成功");
                BookDataWindow.close();
                Grid.dataSource.read();
                BookNameAutoComplete.dataSource.read();
            } else {
                alert("書籍 (" + BookName + ") 出借中, 無法刪除");
            }
        }
        AjaxTool("/BookData/DeleteBookData", "BookId=" + BookId.toString(), AfterDelete);
    } else {
        alert("取消刪除");
    }
}

/**
 * 藉由Grid上的Command按鈕觸發，從該按鈕的Row上取得要更新之書籍資料並顯示於Window上
 * @param {any} BtnShowUpdateWindow 觸發事件的按鈕
 */
function ShowUpdateBookData(BtnShowUpdateWindow) {
    const Grid = $("#SerachGrid").data("kendoGrid");
    const ThisTableRow = $(BtnShowUpdateWindow.target).closest("tr");
    const ThisBookData = Grid.dataItem(ThisTableRow);
    RefreshAndOpenTheWindow("UpdateBook");
    SetWindowComponentValue(ThisBookData);
    BindBookStatusAndKeeper(ThisBookData.KeeperId);
}

/*** 將Window上已經/未經修改的資料帶入物件中，再以Ajax傳遞並更新書籍 ***/
/*** 是否增加書籍借閱紀錄則在後台判斷 ***/
function UpdateBookData() {
    const WindowComponents = GetWindowBookComponents();
    const UpdatedData = {
        "BookName": WindowComponents.BookName.value(),
        "BookAuthor": WindowComponents.BookAuthor.value(),
        "Publisher": WindowComponents.BookPublisher.value(),
        "BookNote": WindowComponents.BookNote.value(),
        "BoughtDate": kendo.toString(WindowComponents.BoughtDate.value(), "yyyy/MM/dd"),
        "ClassId": WindowComponents.BookClass.value(),
        "BookStatus": WindowComponents.BookStatus.value(),
        "BooKeeper": WindowComponents.BooKeeper.value(),
        "BookId": WindowComponents.BookId.val()
    }
    const UpdateWindow = $("#BookDataWindow").data("kendoWindow");
    const Grid = $("#SerachGrid").data("kendoGrid");
    const Validator = $("#Validator").data("kendoValidator");
    const BookNameAutoComplete = $("#BookName").data("kendoAutoComplete");

    if (Validator.validate()) {
        var AfterUpdate = function (Response) {
            if (Response) {
                alert("書籍 (" + WindowComponents.BookName.value() + ") 更新成功");
                UpdateWindow.close();
                Grid.dataSource.read();
                BookNameAutoComplete.dataSource.read();
            } else {
                alert("書籍 (" + WindowComponents.BookName.value() + ") 更新失敗");
            }
        }
        AjaxTool("/BookData/UpdateBook", UpdatedData, AfterUpdate);
    } else {
        alert("You must fill THE EMPTY FIELD to update book data！");
    }
}

/**
 * 藉由Grid上的Command按鈕觸發，從該按鈕的Row上取得要顯示借閱紀錄之BookId並顯示於Window裡的Grid上
 * @param {any} BtnShowBookLendRecord 觸發事件之按鈕
 */
function ShowBookLendRecord(BtnShowBookLendRecord) {
    const Grid = $("#SerachGrid").data("kendoGrid");
    const ThisTableRow = $(BtnShowBookLendRecord.target).closest("tr");
    const ThisBookData = Grid.dataItem(ThisTableRow);

    $("#LendRecordGrid").kendoGrid({
        dataSource: {
            transport: {
                read: {
                    url: "/BookData/LendRecord",
                    dataType: "json",
                    type: "post",
                    data: function () {
                        return {
                            "BookId": ThisBookData.BookId.toString()
                        }
                    }
                }
            },
            schema: {
                model: {
                    fields: {
                        LendDate: { type: "string" },
                        KeeperId: { type: "string" },
                        EnglishName: { type: "string" },
                        ChineseName: { type: "string" }
                    }
                }
            },
            pageSize: 20,
        },
        height: 550,
        width: 1000,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            {
                field: "LendDate",
                title: "借閱日期",
                width: "25%"
            },
            {
                field: "KeeperId",
                title: "借閱人員編號",
                width: "25%"
            },
            {
                field: "EnglishName",
                title: "英文姓名",
                width: "25%"
            },
            {
                field: "ChineseName",
                title: "中文姓名",
                width: "25%"
            }
        ]
    });

    $("#LendRecordWindow").data("kendoWindow").open().center();
}

/**
 * 藉由Grid上的書名Field Templete觸發，
 * 從該超連結標籤之id取得要顯示的書籍明細之資料並顯示於Window上
 * @param {any} DetailsBookId 超連結標籤的id
 */
function ShowBookDetail(DetailsBookId) {

    const Grid = $("#SerachGrid").data("kendoGrid");
    const ThisTableRow = $("#" + DetailsBookId).closest("tr");
    const ThisBookData = Grid.dataItem(ThisTableRow);

    RefreshAndOpenTheWindow("BookDetails");
    //判斷彈出視窗內的欄位是否唯讀
    ControlWindowComponentsReadStatus();
    //將明細內容填進去
    SetWindowComponentValue(ThisBookData);
}

//更改各window的標題字
function ChangeWindowTitle(WindowType) {
    var BookDataWindow = $("#BookDataWindow").data("kendoWindow");
    switch (WindowType) {
        case "AddBook":
            BookDataWindow.title("新增書籍");
            break;
        case "UpdateBook":
            BookDataWindow.title("更新書籍");
            break;
        case "BookDetails":
            BookDataWindow.title("書本明細");
            break;
    }
}