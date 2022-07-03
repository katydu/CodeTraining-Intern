using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibrarySystem.Controllers
{
    public class BookDataController : Controller
    {
        readonly Models.CodeService CodeService = new Models.CodeService();
        readonly Models.BookService BookService = new Models.BookService();

        /// <summary>
        /// 查詢頁面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //書籍類別
            ViewBag.BookClasses = this.CodeService.GetClassTable();
            //會員
            ViewBag.Borrowers = this.CodeService.GetUserTable("O");
            //借閱狀態
            ViewBag.BookStatuses = this.CodeService.GetCodeTable();
            //return "Index"或不放也都會正常顯示
            return View(new Models.BookSearch());
        }

        /// <summary>
        /// 使用條件查詢後頁面
        /// </summary>
        /// <param name="condition">搜尋條件</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(Models.BookSearch condition)
        {
            ViewBag.BookClasses = this.CodeService.GetClassTable();
            ViewBag.Borrowers = this.CodeService.GetUserTable("O");
            ViewBag.BookStatuses = this.CodeService.GetCodeTable();
            ViewBag.SearchResult = this.BookService.GetSearchBookData(condition);
            //ModelState的值有最高的顯示優先權，而透過ModelState.Clear()將值清空之後，改以第二順位的Action指定的Model值來做顯示
            ModelState.Clear();

            return View("Index");
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
            if (ModelState.IsValid||(UpdateBookData.BookStatus == "A") || (UpdateBookData.BookStatus == "U"))
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