USE GSSWEB
GO
--第一題 列出每個使用者每年借書數量，並依使用者編號和年度做排序
--自己
SELECT KEEPER_ID AS KeeperId, USER_CNAME AS CName, USER_ENAME AS EName, year(LEND_DATE) AS BorrowYear, COUNT(YEAR(LEND_DATE))AS BorrowCnt
FROM
--一定要用LEFT JOIN
MEMBER_M INNER JOIN BOOK_LEND_RECORD
ON
MEMBER_M.USER_ID = BOOK_LEND_RECORD.KEEPER_ID
GROUP BY KEEPER_ID,USER_CNAME,USER_ENAME,YEAR(LEND_DATE)
ORDER BY KeeperId, BorrowYear
--正解
SELECT 
	KeeperId = MEMBER.USER_ID, 
	CName = member.USER_CNAME, 
	EName = member.USER_ENAME, 
	BorrowYear = ISNULL(YEAR(record.LEND_DATE), '0000'), 
	BorrowCnt = COUNT(YEAR(record.LEND_DATE))
FROM dbo.MEMBER_M AS member
LEFT JOIN dbo.BOOK_LEND_RECORD AS record
	ON KEEPER_ID = USER_ID
GROUP BY MEMBER.USER_ID, member.USER_CNAME, member.USER_ENAME, YEAR(LEND_DATE)
ORDER BY KeeperId, BorrowYear
--差異
--用了LEFT JOIN而不是INNER JOIN，因為有人可能沒借過書，用INNER JOIN會找不到那位沒借書的人
--檢查過了LEND_DATE沒有NULL值，但是卻要ISNULL(YEAR(record.LEND_DATE), '0000')這行是因為避免沒借過書因此在BOOK_LEND_RECORD表格沒資料的狀況

--第二題 列出最受歡迎的書前五名(借閱數量最多前五名)
--自己
SELECT TOP(5) --增加with ties
	data.BOOK_ID AS BookId, BOOK_NAME AS BookName, COUNT(BOOK_NAME) AS QTY
FROM BOOK_LEND_RECORD AS record 
INNER JOIN BOOK_DATA AS data
	ON record.BOOK_ID = data.BOOK_ID
GROUP BY data.BOOK_ID, BOOK_NAME
ORDER BY QTY DESC
--正解
SELECT TOP (5) WITH TIES --增加with ties，防止同名次被遺漏
	BookId = record.BOOK_ID, 
	BookName = data.BOOK_NAME, 
	QTY = COUNT(*) 
FROM dbo.BOOK_DATA AS data 
INNER JOIN dbo.BOOK_LEND_RECORD AS record
	ON record.BOOK_ID = data.BOOK_ID
GROUP BY record.BOOK_ID, data.BOOK_NAME
ORDER BY QTY DESC

--第三題 以一季列出2019年每一季書籍借閱書量 (請考慮未來需求調整對程式的變動幅度)
--正解(有發現SPAN_TABLE)
--也可以寫一個function
DECLARE @theYear INT = 2019 --使用變數方便未來調整

SELECT 
	Quarter = span.SPAN_YEAR + '/' + span.SPAN_START + '~' + span.SPAN_YEAR + '/' + span.SPAN_END,
	Cnt = COUNT(*)--可以替換成record.BOOK_ID
FROM BOOK_LEND_RECORD AS record
INNER JOIN SPAN_TABLE AS span
	ON 
		YEAR(record.LEND_DATE) = span.SPAN_YEAR
		AND
		MONTH(record.LEND_DATE) BETWEEN span.SPAN_START AND span.SPAN_END
WHERE YEAR(record.LEND_DATE) = @theYear
GROUP BY span.SPAN_YEAR, span.SPAN_START, span.SPAN_END
--注意join條件不能寫下面這個，因為IDENTITY_FILED是流水號，沒有連結意義
SELECT * FROM BOOK_LEND_RECORD AS record
INNER JOIN SPAN_TABLE AS span
ON record.IDENTITY_FILED = span.IDENTITY_FILED

