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
        /// <returns></returns>
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
        /// 新增書籍頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult AddBook()
        {
            ViewBag.BookClasses = this.CodeService.GetClassTable();
            //return "AddBook"或空的也正常顯示
            return View(new Models.BookData());
        }

        /// <summary>
        /// 新增Data到資料庫後清空頁面
        /// </summary>
        /// <param name="Data">新增之書籍資料</param>
        /// <returns></returns>
        /// [缺]沒有考慮重複輸入相同書籍的判斷
        [HttpPost()]
        public ActionResult AddBook(Models.BookData Data)
        {
            ViewBag.BookClasses = this.CodeService.GetClassTable();

            if (ModelState.IsValid)
            {
                this.BookService.AddBook(Data);
                TempData["message"] = "書籍 (" + Data.BookName + ") 新增成功";
                ModelState.Clear();
            }

            return View();
        }
        /// <summary>
        /// 更新頁面
        /// 使用ViewType辨識更新或明細並使用BookId獲得要更新的書籍資料
        /// 並使用Keeper變數儲存借閱人用以辨識借閱人是否有變動
        /// </summary>
        /// <param name="BookId">要檢視之書本的BookId</param>
        /// <param name="ViewType">檢視型態/更新or明細</param>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult UpdateBook(int BookId, string ViewType)
        {
            ViewBag.BookClasses = this.CodeService.GetClassTable();
            ViewBag.Borrowers = this.CodeService.GetUserTable("W");
            ViewBag.BookStatuses = this.CodeService.GetCodeTable();

            if (ViewType == "Update")
            {
                //上傳狀態只供檢視關閉
                ViewData["ReadOnlyOrNot"] = false;
            }
            else if (ViewType == "Detail")
            {
                ViewData["ReadOnlyOrNot"] = true;
            }

            Models.BookData UpdateBookData = this.BookService.GetUpdateBookData(BookId);

            return View(UpdateBookData);
        }

        /// <summary>
        /// 更新後頁面，判斷借閱人是否變動及借閱狀態
        /// 來決定是否新增借閱紀錄
        /// </summary>
        /// <param name="UpdateBookData">要更新的書籍資料</param>
        /// <returns></returns>
        [HttpPost()]
        public ActionResult UpdateBook(Models.BookData UpdateBookData)
        {
            ViewBag.BookClasses = this.CodeService.GetClassTable();
            ViewBag.Borrowers = this.CodeService.GetUserTable("W");
            ViewBag.BookStatuses = this.CodeService.GetCodeTable();
            ViewData["ReadOnlyOrNot"] = false;
            Models.BookData PreviousBookData = this.BookService.GetUpdateBookData(UpdateBookData.BookId);
            DateTime today = DateTime.Now;
            DateTime boughtDay = DateTime.Parse(UpdateBookData.BoughtDate);
            int n = today.CompareTo(boughtDay);
            //比對更新時的購買日期是否正確
            if (n < 0)
            {
                TempData["message"] = "購書日期在未來喔";
                return View(UpdateBookData);
            }
            else if (ModelState.IsValid||(UpdateBookData.BookStatus == "A") || (UpdateBookData.BookStatus == "U"))
            {
                //不懂
                if (PreviousBookData.BooKeeper != (UpdateBookData.BooKeeper == null ? string.Empty : UpdateBookData.BooKeeper))
                {
                    //B:已借出C:已借出(未領)U:不可借出A:可借出
                    if ((UpdateBookData.BookStatus == "B") || (UpdateBookData.BookStatus == "C"))
                    {
                        //若書籍被借出要加上借閱紀錄
                        this.BookService.AddLendRecord(UpdateBookData);
                    }
                }
                this.BookService.UpdateBook(UpdateBookData);
                TempData["message"] = "書籍 (" + UpdateBookData.BookName + ") 更新成功";
                ModelState.Clear();

                return RedirectToAction("Index");
            }
            //驗證沒過就會繼續留在更新畫面
            return View(UpdateBookData);
        }
        /// <summary>
        /// 使用BooKId來決定刪除哪一本書
        /// 如果是已借出的不可刪除
        /// </summary>
        /// <param name="BookId">要刪除的BookId</param>
        /// <returns></returns>
        [HttpPost()]
        public JsonResult DeleteBook(string BookId)
        {
            Models.BookData ThisBook = this.BookService.GetUpdateBookData(Convert.ToInt32(BookId));
            //B:已借出C:已借出(未領)U:不可借出A:可借出
            //借出中不可刪除
            if (ThisBook.BooKeeper != "" && ((ThisBook.BookStatus == "B") || (ThisBook.BookStatus == "C")))
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
        /// <returns></returns>
        [HttpGet()]
        public ActionResult LendRecord(int BookId)
        {
            ViewBag.LendRecord = this.BookService.GetLendRecord(BookId);
            return View();
        }
    }
}