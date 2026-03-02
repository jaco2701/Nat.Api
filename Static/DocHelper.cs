using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Microsoft.Extensions.Configuration;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Text;
using System.Threading.RateLimiting;

namespace Applet.Nat.Api.Static
{
    public static class DocHelper
    {
        public static List<DocumentUploadResponse> UploadDocument(DocumentUploadRequest vioDocumentsUpload, IConfiguration vioConfiguration)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                List<DocumentUploadResponse> lcoDocumentUserResponse = new List<DocumentUploadResponse>();

                Document lioDocument;
                if (string.IsNullOrEmpty(vioDocumentsUpload.ivstrName))
                    throw new Exception($"Nombre de Documento {Resources.lioE_ObjectNoM}");
                if (string.IsNullOrEmpty(vioDocumentsUpload.ivstrData))
                    throw new Exception($"Datos de Documento {Resources.lioE_ObjectNoM}");
                IRawDocument lioRawDocument;
                FileInfo lioFileInfo = new FileInfo(vioDocumentsUpload.ivstrName);
                switch (lioFileInfo.Extension.ToLower())
                {
                    case ".json":
                        lioRawDocument = new InDocumentJSON(vioDocumentsUpload.ivlngCuit, lioContext);
                        break;
                    case ".xml":
                        lioRawDocument = new InDocumentXMLNew(vioDocumentsUpload.ivlngCuit, lioContext);
                        break;
                    case ".txt":
                        lioRawDocument = new InDocumentTXT(vioDocumentsUpload.ivlngCuit, lioContext);
                        break;
                    default:
                        throw new Exception($"Extension de Documento {Resources.lioE_ObjectNoM}");
                }
                if (!vioDocumentsUpload.ivblnComp ?? false) //sino viene comprimido lo comprimo
                    vioDocumentsUpload.ivstrData = Format.Compress(vioDocumentsUpload.ivstrData);
                lioRawDocument.ivstrRaw = vioDocumentsUpload.ivstrData;
                lioRawDocument.ivstrName = vioDocumentsUpload.ivstrName;
                DocumentUser[] lcoDocumentUsers = lioRawDocument.GetDocuments();
                foreach (DocumentUser lioDocumentUser in lcoDocumentUsers)
                {
                    if (!string.IsNullOrEmpty(lioDocumentUser.ivstrLoadErrors))
                    {
                        lcoDocumentUserResponse.Add(
                            new DocumentUploadResponse
                            {
                                ivlngCuitEmisor = lioDocumentUser.ivlngCuitEmisor,
                                ivlngCbte = lioDocumentUser.ivlngCbte,
                                ivnumPvta = lioDocumentUser.ivnumPvta,
                                ivnroTipoDoc = lioDocumentUser.ivnroTipoDoc,
                                ivnroStatus = 2,
                                ivstrDescStatus = lioDocumentUser.ivstrLoadErrors,
                                ivstrIntegracion = JsonConvert.SerializeObject(lioDocumentUser.ioIntegracion)
                            });
                        lioDocumentUser.ivstrLoadErrors = string.Empty;
                    }
                    else
                    {
                        if (lioDocumentUser.ivlngCuitEmisor != vioDocumentsUpload.ivlngCuit)
                            throw new Exception($"Cuit Emisor {Resources.lioE_ObjectNoM}");
                        lioDocument = new Document(lioDocumentUser, lioContext, vioConfiguration);
                        lioDocument.ioDcModel.ivstrInData = lioDocumentUser.ivstrInputData;
                        lioDocument.ioDcModel.ivstrInType = lioFileInfo.Extension.ToLower();
                        lioDocumentUser.ivstrInputData = string.Empty;
                        lioDocument.ioDcModel.ivnroStatus = 10;
                        try
                        {
                            lioDocument.Save();
                            lcoDocumentUserResponse.Add(
                                new DocumentUploadResponse
                                {
                                    ivlngDoc = lioDocument.ioDcModel.ivlngDoc,
                                    ivlngCuitEmisor = lioDocumentUser.ivlngCuitEmisor,
                                    ivlngCbte = lioDocumentUser.ivlngCbte,
                                    ivnumPvta = lioDocumentUser.ivnumPvta,
                                    ivnroTipoDoc = lioDocumentUser.ivnroTipoDoc,
                                    ivnroStatus = 1,
                                    ivstrDescStatus = "OK",
                                    ivstrIntegracion = JsonConvert.SerializeObject(lioDocumentUser.ioIntegracion)
                                });
                            new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc).addTrack(
                                10,
                                string.Empty
                            );
                        }
                        catch (Exception lioE)
                        {
                            lcoDocumentUserResponse.Add(
                                 new DocumentUploadResponse
                                 {
                                     ivlngDoc = lioDocument.ioDcModel.ivlngDoc,
                                     ivlngCuitEmisor = lioDocumentUser.ivlngCuitEmisor,
                                     ivlngCbte = lioDocumentUser.ivlngCbte,
                                     ivnumPvta = lioDocumentUser.ivnumPvta,
                                     ivnroTipoDoc = lioDocumentUser.ivnroTipoDoc,
                                     ivnroStatus = 2,
                                     ivstrDescStatus = lioE.Message
                                 });
                            LogHelper.write(lioE);
                        }
                    }
                }
                return lcoDocumentUserResponse;
            }
        }
        public static string BuildDocumentResponse(Document vioDocument, IConfiguration vioConfiguration, ServiceMapper vioServiceMapper)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                string livstrRta = File.ReadAllText($"{ListHelper.GetValue("PATH", "template", lioContext)}/{vioDocument.ioDcModel.ivlngCuitEmisor}/{vioServiceMapper.ivstrTemplate}"), livstr, livstrPropInFile;
                DocumentTrackingModel lioDocumentTrackingModel;
                UxAuth mioAuthNode = vioDocument.iTribDocument.GetAuth();
                foreach (ServiceMapperItem lioServiceMapperItem in vioServiceMapper.coItems)
                {
                    livstr = string.Empty;
                    switch (lioServiceMapperItem.ivstrProperty)
                    {
                        case "ivdtmGen":
                            livstr = DateTime.Now.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivnroTipo":
                            livstr = vioDocument.ioDcModel.ivnroTipo.ToString();
                            break;
                        case "ivlngDoc":
                            livstr = vioDocument.ioDcModel.ivlngDoc.ToString();
                            break;
                        case "ivnumPvta":
                            livstr = vioDocument.ioDcModel.ivnumPvta.ToString();
                            break;
                        case "ivlngCbte":
                            livstr = vioDocument.ioDcModel.ivlngCbte.ToString();
                            break;
                        case "ivdtmEmision":
                        case "ivstrFechaEmision":
                            if (vioDocument.ioDcModel.ivdtmEmision == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmEmision", "a"));
                            livstr = (vioDocument.ioDcModel.ivdtmEmision ?? DateTime.MinValue).ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdblImporte":
                            livstr = vioDocument.ioDcModel.ivdblImporte.ToString();
                            break;
                        case "ivlngCuitEmisor":
                            livstr = vioDocument.ioDcModel.ivlngCuitEmisor.ToString();
                            break;
                        case "ivlngDocReceptor":
                            livstr = vioDocument.ioDocumentUser.ivlngDocReceptor.ToString();
                            break;
                        case "ivstrIdCliente":
                            livstr = vioDocument.ioDocumentUser.ivstrIdCliente;
                            break;
                        case "ivdtmRec":
                            lioDocumentTrackingModel = lioContext.DocumentTrackings.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == vioDocument.ioDcModel.ivlngDoc && x.ivnroStatus == 10);
                            if (lioDocumentTrackingModel == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmRec", "a"));
                            livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdtmAct":
                            lioDocumentTrackingModel = lioContext.DocumentTrackings.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == vioDocument.ioDcModel.ivlngDoc);
                            if (lioDocumentTrackingModel == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "dtmAct", "a"));
                            livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivstrFileName":
                            livstr = $"{vioDocument.ivstrKey}_{vioDocument.ioDcModel.ivnroTemplateVersion}.pdf";
                            break;
                        case "ivstrAuthCode":
                            livstr = mioAuthNode?.ivstrAuthCode ?? string.Empty;
                            break;
                        case "ivdtmNode":
                            if (mioAuthNode?.ivdtmNode == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmEmision", "a"));
                            livstr = (mioAuthNode?.ivdtmNode ?? DateTime.MinValue).ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdtmAuthVenc":
                            if (mioAuthNode?.ivdtmAuthVenc == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmAuthVenc", "a"));
                            livstr = string.Empty;
                            if (DateTime.TryParseExact(mioAuthNode?.ivdtmAuthVenc, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime livdtm))
                                livstr = livdtm.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivnumTrack":
                            livstr = mioAuthNode?.ivnumtrack.ToString() ?? string.Empty; ;
                            break;
                        case "ivstr3o4":
                            livstr = string.IsNullOrEmpty(mioAuthNode?.ivstrAuthCode) ? "3" : "4";
                            break;
                        case "ivstrAuthDsc":
                            livstr = mioAuthNode?.ivtrStatusDesc ?? string.Empty; ;
                            break;
                        case "ivstrAuthObs":
                            livstr = mioAuthNode?.ivstrErrors ?? string.Empty; ;
                            break;
                        case "ivstrTrackId":
                            livstr = string.Empty;
                            break;
                        default:
                            livstr = string.Empty;
                            break;
                    }
                    if (livstr?.Length > lioServiceMapperItem.ivnumLen)
                        livstr = livstr.Substring(0, lioServiceMapperItem.ivnumLen ?? 0);
                    if (livstr?.Length < lioServiceMapperItem.ivnumLen)
                    {
                        if (!string.IsNullOrEmpty(lioServiceMapperItem.ivstrLPad))
                            livstr = livstr.PadLeft(lioServiceMapperItem.ivnumLen ?? 0, lioServiceMapperItem.ivstrLPad[0]);
                        else if (!string.IsNullOrEmpty(lioServiceMapperItem.ivstrRPad))
                            livstr = livstr.PadRight(lioServiceMapperItem.ivnumLen ?? 0, lioServiceMapperItem.ivstrRPad[0]);
                    }
                    livstrPropInFile = "{" + lioServiceMapperItem.ivstrProperty + "}";
                    livstrRta = livstrRta.Replace(livstrPropInFile, livstr);

                }
                switch (vioServiceMapper.ivstrInputType ?? string.Empty)
                {
                    case "B64str":
                        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(livstrRta)));
                    case "B64Bytes":
                        livstr = $"{vioDocument.ioDcModel.ivnroTipo.ToString().PadLeft(2, '0')}_{vioDocument.ioDcModel.ivnumPvta.ToString().PadLeft(4, '0')}_{vioDocument.ioDcModel.ivlngCbte.ToString().PadLeft(8, '0')}.{vioServiceMapper.ivstrTemplate.Split('.')[1].Trim()}";
                        livstr = Path.GetTempPath() + livstr;
                        if (File.Exists(livstr))
                            File.Delete(livstr);
                        File.WriteAllBytes(livstr, Encoding.UTF8.GetBytes(livstrRta));
                        return Convert.ToBase64String(File.ReadAllBytes(livstr));
                    default:
                        return livstrRta;
                }
            }
        }
    }
}
