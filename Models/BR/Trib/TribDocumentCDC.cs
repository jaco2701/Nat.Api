using Applet.Nat.Afip.ServicesCDC;
using Applet.Nat.Api.AFIP.Model;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.Afip;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Net;
using ServiceSoapClient = Applet.Nat.Afip.ServicesCDC.ServiceSoapClient;

namespace Applet.Nat.Api.Br.Models
{
    public class TribDocumentCdc : ITribDocument
    {
        #region CONSTRUCT
        public TribDocumentCdc(DocumentModel vioDocumentModel, NatContext vioContext)
        {
            mioDcModel = vioDocumentModel;
            mioContext = vioContext;
            ivstrDocWs = "wscdc";
        }
        #endregion
        #region PUBLIC PROPS
        public string ivstrDocWs { get; set; }
        public string ivstrCbteModo { get; set; }
        public string ivstrCodAutorizacion { get; set; }
        public short ivnroTipoReceptor { get; set; }
        public long ivCuitAutorizante { get { return mioDcModel.ivlngCuitReceptor; } }
        public string ivstrSR { get; set; } = "R";
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext { get; set; }
        private DocumentModel mioDcModel { get; set; }

        #endregion
        #region PUBLICS METHODS
        public void SetData(DocumentUser vioDocumentUser)
        {
            string livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
            ivstrCbteModo = vioDocumentUser.ivstrCbteModo ?? string.Empty;
            ivnroTipoReceptor = vioDocumentUser.ivnroTipoDocReceptor ?? 0;
        }
        public async Task<double> GetCotizacion(string vivstrMoneda, DateTime vivdtm)
        {
            return (double)0;
        }
        public async Task<short> Auth()
        {
            DocumentTracking lioDocumentTracking = new DocumentTracking(mioContext, mioDcModel.ivlngDoc);
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)int.Parse(ListHelper.GetValue("FORMAT", "TLS", mioContext));
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            short livnroNextStatus = 50;
            CmpAuthRequest lioAuthRequest = new CmpAuthRequest
            {
                Cuit = ivCuitAutorizante,
                Sign = lioAfipLoginResponse.ivstrSign,
                Token = lioAfipLoginResponse.ivstrToken
            };

