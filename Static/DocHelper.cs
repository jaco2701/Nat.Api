using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Text;

namespace Applet.Nat.Api.Static
{
    public static class DocHelper
    {
        public static List<DocumentUploadResponse> UploadDocument(DocumentUploadRequest vioDocumentsUpload, IConfiguration vioConfiguration, int vivnumUserOriginator)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                List<DocumentUploadResponse> lcoDocumentUserResponse = new List<DocumentUploadResponse>();
                User lioOriginator = new User(vivnumUserOriginator, lioContext);
                Document lioDocument;
                string livstrErrorParams = string.Empty;
                if (string.IsNullOrEmpty(vioDocumentsUpload.ivstrName))
                    livstrErrorParams += $"Nombre de Documento {Resources.lioE_ObjectNoM}";
                if (string.IsNullOrEmpty(vioDocumentsUpload.ivstrData))
                    livstrErrorParams += $"Datos de Documento {Resources.lioE_ObjectNoM}";
                if (!string.IsNullOrEmpty(livstrErrorParams))
                {
                    lcoDocumentUserResponse.Add(
                           new DocumentUploadResponse
                           {
                               ivnroStatus = 2,
                               ivstrDescStatus = livstrErrorParams
                           });
                    return lcoDocumentUserResponse;
                }
                IRawDocument lioRawDocument;
                FileInfo lioFileInfo = new FileInfo(vioDocumentsUpload.ivstrName);
                lioRawDocument = getRawDocument(lioFileInfo.Extension.ToLower(), vioDocumentsUpload.ivlngCuit, lioContext);
                if (!vioDocumentsUpload.ivblnComp ?? false) //sino viene comprimido lo comprimo
                    vioDocumentsUpload.ivstrData = Format.Compress(vioDocumentsUpload.ivstrData);
                lioRawDocument.ivstrRaw = vioDocumentsUpload.ivstrData;
                lioRawDocument.ivstrName = vioDocumentsUpload.ivstrName;
                DocumentUser[] lcoDocumentUsers = lioRawDocument.GetDocuments();
                string livstr;
                foreach (DocumentUser lioDocumentUser in lcoDocumentUsers)
                {
                    lioDocumentUser.ivnumUserOriginator = lioOriginator.ioDcModel.ivnumUser;
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
                        if (lioDocumentUser.ivlngCuitEmisor == null || lioDocumentUser.ivlngCuitEmisor == 0) continue;
                        if (string.IsNullOrEmpty(lioDocumentUser.ivstrInputData))
                        {
                            livstr = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lioDocumentUser)));
                            lioDocumentUser.ivstrInputData = livstr;
                        }
                        if (lioDocumentUser.ivblnSaveOnLoad ?? true)
                        {
                            lioDocument = new Document(lioDocumentUser, lioContext, vioConfiguration);
                            lioDocument.ioDcModel.ivstrInData = lioDocumentUser.ivstrInputData;
                            lioDocument.ioDcModel.ivstrInType = lioFileInfo.Extension.ToLower();
                            lioDocumentUser.ivstrInputData = string.Empty;
                            lioDocument.ioDcModel.ivnroStatus = 10;
                            try
                            {
                                if (!lioOriginator.coCuitsModels?.Any(x => x.ivlngCuit == lioDocument.iTribDocument.ivCuitAutorizante) ?? true)
                                    throw new Exception($"Usuario Originador {Resources.lioE_ObjectNoM}");
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
                                        ivstrIntegracion = lioDocumentUser.ioIntegracion != null ? JsonConvert.SerializeObject(lioDocumentUser.ioIntegracion) : null,
                                        ioDocumentUser = lioDocumentUser
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
                        else
                            lcoDocumentUserResponse.Add(
                                    new DocumentUploadResponse
                                    {
                                        ivlngDoc = 0,
                                        ivlngCuitEmisor = lioDocumentUser.ivlngCuitEmisor,
                                        ivlngCbte = lioDocumentUser.ivlngCbte,
                                        ivnumPvta = lioDocumentUser.ivnumPvta,
                                        ivnroTipoDoc = lioDocumentUser.ivnroTipoDoc,
                                        ivnroStatus = 1,
                                        ivstrDescStatus = "OK",
                                        ivstrIntegracion = lioDocumentUser.ioIntegracion != null ? JsonConvert.SerializeObject(lioDocumentUser.ioIntegracion) : null,
                                        ioDocumentUser = lioDocumentUser
                                    });
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
                DocumentTrackingModel? lioDocumentTrackingModel;
                UxAuth? lioAuthNode = vioDocument.iTribDocument.GetAuth();
                foreach (ServiceMapperItem lioServiceMapperItem in vioServiceMapper.coItems ?? [])
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
                            livstr = vioDocument.ioDcModel.ivlngDoc == 0 ? string.Empty : vioDocument.ioDcModel.ivlngDoc.ToString();
                            break;
                        case "ivnumPvta":
                            livstr = vioDocument.ioDcModel.ivlngDoc == 0 ? string.Empty : vioDocument.ioDcModel.ivnumPvta.ToString();
                            break;
                        case "ivlngCbte":
                            livstr = vioDocument.ioDcModel.ivlngDoc == 0 ? string.Empty : vioDocument.ioDcModel.ivlngCbte.ToString();
                            break;
                        case "ivdtmEmision":
                        case "ivstrFechaEmision":
                            if (vioDocument.ioDcModel.ivdtmEmision == null)
                                throw new Exception($"Fecha Emision {Resources.lioE_ObjectNoM}");
                            livstr = (vioDocument.ioDcModel.ivdtmEmision ?? DateTime.MinValue).ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdblImporte":
                            livstr = vioDocument.ioDcModel.ivdblImporte?.ToString() ?? string.Empty;
                            break;
                        case "ivlngCuitEmisor":
                            livstr = vioDocument.ioDcModel.ivlngCuitEmisor.ToString();
                            break;
                        case "ivlngDocReceptor":
                            livstr = vioDocument.ioDocumentUser.ivlngDocReceptor?.ToString() ?? string.Empty;
                            break;
                        case "ivstrIdCliente":
                            livstr = vioDocument.ioDocumentUser.ivstrIdCliente ?? string.Empty; ;
                            break;
                        case "ivdtmRec":
                            lioDocumentTrackingModel = lioContext.DocumentTrackings?.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == vioDocument.ioDcModel.ivlngDoc && x.ivnroStatus == 10);
                            if (lioDocumentTrackingModel == null)
                                throw new Exception($"Fecha Recepcion {Resources.lioE_ObjectNoM}");
                            livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdtmAct":
                            lioDocumentTrackingModel = lioContext.DocumentTrackings?.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == vioDocument.ioDcModel.ivlngDoc);
                            if (lioDocumentTrackingModel == null)
                                throw new Exception($"Fecha Actualizacion {Resources.lioE_ObjectNoM}");
                            livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivstrFileName":
                            livstr = $"{vioDocument.ivstrKey}_{vioDocument.ioDcModel.ivnroTemplateVersion}.pdf";
                            break;
                        case "ivstrAuthCode":
                            livstr = lioAuthNode?.ivstrAuthCode ?? string.Empty;
                            break;
                        case "ivdtmNode":
                            if (lioAuthNode?.ivdtmNode == null)
                                throw new Exception($"Fecha Emision {Resources.lioE_ObjectNoM}");
                            livstr = (lioAuthNode?.ivdtmNode ?? DateTime.MinValue).ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdtmAuthVenc":
                            livstr = string.Empty;
                            if (DateTime.TryParseExact(lioAuthNode?.ivdtmAuthVenc, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime livdtm))
                                livstr = livdtm.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivnumTrack":
                            livstr = lioAuthNode?.ivnumtrack.ToString() ?? string.Empty; ;
                            break;
                        case "ivstr3o4":
                            livstr = string.IsNullOrEmpty(lioAuthNode?.ivstrAuthCode) ? "3" : "4";
                            break;
                        case "ivstrAuthDsc":
                            livstr = lioAuthNode?.ivtrStatusDesc ?? string.Empty; ;
                            break;
                        case "ivstrAuthObs":
                            livstr = lioAuthNode?.ivstrErrors ?? string.Empty; ;
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
                        livstr = $"{vioDocument.ioDcModel.ivnroTipo.ToString().PadLeft(2, '0')}_{vioDocument.ioDcModel.ivnumPvta.ToString().PadLeft(4, '0')}_{vioDocument.ioDcModel.ivlngCbte.ToString().PadLeft(8, '0')}.{vioServiceMapper.ivstrTemplate?.Split('.')[1].Trim()}";
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
        public static string BuildDocumentResponse(long lngCuit, IConfiguration vioConfiguration, string vivstrObs)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                Cuit lioCuit = new Cuit(lngCuit, lioContext, null);
                if (lioCuit.ioDcModel == null || string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                    throw new Exception($"Configuracion CUIT  {Resources.lioE_ObjectNoM}");
                if (lioCuit.ioDcModel == null || lioCuit.ioDcModel.ivstrCnfg == null)
                    throw new Exception($"Configuracion CUIT  {Resources.lioE_ObjectNoM}");
                ServiceMapper? lioServiceMapper = lioCuit.ioCnfg?.coServiceMappers?.FirstOrDefault(x => x.ivstrWs == "rta");
                if (lioServiceMapper == null)
                    throw new Exception($"Mapeo de Respuesta {Resources.lioE_ObjectNoM}");
                string livstrRta = File.ReadAllText($"{ListHelper.GetValue("PATH", "template", lioContext)}/{lngCuit}/{lioServiceMapper.ivstrTemplate}"), livstr, livstrPropInFile;
                foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems ?? [])
                {
                    livstr = string.Empty;
                    switch (lioServiceMapperItem.ivstrProperty)
                    {
                        case "ivdtmGen":
                            livstr = DateTime.Now.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivstrAuthObs":
                            livstr = vivstrObs ?? string.Empty; ;
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
                return livstrRta;
            }
        }
        public static IRawDocument getRawDocument(string vivstrInType, long vivlngCuit, NatContext vioContext)
        {
            switch (vivstrInType)
            {
                case ".xml": return new InDocumentXMLNew(vivlngCuit, vioContext);
                case ".json": return new InDocumentJSON(vivlngCuit, vioContext);
                case ".txt": return new InDocumentTXT(vivlngCuit, vioContext);
                case ".jpg":
                case ".pdf": return new InDocumentIMG(vivlngCuit, vioContext);
                default: throw new Exception($"Extension de Documento {Resources.lioE_ObjectNoM}");
            }
        }
    }
}
