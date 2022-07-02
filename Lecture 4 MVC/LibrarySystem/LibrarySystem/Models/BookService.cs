using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//才可以用data table
using System.Data;
//才可以用資料庫相關連線字串
using System.Data.SqlClient;

namespace LibrarySystem.Models
{
    public class BookService
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
        /// 使用條件搜尋書籍資料
        /// </summary>
        /// <param name="Condition"></param>
        /// <returns></returns>
        /// Condition內容為吃前端使用者搜尋的內容去對到BookSearch Model裡的資料
        public List<Models.BookData> GetSearchBookData(Models.BookSearch Condition)
        {

            DataTable dt = new DataTable();
            //left join因為不是每個會員都有借書，要以書本為主
            string sql = @"
                            SELECT
                            	Class.BOOK_CLASS_NAME AS '圖書類別',
                            	Data.BOOK_ID AS '書本編號',
                            	DATA.BOOK_NAME AS '書名',
                            	Data.BOOK_NOTE AS '內容簡介',
                            	FORMAT(DATA.BOOK_BOUGHT_DATE, 'yyyy/MM/dd') AS '購書日期',
                            	Data.BOOK_PUBLISHER AS '出版商',
                            	Code.CODE_NAME AS '借閱狀態',
                            	Member.USER_ENAME AS '借閱人',
                            	Data.BOOK_AUTHOR AS '作者',
                            	Data.CREATE_DATE AS '紀錄建立日期',
                            	Data.CREATE_USER AS '紀錄建立人',
                            	Data.MODIFY_DATE AS '紀錄修改日期',
                            	Data.MODIFY_USER AS '紀錄修改人'
                            FROM BOOK_DATA AS Data
                            INNER JOIN BOOK_CLASS AS Class
                            	ON Data.BOOK_CLASS_ID = Class.BOOK_CLASS_ID
                            LEFT JOIN MEMBER_M AS Member
                            	ON Data.BOOK_KEEPER = Member.USER_ID
                            INNER JOIN BOOK_CODE AS Code
                            	ON Data.BOOK_STATUS = Code.CODE_ID
                            	AND Code.CODE_TYPE = 'BOOK_STATUS'
                            WHERE
                            	(Data.BOOK_CLASS_ID = @BookClass OR @BookClass = '') AND
                            	(Data.BOOK_NAME LIKE '%' + @BookName + '%' OR @BookName = '') AND
                            	(Data.BOOK_STATUS = @BookStatus OR @BookStatus = '') AND
                            	(Data.BOOK_KEEPER = @BooKeeper OR @BooKeeper = '')
                            ORDER BY '圖書類別', '購書日期' DESC, '借閱狀態', '書名'";
            //using拿掉也可以正常跑
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                //上方sql語法有用到的變數都要在下面新增進去
                //A?B:C;=>A true則B false則C
                cmd.Parameters.Add(new SqlParameter("@BookClass", Condition.BookClass == null ? string.Empty : Condition.BookClass));
                cmd.Parameters.Add(new SqlParameter("@BookName", Condition.BookName == null ? string.Empty : Condition.BookName));
                cmd.Parameters.Add(new SqlParameter("@BookStatus", Condition.BookStatus == null ? string.Empty : Condition.BookStatus));
                cmd.Parameters.Add(new SqlParameter("@BooKeeper", Condition.BooKeeper == null ? string.Empty : Condition.BooKeeper));
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            return this.MapBookDataToList(dt);
        }
        /// <summary>
        /// 將查詢到的書籍資料從DataTable加到List中
        /// </summary>
        /// <param name="BookData"></param>
        /// <returns></returns>
        private List<Models.BookData> MapBookDataToList(DataTable BookData)
        {
            List<Models.BookData> SearchResult = new List<BookData>();
            foreach (DataRow row in BookData.Rows)
            {
                SearchResult.Add(new BookData()
                {
                    ClassId = row["圖書類別"].ToString(),
                    BookId = Convert.ToInt32(row["書本編號"].ToString()),
                    BookName = row["書名"].ToString(),
                    BookNote = row["內容簡介"].ToString(),
                    BoughtDate = row["購書日期"].ToString(),
                    Publisher = row["出版商"].ToString(),
                    BookStatus = row["借閱狀態"].ToString(),
                    BooKeeper = row["借閱人"].ToString(),
                    BookAuthor = row["作者"].ToString(),
                    //CreateDate = row["紀錄建立日期"].ToString(),
                    //CreateUser = row["紀錄建立人"].ToString(),
                    //ModifyDate = row["紀錄修改日期"].ToString(),
                    //ModifyUser = row["紀錄修改人"].ToString()
                });
            }
            return SearchResult;
        }

