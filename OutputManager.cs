using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using OleDBDataManager;
using LogDefault;
using SpreadsheetLight;


namespace WaspRefresh
{
    class OutputManager
    {
        private LogManager lm = LogManager.GetInstance();
        private ODMDataFactory ODMDataSetFactory = null;
        private NameValueCollection ConfigSettings = null;
        private string outFilePath = "";
        private string entity = "";
        private string sheetName = "";
        private DataSet dSet = new DataSet();
        private string colHeaders = "";
        private SLDocument sldWaspSourceFile = new SLDocument();

        public string ColHeaders
        {
            set{colHeaders = value;}
        }

        public string Entity
        {
            set { entity = value;
                CompleteFilePath();
            }
        }

        public DataSet DSet
        {
            set { dSet = value; }
        }

        public OutputManager()
        {
            ODMDataSetFactory = new ODMDataFactory();
            ConfigSettings = (NameValueCollection)ConfigurationManager.GetSection("appSettings");
            lm.LogFile = ConfigSettings.Get("logFile");
            lm.LogFilePath = ConfigSettings.Get("logFilePath");
            outFilePath = ConfigSettings.Get("xport_path");
        }

        private void CompleteFilePath()
        {
            string storesPath = "";
            if (entity.Equals("[h-hemm]"))
            {
                outFilePath += "HMC";
                sheetName = "HMCDataSource";
            }
            else if (entity.Equals("[u-hemm]"))
            {
                outFilePath += "UWMC";
                sheetName = "UWMCDataSource";
            }
            else if (entity.Equals("[n-hemm]"))
            {
                outFilePath += "NWH";
                sheetName = "NWHDataSource";               
            }
            if (entity.Equals("[h-hemm-dj]"))
            {
                outFilePath += "DoorJambLabels\\HMCDoorJambLabels.xlsx"; // HMCDoorJambLabels
                sheetName = "HMCDataSource";
            }
            else if (entity.Equals("[u-hemm-dj]"))
            {
                outFilePath += "DoorJambLabels\\UW-NWHDoorJambLabels.xlsx";
                sheetName = "UWMCDataSource";
            }
            else if (entity.Equals("[n-hemm-dj]"))
            {
                outFilePath += "DoorJambLabels\\UW-NWHDoorJambLabels.xlsx";
                sheetName = "UW-NWHDataSource";
            }
            else if (entity.Equals("[mpous]"))
            {
                outFilePath += "MPOUS";
                sheetName = "MPOUSDataSource";
            }
            else if(entity.Equals("[medstores_connect]"))
            {
                outFilePath += "MedStores";
                sheetName = "MedStoresDataSource";
                //the medstores data source doesn't have an underscore.
                storesPath = ConfigSettings.Get("out_file_name") + ".xlsx";
                storesPath = storesPath.Substring(1);
                outFilePath += storesPath;
            }
            if (storesPath.Length == 0)
            {
                if (!outFilePath.Contains(".xlsx"))
                    outFilePath += ConfigSettings.Get("out_file_name") + ".xlsx";
            }
        }

        public void CreateSpreadsheet()
        {
            int dataColNo = 0;
            int colNo = 1;
            int rowNo = 1;
            try
            {
                SetColHeaders();
                foreach (DataRow dRow in dSet.Tables[0].Rows)
                {
                    dataColNo = 0;
                    colNo = 1;
                    rowNo++;
                    foreach (object colData in dRow.ItemArray)
                    {
                        sldWaspSourceFile.SetCellValue(rowNo, colNo++, colData.ToString().Trim());
                        dataColNo++;
                    }
                }
                sldWaspSourceFile.RenameWorksheet(SLDocument.DefaultFirstSheetName, sheetName);
                sldWaspSourceFile.SaveAs(outFilePath);
            }catch(Exception ex)
            {
                lm.Write(entity + "  OutputManager.CreateSpreadsheet() " + ex.Message);
            }
        }

        private void SetColHeaders()
        {
            int rowNo = 1;
            int colNo = 1;
            string[] colNames = colHeaders.Split("|".ToCharArray());
            foreach (string cname in colNames)
            {
                sldWaspSourceFile.SetCellValue(rowNo, colNo++, cname);
            }
        }
    }
}