--第四題 撈出每個分類借閱數量前三名書本及數量
--衍伸資料表
SELECT *
FROM (
	SELECT
	--不用ROWNUMBER是因為要嚴格去排序
		Seq = RANK() 
		--用OVER:以BOOK_CLASS_NAME為依據去分群並計算各群內各書籍的數量去排序
			OVER(
			PARTITION BY class.BOOK_CLASS_NAME
			ORDER BY COUNT(*) DESC),--可以改成BOOK_ID
		BookClass = class.BOOK_CLASS_NAME, 
		BookId = DATA.BOOK_ID, 
		BookName = DATA.BOOK_NAME, 
		Cnt = COUNT(data.BOOK_ID)--可以改成BOOK_NAME，但是為了避免書名重複還是用id
	FROM dbo.BOOK_DATA AS data
	INNER JOIN dbo.BOOK_LEND_RECORD AS record 
		ON data.BOOK_ID = record.BOOK_ID
	RIGHT OUTER JOIN dbo.BOOK_CLASS AS class --有些class底下的書可能沒有被借過，為了避免那些資料沒被算到
		ON DATA.BOOK_CLASS_ID = class.BOOK_CLASS_ID
	GROUP BY class.BOOK_CLASS_NAME, DATA.BOOK_ID, data.BOOK_NAME
) AS subQ
WHERE subQ.Seq <= 3

--CTE
;WITH cteBOOK_LEND_RANK
AS( 
	SELECT RANK() OVER(PARTITION BY class.BOOK_CLASS_NAME ORDER BY COUNT(data.BOOK_ID) DESC) AS Seq,
	    class.BOOK_CLASS_NAME AS BookClass,
		data.BOOK_ID AS BookId, 
		data.BOOK_NAME AS BookName, 
		COUNT(data.BOOK_ID) AS Cnt
	FROM BOOK_LEND_RECORD AS record 
		INNER JOIN BOOK_DATA AS data
			ON record.BOOK_ID = data.BOOK_ID 
		RIGHT JOIN  BOOK_CLASS AS class 
			ON class.BOOK_CLASS_ID = data.BOOK_CLASS_ID
	GROUP BY class.BOOK_CLASS_NAME, data.BOOK_ID, data.BOOK_NAME
)

SELECT *
FROM cteBOOK_LEND_RANK AS [rank]
WHERE Seq <= 3;
GO

--第五題 請列出 2016, 2017, 2018, 2019 各書籍類別的借閱數量比較
SELECT bc.BOOK_CLASS_ID AS ClassId,
	bc.BOOK_CLASS_NAME AS ClassName,
	--不能用COUNT因為每進去一次條件就會+1，COUNT是數個數，SUM是加總和
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2016 THEN 1 ELSE 0 END) AS CNT2016,
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2017 THEN 1 ELSE 0 END) AS CNT2017,
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2018 THEN 1 ELSE 0 END) AS CNT2018,
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2019 THEN 1 ELSE 0 END) AS CNT2019
FROM BOOK_LEND_RECORD AS blr 
	INNER JOIN BOOK_DATA AS bd
		ON bd.BOOK_ID = blr.BOOK_ID	 
	INNER JOIN BOOK_CLASS AS bc--或是用RIGHT JOIN，但是他這題是數個數，不要考慮到有些class底下的書可能沒有被借過
		ON bc.BOOK_CLASS_ID = bd.BOOK_CLASS_ID	
GROUP BY bc.BOOK_CLASS_ID, bc.BOOK_CLASS_NAME
ORDER BY bc.BOOK_CLASS_ID;

--第六題 請使用 PIVOT 語法列出2016, 2017, 2018, 2019 各書籍類別的借閱數量比較
IF OBJECT_ID('BOOK_CLASS_LEND') IS NOT NULL
	DROP VIEW BOOK_CLASS_LEND;
GO

