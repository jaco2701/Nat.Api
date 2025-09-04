using Applet.Nat.Api.DC;

namespace Applet.Nat.Api.Static
{
    public static class ListHelper
    {
        public static ListModel Get(string vivcodType, string vivcodId, NatContext vioContext)
        {
            if (string.IsNullOrEmpty(vivcodType))
                throw new Exception("Tipo de Lista de Valores Invalido");
            if (string.IsNullOrEmpty(vivcodId))
                throw new Exception("Id. de Lista de Valores Invalido");
            return vioContext.Lists.Find(vivcodType.ToUpper(), vivcodId);
        }
        public static ListModel[] GetAll(string vivcodLista, NatContext vioContext)
        {
            if (string.IsNullOrEmpty(vivcodLista))
                throw new Exception("Tipo de Lista de Valores Invalido");
            ListModel[] lcoListasModel = vioContext.Lists.Where(x => x.ivcodType == vivcodLista.ToUpper()).ToArray();
            return lcoListasModel;
        }
        public static string GetValue(string vivcodType, string vivcodId, NatContext vioContext)
        {
            ListModel lioListModel = Get(vivcodType.ToUpper(), vivcodId, vioContext);
            if (lioListModel == null)
                throw new Exception(string.Format("Valor de Lista {0}:{1} no encontrado", vivcodType, vivcodId));
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
        public static void DocsProcces(NatContext vioContext, string vivstr )
        {
            ListModel? lioO = vioContext.Lists.Find("DOCPROC", "0");
            if (lioO == null)
                throw new Exception("Tipo de Lista de Valores Invalido");
            lioO.ivstrDesc = vivstr;
            vioContext.Lists.Update(lioO);
            vioContext.SaveChanges();
        }
    }

}

