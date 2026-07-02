using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.BR;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nat.API.Properties;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
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
            if (ListHelper.CanRun(vioContext, "0"))
            {
                ListHelper.SetProcessRunning(vioContext, "0");
                LogHelper.writeinfo($"{DateTime.Now} Procesando Documentos", ListHelper.Verbose(vioContext));
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
                ListHelper.SetProccessEnd(vioContext, "0");
            }
            lcoTasks.Add(BuildTaskUpLoad(vioConfiguration));
            lcoTasks.Add(NatDaily(vioConfiguration));
            foreach (var lioTasks in lcoTasks)
                await lioTasks;
        }
        private static async Task BuildTaskDocs(List<long> vcvlngDocsToTask, IConfiguration vioConfiguration)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                try
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
                        lcoDocuments.Add(new Document(lioDocumentModel, lioContext, vioConfiguration));
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
                        LogHelper.writeinfo($"{DateTime.Now} {Resources.lioM_ProcDoc} {lioDocument.ivstrKey}", ListHelper.GetValue("FORMAT", "VERBOSE", lioContext) == "1");

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
                            //  LogHelper.write($"{DateTime.Now} {Resources.lioM_ProcDoc} {lioDocument.ivstrKey}");
                            if (lioDocument.ioDcModel.ivnroStatus == 50)  //Aprobado
                            {
                                try
                                {
                                    if (!lioDocument.ivblPrintable)
                                        lioDocument.ioDcModel.ivnroStatus = 100;
                                    else
                                    {
                                        await lioDocument.Print();
                                        lioDocument.ioDcModel.ivnroStatus = 60;
                                    }
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
                            else if (lioDocument.ioDcModel.ivnroStatus == 60) //Impreso
                            {
                                try
                                {
                                    if (!lioDocument.ivblPrintable)
                                        lioDocument.ioDcModel.ivnroStatus = 100;
                                    else
                                    {
                                        await lioDocument.Share(vioConfiguration);
                                        lioDocument.ioDcModel.ivnroStatus = 70;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioDocument.ioDcModel.ivnroStatus = 80;
                                    new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc)
                                    .addTrack(
                                      lioDocument.ioDcModel.ivnroStatus,
                                      lioE.ToString()
                                    );
                                    LogHelper.write(lioE);
                                }
                            }
                            else if (lioDocument.ioDcModel.ivnroStatus == 70) //Distribuido
                            {
                                lioDocument.ioDcModel.ivnroStatus = 100;
                                new DocumentTracking(lioContext, lioDocument.ioDcModel.ivlngDoc)
                               .addTrack(
                                  lioDocument.ioDcModel.ivnroStatus,
                                  string.Empty
                               );
                            }
                            else if (lioDocument.ioDcModel.ivnroStatus == 80) //NoDistribuido
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
                catch (Exception lioE)
                {
                    LogHelper.write(lioE);
                }
            }
        }
        private static async Task BuildTaskUpLoad(IConfiguration vioConfiguration)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                try
                {
                    List<CuitToLoadInfo> lcoCuitsByLoadMethod = new List<CuitToLoadInfo>();
                    Cuit lioCuit;
                    short livnroLoadMethod = 0;
                    int livnumOriginator = 0;
                    int[] lcvnumCuitUsers;
                    foreach (CuitModel lioCuitModel in lioContext.Cuits)
                    {
                        lioCuit = new Cuit(lioCuitModel, lioContext, vioConfiguration);
                        CuitParameter lioCuitParameter = lioCuit.ioCnfg?.coParameters?.FirstOrDefault(x => x.ivstrId == "LoadMethod");
                        if (lioCuitParameter == null || string.IsNullOrEmpty(lioCuitParameter.ivstrValue) || !short.TryParse(lioCuitParameter.ivstrValue, out livnroLoadMethod))
                            continue;
                        if (livnroLoadMethod == (short)eLoadMethod.Manual || livnroLoadMethod == (short)eLoadMethod.Api)
                            continue;
                        lcoCuitsByLoadMethod.Add(
                            new CuitToLoadInfo
                            {
                                ioCuit = lioCuit,
                                ivnroLoadMethod = livnroLoadMethod,
                            }
                        );
                    }
                    livnroLoadMethod = 0;
                    foreach (CuitToLoadInfo lioO in lcoCuitsByLoadMethod.OrderBy(x => x.ivnroLoadMethod))
                    {
                        try
                        {
                            if (livnroLoadMethod != lioO.ivnroLoadMethod)
                            {
                                if (livnroLoadMethod != 0)
                                {
                                    ListHelper.SetProccessEnd(lioContext, livnroLoadMethod.ToString());
                                }
                                if (!ListHelper.CanRun(lioContext, lioO.ivnroLoadMethod.ToString())) continue;
                                livnroLoadMethod = lioO.ivnroLoadMethod;
                                ListHelper.SetProcessRunning(lioContext, livnroLoadMethod.ToString());
                                LogHelper.writeinfo($"{DateTime.Now} Cargando Documentos para metodo de ingreso {livnroLoadMethod}", ListHelper.Verbose(lioContext));
                            }
                            IDocsIO liIDocsIO = lioO.ioCuit.getIDocsIO();
                            // originador por defecto del cuit, para usar en cargas via servicio o ftp es el primer usuario administrador del CUIT que encuentra
                            livnumOriginator = 0;
                            lcvnumCuitUsers= lioContext.UserCuits.Where(x => x.ivlngCuit == lioO.ioCuit.ioDcModel.ivlngCuit).Select(x=>x.ivnumUser).ToArray();
                            foreach (int livnumCuitUser in lcvnumCuitUsers)
                                if (lioContext.Users.Find(livnumCuitUser)?.ivnroRol == (short)eRol.CuitAdmin)
                                {
                                    livnumOriginator = livnumCuitUser;
                                    break;
                                }
                            if (livnumOriginator == 0)
                                throw new Exception($"Usuario Originador {Resources.lioE_ObjectNoM}");
                            await liIDocsIO.DocsI(livnumOriginator);
                        }
                        catch (Exception lioE)
                        {
                            LogHelper.write(lioE);
                            continue;
                        }
                    }
                    if (livnroLoadMethod != 0)
                    {
                        ListHelper.SetProccessEnd(lioContext, livnroLoadMethod.ToString());
                    }
                }
                catch (Exception lioE)
                {
                    LogHelper.write(lioE);
                }

            }
        }
        private static async Task NatDaily(IConfiguration vioConfiguration)
        {
            using NatContext lioContext = NatContext.GetContext(vioConfiguration);
            {
                try
                {
                    if (!ListHelper.CanRun(lioContext, "3")) return;
                    LogHelper.writeinfo("******NAT Mantenimiento Diario******", true);
                    LogHelper.writeinfo("Borrado estados Oidc anteriores a 30 minutos", true);
                    lioContext.OidcStates.RemoveRange(lioContext.OidcStates.Where(s => s.ivdtmState < DateTime.UtcNow.AddMinutes(-30)));
                    lioContext.SaveChanges();
                    LogHelper.writeinfo("Borrado de logs anteriores a 3 dias", true);
                    DateTime livdtm = DateTime.Today.AddDays(-3);
                    string lioPath;
                    while (true)
                    {
                        lioPath = $"./log/{livdtm.ToString("yyyyMMdd")}.log";
                        if (File.Exists(lioPath))
                            File.Delete(lioPath);
                        else
                            break;
                        livdtm = livdtm.AddDays(-1);
                    }
                    LogHelper.writeinfo("Cotizacion", true);
                    Double livvalCtz;
                    TribDocumentV1 lio = new TribDocumentV1(new DocumentModel { ivlngCuitEmisor = long.Parse(ListHelper.GetValue("CUIT", "0", lioContext)) }, lioContext);
                    livvalCtz = await lio.GetCotizacion("DOL", DateTime.Today.AddDays(-1));
                    ListModel? lioListModel = lioContext.Lists.Find("FORMAT", "Cotizacion");
                    if (lioListModel != null)
                    {
                        lioListModel.ivstrDesc = livvalCtz.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-AR"));
                        lioContext.Lists.Update(lioListModel);
                        lioContext.SaveChanges();
                    }
                    ListHelper.SetProccessEnd(lioContext, "3");
                }
                catch (Exception lioE)
                {
                    ListHelper.SetProccessEnd(lioContext, "3");
                    LogHelper.write(lioE);
                }
            }

        }
    }
}
