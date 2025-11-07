using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Static;
using Azure.Core;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.Tokens;
using Nat.API.Properties;
using Nat.API.Models.BR;
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
        public Cuit(CuitModel vioCuitModel, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioConfiguration = vioConfiguration;
            mioContext = vioContext;
            ioDcModel = vioCuitModel;
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
        private IConfiguration mioConfiguration;
        private NatContext mioContext;
        #endregion
        #region PUBLICS METHODS
        public void SetDC(NatContext vioContext)
        {
            mioContext = vioContext;
        }
        public async Task Task()
        {
            switch (ieTask)
            {
                case eTask.Delete:
                    mioContext.Cuits.Remove(ioDcModel);
                    mioContext.SaveChanges();
                    break;
                case eTask.Save:
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
        public Encoding GetEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding") == null)
                return Encoding.UTF8;
            if (int.TryParse(ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding")?.ivstrValue, out int livnum))
                return Encoding.GetEncoding(livnum);
            return Encoding.GetEncoding(ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Encoding")?.ivstrValue);
        }
        public IDocsIO getIDocsIO()
        {
            IDocsIO liIDocsIO = null;
            eLoadMethod lioeLoadMethod;
            FileIO lioFileIO;
            lioeLoadMethod = (eLoadMethod)short.Parse(ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "LoadMethod")?.ivstrValue);
            switch (lioeLoadMethod)
            {
                case eLoadMethod.File:
                    lioFileIO = new FileIO(mioConfiguration, mioContext);
                    lioFileIO.ivlngCuit = ioDcModel.ivlngCuit;
                    lioFileIO.ivstrPathIn = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "InFolder")?.ivstrValue ?? string.Empty;
                    lioFileIO.ivstrPathOut = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "OutFolder")?.ivstrValue ?? string.Empty;
                    lioFileIO.coFileExtensions = new string[] { ".txt" };
                    lioFileIO.ioMapper = ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrWs == "rta");
                    liIDocsIO = lioFileIO;
                    break;
                case eLoadMethod.OracleCanonical:
                    OracleCanonical lioOracleCanonical = new OracleCanonical(mioConfiguration, mioContext);
                    lioOracleCanonical.ivlngCuit = ioDcModel.ivlngCuit;
                    lioOracleCanonical.ivstrPathIn = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "InFolder")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrPathOut = ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "OutFolder")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrUser = ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "User")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrPass = ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Pass")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivstrEntityID = ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "EntityID")?.ivstrValue ?? string.Empty;
                    lioOracleCanonical.ivnroDays = short.Parse(ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Days")?.ivstrValue ?? "0");
                    lioOracleCanonical.ivnroRows = short.Parse(ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Rows")?.ivstrValue ?? "0");
                    liIDocsIO = lioOracleCanonical;
                    break;
                default:
                    break;
            }
            return liIDocsIO;
        }
        #endregion
        #region PRIVATE METHODS
        #endregion
    }
}