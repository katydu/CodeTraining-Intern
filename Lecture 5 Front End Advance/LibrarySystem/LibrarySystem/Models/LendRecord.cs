using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace LibrarySystem.Models
{
    public class LendRecord
    {
        /// <summary>
        /// 借閱日期
        /// </summary>
        [DisplayName("借閱日期")]
        public string LendDate { get; set; }

        /// <summary>
        /// 借閱人編號
        /// </summary>
        [DisplayName("借閱人員編號")]
        public string KeeperId { get; set; }

        /// <summary>
        /// 借閱人英文名字
        /// </summary>
        [DisplayName("英文姓名")]
        public string EnglishName { get; set; }

        /// <summary>
        /// 借閱人中文姓名
        /// </summary>
        [DisplayName("中文姓名")]
        public string ChineseName { get; set; }
    }
}