This is a console app that refreshes the Excel spreadsheets that are being used as datasources for the Wasp labeling app.

The password for the data access portion of this app is stored in the file [configFilePath]\HMC [or WUMC] \UserConfig.txt (find configFilePath in the config file).
The referenced library KeyMaster is used to decrypt the password at run time. There is another app called EncryptAndHash 
(found in \\Lapis\h_purchasing$\Purchasing\PMM IS data\HEMM Apps\Executables\ ) that you can use to change the password when that becomes necessary. The key is either "intelliweb" [HMC] or  "RIO" [UW] & [NWH].




//license for SpreadSheetLight
/*
 * Copyright (c) 2011 Vincent Tan Wai Lip

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */














			*************************************************
			query for items using par form bin locations (REQ_ITEM.PAR_BIN_LOC table)
			*************************************************

 string query = "SELECT TOP(100) PERCENT dbo.ITEM.ITEM_ID, dbo.ITEM.ITEM_NO, RTRIM(dbo.REQ.REQ_NO) AS REQ_NO, dbo.REQ_ITEM.LINE_NO, " +
                    "dbo.ITEM.DESCR, dbo.REQ_ITEM.QTY AS PAR, SUBSTRING(dbo.REQ_ITEM.UM_CD, 7, 2) AS[PAR UM], dbo.ITEM.CTLG_NO, " +
                  "RIGHT(RTRIM(dbo.ITEM_VEND_PKG.UM_CD), 2) + ' ' + CAST(RIGHT(RTRIM(dbo.ITEM_VEND_PKG_FACTOR.TO_QTY), 4) AS VARCHAR) " +
                  " + ' ' + RIGHT(RTRIM(dbo.ITEM_VEND_PKG.TO_UM_CD), 2) AS PKG_STR, dbo.REQ_ITEM.PAR_BIN_LOC AS[BIN LOC], dbo.VEND.LEAD_DAYS " +

                  "FROM dbo.REQ INNER JOIN dbo.REQ_ITEM ON dbo.REQ.REQ_ID = dbo.REQ_ITEM.REQ_ID INNER JOIN " +
                "dbo.ITEM ON dbo.REQ_ITEM.ITEM_ID = dbo.ITEM.ITEM_ID LEFT OUTER JOIN " +
                "dbo.ITEM_VEND ON dbo.ITEM.ITEM_ID = dbo.ITEM_VEND.ITEM_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG ON dbo.ITEM_VEND.ITEM_VEND_ID = dbo.ITEM_VEND_PKG.ITEM_VEND_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG_FACTOR ON dbo.ITEM_VEND_PKG.ITEM_VEND_ID = dbo.ITEM_VEND_PKG_FACTOR.ITEM_VEND_ID AND " +
                "dbo.ITEM_VEND_PKG.UM_CD = dbo.ITEM_VEND_PKG_FACTOR.UM_CD AND " +
                "dbo.ITEM_VEND_PKG.TO_UM_CD = dbo.ITEM_VEND_PKG_FACTOR.TO_UM_CD LEFT OUTER JOIN " +
                "dbo.VEND ON dbo.VEND.VEND_ID = dbo.ITEM_VEND.VEND_ID " +

                "WHERE(dbo.ITEM.STAT = 1) AND(dbo.ITEM_VEND.SEQ_NO = 1) AND(dbo.ITEM_VEND_PKG.SEQ_NO = 1) AND(dbo.REQ.REQ_TYPE = 3) AND " +
                "(dbo.REQ.STAT = 13) AND(dbo.REQ_ITEM.QTY > 0) AND(dbo.ITEM_VEND.CORP_ID = 1000)  " +

                "GROUP BY dbo.ITEM.ITEM_ID, dbo.ITEM.ITEM_NO, dbo.REQ.REQ_NO, dbo.REQ_ITEM.LINE_NO, dbo.ITEM.DESCR,  " +
                "dbo.REQ_ITEM.QTY, dbo.REQ_ITEM.UM_CD, dbo.ITEM.CTLG_NO, dbo.ITEM_VEND_PKG.UM_CD, " +
                "dbo.ITEM_VEND_PKG_FACTOR.TO_QTY, dbo.ITEM_VEND_PKG.TO_UM_CD, dbo.REQ_ITEM.PAR_BIN_LOC, " +
                "dbo.VEND.LEAD_DAYS " +

                "ORDER BY REQ_NO, dbo.REQ_ITEM.LINE_NO";
            return query;


			*************************************************
			query for items in med stores (SLOC_ITEM_BIN table)
			*************************************************

