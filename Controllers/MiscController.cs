using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Static;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Models.BR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nat.API.Properties;
using System.Net.Http.Headers;
using Applet.Nat.Api.Ifaces;
using Newtonsoft.Json;
using System.Text;
using System.Xml;
using Applet.Nat.BR;
using OfficeOpenXml.Table.PivotTable;

namespace Applet.Nat.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MiscController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly IConfiguration mioConfiguration;
        public MiscController(NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioContext = vioContext;
            mioConfiguration = vioConfiguration;
        }
        [HttpGet("Statics")]
        public async Task<Response> Statics()
        {

            List<ListModel> lcoLists= new List<ListModel>();
            string[] lcoTypes = ListHelper.GetValue("STATICS", "1", mioContext).Split(',');
            if (lcoTypes.Length == 0)
                throw new Exception(Resources.lioE_NoStatics);
            lcoLists = mioContext.Lists.Where(x => lcoTypes.Contains(x.ivcodType)).ToList();
            IDocument lio;
            Double livvalCtz;
            try
            {
                lio = new DocumentExp(new DocumentModel { ivlngCuitEmisor = long.Parse(ListHelper.GetValue("CUIT", "0", mioContext)) }, mioContext);
                livvalCtz = await lio.GetCotizacion("DOL", DateTime.Today.AddDays(-1));
                lcoLists.Add(new ListModel { ivcodType = "CTZ", ivcodId = "DOLEXP", ivstrDesc = livvalCtz.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-AR")) });
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
            try
            {
                lio = new DocumentV1(new DocumentModel { ivlngCuitEmisor = long.Parse(ListHelper.GetValue("CUIT", "0", mioContext)) }, mioContext);
                livvalCtz = await lio.GetCotizacion("DOL", DateTime.Today.AddDays(-1));
                lcoLists.Add(new ListModel { ivcodType = "CTZ", ivcodId = "DOLV1", ivstrDesc = livvalCtz.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-AR")) });
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
            try
            {
                lio = new DocumentMTXCA(new DocumentModel { ivlngCuitEmisor = long.Parse(ListHelper.GetValue("CUIT", "0", mioContext)) }, mioContext);
                livvalCtz = await lio.GetCotizacion("DOL", DateTime.Today.AddDays(-1));
                lcoLists.Add(new ListModel { ivcodType = "CTZ", ivcodId = "DOLMTX", ivstrDesc = livvalCtz.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-AR")) });
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
            return ResponseHelper.Get(new { coLists = lcoLists, coIdentityProviders = mioContext.IdentityProviders.Where(x => x.ivblnEnable == true).ToList() });
        }
        [HttpPost("Rs")]
        public Response Rs([FromBody] long vivlngCuit)
        {
            try
            {
                string livstrCuitRS = string.Empty;
                try
                {
                    CuitModel lioCuitModel = mioContext.Cuits.Find(vivlngCuit);
                    livstrCuitRS = lioCuitModel?.ivstrCuitRS ?? string.Empty;
                }
                catch (Exception lioE)
                {
                    {
                        LogHelper.write(lioE);
                    }
                }
                return ResponseHelper.Get(livstrCuitRS);

            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }
        [HttpPost("Log")]
        public Response Log([FromBody] string vivFilename)
        {
            try
            {
                string livstrCuitRS = string.Empty;
                try
                {
                    string livstrPath = "./log";
                    if (!Directory.Exists(livstrPath))
                        Directory.CreateDirectory(livstrPath);
                    livstrPath += "/";
                    livstrPath += vivFilename;
                    StringResponse lioStringResponse = new StringResponse();
                    if (!System.IO.File.Exists(livstrPath))
                        return ResponseHelper.Get(string.Empty);
                    return ResponseHelper.Get(Convert.ToBase64String(System.IO.File.ReadAllBytes(livstrPath)));
                }
                catch (Exception lioE)
                {
                    LogHelper.write(lioE);
                    return ResponseHelper.Get(-1, lioE);
                }

            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }
        [HttpGet("Queue")]
        public async Task<Response> Queue()
        {
            try
            {
                //VALIDACION
                string[] lcvstrCreds = HttpsHeaderHelper.GetCredencials(AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]));
                if (lcvstrCreds.Length != 3)
                    throw new Exception(Resources.lioE_NoCreds);
                User lioUser = new User(lcvstrCreds[1], mioContext);
                lioUser.ivstrPass = lcvstrCreds[2];
                lioUser.ieTask = eTask.Auth;
                lioUser.Task();
                //EJECUCION
                if (ListHelper.GetValue("DOCPROC", "0", mioContext) == "1")
                    throw new Exception(Resources.lioE_ProcRun);
                ListHelper.DocsProcces(mioContext, "1");
                await Static.Queue.Run(mioContext, mioConfiguration);
                ListHelper.DocsProcces(mioContext, "0");
                return ResponseHelper.Get("OK");
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                ListHelper.DocsProcces(mioContext, "0");
                return ResponseHelper.Get(-1, lioE);
            }
        }
        [HttpPost("CompUncomp")]
        public string unzip([FromBody] DocumentUploadRequest vioO)
        {
            try
            {
                string livstr;
                if (vioO.ivstrName.StartsWith("U"))
                {
                    livstr = Format.UnCompress(vioO.ivstrData,Encoding.UTF8);
                    if (vioO.ivstrName.EndsWith("64"))
                        livstr = Encoding.UTF8.GetString(Convert.FromBase64String(livstr));
                }
                else
                {
                    if (vioO.ivstrName.EndsWith("64"))
                        livstr = Convert.ToBase64String(Encoding.UTF8.GetBytes(vioO.ivstrData));
                    else
                        livstr = vioO.ivstrData;
                    livstr = Format.Compress(livstr);
                }
                return livstr;
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return string.Empty;
            }
        }
        //[HttpPost("fixv1")]
        //public string fixv1([FromBody] DocumentUploadRequest vioO)
        //{
        //    try
        //    {
        //        long[] lcvlngDoc = { };
        //        DocumentModel[] lcoDocs;
        //        DocumentUser[] lcoDocumentUsers;
        //        IRawDocument liiInDocument;
        //        if (!string.IsNullOrEmpty(vioO.ivstrData))
        //        {
        //            lcvlngDoc = vioO.ivstrData.Split(',').Select(x => long.Parse(x)).ToArray();
        //            lcoDocs = mioContext.Documents.Where(x => x.ivdblImporte == null && lcvlngDoc.Contains(x.ivlngDoc)).ToArray();
        //        }
        //        else
        //            lcoDocs = mioContext.Documents.Where(x => x.ivdblImporte == null).ToArray();
        //        foreach (DocumentModel lioDoc in lcoDocs)
        //        {
        //            try
        //            {
        //                if (string.IsNullOrEmpty(lioDoc.ivstrInData)) continue;
        //                string livstrRaw = Format.UnCompress(lioDoc.ivstrInData).Replace("<Document>", "<Document><ws>wsfe</ws>");
        //                livstrRaw = Format.Compress(Convert.ToBase64String(Encoding.UTF8.GetBytes(livstrRaw)));
        //                liiInDocument = new InDocumentXML(new Token { ivlngCuit = 301710061447 }, mioContext) { ivstrRaw = livstrRaw };
        //                lcoDocumentUsers = liiInDocument.ToUserDocuments();
        //                if (lcoDocumentUsers != null && lcoDocumentUsers.Length > 0)
        //                {
        //                    lioDoc.ivdblImporte = lcoDocumentUsers[0].ivdblImporteTotal ?? 0;
        //                    lioDoc.ivdtmEmision = DateTime.ParseExact(lcoDocumentUsers[0].ivdtmEmision, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None);
        //                    lioDoc.ivstrInData = livstrRaw;
        //                    mioContext.Documents.Update(lioDoc);
        //                }
        //            }
        //            catch (Exception lioE)
        //            {
        //                LogHelper.write(lioE);
        //                continue;
        //            }
        //        }

        //        DocumentTrackingModel[] lcoDocumentTrackingModels;
        //        if (!string.IsNullOrEmpty(vioO.ivstrData))
        //        {
        //            lcvlngDoc = vioO.ivstrData.Split(',').Select(x => long.Parse(x)).ToArray();
        //            lcoDocumentTrackingModels = mioContext.DocumentTrackings.Where(x => lcvlngDoc.Contains(x.ivlngDoc)).OrderBy(x => x.ivlngDoc).ThenBy(x => x.ivdtmTrack).ToArray();
        //        }
        //        else
        //            lcoDocumentTrackingModels = mioContext.DocumentTrackings.OrderBy(x => x.ivlngDoc).ThenBy(x => x.ivdtmTrack).ToArray();
        //        long livlngDoc = 0;
        //        short livnroStatus = 0;
        //        int livnumTrack = 0;
        //        XmlDocument lioXml;
        //        foreach (DocumentTrackingModel lioDocumentTrackingModel in lcoDocumentTrackingModels)
        //        {
        //            try
        //            {
        //                if (livlngDoc != lioDocumentTrackingModel.ivlngDoc)
        //                {
        //                    livlngDoc = lioDocumentTrackingModel.ivlngDoc;
        //                    livnumTrack = 0;
        //                    if (livnroStatus == 0)
        //                    {
        //                        DocumentModel lioDocumentModel = mioContext.Documents.Find(livlngDoc);
        //                        if (lioDocumentModel != null)
        //                            livnroStatus = lioDocumentModel.ivnroStatus;
        //                    }
        //                }
        //                livnumTrack++;
        //                lioDocumentTrackingModel.ivnumTrack = livnumTrack;
        //                if (!string.IsNullOrEmpty(lioDocumentTrackingModel.ivstrwsData))
        //                {
        //                    lioDocumentTrackingModel.ivstrData = lioDocumentTrackingModel.ivstrwsData;
        //                }
        //                if (!string.IsNullOrEmpty(lioDocumentTrackingModel.ivstrData) && lioDocumentTrackingModel.ivstrData.StartsWith("<FECAEResponse")) //busca autorizacion
        //                {
        //                    lioXml = new XmlDocument();
        //                    lioXml.LoadXml(lioDocumentTrackingModel.ivstrData.Substring(0, lioDocumentTrackingModel.ivstrData.Length - 1));
        //                    lioDocumentTrackingModel.ivnroStatus = lioXml.SelectSingleNode("//FECAEResponse/FeDetResp/FECAEDetResponse/Resultado")?.InnerXml == "A" ?50 : 40;
        //                    livnroStatus = lioDocumentTrackingModel.ivnroStatus;
        //                }
        //                mioContext.DocumentTrackings.Update(lioDocumentTrackingModel);
        //            }
        //            catch (Exception lioE)
        //            {
        //                LogHelper.write(lioE);
        //                continue;
        //            }
        //        }
        //        if (livnroStatus == 0)
        //        {
        //            DocumentModel lioDocumentModel = mioContext.Documents.Find(livlngDoc);
        //            if (lioDocumentModel != null)
        //                livnroStatus = lioDocumentModel.ivnroStatus;
        //        }
        //        mioContext.SaveChanges();
        //        return "OK";
        //    }
        //    catch (Exception lioE)
        //    {
        //        LogHelper.write(lioE);
        //        return string.Empty;
        //    }
        //}
        [HttpGet("fixRsMoneda")]
        public string fixRsMoneda()
        {
            try
            {
                long[] lcvlngDoc = { };
                Document lioDocument;
                DocumentUser[] lcoDocumentUsers;
                IRawDocument liiInDocument;
                foreach (DocumentModel lioDocumentModel in mioContext.Documents.Where(x => string.IsNullOrEmpty(x.ivstrMoneda) || string.IsNullOrEmpty(x.ivstrRazonSocial)))
                {
                    lioDocument = new Document(lioDocumentModel, mioContext);
                    lioDocument.ioDcModel.ivstrMoneda = lioDocument.ioDocumentUser?.ivstrMoneda ?? string.Empty;
                    lioDocument.ioDcModel.ivstrRazonSocial = lioDocument.ioDocumentUser?.ivstrRazonSocial ?? string.Empty;
                    mioContext.Documents.Update(lioDocument.ioDcModel);
                    mioContext.SaveChanges();
                }
                return "OK";
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return string.Empty;
            }
        }

    }
}

