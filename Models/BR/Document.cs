using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore.Storage;
using Nat.Api.Models.BR;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Xml;
namespace Applet.Nat.Api.Br.Models
{
    public class Document
    {
        #region PUBLIC PROPS
        public DocumentModel ioDcModel { get; set; }
        public DocumentUser ioDocumentUser
        {
            get
            {
                if (mioDocumentUser == null || mioDocumentUser.ivlngCbte == null)
                {
                    mioDocumentUser = new DocumentUser();
                    iIRawDocument.ivstrRaw = ioDcModel.ivstrInData ?? string.Empty;
                    iIRawDocument.ivstrName = $"{ioDcModel.ivnroTipo}_0";
                    iIRawDocument.ivstrKey = ivstrKey;
                    mioDocumentUser = iIRawDocument?.GetDocuments()[0];
                }
                return mioDocumentUser;
            }
        }
        public IRawDocument iIRawDocument
        {
            get
            {
                if (miIRawDocument == null)
                {
                    switch (ioDcModel.ivstrInType)
                    {
                        case ".xml": miIRawDocument = new InDocumentXMLNew(ioDcModel.ivlngCuitEmisor, mioContext); break;
                        case ".json": miIRawDocument = new InDocumentJSON(ioDcModel.ivlngCuitEmisor, mioContext); break;
                        case ".txt": miIRawDocument = new InDocumentTXT(ioDcModel.ivlngCuitEmisor, mioContext); break;
                        default: miIRawDocument = new InDocumentXML(ioDcModel.ivlngCuitEmisor, mioContext); break;
                    }
                    miIRawDocument.ivstrRaw = ioDcModel.ivstrInData;
                    miIRawDocument.ivstrKey = ivstrKey;
                }
                return miIRawDocument;
            }
        }
        public IDocument ivIDocument { get; set; }
        public bool ivblPrintable
        {
            get
            {
                string livstrTemplatePath = ListHelper.GetValue("PATH", "template", mioContext);
                livstrTemplatePath += $"/{ioDcModel.ivlngCuitEmisor}";
                return Directory.Exists(livstrTemplatePath);
            }
        }
        public string ivstrKey
        {
            get
            {
                return $"{ioDcModel.ivlngCuitEmisor}_{ioDcModel.ivnroTipo}_{ioDcModel.ivnumPvta}_{ioDcModel.ivlngCbte}_{ioDcModel.ivlngDoc}";
            }
        }
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext;
        private DocumentUser mioDocumentUser;
        private IRawDocument miIRawDocument;
        private IConfiguration mioConfiguration;