SELECT TOP (100) PERCENT ITEM.ITEM_ID, ITEM.ITEM_NO, RTRIM(REQ.REQ_NO) AS REQ_NO, REQ_ITEM.LINE_NO, 
                  ITEM.DESCR, REQ_ITEM.QTY AS PAR, SUBSTRING(REQ_ITEM.UM_CD, 7, 2) AS [PAR UM], ITEM.CTLG_NO, 
                  RIGHT(RTRIM(ITEM_VEND_PKG.UM_CD), 2) + ' ' + CAST(RIGHT(RTRIM(ITEM_VEND_PKG_FACTOR.TO_QTY), 4) AS VARCHAR) 
                  + ' ' + RIGHT(RTRIM(ITEM_VEND_PKG.TO_UM_CD), 2) AS PKG_STR,
				SLOC_ITEM_BIN.BIN_LOC AS [BIN LOC], 
                  VEND.LEAD_DAYS
				
FROM     REQ INNER JOIN
                  REQ_ITEM ON REQ.REQ_ID = REQ_ITEM.REQ_ID INNER JOIN				  
                  ITEM ON REQ_ITEM.ITEM_ID = ITEM.ITEM_ID LEFT OUTER JOIN
                  ITEM_VEND ON ITEM.ITEM_ID = ITEM_VEND.ITEM_ID INNER JOIN
                  ITEM_VEND_PKG ON ITEM_VEND.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID INNER JOIN
                  ITEM_VEND_PKG_FACTOR ON ITEM_VEND_PKG.ITEM_VEND_ID = ITEM_VEND_PKG_FACTOR.ITEM_VEND_ID AND 
                  ITEM_VEND_PKG.UM_CD = ITEM_VEND_PKG_FACTOR.UM_CD AND 
                  ITEM_VEND_PKG.TO_UM_CD = ITEM_VEND_PKG_FACTOR.TO_UM_CD INNER JOIN				  
                  VEND ON VEND.VEND_ID = ITEM_VEND.VEND_ID LEFT OUTER JOIN
				  SLOC_ITEM_BIN ON SLOC_ITEM_BIN.ITEM_ID = REQ_ITEM.ITEM_ID 
				   LEFT OUTER JOIN SLOC_ITEM ON SLOC_ITEM.ITEM_ID = SLOC_ITEM_BIN.ITEM_ID

WHERE  (SLOC_ITEM.STAT IN (1,2)) AND (ITEM_VEND.SEQ_NO = 1) AND (ITEM_VEND_PKG.SEQ_NO = 1) AND (REQ.REQ_TYPE = 3) AND 
                  (REQ.STAT = 13) AND (REQ_ITEM.QTY > 0) AND (ITEM_VEND.CORP_ID = 1000)
GROUP BY ITEM.ITEM_ID, ITEM.ITEM_NO, REQ.REQ_NO, REQ_ITEM.LINE_NO, ITEM.DESCR, 
				  SLOC_ITEM_BIN.BIN_LOC,
                  REQ_ITEM.QTY, REQ_ITEM.UM_CD, ITEM.CTLG_NO, ITEM_VEND_PKG.UM_CD, 
				  REQ_ITEM.REQ_ITEM_ID,
                  ITEM_VEND_PKG_FACTOR.TO_QTY, ITEM_VEND_PKG.TO_UM_CD, REQ_ITEM.PAR_BIN_LOC, 
                  VEND.LEAD_DAYS
ORDER BY REQ_NO, REQ_ITEM.LINE_NO 