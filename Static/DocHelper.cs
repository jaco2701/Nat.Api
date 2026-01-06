using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Nat.API.Properties;
using Newtonsoft.Json;

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

    }
}
