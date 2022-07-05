USE GSSWEB
GO
--�Ĥ@�D �C�X�C�ӨϥΪ̨C�~�ɮѼƶq�A�ę̀ϥΪ̽s���M�~�װ��Ƨ�
--�ۤv
SELECT KEEPER_ID AS KeeperId, USER_CNAME AS CName, USER_ENAME AS EName, year(LEND_DATE) AS BorrowYear, COUNT(YEAR(LEND_DATE))AS BorrowCnt
FROM
--�@�w�n��LEFT JOIN
MEMBER_M INNER JOIN BOOK_LEND_RECORD
ON
MEMBER_M.USER_ID = BOOK_LEND_RECORD.KEEPER_ID
GROUP BY KEEPER_ID,USER_CNAME,USER_ENAME,YEAR(LEND_DATE)
ORDER BY KeeperId, BorrowYear
--����
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
--�t��
--�ΤFLEFT JOIN�Ӥ��OINNER JOIN�A�]�����H�i��S�ɹL�ѡA��INNER JOIN�|�䤣�쨺��S�ɮѪ��H
--�ˬd�L�FLEND_DATE�S��NULL�ȡA���O�o�nISNULL(YEAR(record.LEND_DATE), '0000')�o��O�]���קK�S�ɹL�Ѧ]���bBOOK_LEND_RECORD����S��ƪ����p

