using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using LogDefault;

namespace WaspRefresh
{
    class Program
    {
        private static string entity = "";   //either [h-hemm] or [u-hemm]
        private static DataSet dSet = new DataSet();
        private static string colHdr = "";
        private static LogManager lm = LogManager.GetInstance();

        static void Main(string[] args)
        {
     //       in app.config, set the xport_path for test or prod

            entity = args[0];
             lm.Write(entity);
            RefreshSpreadsheet();
            SaveSpreadsheet();
        }

        private static void RefreshSpreadsheet()
        {
            try
            {
                DataManager dm = new DataManager();
                dm.Entity = entity;
                dm.Process();
                dSet = dm.DSet;
                colHdr = dm.ColHeaders;
            }catch(Exception ex)
            {
                lm.Write("Main.RefreshSpreadsheet() " + ex.Message);
            }
        }

        private static void SaveSpreadsheet()
        {
            try
            {
                OutputManager om = new OutputManager();
                om.Entity = entity;
                om.DSet = dSet;
                om.ColHeaders = colHdr;
                om.CreateSpreadsheet();
            }
            catch (Exception ex)
            {
                lm.Write(entity + "Main.SaveSpreadsheet() " + ex.Message);
            }
        }
    }
}
