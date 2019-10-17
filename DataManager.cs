using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using OleDBDataManager;
using KeyMaster;
using LogDefault;

namespace WaspRefresh
{
    class DataManager
    {
        private string entity = "";
        private DataSet dSet = new DataSet();
        private LogManager lm = LogManager.GetInstance();
        private ODMDataFactory ODMDataSetFactory = null;
        private NameValueCollection ConfigSettings = null;
        private string dbConnectString = "";
        private string colHeaders = "";

        public DataManager()
        {
          //  System.Data.SqlClient.SqlCommand.ExecuteReader().
            ODMDataSetFactory = new ODMDataFactory();
            ConfigSettings = (NameValueCollection)ConfigurationManager.GetSection("appSettings");            
            //dbConnectString = ConfigSettings.Get("connect");
            lm.LogFile = ConfigSettings.Get("logFile");
            lm.LogFilePath = ConfigSettings.Get("logFilePath");
        }

        public string ColHeaders
        {
            get { return colHeaders; }
        }

        public DataSet DSet
        {
            get { return dSet; }
        }

        public string Entity
        {
            set { entity = value;
                GetConnectString();
            }
        }

        public void Process()
        {           
            GetData();
            GetColHeaders();
        }

        private void GetConnectString()
        {
            try
            {
                if (entity.Equals("[h-hemm]"))
                    dbConnectString = ConfigSettings.Get("hmc_connect");
                else if (entity.Equals("[u-hemm]"))
                    dbConnectString = ConfigSettings.Get("uwmc_connect");
                else
                    dbConnectString = ConfigSettings.Get("nwh_connect");
                dbConnectString += GetUserConfig();
            }catch(Exception ex)
            {
                lm.Write("DataManager.GetConnectString() " + ex.Message);
            }
        }

        private void GetData()
        {
            lm.Write(entity + "    " + dbConnectString);
            ODMRequest Request = new ODMRequest();
            Request.ConnectString = dbConnectString;
            Request.CommandType = CommandType.Text;
            Request.Command = entity == "[n-hemm]" ? BuildNWHQuery() : BuildQuery();           
            try
            {
                dSet = ODMDataSetFactory.ExecuteDataSetBuild(ref Request);                
            }
            catch (Exception ex)
            {
                lm.Write("DataManager.GetData: " + ex.Message);
            }
        }

        private void GetColHeaders()
        {            
            foreach (DataColumn col in dSet.Tables[0].Columns)
            {
                colHeaders += col.ColumnName + "|";
            }
            colHeaders = colHeaders.Substring(0, colHeaders.Length - 1);            
        }

        private string BuildNWHQuery()
        {
            string query = "Select * from [uwm_BIAdmin].[dbo].[vParFormItemSelect]";
            return query;
        }

