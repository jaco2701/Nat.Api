using Applet.Nat.Api.DC;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Static;
using Azure.Core;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Nat.Api.Properties;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Utilities;
using System.Net.Http.Headers;
using System.Text;
namespace Applet.Nat.Api.Br.Models
{
    public class List
    {
        #region CONSTRUCT
        public List() { }
        public List(long vivlngList, NatContext vioContext)
        {
            mioContext = vioContext;
            ListModel lioListModel = mioContext.Lists.Find(vivlngList);
            if (lioListModel == null)
                throw new Exception($"Lista { Resources.lioE_ObjectNoF }");
            ioDcModel = lioListModel;
        }
        #endregion
        #region PUBLIC PROPS
        public ListModel ioDcModel { get; set; }
        public eTask ieTask { get; set; }
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext;
        #endregion
        #region PUBLICS METHODS
        public void SetDC(NatContext vioContext)
        {
            mioContext = vioContext;
        }
        public void Task()
        {
            switch (ieTask)
            {
                case eTask.Delete:
                    {
                        mioContext.Lists.Remove(ioDcModel);
                        mioContext.SaveChanges();
                        break;
                    }
                case eTask.Save:
                    {
                        ListModel lioDbListModel = mioContext.Lists.Find(ioDcModel.ivcodType, ioDcModel.ivcodId);
                        if (lioDbListModel != null)
                        {
                            lioDbListModel.ivstrDesc= ioDcModel.ivstrDesc;
                            mioContext.Lists.Update(lioDbListModel);
                        }
                        else
                            mioContext.Lists.Add(ioDcModel);
                        mioContext.SaveChanges();
                        break;
                    }
            }
        }
        #endregion
        #region PRIVATE METHODS
        #endregion
    }
}