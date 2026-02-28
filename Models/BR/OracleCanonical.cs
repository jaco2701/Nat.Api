using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Applet.Nat.OracleCanonical.Get;
using Applet.Nat.OracleCanonical.Update;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Applet.Nat.Api.Br.Models
{
    public class OracleCanonical : IDocsIO
    {
        #region CONS
        public OracleCanonical() { }
        public OracleCanonical(IConfiguration vioConfiguration)
        {
            mioConfiguration = vioConfiguration;
        }
        #endregion
        #region PRIVATE PROPS
        private IConfiguration mioConfiguration { get; set; }
        //  private NatContext mioContext { get; set; }

        #endregion
        #region PUBLIC PROPS
        public string ivstrB64Rta { get; set; }
        public long ivlngCuit { get; set; }
        public string ivstrPathIn { get; set; }
        public string ivstrPathOut { get; set; }
        public string ivstrUser { get; set; }
        public string ivstrPass { get; set; }
        public string ivstrEntityID { get; set; }
        public short ivnroDays { get; set; }
        public short ivnroRows { get; set; }
        #endregion
        #region PUBLIC METHODS  
        public async Task DocsI()
        {

            using NatContext lioContext = NatContext.GetContext(mioConfiguration);
            {
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)int.Parse(ListHelper.GetValue("FORMAT", "TLS", lioContext));
                    PublicReportServiceClient lioReportClient = new PublicReportServiceClient();
                    lioReportClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(ivstrPathIn.Split("==>")[0]);
                    //lioReportClient.ClientCredentials.UserName.UserName = ivstrUser;
                    //lioReportClient.ClientCredentials.UserName.Password = ivstrPass;
                    ReportRequest lioRequest = new ReportRequest();
                    lioRequest.attributeFormat = "text";
                    lioRequest.reportAbsolutePath = ivstrPathIn.Split("==>")[1];
                    lioRequest.sizeOfDataChunkDownload = -1;
                    lioRequest.parameterNameValues = new ParamNameValue[3]
                    {
                    new ParamNameValue
                    {
                        name = "P_LEGAL_ENTITY_ID",
                        values = new string[1] { ivstrEntityID.ToString() }
                    },
                    new ParamNameValue
                    {
                        name = "P_DAYS",
                        values = new string[1] { ivnroDays.ToString() }
                    },
                    new ParamNameValue
                    {
                        name = "P_ROWNUM_LIMIT",
                        values = new string[1] { ivnroRows.ToString() }
                    }
                    };
                    LogHelper.writeinfo($"{DateTime.Now} Url:{lioReportClient.Endpoint.Address} Report:{lioRequest.reportAbsolutePath} LegalEntity: {ivstrEntityID.ToString()}", ListHelper.Verbose(lioContext));
                    ReportResponse lioResponse = await lioReportClient.runReportAsync(lioRequest, ivstrUser, ivstrPass);
                    string livstrRawResponse = lioResponse.reportBytes != null ? System.Text.Encoding.UTF8.GetString(lioResponse.reportBytes) : string.Empty;
                    LogHelper.writeinfo($"{DateTime.Now} Response: {livstrRawResponse}", ListHelper.Verbose(lioContext));
                    DocumentUploadRequest lioDocumentsUploadRequest = new DocumentUploadRequest
                    {
                        ivstrName = $"{Guid.NewGuid().ToString("N").Substring(0, 5)}.xml",
                        ivstrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(livstrRawResponse)),
                        ivblnComp = false,
                        ivlngCuit = ivlngCuit
                    };
                    List<DocumentUploadResponse> lcoUDocumentsUploadResponse = DocHelper.UploadDocument(lioDocumentsUploadRequest, mioConfiguration);
                    //Documentos Cargados
                    List<Document> lcoDocumentsToUpdate = new List<Document>();
                    foreach (DocumentUploadResponse lioDocumentUploadResponse in lcoUDocumentsUploadResponse.Where(x => x.ivnroStatus == 1))
                        lcoDocumentsToUpdate.Add(new Document(lioDocumentUploadResponse.ivlngDoc ?? 0, lioContext, mioConfiguration));
                    if (lcoDocumentsToUpdate.Count > 0)
                        await DocO(lcoDocumentsToUpdate.ToArray());
                    //Documentos con errores de carga
                    UxDocumentIntegracion lioUxDocumentIntegracion;
                    string livsrtDFFAttributes;
                    foreach (DocumentUploadResponse lioDocumentUploadResponse in lcoUDocumentsUploadResponse.Where(x => x.ivnroStatus != 1))
                    {
                        lioUxDocumentIntegracion = JsonConvert.DeserializeObject<UxDocumentIntegracion>(lioDocumentUploadResponse.ivstrIntegracion);
                        livsrtDFFAttributes = $"{{\"{lioUxDocumentIntegracion.ivstrEfdStatusAtt}\" : \"ERROR\",\"{lioUxDocumentIntegracion.ivstrEfdMessageAtt}\" : \"{lioUxDocumentIntegracion.ivstrEfdMessage}\"}}";
                        try
                        {
                            await UpdateOracleStatus(livsrtDFFAttributes, lioUxDocumentIntegracion);
                        }
                        catch (Exception lioE)
                        {
                            LogHelper.write(lioE);
                        }
                    }
                }
                catch (Exception lioE)
                {
                    LogHelper.write(lioE);
                }
            }
        }
        public async Task DocO(Document[] vcoDocuments)
        {
            using NatContext lioContext = NatContext.GetContext(mioConfiguration);
            {
                ErpObjectDetails lioErpObjectDetails;
                string livsrtDFFAttributes = string.Empty;
                UxAuth lioUxAuth;
                UxDocumentIntegracion lioUxDocumentIntegracion;
                int livnumIndex = 0;
                string livstr;

                Cuit lioCuit = new Cuit(ivlngCuit, lioContext, mioConfiguration);
                ServiceMapper lioRtaMapper = lioCuit.ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrWs == "rta");
                if (lioRtaMapper == null)
                    throw new Exception($"Mapeador de Respuesta {Resources.lioE_ObjectNoM} para cuit {ivlngCuit}");
                ServiceMapperItem lioServiceMapperItem;
                foreach (Document lioDocument in vcoDocuments)
                {
                    try
                    {
                        if (lioDocument.ioDcModel == null)
                            continue;
                        lioServiceMapperItem = lioRtaMapper.coItems.FirstOrDefault(x => x.coStatus.Contains(lioDocument.ioDcModel.ivnroStatus));
                        if (lioServiceMapperItem == null || string.IsNullOrEmpty(lioServiceMapperItem.ivstrProperty))
                            continue;
                        if (lioDocument.ioDcModel.ivnroStatus == 0) //documentos que no se pudieron cargar
                            lioUxDocumentIntegracion = JsonConvert.DeserializeObject<UxDocumentIntegracion>(lioDocument.ioDcModel.ivstrRazonSocial ?? string.Empty);
                        else
                            lioUxDocumentIntegracion = lioDocument.ioDocumentUser.ioIntegracion;
                        livsrtDFFAttributes = lioServiceMapperItem.ivstrProperty
                            .Replace("{StatusAtt}", lioUxDocumentIntegracion.ivstrEfdStatusAtt)
                            .Replace("{MessageAtt}", lioUxDocumentIntegracion.ivstrEfdMessageAtt)
                            .Replace("{NumberAtt}", lioUxDocumentIntegracion.ivstrEfdKeyNumberAtt)
                            .Replace("{DateAtt}", lioUxDocumentIntegracion.ivstrEfdKeyDateAtt);
                        switch (lioDocument.ioDcModel.ivnroStatus)
                        {
                            case 20:
                                DocumentTracking lioDocumentTracking = lioDocument.LastTracOfStatus(20);
                                if (lioDocumentTracking == null)
                                    throw new Exception($"Tracking {Resources.lioE_ObjectNoM} para estado {lioDocument.ioDcModel.ivnroStatus}");
                                if (!string.IsNullOrEmpty(lioDocumentTracking.ioDcModel.ivstrData))
                                {
                                    livstr = Format.RemoveOracleAttrInvalidChars(lioDocumentTracking.ioDcModel.ivstrData);
                                    livnumIndex = 0;
                                    while (livsrtDFFAttributes.Length < 190 && livnumIndex < livstr.Length)
                                    {
                                        livsrtDFFAttributes += livstr.Substring(livnumIndex, 1);
                                        livnumIndex++;
                                    }
                                }
                                livsrtDFFAttributes += "\"}";
                                break;
                            case 35:
                            case 40:
                                lioUxAuth = lioDocument.iTribDocument.GetAuth();
                                if (!string.IsNullOrEmpty(lioUxAuth.ivstrErrors))
                                {
                                    livstr = Format.RemoveOracleAttrInvalidChars(lioUxAuth.ivstrErrors);
                                    livnumIndex = 0;
                                    while (livsrtDFFAttributes.Length < 190 && livnumIndex < livstr.Length)
                                    {
                                        livsrtDFFAttributes += livstr.Substring(livnumIndex, 1);
                                        livnumIndex++;
                                    }
                                }
                                livsrtDFFAttributes += "\"}";
                                break;
                            case 50:
                            case 55:
                            case 60:
                            case 65:
                            case 70:
                            case 80:
                            case 100:
                                await Task.Delay(5000); // se hace esta espera para asegurar que vaya despues de la carga 
                                lioUxAuth = lioDocument.iTribDocument.GetAuth();
                                livsrtDFFAttributes = livsrtDFFAttributes
                                    .Replace("{ivdtmNode}", (lioUxAuth.ivdtmNode ?? DateTime.Now).ToString(ListHelper.GetValue("FORMAT", "dtmwsfe", lioContext)))
                                    .Replace("{ivstrAuthCode}", lioUxAuth.ivstrAuthCode)
                                    .Replace("{ivdtmAuthVenc}", lioUxAuth.ivdtmAuthVenc);
                                if (!string.IsNullOrEmpty(lioUxAuth.ivstrObs))
                                {
                                    livstr = Format.RemoveOracleAttrInvalidChars(lioUxAuth.ivstrObs);
                                    livnumIndex = 0;
                                    while (livsrtDFFAttributes.Length < 248 && livnumIndex < livstr.Length)
                                    {
                                        livsrtDFFAttributes += livstr.Substring(livnumIndex, 1);
                                        livnumIndex++;
                                    }
                                }
                                livsrtDFFAttributes += "\"}";
                                break;
                            default:
                                break;
                        }
                        await UpdateOracleStatus(livsrtDFFAttributes, lioUxDocumentIntegracion);
                        new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc).addTrack(
                            55,
                            livsrtDFFAttributes
                        );
                    }
                    catch (Exception lioE)
                    {
                        LogHelper.write(lioE);
                        new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc).addTrack(
                            56,
                            $"{Resources.lioE_RtaERP}: {lioE.Message}"
                        );
                    }
                }
                ivstrB64Rta = "OK";
            }
        }

        #endregion
        #region PRIVATE METHODS  
        private async Task UpdateOracleStatus(string vivsrtDFFAttributes, UxDocumentIntegracion lioUxDocumentIntegracion)
        {
            using ErpObjectDFFUpdateServiceClient lioClient = new ErpObjectDFFUpdateServiceClient
            (
                new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
                    Security =
                    {
                        Transport = new System.ServiceModel.HttpTransportSecurity
                        {
                            ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic
                        }
                    }
                },
                new System.ServiceModel.EndpointAddress(ivstrPathOut)
            );
            {
                lioClient.ClientCredentials.UserName.UserName = ivstrUser;
                lioClient.ClientCredentials.UserName.Password = ivstrPass;
                if (string.IsNullOrEmpty(lioUxDocumentIntegracion.ivstrAttributeCategory))
                    throw new Exception($"{Resources.ResourceManager.GetString("lioP_ioIntegracion.ivstrAttributeCategory")} [ioIntegracion.ivstrAttributeCategory] {Resources.lioE_ObjectNoM}");
                if (string.IsNullOrEmpty(lioUxDocumentIntegracion.ivstrInvoiceId))
                    throw new Exception($"{Resources.ResourceManager.GetString("lioP_ioIntegracion.ivstrInvoiceId")} [ioIntegracion.ivstrInvoiceId] {Resources.lioE_ObjectNoM}");
                if (string.IsNullOrEmpty(lioUxDocumentIntegracion.ivstrInvoiceNumber))
                    throw new Exception($"{Resources.ResourceManager.GetString("lioP_ioIntegracion.ivstrInvoiceNumber")} [ioIntegracion.ivstrInvoiceNumber] {Resources.lioE_ObjectNoM}");
                ErpObjectDetails lioErpObjectDetails = new ErpObjectDetails
                {
                    EntityName = "Receivables Invoice",
                    ContextValue = lioUxDocumentIntegracion.ivstrAttributeCategory,
                    UserKeyA = lioUxDocumentIntegracion.ivstrInvoiceNumber,
                    UserKeyB = "#NULL",
                    UserKeyC = "#NULL",
                    UserKeyD = lioUxDocumentIntegracion.ivstrInvoiceId,
                    UserKeyE = "#NULL",
                    UserKeyF = "#NULL",
                    UserKeyG = "#NULL",
                    UserKeyH = "#NULL",
                    DFFAttributes = vivsrtDFFAttributes
                };
                updateDffEntityDetailsResponse lioResponse = await lioClient.updateDffEntityDetailsAsync(
                    null,
                    "SINGLE",
                    lioErpObjectDetails,
                    "10",
                    "#NULL"
                );
                if (lioResponse != null && lioResponse.result != "1")
                    throw new Exception($"Response <> 1 {JsonConvert.SerializeObject(lioErpObjectDetails)} ATTRs: {vivsrtDFFAttributes}");
            }
        }
    }
    #endregion

}