        private string BuildQuery()
        {
            string query = 
                            //"WITH PO_ACTIVITY(ITEM_NO, AVG_DAYS_TO_DELV) AS (" +
                            //"SELECT I.ITEM_NO, " +
                            //"AVG(CONVERT(float,DATEDIFF(day,PO.PO_DATE,PL.LAST_RCV_DATE))) AVG_DAYS_TO_DELIVERY " +
                            //"FROM PO " +
                            //"INNER JOIN PO_LINE PL ON PO.PO_ID = PL.PO_ID " +
                            //"INNER JOIN ITEM I ON PL.ITEM_ID = I.ITEM_ID " +
                            //"INNER JOIN PO_SUB_LINE PSL ON PL.PO_LINE_ID = PSL.PO_LINE_ID " +
                            //"INNER JOIN CC ON PSL.CC_ID = CC.CC_ID " +
                            //"WHERE PL.STAT IN (3,10) " +
                            //"AND LEFT(I.ITEM_NO,2) <> '~[' " +
                            //"AND PO.PO_DATE BETWEEN DATEADD(day,-365,(SELECT GETDATE())) AND DATEADD(ms,-3,DATEADD(day,1,(SELECT GETDATE()))) " +
                            //"GROUP BY I.ITEM_NO ) " +

                "SELECT DISTINCT dbo.ITEM.ITEM_ID, dbo.ITEM.ITEM_NO, RTRIM(dbo.REQ.REQ_NO) AS REQ_NO, dbo.REQ_ITEM.LINE_NO, " +
                    "dbo.ITEM.DESCR, dbo.REQ_ITEM.QTY AS PAR, SUBSTRING(dbo.REQ_ITEM.UM_CD, 7, 2) AS[PAR UM], dbo.ITEM.CTLG_NO, " +
                  "RIGHT(RTRIM(dbo.ITEM_VEND_PKG.UM_CD), 2) + ' ' + CAST(RIGHT(RTRIM(dbo.ITEM_VEND_PKG_FACTOR.TO_QTY), 4) AS VARCHAR) " +
                  " + ' ' + RIGHT(RTRIM(dbo.ITEM_VEND_PKG.TO_UM_CD), 2) AS PKG_STR, " +
                  "ISNULL(REQ_ITEM.PAR_BIN_LOC, '') AS[BIN LOC] " +
                  ////"CASE " +
                  ////  "WHEN LOC.LOC_TYPE = 1 THEN REQ_ITEM.PAR_BIN_LOC " +
                  ////  "ELSE SLOC_ITEM_BIN.BIN_LOC " +
                  ////  "END AS[BIN LOC],  " +
                  //////"CASE " +
                  //////  "WHEN POA.AVG_DAYS_TO_DELV < 1.0 THEN 1.00 " +
                  //////  "ELSE CAST(POA.AVG_DAYS_TO_DELV AS decimal(9,2)) " +
                  //////  "END AS LEAD_DAYS, " +
                  ////  "LOC.LOC_ID " +

            "FROM dbo.REQ INNER JOIN dbo.REQ_ITEM ON dbo.REQ.REQ_ID = dbo.REQ_ITEM.REQ_ID INNER JOIN " +
                "dbo.ITEM ON dbo.REQ_ITEM.ITEM_ID = dbo.ITEM.ITEM_ID LEFT OUTER JOIN " +
                "dbo.ITEM_VEND ON dbo.ITEM.ITEM_ID = dbo.ITEM_VEND.ITEM_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG ON dbo.ITEM_VEND.ITEM_VEND_ID = dbo.ITEM_VEND_PKG.ITEM_VEND_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG_FACTOR ON dbo.ITEM_VEND_PKG.ITEM_VEND_ID = dbo.ITEM_VEND_PKG_FACTOR.ITEM_VEND_ID AND " +
                "dbo.ITEM_VEND_PKG.UM_CD = dbo.ITEM_VEND_PKG_FACTOR.UM_CD AND " +
                "dbo.ITEM_VEND_PKG.TO_UM_CD = dbo.ITEM_VEND_PKG_FACTOR.TO_UM_CD " +
                //LEFT OUTER JOIN  "PO_ACTIVITY POA on POA.ITEM_NO COLLATE SQL_Latin1_General_CP1_CI_AS = ITEM.ITEM_NO JOIN " +
                //"SLOC_ITEM_BIN ON SLOC_ITEM_BIN.ITEM_ID = REQ_ITEM.ITEM_ID JOIN " +
                //"LOC ON LOC.LOC_ID = SLOC_ITEM_BIN.LOC_ID " +

                "WHERE(ITEM.STAT IN(1, 2)) AND(ITEM_VEND.SEQ_NO = 1) AND(ITEM_VEND_PKG.SEQ_NO = 1) AND(REQ.REQ_TYPE = 3) AND " +
                "(REQ.STAT = 13) AND(REQ_ITEM.QTY > 0) AND (ITEM_VEND.CORP_ID = 1000) " +
                //"AND LOC.INACT_IND = 'N' " +

                "GROUP BY ITEM.ITEM_ID, ITEM.ITEM_NO, REQ.REQ_NO, REQ_ITEM.LINE_NO, ITEM.DESCR, " +
                "REQ_ITEM.QTY, REQ_ITEM.UM_CD, ITEM.CTLG_NO, ITEM_VEND_PKG.UM_CD,  " +
                "ITEM_VEND_PKG_FACTOR.TO_QTY, ITEM_VEND_PKG.TO_UM_CD, REQ_ITEM.PAR_BIN_LOC  " +
                //" LOC.LOC_TYPE,SLOC_ITEM_BIN.BIN_LOC, LOC.LOC_ID " +
                //,POA.AVG_DAYS_TO_DELV
                "ORDER BY REQ_NO, REQ_ITEM.LINE_NO ";

            return query;
        }
       
        private string GetUserConfig()
        {//this completes the path to where the credentials are found
            try
            {
                string userConfig = ConfigSettings.Get("configFilePath");
                if (entity == "[h-hemm]")
                    userConfig += "HMC\\UserConfig.txt";
                else if(entity == "[u-hemm]")
                    userConfig += "UWMC\\UserConfig.txt";
                else
                    userConfig += "NWH\\UserConfig.txt";


             //   userConfig += entity == "[h-hemm]" ? "HMC\\UserConfig.txt" : "UWMC\\UserConfig.txt";
                string[] key = File.ReadAllLines(userConfig);
                string user = entity == "[h-hemm]" ? "intelliweb" : "RIO"; //RIO for both UW & NWH
                return StringCipher.Decrypt(key[0], user);
            }
            catch (Exception ex)
            {
                lm.Write("DataManager.GetUserConfig() " + ex.Message);
            }
            return "";
        }
    }
    
}