        private string mivstrDisplay
        {
            get
            {
                return $"{ListHelper.GetValue("TCOMP", ioDcModel.ivnroTipo.ToString(), mioContext)} {ioDcModel.ivnumPvta.ToString().PadLeft(5, '0')}-{ioDcModel.ivlngCbte.ToString().PadLeft(8, '0')}";
            }
        }
        #endregion
        #region CONSTRUCT
        public Document() { }
        public Document(long vivlngDoc, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioContext = vioContext;
            mioConfiguration = vioConfiguration;
            DocumentModel lioDocumentModel = mioContext.Documents.Find(vivlngDoc);
            if (lioDocumentModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Documento", "o"));
            ioDcModel = lioDocumentModel;
            setIDocument();
        }
        public Document(DocumentModel vioDocumentModel, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioContext = vioContext;
            mioConfiguration = vioConfiguration;
            if (vioDocumentModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Documento", "o"));
            ioDcModel = vioDocumentModel;
            setIDocument();
        }
        public Document(DocumentUser vioDocumentUser, NatContext vioContext, IConfiguration vioConfiguration)
        {
            mioContext = vioContext;
            mioConfiguration = vioConfiguration;
            ioDcModel = new DocumentModel
            {
                ivlngCuitEmisor = vioDocumentUser.ivlngCuitEmisor ?? 0,
                ivnroTipo = vioDocumentUser.ivnroTipoDoc ?? 0,
                ivnumPvta = vioDocumentUser.ivnumPvta ?? 0,
                ivlngCbte = vioDocumentUser.ivlngCbte ?? 0,
                ivdtmEmision = Format.DateFromUX(vioDocumentUser.ivstrFechaEmision, ListHelper.GetValue("Format", "ApiDtm", mioContext)),
                ivlngCuitReceptor = vioDocumentUser.ivlngDocReceptor ?? 0,
                ivstrWs = vioDocumentUser.ivstrWs,
                ivstrInData = Convert.ToBase64String(Encoding.UTF8.GetBytes(vioDocumentUser.ivstrInputData)),
                ivdblImporte = vioDocumentUser.ivdblImporteTotal ?? 0,
                ivstrIdCliente = vioDocumentUser.ivstrIdCliente ?? string.Empty,
                ivstrMoneda = vioDocumentUser.ivstrMoneda ?? string.Empty,
                ivstrRazonSocial = vioDocumentUser.ivstrRazonSocial ?? string.Empty
            };
            mioDocumentUser = vioDocumentUser;
            setIDocument();
        }
        #endregion
        #region PUBLICS METHODS
        public void Save()
        {
            DocumentModel lioDBDocumentModel = mioContext.Documents.FirstOrDefault(
                x =>
                x.ivlngCuitEmisor == ioDcModel.ivlngCuitEmisor &&
                x.ivnroTipo == ioDcModel.ivnroTipo &&
                x.ivnumPvta == ioDcModel.ivnumPvta &&
                x.ivlngCbte == ioDcModel.ivlngCbte
            );
            if (lioDBDocumentModel == null)
            {
                ioDcModel.ivnroStatus = 10;
                ioDcModel.ivlngDoc = NN();
                Cuit lioCuit = new Cuit(ioDcModel.ivlngCuitEmisor, mioContext, mioConfiguration);
                if (lioCuit.ioCnfg == null || lioCuit.ioCnfg.coTemplateVersions == null)
                    throw new Exception($"Version de Plantillas {Resources.lioE_ObjectNoM}");
                TemplateVersion lioO = lioCuit.ioCnfg.coTemplateVersions.FirstOrDefault(x => x.ivnroTipo == ioDcModel.ivnroTipo);
                if (lioO == null)
                    throw new Exception($"Version de Plantillas {Resources.lioE_ObjectNoM}");
                ioDcModel.ivnroTemplateVersion = lioO.ivnroTemplateVersion;
                mioContext.Documents.Add(ioDcModel);
            }
            else
            {
                if (lioDBDocumentModel.ivnroStatus >= 50 && ivIDocument.AuthDataModified(new Document(lioDBDocumentModel, mioContext, mioConfiguration).ivIDocument))
                    throw new Exception(Resources.lioE_Doc_AuthInfoMod);
                lioDBDocumentModel.ivnroStatus = ioDcModel.ivnroStatus;
                if (ioDcModel.ivdtmEmision != null)
                    lioDBDocumentModel.ivdtmEmision = ioDcModel.ivdtmEmision;
                if (ioDcModel.ivlngCuitReceptor != 0)
                    lioDBDocumentModel.ivlngCuitReceptor = ioDcModel.ivlngCuitReceptor;
                if (!string.IsNullOrEmpty(ioDcModel.ivstrWs))
                    lioDBDocumentModel.ivstrWs = ioDcModel.ivstrWs;
                if (!string.IsNullOrEmpty(ioDcModel.ivstrInData))
                    lioDBDocumentModel.ivstrInData = ioDcModel.ivstrInData;
                if (ioDcModel.ivdblImporte != 0)
                    lioDBDocumentModel.ivdblImporte = ioDcModel.ivdblImporte;
                if (!string.IsNullOrEmpty(ioDcModel.ivstrIdCliente))
                    lioDBDocumentModel.ivstrIdCliente = ioDcModel.ivstrIdCliente;
                if (!string.IsNullOrEmpty(ioDcModel.ivstrMoneda))
                    lioDBDocumentModel.ivstrMoneda = ioDcModel.ivstrMoneda;
                if (!string.IsNullOrEmpty(ioDcModel.ivstrRazonSocial))
                    lioDBDocumentModel.ivstrRazonSocial = ioDcModel.ivstrRazonSocial;
                if (ioDcModel.ivnroTemplateVersion != 0)
                    lioDBDocumentModel.ivnroTemplateVersion = ioDcModel.ivnroTemplateVersion;
                mioContext.Documents.Update(lioDBDocumentModel);
                ioDcModel.ivlngDoc = lioDBDocumentModel.ivlngDoc;
            }
            mioContext.SaveChanges();
        }
        public string Delete()
        {
            DocumentModel lioDBDocumentModel = mioContext.Documents.Find(ioDcModel.ivlngDoc);
            if (lioDBDocumentModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Documento", "o"));
            if (lioDBDocumentModel.ivnroStatus >= 50)
                throw new Exception(Resources.lioE_Doc_AuthNoDelete);
            mioContext.DocumentTrackings.RemoveRange(mioContext.DocumentTrackings.Where(x => x.ivlngDoc == ioDcModel.ivlngDoc));
            mioContext.SaveChanges();
            mioContext.Documents.Remove(lioDBDocumentModel);
            mioContext.SaveChanges();
            return "OK";
        }
        public async Task Validate()
        {
            ivIDocument.Validate();
            Cuit lioCuit = new Cuit(ioDcModel.ivlngCuitEmisor, mioContext, mioConfiguration);
            IDocsIO liIDocsIO = lioCuit.getIDocsIO();
            await liIDocsIO.DocsUpdate([this]);
        }
        public async Task Share(IConfiguration vioConfiguration)
        {
            if (ioDocumentUser == null)
                throw new Exception(Resources.lioE_Mail_No);
            List<string> lcvstrAddresses = new List<string>();
            //correos del documento
            if (!string.IsNullOrEmpty(ioDocumentUser.ivstrEmail))
                foreach (string livstrAddress in ioDocumentUser.ivstrEmail.Split(";", StringSplitOptions.TrimEntries).ToList())
                    if (MailHelper.IsValidEmail(livstrAddress) && !lcvstrAddresses.Contains(livstrAddress))
                        lcvstrAddresses.Add(livstrAddress);
            //correos del cuit emisor fijos
            Cuit lioCuit = new Cuit(ioDcModel.ivlngCuitEmisor, mioContext, vioConfiguration);
            if (lioCuit.ioCnfg?.coParameters.FirstOrDefault(x => x.ivstrId == "Email") != null)
                foreach (string livstrAddress in lioCuit.ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "Email")?.ivstrValue?.Split(";", StringSplitOptions.TrimEntries))
                    if (MailHelper.IsValidEmail(livstrAddress) && !lcvstrAddresses.Contains(livstrAddress))
                        lcvstrAddresses.Add(livstrAddress);
            //correos del cuit receptor fijos
            CuitCuitModel lioCuitCuitModel = mioContext.CuitCuits.Find(ioDcModel.ivlngCuitEmisor, ioDcModel.ivlngCuitReceptor);
            if (lioCuitCuitModel != null && !string.IsNullOrEmpty(lioCuitCuitModel.ivstrEmail))
                foreach (string livstrAddress in lioCuitCuitModel.ivstrEmail.Split(";", StringSplitOptions.TrimEntries))
                    if (MailHelper.IsValidEmail(livstrAddress) && !lcvstrAddresses.Contains(livstrAddress))
                        lcvstrAddresses.Add(livstrAddress);
            //mapeador
            ServiceMapper lioServiceMapper = lioCuit.ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrWs == "mail" && (x.cvnroDocTypes[0] == 0 || x.cvnroDocTypes.Contains(ioDcModel.ivnroTipo)));
            if (lioServiceMapper == null || string.IsNullOrEmpty(lioServiceMapper.ivstrTemplate) || string.IsNullOrEmpty(lioServiceMapper.ivstrInputType))
                throw new Exception($"Configuracion de Distribucion {Resources.lioE_ObjectNoF}");
            string livstrSubject = lioServiceMapper.ivstrTemplate,
                livstrBody = lioServiceMapper.ivstrInputType,
                livstrfilename = $"{Path.GetTempPath()}/{ivstrKey}_{ioDcModel.ivnroTemplateVersion}.pdf";
            livstrSubject = livstrSubject
                .Replace("#nro", ivstrKey)
                .Replace("#cuit", ioDcModel.ivlngCuitReceptor.ToString());
            livstrBody = livstrBody
                .Replace("#nro", ivstrKey)
                .Replace("#cuit", ioDcModel.ivlngCuitReceptor.ToString())
                .Replace("#nl", Environment.NewLine);
            AlternateView lioHtmlView = AlternateView.CreateAlternateViewFromString(livstrBody, Encoding.UTF8, MediaTypeNames.Text.Html);
            string livstrB46pdf = await Print();
            File.WriteAllBytes(livstrfilename, Convert.FromBase64String(livstrB46pdf));
            Attachment lioPdfAttachment = new Attachment(livstrfilename, MediaTypeNames.Application.Pdf);
            MailHelper.Send(livstrSubject, livstrBody, lcvstrAddresses.ToArray(), null, new List<Attachment> { lioPdfAttachment }, mioContext);
            new DocumentTracking(mioContext, ioDcModel.ivlngDoc)
                .addTrack(
                    70,
                    $"{Resources.lioL_Share}: {string.Join(',', lcvstrAddresses)}"
                );
        }
        public string Tracking()
        {
            try
            {
                List<DocumentTracking> lcoDocumentTracks = new List<DocumentTracking>();
                foreach (DocumentTrackingModel lioModel in mioContext.DocumentTrackings.Where(x => x.ivlngDoc == ioDcModel.ivlngDoc))
                    lcoDocumentTracks.Add(new DocumentTracking(mioContext, lioModel));
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lcoDocumentTracks)));
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return "-1";
            }
        }
        public DocumentTracking LastTracOfStatus(short vivnroStatus)
        {
            DocumentTrackingModel lioModel = mioContext.DocumentTrackings.Where(x => x.ivlngDoc == ioDcModel.ivlngDoc && x.ivnroStatus == vivnroStatus).FirstOrDefault();
            if (lioModel != null)
                return new DocumentTracking(mioContext, lioModel);
            return null;
        }
        public string Original()
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(ioDcModel.ivstrInData));
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return "-1";
            }
        }
        public string EstadoET()
        {
            try
            {
                ioDcModel.ivnroStatus = 9;
                mioContext.Documents.Update(ioDcModel);
                mioContext.SaveChanges();
                return "OK";
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return "-1";
            }
        }
        public async Task Auth()
        {
            try
            {
                ioDcModel.ivnroStatus = await ivIDocument.Auth();
                Save();
                await SendResponse();
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
        }
        public async Task<string> SendResponse()
        {
            try
            {
                UxAuth lioUxAuth;
                Cuit lioCuit = new Cuit(ioDcModel.ivlngCuitEmisor, mioContext, mioConfiguration);
                IDocsIO liIDocsIO = lioCuit.getIDocsIO();
                await liIDocsIO.DocsUpdate([this]);
                return liIDocsIO.ivstrB64Rta;
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return null;
            }
        }
        public async Task<string> Print()
        {
            XmlDocument lioXmlDocument = new XmlDocument();
            try
            {
                string livstrXml, livstr, livstrQR;
                livstrXml = iIRawDocument.ToPrint();
                //QR
                UxAuth lioUxAuth = ivIDocument.GetAuth();
                QRData lioQRData = new QRData
                {
                    ver = 1,
                    fecha = (ioDcModel.ivdtmEmision ?? DateTime.MinValue).ToString("yyyy-MM-dd"),
                    cuit = ioDcModel.ivlngCuitEmisor,
                    ptoVta = ioDcModel.ivnumPvta,
                    tipoCmp = ioDcModel.ivnroTipo,
                    nroCmp = ioDcModel.ivlngCbte,
                    importe = ioDcModel.ivdblImporte,
                    moneda = ioDcModel.ivstrMoneda,
                    ctz = ioDocumentUser.ivdblCotizacion,
                    tipoDocRec = ioDocumentUser.ivnroTipoDocReceptor,
                    nroDocRec = ioDocumentUser.ivlngDocReceptor,
                    tipoCodAut = lioUxAuth?.ivstrAuthType == "CAE" ? "E" : string.Empty,
                    codAut = long.Parse(lioUxAuth?.ivstrAuthCode ?? "0")
                };
                livstrQR = ListHelper.GetValue("PATH", "qr", mioContext);
                livstrQR += "?p=";
                livstrQR += Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lioQRData)));
                //Nodo Auth

                livstr = DateTime.ParseExact(lioUxAuth?.ivdtmAuthVenc, ListHelper.GetValue("FORMAT", "ApiDtm", mioContext), null).ToString(ListHelper.GetValue("FORMAT", "XmlDtm", mioContext), null);
                livstrXml = livstrXml.Replace("</DTE>", $"<Autorizacion><CodAut xmlns=\"http://www.afip.com.ar/fe\">{lioUxAuth?.ivstrAuthCode}</CodAut><FechaVtoAut xmlns=\"http://www.afip.com.ar/fe\">{livstr}</FechaVtoAut><TimeStampAut xmlns=\"http://www.afip.com.ar/fe\">{livstr}T00:00:00</TimeStampAut><BarCodeFont xmlns=\"http://www.afip.com.ar/fe\">{livstrQR}</BarCodeFont><BarCode xmlns=\"http://www.afip.com.ar/fe\">{ListHelper.GetValue("PATH", "qr", mioContext)}</BarCode></Autorizacion></DTE>");
                lioXmlDocument.LoadXml(livstrXml);    //Crystal
                HttpClient lioHttpClient = new HttpClient();
                lioHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //lioHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes(livstrApiWeatherCredentials)));
                HttpResponseMessage lioResponse = await lioHttpClient.PostAsJsonAsync(
                    ListHelper.GetValue("PATH", "rpt", this.mioContext),
                    new PrintRequest
                    {
                        ivstrB64Document = Convert.ToBase64String(Encoding.UTF8.GetBytes(lioXmlDocument.OuterXml)),
                        ivstrTemplatePath = $"{ioDcModel.ivlngCuitEmisor}\\V{ioDcModel.ivnroTemplateVersion}\\{ioDcModel.ivlngCuitEmisor}_{ioDcModel.ivnroTipo}.rpt"
                    });
                string livstrResponse = lioResponse.Content.ReadAsStringAsync().Result;
                if (!lioResponse.IsSuccessStatusCode)
                    throw new Exception(livstrResponse);
                PrintResponse lioPrintResponse = JsonConvert.DeserializeObject<PrintResponse>(livstrResponse);
                if (lioPrintResponse == null || string.IsNullOrEmpty(lioPrintResponse.ivstrB64Pdf))
                    throw new Exception(JsonConvert.SerializeObject(lioPrintResponse));
                return Convert.ToBase64String(Format.UnCompress2(lioPrintResponse.ivstrB64Pdf));
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                if (!string.IsNullOrEmpty(lioXmlDocument.OuterXml))
                    LogHelper.writeinfo(lioXmlDocument.OuterXml, ListHelper.GetValue("FORMAT", "VERBOSE", mioContext) == "1");
                throw new Exception(Resources.lioE_PrintNo);
            }
        }
        public UxAuth GetAuth()
        {
            return ivIDocument.GetAuth();
        }
        #endregion
        #region PRIVATE METHODS
        private void setIDocument()
        {
            switch (ioDcModel.ivstrWs)
            {
                case "wsfev1":
                case "wsfe":
                case "wsmtxca": { ivIDocument = new DocumentV1(ioDcModel, mioContext); break; }
                case "wsfexv1": { ivIDocument = new DocumentExp(ioDcModel, mioContext); break; }
                default: throw new Exception(Resources.lioE_Svc_No);
            }
            ivIDocument.SetData(ioDocumentUser);
        }
        private long NN()
        {
            DocumentModel? lio = this.mioContext.Documents.OrderByDescending(x => x.ivlngDoc).FirstOrDefault();
            if (lio == null)
                return 1;
            return lio.ivlngDoc + 1;
        }
        #endregion
    }
}


