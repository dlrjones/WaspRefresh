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
            ODMDataSetFactory = new ODMDataFactory();
            ConfigSettings = (NameValueCollection)ConfigurationSettings.GetConfig("appSettings");
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
            int x = 0;
            GetData();
            GetColHeaders();
        }

        private void GetConnectString()
        {
            try
            {
                if (entity.Equals("[h-hemm]"))
                    dbConnectString = ConfigSettings.Get("hmc_connect");
                else
                    dbConnectString = ConfigSettings.Get("uwmc_connect");
                dbConnectString += GetUserConfig();
            }catch(Exception ex)
            {
                lm.Write("DataManager.GetConnectString() " + ex.Message);
            }
        }

        private void GetData()
        {
            ODMRequest Request = new ODMRequest();
            Request.ConnectString = dbConnectString;
            Request.CommandType = CommandType.Text;
            Request.Command = BuildQuery();
            string itemNmbr = "";
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

        private string BuildQuery()
        {
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
        }
       
        private string GetUserConfig()
        {
            try
            {
                string userConfig = ConfigSettings.Get("configFilePath");
                userConfig += entity == "[h-hemm]" ? "HMC\\UserConfig.txt" : "UWMC\\UserConfig.txt";
                string[] key = File.ReadAllLines(userConfig);
                string user = entity == "[h-hemm]" ? "intelliweb" : "RIO";
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