        /// <summary>
        /// 將Data加入到資料庫中
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public void AddBook(Models.BookData Data)
        {
            string sql = @" INSERT INTO BOOK_DATA (
	                            BOOK_NAME, 
	                            BOOK_CLASS_ID, 
	                            BOOK_AUTHOR, 
	                            BOOK_BOUGHT_DATE, 
	                            BOOK_PUBLISHER, 
	                            BOOK_NOTE, 
	                            BOOK_STATUS, 
	                            BOOK_KEEPER, 
	                            BOOK_AMOUNT, 
	                            CREATE_DATE, 
	                            CREATE_USER, 
	                            MODIFY_DATE, 
	                            MODIFY_USER)
                            VALUES (
	                            @BookName, 
	                            @ClassId, 
	                            @Author, 
	                            @BoughtDate, 
	                            @Publisher, 
	                            @BookNote, 
	                            @BookStatus, 
	                            @BooKeeper, 
	                            @BoughtFee, 
	                            @CreateDate, 
	                            @CreateUser, 
	                            @ModifyDate, 
	                            @ModifyUser);";

            DateTime Today = DateTime.Now;
            
            string User = "0919";

            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookName", Data.BookName));
                cmd.Parameters.Add(new SqlParameter("@ClassId", Data.ClassId));
                cmd.Parameters.Add(new SqlParameter("@Author", Data.BookAuthor));
                cmd.Parameters.Add(new SqlParameter("@BoughtDate", Convert.ToDateTime(Data.BoughtDate)));
                cmd.Parameters.Add(new SqlParameter("@Publisher", Data.Publisher));
                cmd.Parameters.Add(new SqlParameter("@BookNote", Data.BookNote));
                cmd.Parameters.Add(new SqlParameter("@BookStatus", "A"));
                cmd.Parameters.Add(new SqlParameter("@BooKeeper", ""));
                cmd.Parameters.Add(new SqlParameter("@BoughtFee", 10000));
                cmd.Parameters.Add(new SqlParameter("@CreateDate", Today));
                cmd.Parameters.Add(new SqlParameter("@CreateUser", User));
                cmd.Parameters.Add(new SqlParameter("@ModifyDate", Today));
                cmd.Parameters.Add(new SqlParameter("@ModifyUser", User));

