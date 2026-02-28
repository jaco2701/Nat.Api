using Applet.Nat.Api.DC;
using Nat.API.Properties;

namespace Applet.Nat.Api.Static
{
    public static class ListHelper
    {
        public static ListModel Get(string vivcodType, string vivcodId, NatContext vioContext)
        {
            if (string.IsNullOrEmpty(vivcodType))
                throw new Exception(Resources.lioE_ListTypeNo);
            if (string.IsNullOrEmpty(vivcodId))
                throw new Exception(Resources.lioE_ListIdNo);
            return vioContext.Lists.Find(vivcodType.ToUpper(), vivcodId);
        }
        public static ListModel[] GetAll(string vivcodLista, NatContext vioContext)
        {
            if (string.IsNullOrEmpty(vivcodLista))
                throw new Exception(Resources.lioE_ListTypeNo);
            ListModel[] lcoListasModel = vioContext.Lists.Where(x => x.ivcodType == vivcodLista.ToUpper()).ToArray();
            return lcoListasModel;
        }
        public static string GetValue(string vivcodType, string vivcodId, NatContext vioContext)
        {
            ListModel lioListModel = Get(vivcodType.ToUpper(), vivcodId, vioContext);
            if (lioListModel == null)
                throw new Exception(string.Format(Resources.lioE_ListValNo, vivcodType, vivcodId));
            return lioListModel.ivstrDesc;
        }
        public static Boolean ContainKey(string livstrType, string livstrId, NatContext vioContext)
        {
            return vioContext.Lists.FirstOrDefault(x => x.ivcodType == livstrType.ToUpper() && x.ivcodId == livstrId) != null;
        }
        private static Dictionary<string, short> scoStatus;
        //public static short DocStatus(string vistrStatus, NatContext vioContext)
        //{
        //    if (scoStatus == null)
        //    {
        //        scoStatus = new Dictionary<string, short>();
        //        foreach (ListModel lioO in vioContext.Lists.Where(x => x.ivcodType == "STATUS"))
        //        {
        //            scoStatus.Add(lioO.ivstrDesc, short.Parse(lioO.ivcodId));
        //        }
        //    }
        //    return scoStatus.GetValueOrDefault(vistrStatus);
        //}
        public static void SetProcessRunning(NatContext vioContext, string vivstrProcess)
        {
            ListModel? lioO = vioContext.Lists.Find("DOCPROC", vivstrProcess);
            if (lioO == null)
                throw new Exception(Resources.lioE_ListTypeNo);
            lioO.ivstrDesc = "-1";
            vioContext.Lists.Update(lioO);
            vioContext.SaveChanges();
        }
        public static void SetProccessEnd(NatContext vioContext, string vivstrProcess)
        {
            ListModel? lioO = vioContext.Lists.Find("DOCPROC", vivstrProcess);
            if (lioO == null)
                throw new Exception(Resources.lioE_ListTypeNo);
            lioO.ivstrDesc = Format.TimestampFromDate(DateTime.UtcNow).ToString();
            vioContext.Lists.Update(lioO);
            vioContext.SaveChanges();
        }
        public static bool Verbose(NatContext vioContext)
        {
            ListModel? lioO = vioContext.Lists.Find("FORMAT", "VERBOSE");
            if (lioO == null || lioO.ivstrDesc == null)
                return false;
            return lioO.ivstrDesc =="1";
        }
        public static bool CanRun(NatContext vioContext, string vivstrProcess)
        {
            string livstr="Secs";
            if (vivstrProcess == "0")
                livstr += "Docs";
            else if (vivstrProcess == "1")
                livstr += "Ftp";
            else if (vivstrProcess == "2")
                livstr += "Ocan";
            else if (vivstrProcess == "3")
                livstr += "Daily";
            else
                return false;
            ListModel? lioO = vioContext.Lists.Find("FORMAT", livstr);
            if (lioO == null)
            {
                LogHelper.write(new Exception(string.Format(Resources.lioE_ListValNo, "FORMAT", livstr)));
                return false;
            }
            if (!int.TryParse(lioO.ivstrDesc, out int livnumSecs))
            {
                LogHelper.write(new Exception($"Valor de Lista FORMAT:{livstr} no es numerico"));
                return false;
            }
            if (livnumSecs < 0)
                return false;
            lioO = vioContext.Lists.Find("DOCPROC", vivstrProcess);
            if (lioO == null)
            {
                LogHelper.write(new Exception(string.Format(Resources.lioE_ListValNo, "DOCPROC", vivstrProcess)));
                return false;
            }
            if (string.IsNullOrEmpty(lioO.ivstrDesc.Trim()))
                return true;
            if (!long.TryParse(lioO.ivstrDesc, out long livlngTnsLastRun))
            {
                LogHelper.write(new Exception(string.Format(Resources.lioE_ListValNo, "DOCPROC", vivstrProcess)));
                return false;
            }
            return livlngTnsLastRun>=0 && (livlngTnsLastRun + livnumSecs ) < Format.TimestampFromDate(DateTime.UtcNow);
        }
    }

}