CREATE VIEW BOOK_CLASS_LEND
AS 
	SELECT class.BOOK_CLASS_ID AS ClassId
		  ,class.BOOK_CLASS_NAME AS ClassName
		  ,YEAR(record.LEND_DATE) AS LendYear
		  ,COUNT(class.BOOK_CLASS_ID) AS Qty
	FROM BOOK_CLASS AS class
	LEFT JOIN (
		BOOK_DATA AS data
		INNER JOIN BOOK_LEND_RECORD AS record
			ON data.BOOK_ID = record.BOOK_ID
		)
		ON class.BOOK_CLASS_ID = data.BOOK_CLASS_ID
	GROUP BY class.BOOK_CLASS_ID
		  ,class.BOOK_CLASS_NAME
		  ,record.LEND_DATE;
GO

SELECT ClassId,
	ClassName,
	ISNULL([2016],0) AS CNT2016,
	ISNULL([2017],0) AS CNT2017,
	ISNULL([2018],0) AS CNT2018,
	ISNULL([2019],0) AS CNT2019
FROM ( SELECT ClassId, ClassName, Qty, LendYear 
	 FROM BOOK_CLASS_LEND ) AS bcl
PIVOT(SUM(Qty) FOR LendYear 
		IN([2016],[2017],[2018],[2019])
		) AS pvt
ORDER BY ClassId;
GO
--第七題 呈上請使用 PIVOT 語法列出 2016, 2017, 2018, 2019 各書籍類別的借閱數量比較 (動態分析所有年度)
DECLARE @CNT_columns NVARCHAR(MAX)=''
--@CNT_columns是y軸的欄位內容
DECLARE @StrYears NVARCHAR(MAX)=''
DECLARE @StrSql NVARCHAR(MAX)=''
--處理@CNT_columns
SELECT @CNT_columns =
 @CNT_columns + 'ISNULL(' + QUOTENAME(YEAR(blr.LEND_DATE)) + ',0) AS CNT' + CONVERT(VARCHAR(4),YEAR(blr.LEND_DATE)) + ','
 --ISNULL([2016],0) AS CNT2016,ISNULL([2017],0) AS CNT2017,ISNULL([2018],0) AS CNT2018,ISNULL([2019],0) AS CNT2019
FROM BOOK_LEND_RECORD AS blr
GROUP BY YEAR(blr.LEND_DATE)
ORDER BY YEAR(blr.LEND_DATE)

SET @CNT_columns = LEFT(@CNT_columns, LEN(@CNT_columns) - 1)--去掉最後的"，"
--PRINT @CNT_columns
--處理@StrYears
SELECT @StrYears = @StrYears + QUOTENAME(YEAR(blr.LEND_DATE)) + ',' 
FROM BOOK_LEND_RECORD blr
GROUP BY YEAR(blr.LEND_DATE)
ORDER BY YEAR(blr.LEND_DATE)

SET @StrYears =  Left(@StrYears, Len(@StrYears) - 1)--去掉最後的"，"
--[2016],[2017],[2018],[2019]
PRINT @StrYears 

SET @StrSql = N'SELECT ClassId,
	ClassName,
	' + @CNT_columns +
	N' FROM ( SELECT ClassId, ClassName, Qty, LendYear 
		 FROM BOOK_CLASS_LEND ) AS bcl
	PIVOT(SUM(Qty) FOR LendYear 
			IN(' + @StrYears + N')
			) AS pvt
	ORDER BY ClassId;'

EXEC (@StrSql)
GO
--第八題 請查詢出李四(USER_ID =0002)的借書紀錄，其中包含書本ID、購書日期(yyyy/mm/dd)、借閱日期(yyyy/mm/dd)、
---書籍類別(id-name)、借閱人(id-cname(ename))、狀態(id-name)、購書金額
IF OBJECT_ID('view_LEND_BOOK') IS NOT NULL
	DROP VIEW  view_LEND_BOOK;
