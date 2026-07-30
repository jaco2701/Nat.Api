using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Nat.API.Properties;
using System.Text;

namespace Nat.API.Models.BR
{
    public class FileIO : IDocsIO
    {
        #region CONS
        public FileIO() { }
        public FileIO(IConfiguration vioConfiguration)
        {
            mioConfiguration = vioConfiguration;
        }
        #endregion
        #region PRIVATE PROPS
        private IConfiguration mioConfiguration { get; set; }
        #endregion
        #region PUBLIC PROPS
        public long ivlngCuit { get; set; }
        public string ivstrPathIn { get; set; }
        public string ivstrPathOut { get; set; }
        public string ivstrB64Rta { get; set; }
        public string[] coFileExtensions { get; set; }
        public ServiceMapper ioMapper { get; set; }
        #endregion
        #region PUBLIC METHODS  
        public async Task DocsI(int vivnumUserOriginator)
        {
            using NatContext lioContext = NatContext.GetContext(mioConfiguration);
            {
                ListHelper.SetProcessRunning(lioContext, "1");
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
                        lcoUDocumentsUploadResponse = DocHelper.UploadDocument(lioDocumentsUploadRequest, mioConfiguration,vivnumUserOriginator);
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
        }
        public async Task DocO(Document[] vcoDocuments)
        {
            string livstrRta, livstrPathOut, livstr;
            foreach (Document lioDocument in vcoDocuments)
            {
                livstrRta = DocHelper.BuildDocumentResponse(lioDocument, mioConfiguration, ioMapper);
                livstrPathOut = $"{ivstrPathOut}/{lioDocument.ioDcModel.ivnroTipo.ToString().PadLeft(2, '0')}_{lioDocument.ioDcModel.ivnumPvta.ToString().PadLeft(4, '0')}_{lioDocument.ioDcModel.ivlngCbte.ToString().PadLeft(8, '0')}.{ioMapper.ivstrTemplate.Split('.')[1].Trim()}";
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
            ivstrB64Rta = "OK";
        }
        #endregion
    }
}
