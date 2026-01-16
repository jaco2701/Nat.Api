using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Microsoft.AspNetCore.Mvc;
using Applet.Nat.Api.Static;
using Applet.Nat.Api.Ifaces;
using Nat.API.Properties;
using Nat.Api.Models.BR;
using System.Data;
using Applet.Nat.Api.Models.BR;

namespace Applet.Nat.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly IConfiguration mioConfiguration;
        private readonly Token mioToken;
        public DocumentController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor, IConfiguration vioConfiguration)
        {
            mioContext = vioContext;
            mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
            mioConfiguration = vioConfiguration;
        }

        [HttpPost("Get")]
        public Response Get([FromBody] Filter? vioFilter)
        {
            try
            {
                vioFilter.ivstrDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
                if (vioFilter == null)
                    vioFilter = new Filter();
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                return ResponseHelper.Get(mioContext.Documents.Where(vioFilter.Build<DocumentModel>()));
            }
            catch (Exception lioE)
            {
                {
                    LogHelper.write(lioE);
                    return ResponseHelper.Get(-1, lioE);
                }
            }
        }
        [HttpPost("Export")]
        public Response Export([FromBody] Filter? vioFilter)
        {
            try
            {
                vioFilter.ivstrDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
                Document lioDocument;
                if (vioFilter == null)
                    vioFilter = new Filter();
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                DataSet lioDS = new DataSet();
                DataTable lioDT = new DataTable("Documents");
                // Add columns to the DataTable
                lioDT.Columns.Add("Id.", typeof(long));
                lioDT.Columns.Add("C.U.I.T.", typeof(long));
                lioDT.Columns.Add("Razon Social", typeof(string));
                lioDT.Columns.Add("Fecha Emision", typeof(string));
                lioDT.Columns.Add("Tipo", typeof(string));
                lioDT.Columns.Add("Punto de Venta", typeof(int));
                lioDT.Columns.Add("Nro.", typeof(long));
                lioDT.Columns.Add("Importe", typeof(double));
                lioDT.Columns.Add("Moneda", typeof(string));
                lioDT.Columns.Add("Estado", typeof(string));
                lioDT.Columns.Add("CAE", typeof(string));
                lioDT.Columns.Add("Fecha Vencimiento CAE", typeof(string));
                lioDT.Columns.Add("Nro.Interno", typeof(string));
                ListModel[] lcoStatus = ListHelper.GetAll("STATUS", mioContext);
                ListModel[] lcoTypes = ListHelper.GetAll("TCOMP", mioContext);
                UxAuth? lioUxAuth;
                foreach (DocumentModel lioDocumentModel in mioContext.Documents.Where(vioFilter.Build<DocumentModel>()))
                {
                    lioDocument = new Document(lioDocumentModel, mioContext, mioConfiguration);
                    try
                    {
                        lioUxAuth = lioDocument.GetAuth();
                    }
                    catch
                    {
                        lioUxAuth = null;
                    }
                    DataRow row = lioDT.NewRow();
                    row["Id."] = lioDocumentModel.ivlngDoc;
                    row["C.U.I.T."] = lioDocumentModel.ivlngCuitReceptor;
                    row["Razon Social"] = lioDocument.ioDcModel?.ivstrRazonSocial ?? string.Empty;
                    row["Fecha Emision"] = (lioDocumentModel.ivdtmEmision ?? DateTime.MinValue).ToString("dd/MM/yyyy");
                    row["Tipo"] = lcoTypes.FirstOrDefault(x => x.ivcodId == lioDocumentModel.ivnroTipo.ToString())?.ivstrDesc ?? string.Empty; ;
                    row["Punto de Venta"] = lioDocumentModel.ivnumPvta;
                    row["Nro."] = lioDocumentModel.ivlngCbte;
                    row["Importe"] = lioDocumentModel.ivdblImporte;
                    row["Moneda"] = lioDocument.ioDocumentUser?.ivstrMoneda ?? string.Empty;
                    row["Estado"] = lcoStatus.FirstOrDefault(x => x.ivcodId == lioDocumentModel.ivnroStatus.ToString())?.ivstrDesc ?? string.Empty;
                    row["CAE"] = lioUxAuth?.ivstrAuthCode ?? string.Empty;
                    row["Fecha Vencimiento CAE"] = string.IsNullOrEmpty(lioUxAuth?.ivdtmAuthVenc) ? string.Empty : DateTime.ParseExact(lioUxAuth.ivdtmAuthVenc, "yyyyMMdd", null).ToString("dd/MM/yyyy");
                    row["Nro.Interno"] = lioDocument.ioDcModel.ivstrIdCliente;
                    lioDT.Rows.Add(row);
                }
                lioDS.Tables.Add(lioDT);
                return ResponseHelper.Get(Convert.ToBase64String(Excel.GenerateFormDataSet(lioDS, "Documentos", "10,10,20,10,10,10,10,10,10,10").ToArray()));
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }
        [HttpPost("Upload")]
        public Response Upload([FromBody] DocumentUploadRequest vioDocumentsUpload)
        {
            try
            {
                return ResponseHelper.Get(DocHelper.UploadDocument(vioDocumentsUpload, mioConfiguration));
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }
        [HttpPost("Task")]
        public async Task<Response> Task([FromBody] DocumentTask vioDocumentTask)
        {
            try
            {
                User lioUser = new User(mioToken.ivnumUser, mioContext); 
                if (lioUser == null)
                    throw new Exception(Resources.lioE_NoCreds);
                if (!lioUser.ioDcModel.ivblnEnable?? false)
                    throw new Exception(Resources.lioE_User_Block);
                if ((vioDocumentTask.cvlngDocs == null || vioDocumentTask.cvlngDocs.Length == 0) && (vioDocumentTask.coKeys == null || vioDocumentTask.coKeys.Length == 0))
                    throw new Exception(Resources.lioE_NoDocs);
                if (vioDocumentTask.coKeys != null && vioDocumentTask.coKeys.Length > 0)
                {
                    List<long> lcoDocs = new List<long>();
                    DocumentModel lioDocumentModel;
                    foreach (DocumentKey lioDocumentKey in vioDocumentTask.coKeys)
                    {
                        lioDocumentModel = mioContext.Documents.FirstOrDefault(x => x.ivlngCuitEmisor == lioDocumentKey.ivlngCuitEmisor && x.ivnroTipo == lioDocumentKey.ivnroTipo && x.ivnumPvta == lioDocumentKey.ivnumPvta && x.ivlngCbte == lioDocumentKey.ivlngCbte);
                        if (lioDocumentModel != null)
                            lcoDocs.Add(lioDocumentModel.ivlngDoc);
                    }
                    vioDocumentTask.cvlngDocs = lcoDocs.ToArray();
                }
                List<DocumentTaskResponse> lcoDocumentTaskResponses = new List<DocumentTaskResponse>();
                DocumentTaskResponse lioDocumentTaskResponse;
                foreach (DocumentModel lioDocumentModel in mioContext.Documents.Where(x => vioDocumentTask.cvlngDocs.Contains(x.ivlngDoc)))
                {
                    if (!lioUser.coCuitsModels.Any(x => x.ivlngCuit == lioDocumentModel.ivlngCuitEmisor))
                        continue;
                    Document lioDocument = new Document(lioDocumentModel, mioContext, mioConfiguration);
                    lioDocumentTaskResponse = new DocumentTaskResponse
                    {
                        ivlngDoc = lioDocumentModel.ivlngDoc,
                        ioKey = new DocumentKey
                        {
                            ivlngCbte = lioDocumentModel.ivlngCbte,
                            ivlngCuitEmisor = lioDocumentModel.ivlngCuitEmisor,
                            ivnroTipo = lioDocumentModel.ivnroTipo,
                            ivnumPvta = lioDocumentModel.ivnumPvta
                        }
                    };
                    switch (vioDocumentTask.ieTask)
                    {
                        case eTask.GetPdf:
                            if (!lioDocument.ivblPrintable)
                                throw new Exception(Resources.lioE_DocNoPrint);
                            lioDocumentTaskResponse.ioData = await lioDocument.Print();
                            if (lioDocument.ioDcModel.ivnroStatus == 65)
                            {
                                lioDocument.ioDcModel.ivnroStatus = 60;
                                lioDocument.Save();
                            }
                            new DocumentTracking(mioContext, lioDocument.ioDcModel.ivlngDoc)
                                .addTrack(
                                    60,
                                    string.Empty
                                );
                            break;
                        case eTask.Share:
                            if (!lioDocument.ivblPrintable)
                                throw new Exception(Resources.lioE_DocNoPrint);
                            await lioDocument.Share(mioConfiguration);
                            lioDocumentTaskResponse.ioData = "OK";
                            break;
                        case eTask.Original:
                            lioDocumentTaskResponse.ioData = lioDocument.Original();
                            break;
                        case eTask.Tracking:
                            lioDocumentTaskResponse.ioData = lioDocument.Tracking();
                            break;
                        case eTask.Auth:
                            await lioDocument.Auth();
                            lioDocumentTaskResponse.ioData = "OK";
                            break;
                        case eTask.ETsts:
                            lioDocumentTaskResponse.ioData = lioDocument.EstadoET();
                            break;
                        case eTask.Delete:
                            lioDocument.Delete();
                            lioDocumentTaskResponse.ioData = "OK";
                            break;
                        case eTask.Rta:
                            lioDocumentTaskResponse.ioData = await lioDocument.SendResponse();
                            break;
                    }
                    lcoDocumentTaskResponses.Add(lioDocumentTaskResponse);
                }
                return ResponseHelper.Get(lcoDocumentTaskResponses);
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE.Message);
            }
        }
    }
}