            CmpDatos lioCmpDatos = new CmpDatos
            {
                CbteModo = ivstrCbteModo,
                CuitEmisor = mioDcModel.ivlngCuitEmisor,
                PtoVta = mioDcModel.ivnumPvta,
                CbteTipo = mioDcModel.ivnroTipo,
                CbteNro = mioDcModel.ivlngCbte,
                CbteFch = mioDcModel.ivdtmEmision?.ToString(lioAfipService.ivstrDateformat),
                ImpTotal = mioDcModel.ivdblImporte ?? 0,
                CodAutorizacion = ivstrCodAutorizacion,
                DocNroReceptor = mioDcModel.ivlngCuitReceptor.ToString(),
                DocTipoReceptor = ivnroTipoReceptor.ToString(),
            };
            //CONSULTA COMPROBANTE 
            ServiceSoapClient lioService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            lioService.Endpoint.Address = new System.ServiceModel.EndpointAddress(lioAfipService.ivstrUrl);
            short livnroIntento = 0;
            ComprobanteConstatarResponse lioCmpResponse;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioCmpResponse = await lioService.ComprobanteConstatarAsync(lioAuthRequest, lioCmpDatos);
                    break;
                }
                catch (Exception lioE)
                {
                    if (lioE.Message.Contains("The SSL connection could not be established") && livnroIntento < 3)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                    lioDocumentTracking.addTrack(
                       40,
                       JsonConvert.SerializeObject(
                           new
                           {
                               Request = new
                               {
                                   lioAuthRequest,
                                   lioCmpDatos
                               },
                               Response = ExceptionToResponse(lioE)
                           }
                       )
                    );
                    LogHelper.write(lioE);
                    return livnroNextStatus;
                }
            }
            if (lioCmpResponse.Body.ComprobanteConstatarResult != null && lioCmpResponse.Body.ComprobanteConstatarResult.Resultado == "A")
                livnroNextStatus = 50;
            else
                livnroNextStatus = 40;
            lioDocumentTracking.addTrack(
                livnroNextStatus,
                JsonConvert.SerializeObject(
                    new
                    {
                        Request = new
                        {
                            lioAuthRequest,
                            lioCmpDatos
                        },
                        Response = lioCmpResponse
                    }
                )
            );
            return livnroNextStatus;
        }
        public void SetContext(NatContext vioContext)
        {
            mioContext = vioContext;
        }
        public UxAuth GetAuth()
        {
            string livstr;
            short[] lcvnroStatusRTA = new short[] { 20, 35, 40, 50 };
            DocumentTrackingModel[] lcoTracks = mioContext.DocumentTrackings.OrderByDescending(x => x.ivdtmTrack).Where(x => x.ivlngDoc == mioDcModel.ivlngDoc).ToArray();
            if (lcoTracks == null || lcoTracks.Length == 0 || !lcoTracks.Any(x => lcvnroStatusRTA.Contains(x.ivnroStatus)))
                throw new Exception($"{Resources.lioE_CAENoSts}: ivlngDoc {mioDcModel.ivlngDoc}");
            DocumentTrackingModel lioTrack = lcoTracks.FirstOrDefault(x => lcvnroStatusRTA.Contains(x.ivnroStatus));
            if (lioTrack == null || string.IsNullOrEmpty(lioTrack.ivstrData))
                throw new Exception($"{Resources.lioE_CAERespErr}: ivlngDoc {mioDcModel.ivlngDoc}");
            dynamic lioTrackData = JsonConvert.DeserializeObject(lioTrack.ivstrData);
            UxAuth lioUxAuth = new UxAuth
            {
                ivdtmNode = lioTrack.ivdtmTrack,
                ivnumtrack = lioTrack.ivnumTrack
            };
            ComprobanteConstatarResponse lioCmpResponse = JsonConvert.DeserializeObject<ComprobanteConstatarResponse>(lioTrackData.Response.ToString());
            if (lioCmpResponse != null && lioCmpResponse.Body != null && lioCmpResponse.Body.ComprobanteConstatarResult != null)
            {
                lioUxAuth.ivstrAuthCode = lioCmpResponse.Body.ComprobanteConstatarResult.CmpResp.CodAutorizacion;
                lioUxAuth.ivdtmAuthVenc = lioCmpResponse.Body.ComprobanteConstatarResult.CmpResp.CbteFch;
                lioUxAuth.ivstrAuthType = lioCmpResponse.Body.ComprobanteConstatarResult.CmpResp.CbteModo;
                lioUxAuth.ivtrStatusDesc = lioCmpResponse.Body.ComprobanteConstatarResult.Resultado;
                livstr = string.Empty;
                if (lioCmpResponse.Body.ComprobanteConstatarResult.Errors != null)
                    livstr = string.Join(", ", lioCmpResponse.Body.ComprobanteConstatarResult.Errors.Select(x => $"{x.Code}:{x.Msg}"));
                lioUxAuth.ivstrErrors = livstr;
                livstr = string.Empty;
                if (lioCmpResponse.Body.ComprobanteConstatarResult.Events != null)
                    livstr = string.Join(", ", lioCmpResponse.Body.ComprobanteConstatarResult.Events.Select(x => $"{x.Code}:{x.Msg}"));
                lioUxAuth.ivstrErrors += livstr;
                livstr = string.Empty;
                if (lioCmpResponse.Body.ComprobanteConstatarResult.Observaciones != null)
                    livstr = string.Join(", ", lioCmpResponse.Body.ComprobanteConstatarResult.Observaciones.Select(x => $"{x.Code}:{x.Msg}"));
                lioUxAuth.ivstrObs = livstr;
                return lioUxAuth;
            }
            return lioUxAuth;
        }
        public void Validate()
        {
            string livstrError = string.Empty;
            if (mioDcModel.ivnroTipo == 0 || !ListHelper.ContainKey("TCOMP", this.mioDcModel.ivnroTipo.ToString(), mioContext))
                livstrError += Resources.lioE_Tipo_No + Environment.NewLine;
            if (mioDcModel.ivlngCbte == 0)
                livstrError += Resources.lioE_Nro_No + Environment.NewLine;
            if (mioDcModel.ivnumPvta == 0)
                livstrError += Resources.lioE_Pventa_No + Environment.NewLine;
            if (ivnroTipoReceptor== 0)
                livstrError += $"Tipo Documento Receptor {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (String.IsNullOrEmpty(ivstrCbteModo))
                livstrError += $"Modo {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (String.IsNullOrEmpty(ivstrCodAutorizacion))
                livstrError += $"Código Autorización {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            return;
        }
        public bool AuthDataModified(ITribDocument vioIDocument)
        {
            if (vioIDocument == null) return true;
            TribDocumentCdc? lioCurrentDocument = vioIDocument as TribDocumentCdc;
            if (lioCurrentDocument == null) return true;
            if (lioCurrentDocument.mioDcModel.ivnroStatus < 50) return false;
            if (lioCurrentDocument.mioDcModel.ivlngCuitEmisor != mioDcModel.ivlngCuitEmisor) return true;
            if (lioCurrentDocument.mioDcModel.ivlngCbte != mioDcModel.ivlngCbte) return true;
            if (lioCurrentDocument.mioDcModel.ivnroTipo != mioDcModel.ivnroTipo) return true;
            if (lioCurrentDocument.mioDcModel.ivnumPvta != mioDcModel.ivnumPvta) return true;
            if (lioCurrentDocument.mioDcModel.ivdtmEmision != mioDcModel.ivdtmEmision) return true;
            if (lioCurrentDocument.mioDcModel.ivdblImporte != mioDcModel.ivdblImporte) return true;
            return false;
        }
        #endregion
        #region PRIVATE METHODS
        private ComprobanteConstatarResponse ExceptionToResponse(Exception lioE)
        {
            ComprobanteConstatarResponse lioO = new ComprobanteConstatarResponse
            {
                Body = new ComprobanteConstatarResponseBody
                {
                    ComprobanteConstatarResult = new CmpResponse
                    {
                        Resultado = "ERROR",
                        CmpResp = new CmpDatos
                        {
                            CodAutorizacion = string.Empty,
                            CbteFch = string.Empty,
                            CbteModo = string.Empty
                        },
                        Errors = new Err[] { new Err { Code = 999, Msg = lioE.Message } },
                        Events = null,
                        Observaciones = null
                    }
                },
            };
            return lioO;
        }
        #endregion
    }

}