                SqlTransaction Transaction = conn.BeginTransaction();
                cmd.Transaction = Transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    Transaction.Commit();
                }
                catch
                {
                    Transaction.Rollback();
                    throw;//用來重新擲回 catch 陳述式攔截到的例外狀況
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 使用BookId取得(要更新的)書籍更新前資料並顯示出來
        /// </summary>
        /// <param name="BookId"></param>
        /// <returns></returns>
        public Models.BookData GetUpdateBookData(int BookId)
        {
            string sql = @"
                            SELECT 
                            	Data.BOOK_ID AS '書籍編號',
                            	Data.BOOK_NAME AS '書名',
                            	Data.BOOK_CLASS_ID AS '圖書類別',
                            	Data.BOOK_AUTHOR AS '作者',
                            	FORMAT(Data.BOOK_BOUGHT_DATE, 'yyyy/MM/dd') AS '購書日期',
                            	Data.BOOK_PUBLISHER AS '出版商',
                            	Data.BOOK_NOTE AS '內容簡介',
                            	Data.BOOK_STATUS AS '書本狀態',
                            	Data.BOOK_KEEPER AS '借閱人'
                            FROM BOOK_DATA AS Data
                            WHERE BOOK_ID = @BookId";

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", BookId));
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            //dt是陣列，我們只要第一筆
            DataRow row = dt.Rows[0];

            Models.BookData ThisBookData = new BookData()
            {
                BookId = Convert.ToInt32(row["書籍編號"].ToString()),
                BookName = row["書名"].ToString(),
                ClassId = row["圖書類別"].ToString(),
                BookAuthor = row["作者"].ToString(),
                BoughtDate = row["購書日期"].ToString(),
                Publisher = row["出版商"].ToString(),
                BookNote = row["內容簡介"].ToString(),
                BookStatus = row["書本狀態"].ToString(),
                BooKeeper = row["借閱人"].ToString(),
            };

            return ThisBookData;
        }
        /// <summary>
        /// 新增借閱紀錄
        /// </summary>
        /// <param name="UpdataBookData"></param>
        /// <returns></returns>
        public void AddLendRecord(Models.BookData UpdataBookData)
        {
            string sql = @"INSERT INTO
                            BOOK_LEND_RECORD(
	                            BOOK_ID,
	                            KEEPER_ID,
	                            LEND_DATE,
	                            CRE_DATE,
	                            CRE_USR,
	                            MOD_DATE,
	                            MOD_USR
                            ) VALUES (
	                            @BookId,
	                            @KeeperId,
	                            @LendDate,
	                            @CreateDate,
	                            @CreateUser,
	                            @ModifyDate,
	                            @ModifyUser
                            );";

            DateTime Today = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", UpdataBookData.BookId));
                cmd.Parameters.Add(new SqlParameter("@KeeperId", UpdataBookData.BooKeeper));
                cmd.Parameters.Add(new SqlParameter("@LendDate", Convert.ToDateTime(Today)));
                cmd.Parameters.Add(new SqlParameter("@CreateDate", Convert.ToDateTime(Today)));
                cmd.Parameters.Add(new SqlParameter("@CreateUser", UpdataBookData.BooKeeper));
                cmd.Parameters.Add(new SqlParameter("@ModifyDate", Convert.ToDateTime(Today)));
                cmd.Parameters.Add(new SqlParameter("@ModifyUser", UpdataBookData.BooKeeper));

                SqlTransaction Transaction = conn.BeginTransaction();
                cmd.Transaction = Transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    Transaction.Commit();
                }
                catch
                {
                    Transaction.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 更新書籍資料
        /// </summary>
        /// <param name="UpdateBookData"></param>
        public void UpdateBook(Models.BookData UpdateBookData)
        {
            string sql = @"
                        UPDATE BOOK_DATA
                        SET
                        	BOOK_NAME = @BookName,
                        	BOOK_CLASS_ID = @ClassId,
                        	BOOK_AUTHOR = @Author,
                        	BOOK_BOUGHT_DATE = @BoughtDate,
                        	BOOK_PUBLISHER = @Publisher,
                        	BOOK_NOTE = @BookNote,
                        	BOOK_STATUS = @BookStatus,
                        	BOOK_KEEPER = @BooKeeper
                        WHERE
                        	BOOK_ID = @BookId";

            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", UpdateBookData.BookId));
                cmd.Parameters.Add(new SqlParameter("@BookName", UpdateBookData.BookName));
                cmd.Parameters.Add(new SqlParameter("@ClassId", UpdateBookData.ClassId));
                cmd.Parameters.Add(new SqlParameter("@Author", UpdateBookData.BookAuthor));
                cmd.Parameters.Add(new SqlParameter("@BoughtDate", Convert.ToDateTime(UpdateBookData.BoughtDate)));
                cmd.Parameters.Add(new SqlParameter("@Publisher", UpdateBookData.Publisher));
                cmd.Parameters.Add(new SqlParameter("@BookNote", UpdateBookData.BookNote));
                cmd.Parameters.Add(new SqlParameter("@BookStatus", UpdateBookData.BookStatus));
                //避免有null值
                cmd.Parameters.Add(new SqlParameter("@BooKeeper", UpdateBookData.BooKeeper == null ? string.Empty : UpdateBookData.BooKeeper));
                SqlTransaction Transaction = conn.BeginTransaction();
                cmd.Transaction = Transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    Transaction.Commit();
                }
                catch
                {
                    Transaction.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 使用BookId決定刪除哪一本書
        /// </summary>
        /// <param name="BookId"></param>
        public void DeleteBookData(string BookId)
        {
            string sql = @"DELETE
                            FROM BOOK_DATA WHERE BOOK_ID = @BookId";
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", BookId));

                SqlTransaction Transaction = conn.BeginTransaction();
                cmd.Transaction = Transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    Transaction.Commit();
                }
                catch
                {
                    Transaction.Rollback();
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 使用BookId取得該本書的借閱紀錄
        /// </summary>
        /// <param name="BookId"></param>
        /// <returns></returns>
        public List<Models.LendRecord> GetLendRecord(int BookId)
        {
            DataTable dt = new DataTable();

            string sql = @"
                        SELECT 
                        	FORMAT(record.LEND_DATE, 'yyyy/MM/dd') AS '借閱日期',
                        	record.KEEPER_ID AS '借閱人員編號',
                        	member.USER_ENAME AS '英文姓名',
                        	member.USER_CNAME AS '中文姓名'
                        FROM BOOK_LEND_RECORD AS record
                        INNER JOIN MEMBER_M AS member
                        	ON record.KEEPER_ID = MEMBER.USER_ID
                        WHERE record.BOOK_ID = @BookId
                        ORDER BY '借閱日期' DESC";


            using (SqlConnection conn = new SqlConnection(this.GetDBConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@BookId", BookId));
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dt);
                conn.Close();
            }
            return this.MapLendRecordToList(dt);
        }

        /// <summary>
        /// 將借閱紀錄從DataTable加到List中
        /// </summary>
        /// <param name="BookRecord"></param>
        /// <returns></returns>
        private List<Models.LendRecord> MapLendRecordToList(DataTable BookRecord)
        {
            List<Models.LendRecord> SearchResult = new List<LendRecord>();
            foreach (DataRow row in BookRecord.Rows)
            {
                SearchResult.Add(new LendRecord()
                {
                    LendDate = row["借閱日期"].ToString(),
                    KeeperId = row["借閱人員編號"].ToString(),
                    EnglishName = row["英文姓名"].ToString(),
                    ChineseName = row["中文姓名"].ToString()
                });
            }
            return SearchResult;
        }

    }
}