--�ĤG�D �C�X�̨��w�諸�ѫe���W(�ɾ\�ƶq�̦h�e���W)
--�ۤv
SELECT TOP(5) --�W�[with ties
	data.BOOK_ID AS BookId, BOOK_NAME AS BookName, COUNT(BOOK_NAME) AS QTY
FROM BOOK_LEND_RECORD AS record 
INNER JOIN BOOK_DATA AS data
	ON record.BOOK_ID = data.BOOK_ID
GROUP BY data.BOOK_ID, BOOK_NAME
ORDER BY QTY DESC
--����
SELECT TOP (5) WITH TIES --�W�[with ties�A����P�W���Q��|
	BookId = record.BOOK_ID, 
	BookName = data.BOOK_NAME, 
	QTY = COUNT(*) 
FROM dbo.BOOK_DATA AS data 
INNER JOIN dbo.BOOK_LEND_RECORD AS record
	ON record.BOOK_ID = data.BOOK_ID
GROUP BY record.BOOK_ID, data.BOOK_NAME
ORDER BY QTY DESC

--�ĤT�D �H�@�u�C�X2019�~�C�@�u���y�ɾ\�Ѷq (�ЦҼ{���ӻݨD�վ��{�����ܰʴT��)
--����(���o�{SPAN_TABLE)
--�]�i�H�g�@��function
DECLARE @theYear INT = 2019 --�ϥ��ܼƤ�K���ӽվ�
SELECT 
	Quarter = span.SPAN_YEAR + '/' + span.SPAN_START + '~' + span.SPAN_YEAR + '/' + span.SPAN_END,
	Cnt = COUNT(*)--�i�H������record.BOOK_ID
FROM BOOK_LEND_RECORD AS record
INNER JOIN SPAN_TABLE AS span
	ON 
		YEAR(record.LEND_DATE) = span.SPAN_YEAR
		AND
		MONTH(record.LEND_DATE) BETWEEN span.SPAN_START AND span.SPAN_END
WHERE YEAR(record.LEND_DATE) = @theYear
GROUP BY span.SPAN_YEAR, span.SPAN_START, span.SPAN_END
--�`�Njoin���󤣯�g�U���o�ӡA�]��IDENTITY_FILED�O�y�����A�S���s���N�q
SELECT * FROM BOOK_LEND_RECORD AS record
INNER JOIN SPAN_TABLE AS span
ON record.IDENTITY_FILED = span.IDENTITY_FILED


--�ĥ|�D ���X�C�Ӥ����ɾ\�ƶq�e�T�W�ѥ��μƶq
--�l����ƪ�
SELECT *
FROM (
	SELECT
	--����ROWNUMBER�O�]���n�Y��h�Ƨ�
		Seq = RANK() 
		--��OVER:�HBOOK_CLASS_NAME���̾ڥh���s�íp��U�s���U���y���ƶq�h�Ƨ�
			OVER(
			PARTITION BY class.BOOK_CLASS_NAME
			--DESC�ܭ��n�A�L�̾�COUNT�X�Ӫ��h��h�Ƨ�
			ORDER BY COUNT(*) DESC ),--�i�H�令BOOK_ID
		BookClass = class.BOOK_CLASS_NAME, 
		BookId = DATA.BOOK_ID, 
		BookName = DATA.BOOK_NAME, 
		Cnt = COUNT(data.BOOK_ID)--�i�H�令BOOK_NAME�A���O���F�קK�ѦW�����٬O��id
	FROM dbo.BOOK_DATA AS data
	INNER JOIN dbo.BOOK_LEND_RECORD AS record 
		ON data.BOOK_ID = record.BOOK_ID
	RIGHT OUTER JOIN dbo.BOOK_CLASS AS class --����class���U���ѥi��S���Q�ɹL�A���F�קK���Ǹ�ƨS�Q���
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

--�Ĥ��D �ЦC�X 2016, 2017, 2018, 2019 �U���y���O���ɾ\�ƶq���
SELECT bc.BOOK_CLASS_ID AS ClassId,
	bc.BOOK_CLASS_NAME AS ClassName,
	--�����COUNT�]���C�i�h�@������N�|+1�ACOUNT�O�ƭӼơASUM�O�[�`�M
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2016 THEN 1 ELSE 0 END) AS CNT2016,
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2017 THEN 1 ELSE 0 END) AS CNT2017,
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2018 THEN 1 ELSE 0 END) AS CNT2018,
	SUM(CASE WHEN YEAR(blr.LEND_DATE) = 2019 THEN 1 ELSE 0 END) AS CNT2019
FROM BOOK_LEND_RECORD AS blr 
	INNER JOIN BOOK_DATA AS bd
		ON bd.BOOK_ID = blr.BOOK_ID	 
	INNER JOIN BOOK_CLASS AS bc--�άO��RIGHT JOIN�A���O�L�o�D�O�ƭӼơA���ΦҼ{�즳��class���U���ѥi��S���Q�ɹL
		ON bc.BOOK_CLASS_ID = bd.BOOK_CLASS_ID	
GROUP BY bc.BOOK_CLASS_ID, bc.BOOK_CLASS_NAME
ORDER BY bc.BOOK_CLASS_ID;

--�Ĥ��D �Шϥ� PIVOT �y�k�C�X2016, 2017, 2018, 2019 �U���y���O���ɾ\�ƶq���
IF OBJECT_ID('BOOK_CLASS_LEND') IS NOT NULL
	DROP VIEW BOOK_CLASS_LEND;
GO
--�N�C�����O���ɾ\���ƦC�X��(�ҥH�i��|�����ƪ�)
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
--�l����ƪ�
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
--�ĤC�D �e�W�Шϥ� PIVOT �y�k�C�X 2016, 2017, 2018, 2019 �U���y���O���ɾ\�ƶq��� (�ʺA���R�Ҧ��~��)
DECLARE @CNT_columns NVARCHAR(MAX)=''
--@CNT_columns�Oy�b����줺�e
DECLARE @StrYears NVARCHAR(MAX)=''
DECLARE @StrSql NVARCHAR(MAX)=''
--�B�z@CNT_columns
SELECT @CNT_columns =
 @CNT_columns + 'ISNULL(' + QUOTENAME(YEAR(blr.LEND_DATE)) + ',0) AS CNT' + CONVERT(VARCHAR(4),YEAR(blr.LEND_DATE)) + ','
 --ISNULL([2016],0) AS CNT2016,ISNULL([2017],0) AS CNT2017,ISNULL([2018],0) AS CNT2018,ISNULL([2019],0) AS CNT2019,
FROM BOOK_LEND_RECORD AS blr
GROUP BY YEAR(blr.LEND_DATE)
ORDER BY YEAR(blr.LEND_DATE)

SET @CNT_columns = LEFT(@CNT_columns, LEN(@CNT_columns) - 1)--�h���̫᪺"�A"
--PRINT @CNT_columns
--�B�z@StrYears
SELECT @StrYears = @StrYears + QUOTENAME(YEAR(blr.LEND_DATE)) + ',' 
FROM BOOK_LEND_RECORD as blr
GROUP BY YEAR(blr.LEND_DATE)
ORDER BY YEAR(blr.LEND_DATE)

SET @StrYears =  Left(@StrYears, Len(@StrYears) - 1)--�h���̫᪺"�A"
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
--�ĤK�D �Ьd�ߥX���|(USER_ID =0002)���ɮѬ����A�䤤�]�t�ѥ�ID�B�ʮѤ��(yyyy/mm/dd)�B�ɾ\���(yyyy/mm/dd)�B
---���y���O(id-name)�B�ɾ\�H(id-cname(ename))�B���A(id-name)�B�ʮѪ��B
IF OBJECT_ID('view_LEND_BOOK') IS NOT NULL
	DROP VIEW  view_LEND_BOOK;
GO
--�U��{���X,�Ѽ�1�O: �O�d�p�Ʀ����,�å|�ˤ��J,�B���d����r�I,�]���n�����p���I����
--Replace��ƬO�Ĥ@�ӰѼƸ̦��ĤG�ӰѼƥX�{�ɥβĤT�ӰѼƥh���N�ĤG�ӰѼ�

CREATE VIEW view_LEND_BOOK
AS
	SELECT blr.BOOK_ID AS �ѥ�ID
	      ,CONVERT(VARCHAR(10), bd.BOOK_BOUGHT_DATE,111) AS �ʮѤ��
		  ,CONVERT(VARCHAR(10), blr.LEND_DATE,111) AS �ɾ\���--111�Oyyyy/mm/dd�Φ�
		  ,bd.BOOK_CLASS_ID + '-' + bc1.BOOK_CLASS_NAME AS ���y���O
		  ,mm.[USER_ID] + '-' +mm.USER_CNAME + '(' + mm.USER_ENAME + ')' AS �ɾ\�H
		  ,bc.CODE_ID + '-' + bc.CODE_NAME AS ���A
		  ,Replace(Convert(Varchar(10),CONVERT(money,bd.BOOK_AMOUNT),1),'.00','')+'��' AS �ʮѪ��B
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
	--�n���wCode.CODE_TYPE='BOOK_STATUS'����]�����嫬��ƲV�b�̭�,�n�ư���
	AND mm.[USER_ID] ='0002';
GO


SELECT *
FROM view_LEND_BOOK
ORDER BY �ѥ�ID DESC;

--�ĤE�D �s�W�@���ɾ\�����A�ɮѤH�����|�A�ѥ�ID��2294�A�ɾ\�����2021/01/01�A�ç�s���y�D�ɪ����A���w�ɥX�B�ɾ\�H�����|
--CRE_DATE:�إ߮ɶ� CRE_USR:�إߨϥΪ�
DECLARE @USERID VARCHAR(4) ;
SET @USERID = (SELECT mm.[USER_ID] FROM MEMBER_M mm WHERE mm.USER_CNAME = '���|');
--@USERID = '0002'
--PRINT @USERID;
BEGIN TRY
	BEGIN TRAN
		INSERT INTO BOOK_LEND_RECORD(KEEPER_ID, BOOK_ID, LEND_DATE, CRE_DATE, CRE_USR) 
			VALUES(@USERID, '2294', '2021/01/01', GETDATE(), '0002')
		
		UPDATE BOOK_DATA
		SET BOOK_STATUS = 'B',--�w�ɥX
			BOOK_KEEPER = @USERID,
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
FROM view_LEND_BOOK--�ΨӬd���|���ɮѬ���
ORDER BY �ѥ�ID DESC;

--�ĤQ�D �бN�D9�s�W���ɾ\����(�ѥ�ID=2004)�R�� 
--����:�O�R��ID=2294
BEGIN TRY
	BEGIN TRAN
		DELETE FROM BOOK_LEND_RECORD WHERE BOOK_ID = '2294' AND KEEPER_ID = '0002' AND LEND_DATE = '2021/01/01'

		UPDATE BOOK_DATA
		SET BOOK_STATUS = 'A',--�i�ɥX
			BOOK_KEEPER = (SELECT mm.[USER_ID] FROM MEMBER_M mm WHERE mm.USER_CNAME = '���|'),--@USERID
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
ORDER BY �ѥ�ID DESC;