GO
--下方程式碼,參數1是: 保留小數位後兩位,並四捨五入,且有千分位逗點,因此要消掉小數點後兩位
--Replace函數是第一個參數裡有第二個參數出現時用第三個參數去取代第二個參數
CREATE VIEW view_LEND_BOOK
AS
	SELECT blr.BOOK_ID AS 書本ID
		  ,CONVERT(VARCHAR(4),YEAR(bd.BOOK_BOUGHT_DATE))+'/'+CONVERT(VARCHAR(2),MONTH(bd.BOOK_BOUGHT_DATE))
		  +'/'+CONVERT(VARCHAR(2),DAY(bd.BOOK_BOUGHT_DATE)) AS 購書日期
		  ,CONVERT(VARCHAR(10), blr.LEND_DATE,111) AS 借閱日期--111是yyyy/mm/dd形式
		  ,bd.BOOK_CLASS_ID + '-' + bc1.BOOK_CLASS_NAME AS 書籍類別
		  ,mm.[USER_ID] + '-' +mm.USER_CNAME + '(' + mm.USER_ENAME + ')' AS 借閱人
		  ,bc.CODE_ID + '-' + bc.CODE_NAME AS 狀態
		  ,Replace(Convert(Varchar(10),CONVERT(money,bd.BOOK_AMOUNT),1),'.00','')+'元' AS 購書金額
	FROM BOOK_LEND_RECORD AS blr
	INNER JOIN MEMBER_M AS mm
		ON mm.[USER_ID] = blr.KEEPER_ID
	INNER JOIN BOOK_DATA AS bd
		ON blr.BOOK_ID = bd.BOOK_ID
	INNER JOIN BOOK_CODE AS bc
		ON bd.BOOK_STATUS = bc.CODE_ID
	INNER JOIN BOOK_CLASS AS bc1
		ON bc1.BOOK_CLASS_ID = bd.BOOK_CLASS_ID
	WHERE bc.CODE_TYPE = 'BOOK_STATUS'
	--要指定Code.CODE_TYPE='BOOK_STATUS'條件因為有血型資料混在裡面,要排除掉
	AND mm.[USER_ID] ='0002';
GO

SELECT *
FROM view_LEND_BOOK
ORDER BY 書本ID DESC;

--第九題 新增一筆借閱紀錄，借書人為李四，書本ID為2294，借閱日期為2021/01/01，並更新書籍主檔的狀態為已借出且借閱人為李四
--CRE_DATE:建立時間 CRE_USR:建立使用者
BEGIN TRY
	BEGIN TRAN
		INSERT INTO BOOK_LEND_RECORD(KEEPER_ID, BOOK_ID, LEND_DATE, CRE_DATE, CRE_USR) 
			VALUES((SELECT mm.[USER_ID] FROM MEMBER_M mm WHERE mm.USER_CNAME = '李四'), '2294', '2021/01/01', GETDATE(), '0002')
		
		UPDATE BOOK_DATA
		SET BOOK_STATUS = 'B',
			BOOK_KEEPER = (SELECT mm.[USER_ID] FROM MEMBER_M mm WHERE mm.USER_CNAME = '李四'),
			MODIFY_DATE = GETDATE(),
			MODIFY_USER = '0002'
		WHERE BOOK_ID = '2294'
	COMMIT TRAN
END TRY
BEGIN CATCH
	SELECT ERROR_NUMBER() AS ERR_NUM, ERROR_MESSAGE() AS ERR_MSG
	ROLLBACK TRAN
END CATCH;

SELECT *
FROM view_LEND_BOOK--用來查李四的借書紀錄
ORDER BY 書本ID DESC;

--第十題 請將題9新增的借閱紀錄(書本ID=2004)刪除 
--陷阱:是刪掉ID=2294
BEGIN TRY
	BEGIN TRAN
		DELETE FROM BOOK_LEND_RECORD WHERE BOOK_ID = '2294' AND KEEPER_ID = '0002' AND LEND_DATE = '2021/01/01'

		UPDATE BOOK_DATA
		SET BOOK_STATUS = 'A',
			BOOK_KEEPER = (SELECT mm.[USER_ID] FROM MEMBER_M mm WHERE mm.USER_CNAME = '李四'),--不要用USER_CNAME怕有人同名要用id
			MODIFY_DATE = GETDATE(),
			MODIFY_USER = '0002'
		WHERE BOOK_ID = '2294'
		COMMIT TRAN
END TRY
BEGIN CATCH
	SELECT ERROR_NUMBER() AS ERR_NUM, ERROR_MESSAGE() AS ERR_MSG
	ROLLBACK TRAN
END CATCH;

SELECT *
FROM view_LEND_BOOK
ORDER BY 書本ID DESC;