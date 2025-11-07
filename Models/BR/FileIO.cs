using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Configuration;
using Nat.API.Properties;
using System.Text;

namespace Nat.API.Models.BR
{
    public class FileIO : IDocsIO
    {
        #region CONS
        public FileIO() { }
        public FileIO(IConfiguration vioConfiguration, NatContext vioContext )
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
        public string[] coFileExtensions { get; set; }
        public ServiceMapper ioMapper { get; set; }
        #endregion
        #region PUBLIC METHODS  
        public async Task DocsI()
        {
            List<DocumentUploadResponse> lcoUDocumentsUploadResponse;
            List<DocumentUploadRequest> locDocumentUploadRequests = new List<DocumentUploadRequest>();
            List<Tuple<long, string>> lcoFilesToProcess = new List<Tuple<long, string>>();
            try
            {
                if (!Directory.Exists(ivstrPathIn))
                    throw new Exception($"El path {ivstrPathIn} no existe.");
                foreach (string livstrFile in Directory.GetFiles(ivstrPathIn).Where(x => coFileExtensions.Any(vivstrFileExtension => x.EndsWith(vivstrFileExtension, StringComparison.OrdinalIgnoreCase))))
                    lcoFilesToProcess.Add(new Tuple<long, string>(ivlngCuit, livstrFile));
                DocumentUploadRequest lioDocumentsUploadRequest;
                foreach (Tuple<long, string> lioO in lcoFilesToProcess)
                {
                    lioDocumentsUploadRequest = new DocumentUploadRequest
                    {
                        ivstrName = Path.GetFileName(lioO.Item2),
                        ivstrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(lioO.Item2))),
                        ivblnComp = false,
                        ivlngCuit = lioO.Item1
                    };
                    lcoUDocumentsUploadResponse = DocHelper.UploadDocument(lioDocumentsUploadRequest, mioConfiguration);
                    if (lcoUDocumentsUploadResponse.Count == 0)
                        continue;
                    if (lcoUDocumentsUploadResponse[0].ivstrDescStatus != "OK")
                        File.WriteAllText(Path.ChangeExtension(lioO.Item2, ".log"), lcoUDocumentsUploadResponse[0].ivstrDescStatus);
                    File.Delete(lioO.Item2);
                }
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
        }
        public async Task DocsO(Document[] vcoDocuments)
        {
            foreach (Document lioDocument in vcoDocuments)
            {
                if (ioMapper == null)
                    throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Mapeador", "o"));
                if (string.IsNullOrEmpty(ioMapper.ivstrTemplate))
                    throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Template", "o"));
                if (!File.Exists($"{ListHelper.GetValue("PATH", "template", mioContext)}/{lioDocument.ioDcModel.ivlngCuitEmisor}/{ioMapper.ivstrTemplate}"))
                    throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Template", "o"));
                string livstrRta = File.ReadAllText($"{ListHelper.GetValue("PATH", "template", mioContext)}/{lioDocument.ioDcModel.ivlngCuitEmisor}/{ioMapper.ivstrTemplate}"), livstr, livstrPropInFile;
                DocumentTrackingModel lioDocumentTrackingModel;
                UxAuth mioAuthNode = lioDocument.ivIDocument.GetAuth();
                foreach (ServiceMapperItem lioServiceMapperItem in ioMapper.coItems)
                {
                    livstr = string.Empty;
                    switch (lioServiceMapperItem.ivstrProperty)
                    {
                        case "ivdtmGen":
                            livstr = DateTime.Now.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivnroTipo":
                            livstr = lioDocument.ioDcModel.ivnroTipo.ToString();
                            break;
                        case "ivlngDoc":
                            livstr = lioDocument.ioDcModel.ivlngDoc.ToString();
                            break;
                        case "ivnumPvta":
                            livstr = lioDocument.ioDcModel.ivnumPvta.ToString();
                            break;
                        case "ivlngCbte":
                            livstr = lioDocument.ioDcModel.ivlngCbte.ToString();
                            break;
                        case "ivdtmEmision":
                            if (lioDocument.ioDcModel.ivdtmEmision == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmEmision", "a"));
                            livstr = (lioDocument.ioDcModel.ivdtmEmision ?? DateTime.MinValue).ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdblImporte":
                            livstr = lioDocument.ioDcModel.ivdblImporte.ToString();
                            break;
                        case "ivlngCuitEmisor":
                            livstr = lioDocument.ioDcModel.ivlngCuitEmisor.ToString();
                            break;
                        case "ivlngDocReceptor":
                            livstr = lioDocument.ioDocumentUser.ivlngDocReceptor.ToString();
                            break;
                        case "ivstrIdCliente":
                            livstr = lioDocument.ioDocumentUser.ivstrIdCliente;
                            break;
                        case "ivdtmRec":
                            lioDocumentTrackingModel = mioContext.DocumentTrackings.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == lioDocument.ioDcModel.ivlngDoc && x.ivnroStatus == 10);
                            if (lioDocumentTrackingModel == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "DtmRec", "a"));
                            livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivdtmAct":
                            lioDocumentTrackingModel = mioContext.DocumentTrackings.OrderByDescending(x => x.ivnumTrack).FirstOrDefault(x => x.ivlngDoc == lioDocument.ioDcModel.ivlngDoc);
                            if (lioDocumentTrackingModel == null)
                                throw new Exception(string.Format(Resources.lioE_ObjectNoM, "dtmAct", "a"));
                            livstr = lioDocumentTrackingModel.ivdtmTrack.ToString(lioServiceMapperItem.ivstrformat);
                            break;
                        case "ivstrFileName":
                            livstr = $"{lioDocument.ivstrKey}_{lioDocument.ioDcModel.ivnroTemplateVersion}.pdf";
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
                string livstrPathOut = $"{ivstrPathOut}/{lioDocument.ioDcModel.ivnroTipo.ToString().PadLeft(2, '0')}_{lioDocument.ioDcModel.ivnumPvta.ToString().PadLeft(4, '0')}_{lioDocument.ioDcModel.ivlngCbte.ToString().PadLeft(8, '0')}.{ioMapper.ivstrTemplate.Split('.')[1].Trim()}";
                if (File.Exists(livstrPathOut))
                    File.Delete(livstrPathOut);
                foreach (string livstrline in livstrRta.Split("\r\n"))
                {
                    if (string.IsNullOrEmpty(livstrline.Trim()))
                        continue;
                    livstr = livstrline;
                    if (livstr.Length < ioMapper.ivnumRecLen)
                        livstr = livstrline.PadRight(ioMapper.ivnumRecLen ?? 0, ' ');
                    File.AppendAllText(livstrPathOut, $"{livstr}\r\n", Encoding.UTF8);
                }
            }
        }
        #endregion
    }
}
