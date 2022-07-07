using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//才可以使用display name
using System.ComponentModel;
//才可以使用required
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class BookData
    {
        /// <summary>
        /// 書本編號
        /// </summary>
        [DisplayName("書本編號")]
        public int BookId { get; set; }

        /// <summary>
        /// 書籍名稱
        /// </summary>
        [DisplayName("書名")]
        [Required(ErrorMessage = "此欄位必填")]
        public string BookName { get; set; }

        /// <summary>
        /// 書本作者
        /// </summary>
        [DisplayName("作者")]
        [Required(ErrorMessage = "此欄位必填")]
        public string BookAuthor { get; set; }

        /// <summary>
        /// 書籍出版商
        /// </summary>
        [DisplayName("出版商")]
        [Required(ErrorMessage = "此欄位必填")]
        public string Publisher { get; set; }

        /// <summary>
        /// 書籍內容簡介
        /// </summary>
        [DisplayName("內容簡介")]

        [Required(ErrorMessage = "此欄位必填")]
        [MaxLength(1100,ErrorMessage ="長度需小於1100個字")]
        public string BookNote { get; set; }

        /// <summary>
        /// 書本購買日期
        /// </summary>
        [DisplayName("購書日期(yyyy/MM/dd)")]
        [Required(ErrorMessage = "此欄位必填(yyyy/MM/dd)")]
        [RegularExpression(@"^(19|20)[0-9]{2}[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "請輸入正確的日期格式")]
        public string BoughtDate { get; set; }

        /// <summary>
        /// 書籍分類
        /// </summary>
        [DisplayName("圖書類別")]
        [Required(ErrorMessage = "此欄位必填")]
        public string ClassId { get; set; }

        /// <summary>
        /// 書本借閱狀態
        /// </summary>
        [DisplayName("借閱狀態")]
        [Required(ErrorMessage = "此欄位必填")]
        public string BookStatus { get; set; }

        /// <summary>
        /// 書籍借閱人
        /// </summary>
        [DisplayName("借閱人")]
        [Required(ErrorMessage = "請選擇借閱人")]
        public string BooKeeper { get; set; }

        /// <summary>
        /// 書籍購買金額
        /// </summary>
        /// 要這個因為新增的時候資料庫有這項欄位，雖然沒有要求使用者填
        [DisplayName("購買金額")]
        public int BookAmount { get; set; }

        /// <summary>
        /// 此筆紀錄之建立日期
        /// </summary>
        [DisplayName("紀錄建立日期")]
        public string CreateDate { get; set; }

        /// <summary>
        /// 此筆紀錄之建立者
        /// </summary>
        [DisplayName("紀錄建立人")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 此筆紀錄之修改日期
        /// </summary>
        [DisplayName("紀錄修改日期")]
        public string ModifyDate { get; set; }

        /// <summary>
        /// 此筆紀錄之修改者
        /// </summary>
        [DisplayName("紀錄修改人")]
        public string ModifyUser { get; set; }
    }
}
