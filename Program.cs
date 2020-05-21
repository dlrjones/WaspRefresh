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
        private static string entity = "";   //[h-hemm] [u-hemm]  [n-hemm]  [medstores_connect]  [mpous] [h-hemm-dj] [u-hemm-dj] [n-hemm-dj]
        private static DataSet dSet = new DataSet();
        private static string colHdr = "";
        private static LogManager lm = LogManager.GetInstance();

        static void Main(string[] args)
        {
     // --->>>   in app.config, set the xport_path for test or prod   
            entity = args[0];
            if (args.Length > 1)
            {  //the fact of a second param means it's running door jamb labels
                //so to set up a Scheduled Task you'd put   [h-hemm] djamb
                //what you type for the second param doesn't matter just don't put a space in it
                switch (entity)
                {
                    case "[h-hemm]":
                        entity = "[h-hemm-dj]";
                        break;
                    case "[u-hemm]":
                        entity = "[u-hemm-dj]";  
                        break;
                    case "[n-hemm]":
                        entity = "[n-hemm-dj]";
                        break;
                }
            }
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
