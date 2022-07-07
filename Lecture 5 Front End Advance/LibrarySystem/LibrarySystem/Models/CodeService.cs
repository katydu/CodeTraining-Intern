using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//才可以用SelectListItem
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace LibrarySystem.Models
{
    public class CodeService
    {
        /// <summary>
        /// 取得資料庫連線字串
        /// </summary>
        /// <returns></returns>
        private string GetDBConnectionString()
        {
            return
                System.Configuration.ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString.ToString();
        }

        /// <summary>
        /// 取得書籍名稱及代號
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetClassTable()
        {
            DataTable DataTable = new DataTable();
            string sql = @"SELECT BOOK_CLASS_ID AS CodeId, BOOK_CLASS_NAME AS CodeName
                        FROM BOOK_CLASS";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(DataTable);
                conn.Close();
            }
            return this.MapCodeData(DataTable);
        }

        /// <summary>
        /// 將書籍之名稱及代號從DataTable加到List中
        /// </summary>
        /// <param name="ClassTable"></param>
        /// <returns></returns>
        private List<SelectListItem> MapCodeData(DataTable DataTable)
        {
            List<SelectListItem> Codes = new List<SelectListItem>();
            foreach (DataRow row in DataTable.Rows)
            {
                Codes.Add(new SelectListItem()
                {
                    //用別稱
                    Text = row["CodeName"].ToString(),
                    Value = row["CodeId"].ToString()
                });
            }
            return Codes;
        }

        /// <summary>
        /// 取得所有使用者的編號、中文姓名、英文姓名
        /// </summary>
        /// <param name="ReturnType">O代表Only，W代表With</param>
        /// <returns>回傳是否只有英文姓名或帶有中文姓名</returns>
        public List<SelectListItem> GetUserTable(string ReturnType)
        {
            DataTable BorrowerTable = new DataTable();
            string sql = @"SELECT USER_ID AS Id, USER_CNAME AS Cname, USER_ENAME AS EName
                        FROM MEMBER_M
                        ORDER BY Ename";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(BorrowerTable);
                conn.Close();
            }
            return this.MapUserData(BorrowerTable, ReturnType);
        }

        /// <summary>
        /// 將使用者編號、中英文姓名從DataTable加到List中
        /// </summary>
        /// <param name="ClassTable"></param>
        /// <param name="ReturnType"></param>
        /// <returns></returns>
        private List<SelectListItem> MapUserData(DataTable ClassTable, string ReturnType)
        {
            List<SelectListItem> Borrowers = new List<SelectListItem>();

            foreach (DataRow row in ClassTable.Rows)
            {

                if (ReturnType == "W")
                {
                    Borrowers.Add(new SelectListItem()
                    {
                        Text = row["EName"].ToString() + "-" + row["CName"].ToString(),
                        Value = row["Id"].ToString()
                    });
                }
                else if (ReturnType == "O")
                {
                    Borrowers.Add(new SelectListItem()
                    {
                        Text = row["EName"].ToString(),
                        Value = row["Id"].ToString()
                    });
                }
            }
            return Borrowers;
        }

        /// <summary>
        /// 取得書籍狀態名稱及代號
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> GetCodeTable()
        {
            DataTable DataTable = new DataTable();
            string sql = @"SELECT CODE_ID AS CodeId, CODE_NAME AS CodeName
                        FROM BOOK_CODE
                        WHERE CODE_TYPE = 'BOOK_STATUS'";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(DataTable);
                conn.Close();
            }
            return this.MapCodeData(DataTable);
        }
    }
}