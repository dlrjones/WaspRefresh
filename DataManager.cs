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
            //the connections string is initiated from the Entity property
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
        { //this is called from the Entity property
            try
            {
                if (entity.Equals("[h-hemm]"))
                    dbConnectString = ConfigSettings.Get("hmc_connect");
                else if (entity.Equals("[u-hemm]"))
                    dbConnectString = ConfigSettings.Get("uwmc_connect");
                else if (entity.Equals("[n-hemm]"))
                    dbConnectString = ConfigSettings.Get("uwmc_connect");     // old:  Get("nwh_connect");
                else if (entity.Equals("[n_parforms]"))
                    dbConnectString = ConfigSettings.Get("uwmc_connect");
                if (entity.Equals("[h-hemm-dj]"))
                    dbConnectString = ConfigSettings.Get("hmc_connect");
                else if (entity.Equals("[u-hemm-dj]"))
                    dbConnectString = ConfigSettings.Get("uwmc_connect");
                else if (entity.Equals("[n-hemm-dj]"))
                    dbConnectString = ConfigSettings.Get("uwmc_connect");
                else if (entity.Equals("[mpous]"))
                    dbConnectString = ConfigSettings.Get("mpous_connect");
                else if (entity.Equals("[medstores_connect]"))
                    dbConnectString = ConfigSettings.Get("medstores_connect");
                if(!entity.Equals("[mpous]"))  //the connect string is complete already for mpous
                    dbConnectString += GetUserConfig();
            }
            catch(Exception ex)
            {
                lm.Write("DataManager.GetConnectString() " + ex.Message);
            }
        }

        private void GetData()
        {
            string[] cnctn = dbConnectString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string cnctnLogEntry = cnctn[0] + ";" + cnctn[1] + ";" + cnctn[2] + ";"; //strips off username and pw
            string cmd = "";
            lm.Write(entity + "    " + cnctnLogEntry);
            ODMRequest Request = new ODMRequest();
            Request.ConnectString = dbConnectString;
            Request.CommandType = CommandType.Text;
            switch (entity)
            {
                case "[n-hemm]":
                    cmd = BuildNWHQuery();
                    break;
                case "[n_parforms]":
                    cmd = BuildNWHParForms();
                    break;
                case "[medstores_connect]":
                    cmd = BuildMedStoresQuery();
                    break;
                case "[mpous]":
                    cmd = BuildMPOUSQuery();
                    break;
                case "[nwclinic]":
                    cmd = "";
                    break;
                case "[h-hemm]":
                    cmd = BuildQuery();
                    break;
                case "[u-hemm]":
                    cmd = BuildQuery();
                    break;
                case "[u-hemm-dj]":
                    cmd = BuildDoorJambQuery();
                    break;
                case "[h-hemm-dj]":
                    cmd = BuildDoorJambQuery();
                    break;
                case "[n-hemm-dj]":
                    cmd = BuildDoorJambQuery();
                    break;
            }
            Request.Command = cmd;
            try
            {
                dSet = ODMDataSetFactory.ExecuteDataSetBuild(ref Request);                
            }
            catch (Exception ex)
            {
                lm.Write("DataManager.GetData: " + ex.Message);
            }
            if(entity == "[n-hemm]")
            {
                lm.Write(Environment.NewLine); //provides separation between each hour's entry
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

        private string BuildDoorJambQuery()
        {//the query is set for UWMC and NWH initially
            string query = "SELECT  CASE LEFT(ROUTE_NO,3) WHEN '000' THEN 'UWMC' WHEN 100 THEN 'UWMC' WHEN 200 THEN 'UWMC' WHEN '300' THEN 'NWH' END AS ENTITY, " +
                           "[NAME] AS[DESCRIPTION],'STP' + REPLACE(ISNULL(ROUTE_NO, ''), '-', '') AS BARCODE, RTRIM(ISNULL(ROUTE_NO, '')) AS ROUTE_NO, " +
                           "SUBSTRING(ROUTE_NO, 5, 2) AS BLDG " +
                           "FROM LOC " +
                           "WHERE ROUTE_NO<> '' " +
                           "ORDER BY ENTITY,BLDG ";
            switch (entity)
            {//this switch is here to accommodate future door jamb needs.
                case "[h-hemm-dj]":
                    query = "SELECT  CASE LEFT(ROUTE_NO,3) WHEN '000' THEN 'HMC' WHEN 100 THEN 'UWMC' WHEN 200 THEN 'HMC' WHEN '300' THEN 'NWH' ELSE '' END AS ENTITY, " +
                            "[NAME] AS[DESCRIPTION],'STP' + REPLACE(ISNULL(ROUTE_NO, ''), '-', '') AS BARCODE, RTRIM(ISNULL(ROUTE_NO, '')) AS ROUTE_NO, " +
                            "SUBSTRING(ROUTE_NO, 5, 2) AS BLDG " +
                            "FROM LOC " +
                            "WHERE ROUTE_NO <> '' AND LEFT(ROUTE_NO,3) NOT IN('000','001','002','003','004','4','038') " +
                            "ORDER BY ENTITY,BLDG";
                    break;
                case "[u-hemm-dj]":   //already set                 
                    break;
                case "[n-hemm-dj]":   //already set              
                    break;
            }
            return query;
        }

        private string BuildMedStoresQuery()
        {
            string query = "Select * from [uwm_BIAdmin].[dbo].[vMedStoresItemLabels]";
            return query;
        }

        private string BuildNWHQuery()
        {                                           //"Select * from [uwm_BIAdmin].[dbo].[vParFormItemSelectNWH]";
            string query = "SELECT DISTINCT TOP(100) PERCENT 'NWH' AS ENTITY, ITEM.ITEM_ID, RTRIM(ITEM.MISC3) AS ITEM_NO, ITEM.DESCR,ITEM.CTLG_NO,LOC.NAME AS[NWH_LOCATION], " +
                        "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.UM_CD), 2) FROM[HEMM].[dbo].ITEM_VEND_PKG_FACTOR IVPF " +
                        "JOIN[HEMM].[dbo].ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID WHERE SEQ_NO IN(1) " +
                        "AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID " +
                        "AND IVP.TO_UM_CD = IVPF.TO_UM_CD " +
                        "AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)), '')) + ' ' + " +
                        "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_QTY), 4) FROM[HEMM].[dbo].ITEM_VEND_PKG_FACTOR IVPF " +
                        "JOIN[HEMM].[dbo].ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                        "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                        "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + ' ' + " +
                        "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_UM_CD), 2) FROM[HEMM].[dbo].ITEM_VEND_PKG_FACTOR IVPF " +
                        "JOIN[HEMM].[dbo].ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                        "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID " +
                        "AND IVP.UM_CD <> IVP.TO_UM_CD AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + ' ' + " +
                        "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_QTY), 4) FROM[HEMM].[dbo].ITEM_VEND_PKG_FACTOR IVPF " +
                        "JOIN[HEMM].[dbo].ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                        "WHERE SEQ_NO IN(2) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                        "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + ' ' + " +
                        "(SELECT ISNULL(CAST((SELECT CAST(RIGHT(RTRIM(IVPF.TO_UM_CD), 2) AS VARCHAR(4)) FROM[HEMM].[dbo].ITEM_VEND_PKG_FACTOR IVPF " +
                        "JOIN[HEMM].[dbo].ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                        "WHERE SEQ_NO IN(2) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                        "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) AS PKG_STR, " +
            "ISNULL(SIB.BIN_LOC, '') AS [BIN LOC],RTRIM(ISNULL(ITEM.ITEM_NO, '')) AS[UW / NW] " +
    "FROM[HEMM].dbo.ITEM JOIN " +
        "[HEMM].dbo.SLOC_ITEM_BIN SIB on SIB.ITEM_ID = ITEM.ITEM_ID JOIN " +
        "[HEMM].dbo.ITEM_VEND ON ITEM.ITEM_ID = ITEM_VEND.ITEM_ID INNER JOIN " +
        "[HEMM].dbo.ITEM_VEND_PKG ON ITEM_VEND.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID INNER JOIN " +
        "[HEMM].dbo.ITEM_VEND_PKG_FACTOR ON ITEM_VEND_PKG.ITEM_VEND_ID = ITEM_VEND_PKG_FACTOR.ITEM_VEND_ID AND " +
        "[HEMM].dbo.ITEM_VEND_PKG.UM_CD = ITEM_VEND_PKG_FACTOR.UM_CD " +
        "JOIN[HEMM].dbo.SLOC_ITEM SI ON SI.ITEM_ID = SIB.ITEM_ID " +
        "JOIN[HEMM].dbo.LOC ON SIB.LOC_ID = LOC.LOC_ID " +
    "WHERE(ITEM.STAT IN(1, 2)) AND(ITEM_VEND.SEQ_NO = 1) AND(ITEM_VEND_PKG.SEQ_NO = 1) AND LEN(MISC3) > 0";
            return query;
        }

        private string BuildNWHParForms()
        {
            string query = "SELECT DISTINCT dbo.ITEM.ITEM_ID, dbo.ITEM.MISC3 AS ITEM_NO, RTRIM(dbo.REQ.REQ_NO) AS [PAR FORM], dbo.REQ_ITEM.LINE_NO, " +
                    "dbo.ITEM.DESCR, dbo.REQ_ITEM.QTY AS PAR, SUBSTRING(dbo.REQ_ITEM.UM_CD, 7, 2) AS[PAR UM], dbo.ITEM.CTLG_NO,  " +
                  "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.UM_CD), 2) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN dbo.ITEM_VEND_PKG IVP ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.TO_UM_CD = IVPF.TO_UM_CD " +
                   "AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) +'' + " +
                   "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_QTY), 4) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) +'' + " +
                   "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_UM_CD), 2) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) +'' + " +
                   "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_QTY), 4) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(2) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) +'' + " +
                   "(SELECT ISNULL(CAST((SELECT CAST(RIGHT(RTRIM(IVPF.TO_UM_CD), 2) AS VARCHAR(4)) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(2) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) AS PKG_STR, " +

                  "ISNULL(REQ_ITEM.PAR_BIN_LOC, '') AS[BIN LOC], ' NWH ' AS ENTITY " +

            "FROM dbo.REQ INNER JOIN dbo.REQ_ITEM ON dbo.REQ.REQ_ID = dbo.REQ_ITEM.REQ_ID INNER JOIN " +
                "dbo.ITEM ON dbo.REQ_ITEM.ITEM_ID = dbo.ITEM.ITEM_ID LEFT OUTER JOIN " +
                "dbo.ITEM_VEND ON dbo.ITEM.ITEM_ID = dbo.ITEM_VEND.ITEM_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG ON dbo.ITEM_VEND.ITEM_VEND_ID = dbo.ITEM_VEND_PKG.ITEM_VEND_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG_FACTOR ON dbo.ITEM_VEND_PKG.ITEM_VEND_ID = dbo.ITEM_VEND_PKG_FACTOR.ITEM_VEND_ID AND " +
                "dbo.ITEM_VEND_PKG.UM_CD = dbo.ITEM_VEND_PKG_FACTOR.UM_CD AND " +
                "dbo.ITEM_VEND_PKG.TO_UM_CD = dbo.ITEM_VEND_PKG_FACTOR.TO_UM_CD " +

                "WHERE(ITEM.STAT IN(1, 2)) AND(ITEM_VEND.SEQ_NO = 1) AND(ITEM_VEND_PKG.SEQ_NO = 1) AND(REQ.REQ_TYPE = 3) " +
                "AND(REQ.STAT = 13) AND(REQ_ITEM.QTY > 0) AND(ITEM_VEND.CORP_ID = 1000) AND LEN(MISC3) > 0 " +
                "AND LEFT(REQ_NO,2) = 'XN' " +
                "GROUP BY ITEM.ITEM_ID, ITEM.MISC3, REQ.REQ_NO, REQ_ITEM.LINE_NO, ITEM.DESCR,  " +
                "REQ_ITEM.QTY, REQ_ITEM.UM_CD, ITEM.CTLG_NO, ITEM_VEND_PKG.UM_CD,   " +
                "ITEM_VEND_PKG_FACTOR.TO_QTY, ITEM_VEND_PKG.TO_UM_CD, REQ_ITEM.PAR_BIN_LOC, ITEM_VEND_PKG.ITEM_VEND_ID " +
                "ORDER BY[PAR FORM], REQ_ITEM.LINE_NO ";
            return query;
        }

        private string BuildMPOUSQuery()
        {
            //this script draws from the production db
            string query = "SELECT ALIAS_ID, RTRIM(AIL.LOCATION_ID) + ' ' + IBL.BIN_LOC AS BIN_LOC, " +
                           "PAR_LEVEL,MAXIMUM_INV,ITEM_DESCRIPTION,MFG_CAT_NUM,  " +
                           "UOM + '' + CONVERT(VARCHAR(3), UOM_FACTOR) + '' + BASE_UOM AS PKG_STR  " +
                           "FROM ITEM_BIN_LOCATION IBL  " +
                           "JOIN AHI_ITEM_ALIAS AIA ON AIA.Item_Id = IBL.Item_Id  " +
                           "JOIN D_INVENTORY_ITEMS DII ON DII.Item_Id = IBL.Item_Id  " +
                           "JOIN D_SUPPLY_ITEM DSI ON DSI.Supply_Item_Id = IBL.Item_Id  " +
                           "JOIN D_SUPPLY_SOURCE_ITEM DSSI ON DSSI.Supply_Item_Id = DSI.Supply_Item_Id  " +
                           "JOIN D_VENDOR_ITEM_PACKAGING DVIP ON DVIP.Item_Id = DSSI.Supply_Item_Id  " +
                           "JOIN AHI_INVENTORY_LOCATION AIL ON AIL.System_Location_ID = IBL.System_Location_Id  " +
                           "WHERE DII.ACTIVE_FLAG = 1  " +
                           "AND NOT(PAR_LEVEL = 0 AND Maximum_Inv = 0)  ";
            return query;
        }

        private string BuildQuery()
        {   //this script draws from the production db so once the view object is put there then this will read
            //Select * from [dbo].[vParFormItemSelectUW]  ( or [vParFormItemSelectHMC])
            //this is the same script that is behind the view object on the HEMM test servers UVM-HEMMDB-T.UWM_ICDB and 
            //HVM-HEMMDB-T.UWM_ICDB  on both servers, the view is named vParFormItemSelect 
            string hosp = "";
            switch (entity)
            {
                case "[h-hemm]":
                    hosp = "HMC";
                    break;
                case "[u-hemm]":
                    hosp = "UWMC";
                    break;                  
            }
            string query =             
                "SELECT DISTINCT dbo.ITEM.ITEM_ID, dbo.ITEM.ITEM_NO, RTRIM(dbo.REQ.REQ_NO) AS [PAR FORM], dbo.REQ_ITEM.LINE_NO, " +
                    "dbo.ITEM.DESCR, dbo.REQ_ITEM.QTY AS PAR, SUBSTRING(dbo.REQ_ITEM.UM_CD, 7, 2) AS[PAR UM], dbo.ITEM.CTLG_NO, " +
                  "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.UM_CD), 2) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN dbo.ITEM_VEND_PKG IVP ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.TO_UM_CD = IVPF.TO_UM_CD " +
                   "AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + '' + " + 
                   "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_QTY), 4) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + '' + " +
                   "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_UM_CD), 2) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(1) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + '' +  " +
				   "(SELECT ISNULL(CAST((SELECT RIGHT(RTRIM(IVPF.TO_QTY), 4) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(2) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) + '' + " +
                   "(SELECT ISNULL(CAST((SELECT CAST(RIGHT(RTRIM(IVPF.TO_UM_CD), 2) AS VARCHAR(4)) FROM dbo.ITEM_VEND_PKG_FACTOR IVPF " +
                   "JOIN  dbo.ITEM_VEND_PKG IVP  ON IVPF.ITEM_VEND_ID = IVP.ITEM_VEND_ID " +
                   "WHERE SEQ_NO IN(2) AND IVP.ITEM_VEND_ID = ITEM_VEND_PKG.ITEM_VEND_ID AND IVP.UM_CD <> IVP.TO_UM_CD " +
                   "AND IVP.TO_UM_CD = IVPF.TO_UM_CD AND IVP.UM_CD = IVPF.UM_CD) AS VARCHAR(4)),'')) AS PKG_STR, " +

                  "ISNULL(REQ_ITEM.PAR_BIN_LOC, '') AS[BIN LOC], '" + hosp + "' AS ENTITY " +        

            "FROM dbo.REQ INNER JOIN dbo.REQ_ITEM ON dbo.REQ.REQ_ID = dbo.REQ_ITEM.REQ_ID INNER JOIN " +
                "dbo.ITEM ON dbo.REQ_ITEM.ITEM_ID = dbo.ITEM.ITEM_ID LEFT OUTER JOIN " +
                "dbo.ITEM_VEND ON dbo.ITEM.ITEM_ID = dbo.ITEM_VEND.ITEM_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG ON dbo.ITEM_VEND.ITEM_VEND_ID = dbo.ITEM_VEND_PKG.ITEM_VEND_ID INNER JOIN " +
                "dbo.ITEM_VEND_PKG_FACTOR ON dbo.ITEM_VEND_PKG.ITEM_VEND_ID = dbo.ITEM_VEND_PKG_FACTOR.ITEM_VEND_ID AND " +
                "dbo.ITEM_VEND_PKG.UM_CD = dbo.ITEM_VEND_PKG_FACTOR.UM_CD AND " +
                "dbo.ITEM_VEND_PKG.TO_UM_CD = dbo.ITEM_VEND_PKG_FACTOR.TO_UM_CD " +

                "WHERE(ITEM.STAT IN(1, 2)) AND(ITEM_VEND.SEQ_NO = 1) AND(ITEM_VEND_PKG.SEQ_NO = 1) AND(REQ.REQ_TYPE = 3) AND " +
                "(REQ.STAT = 13) AND(REQ_ITEM.QTY > 0) AND (ITEM_VEND.CORP_ID = 1000) " +

                "GROUP BY ITEM.ITEM_ID, ITEM.ITEM_NO, REQ.REQ_NO, REQ_ITEM.LINE_NO, ITEM.DESCR, " +
                "REQ_ITEM.QTY, REQ_ITEM.UM_CD, ITEM.CTLG_NO, ITEM_VEND_PKG.UM_CD,  " +
                "ITEM_VEND_PKG_FACTOR.TO_QTY, ITEM_VEND_PKG.TO_UM_CD, REQ_ITEM.PAR_BIN_LOC, ITEM_VEND_PKG.ITEM_VEND_ID  " +

                "ORDER BY [PAR FORM], REQ_ITEM.LINE_NO ";

            return query;
        }
       
        private string GetUserConfig()
        {//this completes the path to where the credentials are found. The UserConfig.txt file for the specific entity
         //contains an encrypted string which is the user name and password. You can see for yourself by launching the 
         //EncryptAndHash app, and copy the encrypted string into it. The Key (typically the user name) can be found in
         //the source code source code directory for WaspRefresh

            //if you need to add to this if/else list then start by  
            //running EncryptAndHash.exe and create an encrypted string for the user name and password - as in
            //user name=rosco; password = [rosco's password]
            //put that encrypted string into a text file and use the if block below as a template for where to 
            //save it.
            string deCipher = "";
            try
            {
                string userConfig = ConfigSettings.Get("configFilePath");                
                if (entity == "[h-hemm]")
                    userConfig += "HMC\\UserConfig.txt";
                else if (entity == "[u-hemm]")
                    userConfig += "UWMC\\UserConfig.txt";
                else if (entity == "[n-hemm]")
                    userConfig += "NWH\\UserConfig.txt";
                else if (entity == "[n_parforms]")
                    userConfig += "NWH\\UserConfig.txt";                    
                else if (entity == "[medstores_connect]")                    
                    userConfig += "MedStores\\UserConfig.txt";

                string[] key = File.ReadAllLines(userConfig);
                string user = entity == "[h-hemm]" ? "intelliweb" : "RIO"; //RIO for UW, NWH and MedStores
                deCipher = StringCipher.Decrypt(key[0], user);
            }
            catch (Exception ex)
            {
                lm.Write("DataManager.GetUserConfig() " + ex.Message);
                deCipher = "";
            }
            return deCipher;
        }
    }
    
}
