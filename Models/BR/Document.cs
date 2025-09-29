using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Nat.Api.Models.BR;
using Nat.Api.Properties;
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
                if (mioDocumentUser == null || mioDocumentUser.ivlngCbte==null)
                {
                    mioDocumentUser = new DocumentUser();
                    iIRawDocument.ivstrRaw = ioDcModel.ivstrInData ?? String.Empty;
                    iIRawDocument.ivstrName = $"{ioDcModel.ivnroTipo}_0";
                    iIRawDocument.ivstrKey = ivstrKey;
                    mioDocumentUser = iIRawDocument?.ToDocumentUser();
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
                        case ".xml": miIRawDocument = new InDocumentXML(ioDcModel.ivlngCuitEmisor, mioContext); break;
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
        //private Token mioToken;
        private IDocument mivIDocument { get; set; }
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
        public Document(long vivlngDoc, NatContext vioContext)
        {
            mioContext = vioContext;
            DocumentModel lioDocumentModel = mioContext.Documents.Find(vivlngDoc);
            if (lioDocumentModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Documento", "o"));
            ioDcModel = lioDocumentModel;
            setIDocument();
        }
        public Document(DocumentModel vioDocumentModel, NatContext vioContext)
        {
            mioContext = vioContext;
            if (vioDocumentModel == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Documento", "o"));
            ioDcModel = vioDocumentModel;
            setIDocument();
        }
        public Document(DocumentUser vioDocumentUser, NatContext vioContext)
        {
            mioContext = vioContext;
            ioDcModel = new DocumentModel
            {
                ivlngCuitEmisor = vioDocumentUser.ivlngCuitEmisor ?? 0,
                ivnroTipo = vioDocumentUser.ivnroTipoDoc ?? 0,
                ivnumPvta = vioDocumentUser.ivnumPvta ?? 0,
                ivlngCbte = vioDocumentUser.ivlngCbte ?? 0,
                ivdtmEmision = Format.DateFromUX(vioDocumentUser.ivdtmEmision, ListHelper.GetValue("Format", "ApiDtm", mioContext)),
                ivlngCuitReceptor = vioDocumentUser.ivlngDocReceptor ?? 0,
                ivstrWs = vioDocumentUser.ivstrWs,
                ivstrInData = Convert.ToBase64String(Encoding.UTF8.GetBytes(vioDocumentUser.ivstrInputData)),
                ivdblImporte = vioDocumentUser.ivdblImporteTotal ?? 0,
                ivstrIdCliente = vioDocumentUser.ivstrIdCliente ?? string.Empty,
                ivstrMoneda = vioDocumentUser.ivstrMoneda ?? string.Empty,
                ivstrRazonSocial = vioDocumentUser.ivstrRazonSocial ?? string.Empty
            };
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
                Cuit lioCuit = new Cuit(ioDcModel.ivlngCuitEmisor, mioContext);
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
                if (lioDBDocumentModel.ivnroStatus >= 50 && mivIDocument.AuthDataModified(new Document(lioDBDocumentModel, mioContext).mivIDocument))
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
        public void Validate()
        {
            mivIDocument.Validate();
        }
        public async Task<short> Share()
        {
            try
            {
                if (ioDocumentUser == null)
                    throw new Exception(Resources.lioE_Mail_No);
                if (string.IsNullOrEmpty(ioDocumentUser.ivstrEmail))
                    throw new Exception(Resources.lioE_Mail_No);
                if (!MailHelper.IsValidEmail(ioDocumentUser?.ivstrEmail))
                    throw new Exception(Resources.lioE_Mail_No);
                string vivstrImgPath = "./Rpt/nat.png";
                string vivstrTemplatePath = "./Rpt/email-doc.html";
                System.Net.Mail.LinkedResource lioImgLinkResource = new System.Net.Mail.LinkedResource(vivstrImgPath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = System.Guid.NewGuid().ToString()
                };
                string livstrTitle = $"{Resources.lioL_Mail_Subject}: {mivstrDisplay}";
                string livstrBody = System.IO.File.ReadAllText(vivstrTemplatePath)
                    .Replace("[IVSTRTITLE]", livstrTitle)
                    .Replace("[IMG_NAT]", $"cid:{lioImgLinkResource.ContentId}");
                AlternateView lioHtmlView = AlternateView.CreateAlternateViewFromString(livstrBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                string livstr = await Print(), strfilename = $"{Path.GetTempPath()}/{ivstrKey}_{ioDcModel.ivnroTemplateVersion}.pdf";
                File.WriteAllBytes(strfilename, Format.UnCompress2(livstr));
                Attachment lioPdfAttachment = new Attachment(strfilename, MediaTypeNames.Application.Pdf);
                lioHtmlView.LinkedResources.Add(lioImgLinkResource);
                MailHelper.Send(mioDocumentUser.ivstrEmail, Resources.lioL_Mail_Subject, livstrBody, new List<AlternateView> { lioHtmlView }, new List<Attachment> { lioPdfAttachment }, mioContext);
                return 70;
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                new DocumentTracking(mioContext, ioDcModel.ivlngDoc)
                .addTrack(
                   80,
                   string.Empty
                );
                return 80;
            }
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
                ioDcModel.ivnroStatus = await mivIDocument.Auth();
                Save();
                SendResponse();
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
        }
        public string SendResponse()
        {
            UxAuth lioUxAuth;
            Cuit lioCuit = new Cuit(ioDcModel.ivlngCuitEmisor, mioContext);
            if (lioCuit.ioCnfg == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "CnfgCuit", "a"));
            ServiceMapper lioMapper = lioCuit.ioCnfg?.coServiceMappers.FirstOrDefault(x => x.ivstrWs == "rta");
            if (lioMapper == null)
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Mapeador", "o"));
            if (string.IsNullOrEmpty(lioMapper.ivstrTemplate))
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Template", "o"));
            if (!File.Exists($"{ListHelper.GetValue("PATH", "template", mioContext)}/{ioDcModel.ivlngCuitEmisor}/{lioMapper.ivstrTemplate}"))
                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Template", "o"));
            string livstrRta = File.ReadAllText($"{ListHelper.GetValue("PATH", "template", mioContext)}/{ioDcModel.ivlngCuitEmisor}/{lioMapper.ivstrTemplate}"), livstr, livstrPropInFile;
            DocumentTrackingModel lioDocumentTrackingModel;
            UxAuth mioAuthNode = mivIDocument.GetAuth();
            foreach (ServiceMapperItem lioServiceMapperItem in lioMapper.coItems)
            {
                livstr = string.Empty;
                switch (lioServiceMapperItem.ivstrProperty)
                {
                    case "ivdtmGen":
                        livstr = DateTime.Now.ToString(lioServiceMapperItem.ivstrformat);
                        break;
                    case "ivnroTipo":
                        livstr = ioDcModel.ivnroTipo.ToString();
                        break;
                    case "ivlngDoc":
                        livstr = ioDcModel.ivlngDoc.ToString();
                        break;
                    case "ivnumPvta":
                        livstr = ioDcModel.ivnumPvta.ToString();
                        break;
                    case "ivlngCbte":
                        livstr = ioDcModel.ivlngCbte.ToString();
                        break;
                    case "ivdtmEmision":
                        if (ioDcModel.ivdtmEmision == null)
                            throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmEmision", "a"));
                        livstr = (ioDcModel.ivdtmEmision ?? DateTime.MinValue).ToString(lioServiceMapperItem.ivstrformat);
                        break;
                    case "ivdblImporte":
                        livstr = ioDcModel.ivdblImporte.ToString();
                        break;
                    case "ivlngCuitEmisor":
                        livstr = ioDcModel.ivlngCuitEmisor.ToString();
                        break;
                    case "ivlngDocReceptor":
                        livstr = ioDocumentUser.ivlngDocReceptor.ToString();
                        break;
                    case "ivstrIdCliente":
                        livstr = ioDocumentUser.ivstrIdCliente;
                        break;
                    case "ivdtmRec":
                        lioDocumentTrackingModel = mioContext.DocumentTrackings.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == ioDcModel.ivlngDoc && x.ivnroStatus == 10);
                        if (lioDocumentTrackingModel == null)
                            throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmRec", "a"));
                        livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                        break;
                    case "ivdtmAct":
                        lioDocumentTrackingModel = mioContext.DocumentTrackings.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == ioDcModel.ivlngDoc);
                        if (lioDocumentTrackingModel == null)
                            throw new Exception(string.Format(Resources.lioE_ObjectNoM, "dtmAct", "a"));
                        livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                        break;
                    case "ivstrFileName":
                        livstr = $"{ivstrKey}_{ioDcModel.ivnroTemplateVersion}.pdf";
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
            if (!string.IsNullOrEmpty(lioCuit.ioCnfg.ivstrOutFolder))
            {
                string livstrFileName = $"{ioDcModel.ivnroTipo.ToString().PadLeft(2, '0')}_{ioDcModel.ivnumPvta.ToString().PadLeft(4, '0')}_{ioDcModel.ivlngCbte.ToString().PadLeft(8, '0')}.{lioMapper.ivstrTemplate.Split('.')[1].Trim()}";
                string livstrPathOut = $"{lioCuit.ioCnfg.ivstrOutFolder}/{livstrFileName}";
                if (File.Exists(livstrPathOut))
                    File.Delete(livstrPathOut);
                foreach (string livstrline in livstrRta.Split("\r\n"))
                {
                    if (string.IsNullOrEmpty(livstrline.Trim()))
                        continue;
                    livstr = livstrline;
                    if (livstr.Length < lioMapper.ivnumRecLen)
                        livstr = livstrline.PadRight(lioMapper.ivnumRecLen ?? 0, ' ');
                    File.AppendAllText(livstrPathOut, $"{livstr}\r\n", Encoding.UTF8);
                }
                new DocumentTracking(mioContext, ioDcModel.ivlngDoc)
                   .addTrack(
                       55,
                       $"{Resources.lioL_Rta} {livstrFileName}"
                   );
            }
            else
                new DocumentTracking(mioContext, ioDcModel.ivlngDoc)
                    .addTrack(
                        55,
                        Resources.lioL_RtaWS
                    );
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(livstrRta));
        }
        public async Task<string> Print()
        {
            XmlDocument lioXmlDocument = new XmlDocument();
            try
            {
                string livstrXml, livstr, livstrQR;
                livstrXml = iIRawDocument.ToPrint();
                //QR
                UxAuth lioUxAuth = mivIDocument.GetAuth();
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
                    LogHelper.writeinfo(lioXmlDocument.OuterXml,ListHelper.GetValue("FORMAT", "VERBOSE", mioContext)=="1");
                throw new Exception(Resources.lioE_PrintNo);
            }
        }
        public UxAuth GetAuth()
        {
            return mivIDocument.GetAuth();
        }
        #endregion
        #region PRIVATE METHODS
        private void setIDocument()
        {
            switch (ioDcModel.ivstrWs)
            {
                case "wsfev1":
                case "wsfe":
                case "wsmtxca": { mivIDocument = new DocumentV1(ioDcModel, mioContext); break; }
                case "wsfexv1": { mivIDocument = new DocumentExp(ioDcModel, mioContext); break; }
                default: throw new Exception(Resources.lioE_Svc_No);
            }
            mivIDocument.SetData(ioDocumentUser);
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


