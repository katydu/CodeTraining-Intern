using System;
using System.Web.Mvc;

namespace LibrarySystem.Controllers
{
    public class BookDataController : Controller
    {
        /// <summary>
        /// 基礎畫面顯示以及書籍相關之服務
        /// </summary>
        readonly Models.CodeService CodeService = new Models.CodeService();
        readonly Models.BookService BookService = new Models.BookService();

        /// <summary>
        /// 查詢頁面
        /// </summary>
        /// <returns>Index.cshtml</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 取得圖書類別之資料
        /// </summary>
        /// <returns>以Json格式將List(類別名稱、類別代號)回傳至Kendo DropDownList</returns>
        [HttpPost]
        public JsonResult GetBookClassTable()
        {
            return this.Json(CodeService.GetClassTable());
        }

        /// <summary>
        /// 藉由傳入參數決定如何顯示使用者
        /// </summary>
        /// <param name="ReturnType">有WithChName以及OnlyEngName兩種，
        /// 分別是帶有中文姓名或只有英文姓名</param>
        /// <returns>以Json格式將List(英文姓名、使用者編號或英-中文姓名、使用者編號)
        /// 回傳至Kendo DropDownList</returns>
        [HttpPost]
        public JsonResult GetSearchUserTable(string ReturnType)
        {
            return this.Json(CodeService.GetUserTable(ReturnType));
        }

        /// <summary>
        /// 取得借閱狀態之資料
        /// </summary>
        /// <returns>以Json格式將List(借閱狀態名稱、借閱狀態代號)回傳至Kendo DropDownList</returns>
        [HttpPost]
        public JsonResult GetBookStatusTable()
        {
            return this.Json(CodeService.GetCodeTable());
        }

        /// <summary>
        /// 藉由傳入的物件(查詢條件)來進行書籍資料的查詢
        /// </summary>
        /// <param name="SearchCondition">查詢畫面上的各個條件
        /// (書名、圖書類別、借閱人、借閱狀態)所組成的物件</param>
        /// <returns>以Json格式將查詢結果(圖書類別、書名、購書日期、借閱狀態、借閱人以及其他資料)
        /// 回傳至Kendo Grid上</returns>
        [HttpPost]
        public JsonResult GetSearchResult(Models.BookSearch SearchCondition)
        {
            return this.Json(BookService.GetSearchBookData(SearchCondition));
        }

        /// <summary>
        /// 接收前端傳入的物件作為參數並新增該筆資料
        /// </summary>
        /// <param name="AddedBookData">由書名、作者、出版商、內容簡介、購書日期、書籍種類
        /// 所建立的物件</param>
        /// <returns>用try...catch檢查錯誤並
        /// 以Json的格式回傳true或false來告知前端是否完成新增</returns>
        [HttpPost]
        public JsonResult AddBook(Models.BookData AddedBookData)
        {
            try
            {
                this.BookService.AddBook(AddedBookData);
            }
            catch
            {
                return this.Json(false);
            }
            return this.Json(true);
        }

        /// <summary>
        /// 接收前端傳入的物件作為參數並編輯該筆資料，並藉由是否變動借閱人以及編輯後借閱狀態
        /// 判斷是否新增借閱紀錄
        /// </summary>
        /// <param name="UpdateBookData">由書名、作者、出版商、內容簡介、購書日期、書籍種類
        /// 、借閱狀態、借閱人所建立的物件</param>
        /// <returns>用try...catch檢查錯誤並
        /// 以Json的格式回傳true或false來告知前端是否完成更新</returns>
        [HttpPost]
        public JsonResult UpdateBook(Models.BookData UpdateBookData)
        {
            //TODO: 透過回傳Json物件來判斷狀態和傳送對應的訊息
            try
            {
                Models.BookData PreviousBookData = this.BookService.GetUpdateBookData(UpdateBookData.BookId);
                if (PreviousBookData.BooKeeper != (UpdateBookData.BooKeeper == null ? string.Empty : UpdateBookData.BooKeeper))
                {
                    if ((UpdateBookData.BookStatus == "B") || (UpdateBookData.BookStatus == "C"))
                    {
                        this.BookService.AddLendRecord(UpdateBookData);
                    }
                }

                this.BookService.UpdateBook(UpdateBookData);
            }
            catch
            {
                return this.Json(false);
            }
            return this.Json(true);
        }

        /// <summary>
        /// 使用BooKId來決定刪除哪一本書
        /// 如果是已借出的不可刪除
        /// </summary>
        /// <param name="BookId">要刪除的BookId</param>
        /// <returns>如果已經借出則以Json格式回傳false
        /// 如果尚未出借，就刪除該本書並且以Json格式回傳true</returns>
        [HttpPost()]
        public JsonResult DeleteBookData(string BookId)
        {
            Models.BookData DeleteData = this.BookService.GetUpdateBookData(Convert.ToInt32(BookId));

            if (DeleteData.BooKeeper != "" && ((DeleteData.BookStatus == "B") || (DeleteData.BookStatus == "C")))
            {
                return this.Json(false);
            }
            else
            {
                this.BookService.DeleteBookData(BookId);
                return this.Json(true);
            }
        }

        /// <summary>
        /// 使用BookId取得該本書的借閱紀錄
        /// </summary>
        /// <param name="BookId">要查詢借閱紀錄的BookId</param>
        /// <returns>以Json格式將查詢結果(借閱日期、借閱人員編號、英文姓名、中文姓名)
        /// 回傳至Kendo Grid上</returns>
        [HttpPost()]
        public JsonResult LendRecord(string BookId)
        {
            return this.Json(BookService.GetLendRecord(Convert.ToInt32(BookId)));
        }
    }
}