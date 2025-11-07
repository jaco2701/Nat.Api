using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Applet.Nat.OracleCanonical.Get;
using Applet.Nat.OracleCanonical.Update;
using System.Net;
using System.Text;
using System.Xml;

namespace Applet.Nat.Api.Br.Models
{
    public class OracleCanonical : IDocsIO
    {
        #region CONS
        public OracleCanonical() { }
        public OracleCanonical(IConfiguration vioConfiguration, NatContext vioContext)
        {
            mioConfiguration = vioConfiguration;
            if (vioContext != null)
                mioContext = vioContext;
        }
        #endregion
        #region PRIVATE PROPS
        private IConfiguration mioConfiguration { get; set; }
        private NatContext mioContext { get; set; }
        #endregion
        #region PUBLIC PROPS
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
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)int.Parse(ListHelper.GetValue("FORMAT", "TLS", lioContext));
                PublicReportServiceClient lioClient = new PublicReportServiceClient();

                lioClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(ivstrPathIn);
                //lioClient.ClientCredentials.UserName.UserName = ivstrUser;
                //lioClient.ClientCredentials.UserName.Password = ivstrPass;
                ReportRequest lioRequest = new ReportRequest();
                lioRequest.attributeFormat = "text";
                lioRequest.reportAbsolutePath = "/Custom/Local Solution/AR/E-INVOICE/Process/E-INVOICE Send.xdo";
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
                ReportResponse lioResponse = await lioClient.runReportAsync(lioRequest, ivstrUser, ivstrPass);
                string livstrRawResponse = lioResponse.reportBytes != null ? System.Text.Encoding.UTF8.GetString(lioResponse.reportBytes) : string.Empty;
                XmlDocument lioXmlResponse = new XmlDocument();
                lioXmlResponse.LoadXml(livstrRawResponse);
                DocumentUploadRequest lioDocumentsUploadRequest;
                foreach (XmlNode lioXmlNodeDocument in lioXmlResponse.SelectNodes("//DATA_DS/Invoice"))
                {
                    lioDocumentsUploadRequest = new DocumentUploadRequest
                    {
                        ivstrName = lioXmlNodeDocument.SelectSingleNode("//Header/InvoiceId")?.InnerText,
                        ivstrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(lioXmlNodeDocument.OuterXml)),
                        ivblnComp = false,
                        ivlngCuit = ivlngCuit
                    };
                    //List<DocumentUploadResponse> lcoUDocumentsUploadResponse = DocHelper.UploadDocument(lioDocumentsUploadRequest, mioConfiguration);
                    //if (lcoUDocumentsUploadResponse.Count == 0)
                    //    continue;
                }
            }
        }
        public async Task DocsO(Document[] vcoDocuments)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)int.Parse(ListHelper.GetValue("FORMAT", "TLS", mioContext));
            ErpObjectDFFUpdateServiceClient lioClient = new ErpObjectDFFUpdateServiceClient
            (
                new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport)
                {
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max
                },
                new System.ServiceModel.EndpointAddress(ivstrPathOut)
            );
            string livstrDFFAttributes = "{";
            ErpObjectDetails lioErpObjectDetails;
            foreach (Document lioDocument in vcoDocuments)
            {
                livstrDFFAttributes = "{";
                lioErpObjectDetails = new ErpObjectDetails
                {
                    EntityName = "Receivables Invoice",
                    ContextValue = "Value of tag <Invoice>/<Header>/<Integration>/<AttributeCategory>",
                    UserKeyA = "Value of tag <Invoice>/<Header>/<InvoiceNumber>",
                    UserKeyB = "#NULL",
                    UserKeyC = "#NULL",
                    UserKeyD = "Value of tag <Invoice>/<Header>/<InvoiceId>",
                    UserKeyE = "#NULL",
                    UserKeyF = "#NULL",
                    UserKeyG = "#NULL",
                    UserKeyH = "#NULL",
                    DFFAttributes = livstrDFFAttributes
                };

                updateDffEntityDetailsResponse lioResponse = await lioClient.updateDffEntityDetailsAsync(
                    null,
                    "SINGLE",
                    lioErpObjectDetails,
                    "NotificationCode",
                    null
                );

            }

        }
        #endregion
    }
}
