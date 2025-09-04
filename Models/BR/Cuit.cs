using Applet.Nat.Api.DC;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Static;
using Azure.Core;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.Tokens;
using Nat.Api.Properties;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Utilities;
using System.Net.Http.Headers;
using System.Text;
namespace Applet.Nat.Api.Br.Models
{
    public class Cuit
    {
        #region CONSTRUCT
        public Cuit() { }
        public Cuit(long vivlngCuit, NatContext vioContext)
        {
            mioContext = vioContext;
            CuitModel lioCuitModel = mioContext.Cuits.Find(vivlngCuit);
            if (lioCuitModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "C.U.I.T.", "a"));
            ioDcModel = lioCuitModel;
        }
        #endregion
        #region PUBLIC PROPS
        public CuitModel ioDcModel { get; set; }
        public CuitCnfg? ioCnfg
        {
            get
            {
                return JsonConvert.DeserializeObject<CuitCnfg>(Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ioDcModel.ivstrCnfg ?? string.Empty, Encoding.UTF8))));
            }
        }
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
                        mioContext.Cuits.Remove(ioDcModel);
                        mioContext.SaveChanges();
                        break;
                    }
                case eTask.Save:
                    {
                        CuitModel lioDbCuitModel = mioContext.Cuits.Find(ioDcModel.ivlngCuit);
                        if (lioDbCuitModel != null)
                        {
                            lioDbCuitModel.ivstrCuitRS = ioDcModel.ivstrCuitRS;
                            lioDbCuitModel.ivstrCnfg = ioDcModel.ivstrCnfg;
                            mioContext.Cuits.Update(lioDbCuitModel);
                        }
                        else
                            mioContext.Cuits.Add(ioDcModel);
                        mioContext.SaveChanges();
                        break;
                    }
            }
        }
        public Encoding GetEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (string.IsNullOrEmpty(ioCnfg.ivstrEncoding))
                return Encoding.UTF8;
            if (int.TryParse(ioCnfg.ivstrEncoding, out int livnum))
                return Encoding.GetEncoding(livnum);
            return Encoding.GetEncoding(ioCnfg.ivstrEncoding);
        }
        #endregion
        #region PRIVATE METHODS
        #endregion
    }
}