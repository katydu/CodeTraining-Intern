using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//要加下面兩個引用
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class BookSearch
    {
        /// <summary>
        /// 作為搜尋條件的書名
        /// </summary>
        [DisplayName("書名")]
        public string BookName { get; set; }

        /// <summary>
        /// 作為搜尋條件的圖書類別
        /// </summary>
        [DisplayName("圖書類別")]
        public string BookClass { get; set; }

        /// <summary>
        /// 作為搜尋條件的借閱人
        /// </summary>
        [DisplayName("借閱人")]
        public string BooKeeper { get; set; }

        /// <summary>
        /// 作為搜尋條件的借閱狀態
        /// </summary>
        [DisplayName("借閱狀態")]
        public string BookStatus { get; set; }

    }
}