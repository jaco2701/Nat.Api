using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nat.Api.Properties;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
namespace Applet.Nat.Api.Static
{
    public static class Queue
    {
        public async static Task Run(NatContext vioContext, IConfiguration vioConfiguration)
        {
            List<Task> lcoTasks = new List<Task>();
            long livlngPrevCuit = 0;
            int livnumPrevPvta = 0;
            short livnroPrevTipo = 0;
            List<long> lcvlngDocsToTask = new List<long>();
            foreach (DocumentModel lioDocumentModel in vioContext.Documents
                .Where(x => new short[] { 35, 10, 50, 70, 80, 60 }
                .Contains(x.ivnroStatus))
                .OrderBy(x => x.ivlngCuitEmisor).ThenBy(x => x.ivnroTipo).ThenBy(x => x.ivnumPvta)
                )
            {
                try
                {
                    if (livlngPrevCuit == lioDocumentModel.ivlngCuitEmisor && livnumPrevPvta == lioDocumentModel.ivnumPvta && livnroPrevTipo == lioDocumentModel.ivnroTipo)
                    {
                        lcvlngDocsToTask.Add(lioDocumentModel.ivlngDoc);
                        continue;
                    }
                    lcoTasks.Add(BuildTaskDocs(lcvlngDocsToTask, vioConfiguration));
                    livlngPrevCuit = lioDocumentModel.ivlngCuitEmisor;
                    livnumPrevPvta = lioDocumentModel.ivnumPvta;
                    livnroPrevTipo = lioDocumentModel.ivnroTipo;
                    lcvlngDocsToTask.Clear();
                    lcvlngDocsToTask.Add(lioDocumentModel.ivlngDoc);
                }
                catch (Exception lioE)
                {
                    LogHelper.write(lioE);
                }
            }
            lcoTasks.Add(BuildTaskDocs(lcvlngDocsToTask, vioConfiguration));
            lcoTasks.Add(BuildTaskUpLoad(vioConfiguration));
            //lcoTasks.Add(BuildTaskExtract(vioConfiguration));
            foreach (var lioTasks in lcoTasks)
                await lioTasks;
        }
        private static async Task BuildTaskDocs(List<long> vcvlngDocsToTask, IConfiguration vioConfiguration)
        {
            try
            {
                using NatContext lioContext = NatContext.GetContext(vioConfiguration);
                {
                    short[] lcvnroStatusAuth = { 10, 35 };
                    bool livbln1st = true;
                    if (vcvlngDocsToTask.Count() == 0) return;
                    List<Document> lcoDocuments = new List<Document>();
                    foreach (long lcvlngDoc in vcvlngDocsToTask)
                    {
                        DocumentModel lioDocumentModel = lioContext.Documents.Find(lcvlngDoc);
                        if (lioDocumentModel == null)
                            continue;
                        lcoDocuments.Add(new Document(lioDocumentModel, lioContext));
                    }
                    if (lcoDocuments.Count() == 0) return;
                    //documentos para autorizar
                    foreach (Document lioDocument in lcoDocuments.Where(x => lcvnroStatusAuth.Contains(x.ioDcModel.ivnroStatus)).OrderBy(x => x.ioDcModel.ivlngCbte))
                    {
                        if (livbln1st) // si el 1er documento de la serie esta esperando predecesor se detiene el proceso
                        {
                            livbln1st = false;
                            if (lioDocument.ioDcModel.ivnroStatus == 35)
                            {
                                //LogHelper.write($"{DateTime.Now} {string.Format(Resources.lioE_DocCorrel, $"[{lioDocument.ioDcModel.ivlngCuitEmisor}|{lioDocument.ioDcModel.ivnroTipo}|{lioDocument.ioDcModel.ivnumPvta}|{lioDocument.ioDcModel.ivlngCbte}]")}"); // se detiene el proceso
                                break;
                            }
                        }
                        try
                        {
                            lioDocument.Validate();
                        }
                        catch (Exception lioE)
                        {
                            lioDocument.ioDcModel.ivnroStatus = 20;
                            lioDocument.Save();
                            new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc)
                              .addTrack(
                                  lioDocument.ioDcModel.ivnroStatus,
                                  lioE.Message
                              );
                            LogHelper.write(lioE);
                            continue;
                        }
                        LogHelper.writeinfo($"{DateTime.Now} {Resources.lioM_ProcDoc} {lioDocument.ivstrKey}",ListHelper.GetValue("FORMAT", "VERBOSE", lioContext) == "1");

                        await lioDocument.Auth();
                        if (lioDocument.ioDcModel.ivnroStatus == 35)
                        {
                            LogHelper.writeinfo(
                                $"{DateTime.Now} {string.Format(Resources.lioE_DocCorrel, $"[{lioDocument.ioDcModel.ivlngCuitEmisor}|{lioDocument.ioDcModel.ivnroTipo}|{lioDocument.ioDcModel.ivnumPvta}|{lioDocument.ioDcModel.ivlngCbte}]")}",
                                ListHelper.GetValue("FORMAT", "VERBOSE", lioContext) == "1"
                                );
                            break;
                        }

                    }
                    //documentos para otras tareas
                    foreach (Document lioDocument in lcoDocuments.Where(x => !lcvnroStatusAuth.Contains(x.ioDcModel.ivnroStatus)).OrderBy(x => x.ioDcModel.ivlngCbte))
                    {
                        try
                        {
                            string livstrWsData = string.Empty;
                            //  LogHelper.write($"{DateTime.Now} {Resources.lioM_ProcDoc} {lioDocument.ivstrKey}");
                            if (lioDocument.ioDcModel.ivnroStatus == 50)
                            {
                                try
                                {
                                    await lioDocument.Print();
                                    lioDocument.ioDcModel.ivnroStatus = 60;
                                }
                                catch (Exception lioE)
                                {
                                    lioDocument.ioDcModel.ivnroStatus = 65;
                                    new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc)
                                    .addTrack(
                                      lioDocument.ioDcModel.ivnroStatus,
                                      lioE.ToString()
                                    );
                                    LogHelper.write(lioE);
                                }
                            }
                            else if (lioDocument.ioDcModel.ivnroStatus == 60)
                            {
                                lioDocument.ioDcModel.ivnroStatus = await lioDocument.Share();
                                livstrWsData = lioDocument.ioDocumentUser?.ivstrEmail ?? string.Empty;
                            }
                            else if (lioDocument.ioDcModel.ivnroStatus == 80)
                            {
                                lioDocument.ioDcModel.ivnroStatus = 100;
                                new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc)
                               .addTrack(
                                  lioDocument.ioDcModel.ivnroStatus,
                                  string.Empty
                               );
                            }
                            else if (lioDocument.ioDcModel.ivnroStatus == 70)
                            {
                                lioDocument.ioDcModel.ivnroStatus = 100;
                                new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc)
                               .addTrack(
                                  lioDocument.ioDcModel.ivnroStatus,
                                  string.Empty
                               );
                            }
                            lioDocument.Save();
                        }
                        catch (Exception lioE)
                        {
                            LogHelper.write(lioE);
                        }
                    }
                }
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
            }
        }
        private static async Task BuildTaskUpLoad(IConfiguration vioConfiguration)
        {
            try
            {
                if (!vioConfiguration.GetValue<bool>("AutoUpload"))
                    return;
                List<DocumentUploadResponse> lcoUDocumentsUploadResponse;
                List<Tuple<long, string>> lcoFilesToProcess = new List<Tuple<long, string>>();
                using NatContext lioContext = NatContext.GetContext(vioConfiguration);
                {
                    foreach (CuitModel lioCuitModel in lioContext.Cuits)
                    {
                        Cuit lioCuit = new Cuit { ioDcModel = lioCuitModel };
                        if (string.IsNullOrEmpty(lioCuit.ioCnfg?.ivstrInFolder))
                            continue;
                        try
                        {
                            foreach (string livstrFile in Directory.GetFiles(lioCuit.ioCnfg.ivstrInFolder).Where(file => !file.EndsWith(".log", StringComparison.OrdinalIgnoreCase)))
                                lcoFilesToProcess.Add(new Tuple<long, string>(lioCuitModel.ivlngCuit, livstrFile));
                        }
                        catch (Exception lioE)
                        {
                            LogHelper.write(lioE);
                            continue;
                        }
                    }
                }
                DocumentsUploadRequest lioDocumentsUploadRequest;
                foreach (Tuple<long, string> lioO in lcoFilesToProcess)
                {
                    lioDocumentsUploadRequest = new DocumentsUploadRequest
                    {
                        ivstrName = Path.GetFileName(lioO.Item2),
                        ivstrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(lioO.Item2))),
                        ivblnComp = false,
                        ivlngCuit = lioO.Item1
                    };
                    try
                    {
                        lcoUDocumentsUploadResponse = UploadDocument(lioDocumentsUploadRequest, vioConfiguration);
                        if (lcoUDocumentsUploadResponse.Count == 0)
                            continue;
                        if (lcoUDocumentsUploadResponse[0].ivstrDescStatus != "OK")
                            File.WriteAllText(Path.ChangeExtension(lioO.Item2, ".log"), lcoUDocumentsUploadResponse[0].ivstrDescStatus);
                        File.Delete(lioO.Item2);
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
        private static async Task BuildTaskExtract(IConfiguration vioConfiguration)
        {
            try
            {
                if (!vioConfiguration.GetValue<bool>("AutoExtract"))
                    return;
                List<DocumentUploadResponse> lcoUDocumentsUploadResponse;
                List<Tuple<long, string>> lcoFilesToProcess = new List<Tuple<long, string>>();
                using NatContext lioContext = NatContext.GetContext(vioConfiguration);
                {
                    foreach (CuitModel lioCuitModel in lioContext.Cuits)
                    {
                        Cuit lioCuit = new Cuit { ioDcModel = lioCuitModel };
                        if (string.IsNullOrEmpty(lioCuit.ioCnfg?.ivstrInFolder))
                            continue;
                        try
                        {

                        }
                        catch (Exception lioE)
                        {
                            LogHelper.write(lioE);
                            continue;
                        }
                    }
                }
                DocumentsUploadRequest lioDocumentsUploadRequest;
                foreach (Tuple<long, string> lioO in lcoFilesToProcess)
                {
                    lioDocumentsUploadRequest = new DocumentsUploadRequest
                    {
                        ivstrName = Path.GetFileName(lioO.Item2),
                        ivstrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(lioO.Item2))),
                        ivblnComp = false,
                        ivlngCuit = lioO.Item1
                    };
                    try
                    {
                        lcoUDocumentsUploadResponse = UploadDocument(lioDocumentsUploadRequest, vioConfiguration);
                        if (lcoUDocumentsUploadResponse.Count == 0)
                            continue;
                        if (lcoUDocumentsUploadResponse[0].ivstrDescStatus != "OK")
                            File.WriteAllText(Path.ChangeExtension(lioO.Item2, ".log"), lcoUDocumentsUploadResponse[0].ivstrDescStatus);
                        File.Delete(lioO.Item2);
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
        public static List<DocumentUploadResponse> UploadDocument(DocumentsUploadRequest vioDocumentsUpload, IConfiguration vioConfiguration)
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
                        lioRawDocument = new InDocumentXML(vioDocumentsUpload.ivlngCuit, lioContext);
                        break;
                    case ".txt":
                        lioRawDocument = new InDocumentTXT(vioDocumentsUpload.ivlngCuit, lioContext);
                        break;
                    default:
                        throw new Exception(string.Format(Resources.lioE_ObjectNoM, "Document Extension", "a"));
                }
                if (!vioDocumentsUpload.ivblnComp ?? false) //sino viene comprimido lo comprimo
                    vioDocumentsUpload.ivstrData = Format.Compress(vioDocumentsUpload.ivstrData);
                lioRawDocument.ivstrRaw = vioDocumentsUpload.ivstrData;
                lioRawDocument.ivstrName = vioDocumentsUpload.ivstrName;
                DocumentUser lioDocumentUser = lioRawDocument.ToDocumentUser();
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
                            ivstrDescStatus = lioDocumentUser.ivstrLoadErrors
                        });
                    lioDocumentUser.ivstrLoadErrors = string.Empty;
                }
                else
                {
                    if (lioDocumentUser.ivlngCuitEmisor != vioDocumentsUpload.ivlngCuit)
                        throw new Exception($"Cuit Emisor {Resources.lioE_ObjectNoM}");
                    lioDocument = new Document(lioDocumentUser, lioContext);
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
                                ivstrDescStatus = "OK"
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
                return lcoDocumentUserResponse;

            }
        }
    }
}
