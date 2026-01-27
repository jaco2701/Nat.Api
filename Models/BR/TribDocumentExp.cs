using Applet.Nat.Afip.ServicesFEX;
using Applet.Nat.Afip.ServicesV1;
using Applet.Nat.Api.AFIP.Model;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.Afip;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using Opcional = Applet.Nat.Afip.ServicesFEX.Opcional;
using ServiceSoapClient = Applet.Nat.Afip.ServicesFEX.ServiceSoapClient;

namespace Applet.Nat.Api.Br.Models
{
    public class TribDocumentExp : ITribDocument
    {
        #region CONSTRUCT
        public TribDocumentExp(DocumentModel vioDocumentModel, NatContext vioContext)
        {
            mioDcModel = vioDocumentModel;
            mioContext = vioContext;
            ivstrDocWs = "wsfex";
        }
        #endregion
        #region PUBLIC PROPS
        public double ivdblCotizacion { get; set; }
        public string ivstrMoneda { get; set; }
        public string ivstrDocWs { get; set; }
        public List<DocumentAsociado> coAsociados { get; set; }
        public List<DocumentOpcional> coOpcionales { get; set; }
        public short ivnroTipoExpo { get; set; }
        public string ivstrPermisoExistente { get; set; }
        public short ivnroDestinoCmp { get; set; }
        public string ivstrRazonSocial { get; set; }
        public string ivstrDomicilioCliente { get; set; }
        public long ivlngCuitPaisCliente { get; set; }
        public long ivlngIDImpositivo { get; set; }
        public string ivstrObs { get; set; }
        public string ivstrObsComerciales { get; set; }
        public string ivstrCondpago { get; set; }
        public string ivstrIncoterms { get; set; }
        public string ivstrIncotermsds { get; set; }
        public short ivnroIdioma { get; set; }
        public string? ivstrCanMisMonExt { get; set; }
        public DateTime? ivdtmVtopago { get; set; }
        List<DocumentPermisoExp> coPermisos { get; set; }
        List<DocumentItem> coItems { get; set; }
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext { get; set; }
        private DocumentModel mioDcModel { get; set; }

        #endregion
        #region PUBLICS METHODS
        public void SetData(DocumentUser vioDocumentUser)
        {
            string livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
            vioDocumentUser.FormatAmounts();
            ivnroTipoExpo = vioDocumentUser.ivnroTipoExpo ?? 0;
            ivstrPermisoExistente = vioDocumentUser.ivstrPermisoExistente ?? string.Empty;
            ivnroDestinoCmp = vioDocumentUser.ivnroDestinoCmp ?? 0;
            ivstrRazonSocial = vioDocumentUser.ivstrRazonSocial ?? string.Empty;
            ivstrDomicilioCliente = $"{vioDocumentUser.ioDomicilioReceptor?.ivstrCalle ?? string.Empty} {vioDocumentUser.ioDomicilioReceptor?.ivstrNro ?? string.Empty} {vioDocumentUser.ioDomicilioReceptor?.ivstrDepto ?? string.Empty} {vioDocumentUser.ioDomicilioReceptor?.ivstrCuidad ?? string.Empty} {vioDocumentUser.ioDomicilioReceptor?.ivstrPais ?? string.Empty}";
            ivlngCuitPaisCliente = vioDocumentUser.ivlngCuitPaisCliente ?? 0;
            ivlngIDImpositivo = vioDocumentUser.ivlngIDImpositivo ?? 0;
            ivdblCotizacion = vioDocumentUser.ivdblCotizacion ?? 0;
            ivstrMoneda = vioDocumentUser.ivstrMoneda ?? string.Empty;
            ivstrObsComerciales = vioDocumentUser.ivstrObsComerciales ?? string.Empty;
            ivstrObs = vioDocumentUser.ivstrObs ?? string.Empty;
            ivstrCondpago = vioDocumentUser.ivstrCondPago ?? string.Empty;
            ivstrIncoterms = vioDocumentUser.ivstrIncoterms ?? string.Empty;
            ivstrIncotermsds = vioDocumentUser.ivstrIncotermsDs ?? string.Empty;
            ivnroIdioma = vioDocumentUser.ivnroIdioma ?? 0;
            ivstrCanMisMonExt = vioDocumentUser.ivstrCanMisMonExt ?? string.Empty;
            ivdtmVtopago = Format.DateFromUX(vioDocumentUser.ivstrFechaVtopago, livstrApiDtmFormat);
            if (vioDocumentUser.coAsociados != null && vioDocumentUser.coAsociados.Count > 0)
            {
                coAsociados = new List<DocumentAsociado>();
                foreach (UxDocumentAsociado lioO in vioDocumentUser.coAsociados)
                    coAsociados.Add(
                        new DocumentAsociado
                        {
                            ivdtmFechaEmision = Format.DateFromUX(lioO.ivstrFechaEmision, livstrApiDtmFormat),
                            ivlngCbteCUIT = lioO.ivlngCbteCUIT,
                            ivlngCbteNro = lioO.ivlngCbteNro,
                            ivnumCbtePuntovta = lioO.ivnumCbtePuntovta,
                            ivnroCbtetipo = lioO.ivnroCbtetipo
                        });
            }
            if (vioDocumentUser.coItems != null && vioDocumentUser.coItems.Count > 0)
            {
                coItems = new List<DocumentItem>();
                foreach (UxDocumentItem lioO in vioDocumentUser.coItems)
                    coItems.Add(
                         new DocumentItem
                         {
                             ivdblBonificaion = lioO.ivdblBonificaion,
                             ivdblCantidad = lioO.ivdblCantidad,
                             ivdblImporteIVA = lioO.ivdblImporteIVA ?? 0,
                             ivdblImporteTotal = lioO.ivdblImporteTotal ?? 0,
                             ivdblPrecioUnitario = lioO.ivdblPrecioUnitario ?? 0,
                             ivnroTipoIVA = lioO.ivnroTipoIVA,
                             ivnroUM = lioO.ivnroUM,
                             ivstrCodigo = lioO.ivstrId.ToString(),
                             ivstrDescripcion = lioO.ivstrDescripcion
                         });
            }
            if (vioDocumentUser.coPermisosExp != null && vioDocumentUser.coPermisosExp.Count > 0)
            {
                coPermisos = new List<DocumentPermisoExp>();
                foreach (UxDocumentPermisoExp lioO in vioDocumentUser.coPermisosExp)
                    coPermisos.Add(
                         new DocumentPermisoExp
                         {
                             ivnumDestinoMercaderia = lioO.ivnumDestMerc,
                             ivstrIdPermiso = lioO.ivstrId
                         });
            }
            if (vioDocumentUser.ivblnCalcPermisoExistente ?? false)
                ivstrPermisoExistente =
                    ivnroTipoExpo == 1
                    ?
                        mioDcModel.ivnroTipo == 19
                        ?
                            (coPermisos != null && coPermisos.Count > 0)
                            ?
                                "S"
                            :
                                "N"
                        :
                            string.Empty
                    :
                        string.Empty;
            if (vioDocumentUser.coOpcionales != null && vioDocumentUser.coOpcionales.Count > 0)
            {
                coOpcionales = new List<DocumentOpcional>();
                foreach (UxDocumentOpcional lioO in vioDocumentUser.coOpcionales)
                    coOpcionales.Add(
                         new DocumentOpcional
                         {
                             ivstrId = lioO.ivstrId,
                             ivstrValor = lioO.ivstrValor
                         });
            }
        }
        public async Task<double> GetCotizacion(string vivstrMoneda, DateTime vivdtm)
        {
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            ClsFEXAuthRequest lioAutRequest = new ClsFEXAuthRequest
            {
                Cuit = this.mioDcModel?.ivlngCuitEmisor ?? 0,
                Sign = lioAfipLoginResponse.ivstrSign,
                Token = lioAfipLoginResponse.ivstrToken
            };
            ServiceSoapClient lioService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            short livnroIntento = 0;
            FEXResponse_Ctz lioFEXResponse_Ctz = null;
            while (true)
            {
                livnroIntento++;
                try
                {
                    LogHelper.writeinfo(
                       $"FEXGetPARAM_CtzAsync: Url:{lioAfipService.ivstrUrl}  Auth:{JsonConvert.SerializeObject(lioAutRequest)}, Moneda: {vivstrMoneda}, Fecha:{vivdtm.ToString(lioAfipService.ivstrDateformat)}",
                       ListHelper.GetValue("FORMAT", "VERBOSE", mioContext) == "1"
                    );
                    lioFEXResponse_Ctz = await lioService.FEXGetPARAM_CtzAsync(lioAutRequest, vivstrMoneda, vivdtm.ToString("yyyy-MM-dd"));
                    break;
                }
                catch (Exception lioE)
                {
                    if (lioE.Message.Contains("The SSL connection could not be established"))
                    {
                        if (livnroIntento < 3)
                            await Task.Delay(2000);
                        continue;
                    }
                    else
                    {
                        LogHelper.write(lioE);
                        break;
                    }
                }
            }
            if (lioFEXResponse_Ctz == null)
                throw new Exception(Resources.lioE_Cotiz_No);
            if (lioFEXResponse_Ctz.FEXErr != null && lioFEXResponse_Ctz.FEXErr.ErrMsg != "OK")
                throw new Exception($"{Resources.lioE_Cotiz_No} => {lioFEXResponse_Ctz.FEXErr.ErrCode}:{lioFEXResponse_Ctz.FEXErr.ErrMsg}");
            if (lioFEXResponse_Ctz.FEXResultGet == null)
                throw new Exception(Resources.lioE_Cotiz_No);
            return (double)lioFEXResponse_Ctz.FEXResultGet.Mon_ctz;
        }
        public async Task<short> Auth()
        {
            short livnroNextStatus = 40; ;
            DocumentTracking lioDocumentTracking = new DocumentTracking(mioContext, mioDcModel.ivlngDoc);
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)int.Parse(ListHelper.GetValue("FORMAT", "TLS", mioContext));
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            ClsFEXAuthRequest lioAutRequest = new ClsFEXAuthRequest
            {
                Cuit = this.mioDcModel.ivlngCuitEmisor,
                Sign = lioAfipLoginResponse.ivstrSign,
                Token = lioAfipLoginResponse.ivstrToken
            };
            //CONSULTA ULTIMO COMPROBANTE AUTORIZADO
            ServiceSoapClient lioService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            lioService.Endpoint.Address = new System.ServiceModel.EndpointAddress(lioAfipService.ivstrUrl);
            ClsFEX_LastCMP lioClsFEX_LastCMP = new ClsFEX_LastCMP { Cbte_Tipo = mioDcModel?.ivnroTipo ?? 0, Cuit = mioDcModel?.ivlngCuitEmisor ?? 0, Pto_venta = mioDcModel?.ivnumPvta ?? 0, Sign = lioAutRequest.Sign, Token = lioAutRequest.Token };
            FEXResponseLast_CMP lioFEXResponseLast_CMP = null;
            short livnroIntento = 0;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioFEXResponseLast_CMP = await lioService.FEXGetLast_CMPAsync(lioClsFEX_LastCMP);
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
                       livnroNextStatus,
                       JsonConvert.SerializeObject(
                           new
                           {
                               Request = new
                               {
                                   lioAutRequest,
                                   lioClsFEX_LastCMP
                               },
                               Response = lioE.Message
                           }
                       )
                    );
                    LogHelper.write(lioE);
                    return livnroNextStatus;
                }
            }
            if (lioFEXResponseLast_CMP.FEXResult_LastCMP == null)
                throw new Exception($"{Resources.lioE_HeaderAuth}:{Resources.lioE_AfipRespNo}:{JsonConvert.SerializeObject(lioFEXResponseLast_CMP)}");
            //EL DOCUMENTO ES MAYOR AL ULTIMO AUTORIZADO ==> EsperaPredecesor
            if (lioFEXResponseLast_CMP.FEXResult_LastCMP.Cbte_nro + 1 < this.mioDcModel?.ivlngCbte)
            {
                livnroNextStatus = 35;
                lioDocumentTracking.addTrack(
                    livnroNextStatus,
                    JsonConvert.SerializeObject(
                        new
                        {
                            Request = new
                            {
                                lioAutRequest,
                                lioClsFEX_LastCMP
                            },
                            Response = lioFEXResponseLast_CMP
                        }
                    )
                );
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES MENOR AL ULTIMO AUTORIZADO  ==> CONSULTAR CAE
            if (lioFEXResponseLast_CMP.FEXResult_LastCMP.Cbte_nro + 1 > this.mioDcModel.ivlngCbte)
            {
                ClsFEXGetCMP lioClsFEXGetCMP = new ClsFEXGetCMP
                {
                    Cbte_nro = this.mioDcModel.ivlngCbte,
                    Cbte_tipo = this.mioDcModel.ivnroTipo,
                    Punto_vta = this.mioDcModel.ivnumPvta
                };
                FEXGetCMPResponse lioFEXGetCMPResponse = null;
                while (true)
                {
                    livnroIntento++;
                    try
                    {
                        lioFEXGetCMPResponse = await lioService.FEXGetCMPAsync(lioAutRequest, lioClsFEXGetCMP);
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
                           livnroNextStatus,
                           JsonConvert.SerializeObject(
                               new
                               {
                                   Request = new
                                   {
                                       lioAutRequest,
                                       lioClsFEXGetCMP
                                   },
                                   Response = lioE.Message
                               }
                           )
                        );
                        LogHelper.write(lioE);
                        return livnroNextStatus;
                    }
                }
                if (lioFEXGetCMPResponse?.FEXResultGet != null && !string.IsNullOrEmpty(lioFEXGetCMPResponse.FEXResultGet.Cae))
                    livnroNextStatus = 50;
                lioDocumentTracking.addTrack(
                   livnroNextStatus,
                   JsonConvert.SerializeObject(
                       new
                       {
                           Request = new
                           {
                               lioAutRequest,
                               lioClsFEXGetCMP
                           },
                           Response = lioFEXGetCMPResponse
                       }
                   )
                );
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES EL SIGUIENTE  ==> AUTORIZAR
            ClsFEXRequest lioClsFEXRequest = new ClsFEXRequest
            {
                Id = 1,
                Fecha_cbte = mioDcModel.ivdtmEmision?.ToString(lioAfipService.ivstrDateformat),
                Cbte_Tipo = mioDcModel.ivnroTipo,
                Punto_vta = mioDcModel.ivnumPvta,
                Cbte_nro = mioDcModel.ivlngCbte,
                Tipo_expo = ivnroTipoExpo,
                Permiso_existente = ivstrPermisoExistente,
                Dst_cmp = ivnroDestinoCmp,
                Cliente = ivstrRazonSocial,
                Cuit_pais_cliente = ivlngCuitPaisCliente,
                Domicilio_cliente = ivstrDomicilioCliente,
                Id_impositivo = ivlngIDImpositivo.ToString(),
                Moneda_Id = ivstrMoneda,
                Moneda_ctz = (decimal)ivdblCotizacion,
                Obs_comerciales = ivstrObsComerciales,
                Imp_total = (decimal)(mioDcModel?.ivdblImporte ?? 0),
                Obs = ivstrObs,
                Forma_pago = ivstrCondpago,
                Incoterms = ivstrIncoterms,
                Incoterms_Ds = ivstrIncotermsds,
                Idioma_cbte = ivnroIdioma,
                Moneda_ctzSpecified = ivdblCotizacion != 0,
                CanMisMonExt = string.IsNullOrEmpty(ivstrCanMisMonExt) ? null : ivstrCanMisMonExt,
            };
            short livnro = 0;
            //
            // Fecha de pago
            if (new short[] { 20, 21 }.Contains(this.mioDcModel.ivnroTipo))
                lioClsFEXRequest.Fecha_pago = null;
            else if (this.ivnroTipoExpo == 1)
                lioClsFEXRequest.Fecha_pago = null;
            else
                lioClsFEXRequest.Fecha_pago = this.ivdtmVtopago?.ToString(lioAfipService.ivstrDateformat);
            if (this.coPermisos != null && this.coPermisos.Count > 0)
            {
                lioClsFEXRequest.Permisos = new Permiso[coPermisos.Count()];
                foreach (DocumentPermisoExp lioDocumentPermisoExp in this.coPermisos)
                {
                    lioClsFEXRequest.Permisos[livnro] = new Permiso();
                    lioClsFEXRequest.Permisos[livnro].Dst_merc = lioDocumentPermisoExp.ivnumDestinoMercaderia ?? 0;
                    lioClsFEXRequest.Permisos[livnro].Id_permiso = lioDocumentPermisoExp.ivstrIdPermiso;
                    livnro++;
                }
            }
            if (this.coAsociados != null && this.coAsociados.Count > 0)
            {
                livnro = 0;
                lioClsFEXRequest.Cmps_asoc = new Cmp_asoc[this.coAsociados.Count];
                foreach (DocumentAsociado lioDocumentAsociado in this.coAsociados)
                {
                    lioClsFEXRequest.Cmps_asoc[livnro] = new Cmp_asoc();
                    lioClsFEXRequest.Cmps_asoc[livnro].Cbte_cuit = lioDocumentAsociado.ivlngCbteCUIT ?? 0;
                    lioClsFEXRequest.Cmps_asoc[livnro].Cbte_nro = lioDocumentAsociado.ivlngCbteNro ?? 0;
                    lioClsFEXRequest.Cmps_asoc[livnro].Cbte_punto_vta = (int)(lioDocumentAsociado.ivnumCbtePuntovta ?? 0);
                    lioClsFEXRequest.Cmps_asoc[livnro].Cbte_tipo = lioDocumentAsociado.ivnroCbtetipo ?? 0;
                    livnro++;
                }
            }
            if (coItems != null && coItems.Count() > 0)
            {
                livnro = 0;
                lioClsFEXRequest.Items = new Item[this.coItems.Count()];
                foreach (DocumentItem lioDocumentItem in this.coItems)
                {
                    lioClsFEXRequest.Items[livnro] = new Item();
                    lioClsFEXRequest.Items[livnro].Pro_bonificacion = System.Convert.ToDecimal(lioDocumentItem.ivdblBonificaion);
                    lioClsFEXRequest.Items[livnro].Pro_codigo = lioDocumentItem.ivstrCodigo;
                    lioClsFEXRequest.Items[livnro].Pro_ds = lioDocumentItem.ivstrDescripcion;
                    lioClsFEXRequest.Items[livnro].Pro_precio_uni = System.Convert.ToDecimal(lioDocumentItem.ivdblPrecioUnitario);
                    lioClsFEXRequest.Items[livnro].Pro_qty = System.Convert.ToDecimal(lioDocumentItem.ivdblCantidad);
                    lioClsFEXRequest.Items[livnro].Pro_total_item = System.Convert.ToDecimal(lioDocumentItem.ivdblImporteTotal);
                    lioClsFEXRequest.Items[livnro].Pro_umed = lioDocumentItem.ivnroUM ?? 0;
                    livnro++;
                }
            }
            if (this.coOpcionales != null && this.coOpcionales.Count > 0)
            {
                livnro = 0;
                lioClsFEXRequest.Opcionales = new Opcional[this.coOpcionales.Count];
                foreach (DocumentOpcional opcional in this.coOpcionales)
                {
                    lioClsFEXRequest.Opcionales[livnro] = new Opcional();
                    lioClsFEXRequest.Opcionales[livnro].Id = opcional.ivstrId;
                    lioClsFEXRequest.Opcionales[livnro].Valor = opcional.ivstrValor;
                    livnro++;
                }
            }
            lioClsFEXRequest.Id = mioDcModel.ivlngDoc;
            FEXResponseAuthorize lioFEXResponseAuthorize = null;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioFEXResponseAuthorize = await lioService.FEXAuthorizeAsync(lioAutRequest, lioClsFEXRequest);
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
                       livnroNextStatus,
                       JsonConvert.SerializeObject(
                           new
                           {
                               Request = new
                               {
                                   lioAutRequest,
                                   lioClsFEXRequest
                               },
                               Response = lioE.Message
                           }
                       )
                   );
                    LogHelper.write(lioE);
                    return 40;
                }
            }
            if (lioFEXResponseAuthorize.FEXResultAuth != null && !string.IsNullOrEmpty(lioFEXResponseAuthorize.FEXResultAuth.Cae))
                livnroNextStatus = 50;
            lioDocumentTracking.addTrack(
                livnroNextStatus,
                JsonConvert.SerializeObject(
                    new
                    {
                        Request = new
                        {
                            lioAutRequest,
                            lioClsFEXRequest
                        },
                        Response = lioFEXResponseAuthorize
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
            DocumentTrackingModel lioTrack = mioContext.DocumentTrackings.OrderByDescending(x => x.ivdtmTrack).FirstOrDefault(x => x.ivlngDoc == mioDcModel.ivlngDoc && (x.ivnroStatus == 50 || x.ivnroStatus == 40));
            if (lioTrack == null)
                throw new Exception(Resources.lioE_CAEQry_Err);
            if (lioTrack == null || string.IsNullOrEmpty(lioTrack.ivstrData))
                throw new Exception(Resources.lioE_CAEQry_Err);
            dynamic lioTrackData = JsonConvert.DeserializeObject(lioTrack.ivstrData);
            FEXResponseAuthorize lioFEXResponseAuthorize = JsonConvert.DeserializeObject<FEXResponseAuthorize>(lioTrackData.Response.ToString());
            if (lioFEXResponseAuthorize != null && (lioFEXResponseAuthorize.FEXResultAuth != null || (lioFEXResponseAuthorize.FEXErr != null && lioFEXResponseAuthorize.FEXErr.ErrCode != 0)))
            {
                return new UxAuth
                {
                    ivdtmNode = lioTrack.ivdtmTrack,
                    ivnumtrack = lioTrack.ivnumTrack,
                    ivstrAuthCode = lioFEXResponseAuthorize.FEXResultAuth?.Cae ?? string.Empty,
                    ivdtmAuthVenc = lioFEXResponseAuthorize.FEXResultAuth?.Fch_venc_Cae ?? string.Empty,
                    ivstrAuthType = "CAE",
                    ivtrStatusDesc = string.IsNullOrEmpty(lioFEXResponseAuthorize?.FEXResultAuth?.Cae) ? "Rechazado" : "Autorizado",
                    ivstrErrors = lioFEXResponseAuthorize?.FEXErr != null ? $"{lioFEXResponseAuthorize.FEXErr.ErrCode}:{lioFEXResponseAuthorize.FEXErr.ErrMsg}" : string.Empty,
                    ivstrObs = lioFEXResponseAuthorize?.FEXEvents != null ? $"{lioFEXResponseAuthorize.FEXEvents.EventCode}:{lioFEXResponseAuthorize.FEXEvents.EventMsg}" : string.Empty
                };
            }
            FEXGetCMPResponse lioFEXGetCMPResponse = JsonConvert.DeserializeObject<FEXGetCMPResponse>(lioTrackData.Response.ToString());
            if (lioFEXGetCMPResponse == null || lioFEXGetCMPResponse.FEXResultGet == null)
                throw new Exception(Resources.lioE_CAEQry_Err);
            return new UxAuth
            {
                ivdtmNode = lioTrack.ivdtmTrack,
                ivnumtrack = lioTrack.ivnumTrack,
                ivstrAuthCode = lioFEXGetCMPResponse.FEXResultGet.Cae,
                ivdtmAuthVenc = lioFEXGetCMPResponse.FEXResultGet.Fch_venc_Cae,
                ivstrAuthType = "CAE",
                ivtrStatusDesc = string.IsNullOrEmpty(lioFEXGetCMPResponse.FEXResultGet.Cae) ? "Rechazado" : "Autorizado",
                ivstrErrors = lioFEXGetCMPResponse.FEXErr != null ? $"{lioFEXGetCMPResponse.FEXErr.ErrCode}:{lioFEXGetCMPResponse.FEXErr.ErrMsg}" : string.Empty,
                ivstrObs = lioFEXGetCMPResponse.FEXEvents != null ? $"{lioFEXGetCMPResponse.FEXEvents.EventCode}:{lioFEXGetCMPResponse.FEXEvents.EventMsg}" : string.Empty
            };
        }
        public void Validate()
        {
            string livstrError = string.Empty;
            if (this.mioDcModel.ivnroTipo == 0 || !ListHelper.ContainKey("TCOMP", this.mioDcModel.ivnroTipo.ToString(), mioContext))
                livstrError += Resources.lioE_Tipo_No + Environment.NewLine;
            if (this.mioDcModel.ivlngCbte == 0)
                livstrError += Resources.lioE_Nro_No + Environment.NewLine;
            if (this.mioDcModel.ivnumPvta == 0)
                livstrError += Resources.lioE_Pventa_No + Environment.NewLine;
            //if (this.mioDcModel.ivdblImporte == 0)
            //    livstrError += Resources.lioE_Importe_No + Environment.NewLine;
            if (!ListHelper.ContainKey("TEXP", ivnroTipoExpo.ToString(), mioContext))
                livstrError += $"Tipo Expo {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (!string.IsNullOrEmpty(ivstrPermisoExistente) && ivstrPermisoExistente != "S" && ivstrPermisoExistente != "N")
                livstrError += $"Permiso Existente {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (!ListHelper.ContainKey("PAISD", ivnroDestinoCmp.ToString(), mioContext))
                livstrError += $"Pais Destinto {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (String.IsNullOrEmpty(ivstrRazonSocial))
                livstrError += $"Cliente {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (String.IsNullOrEmpty(ivstrDomicilioCliente))
                livstrError += $"Domicilio Cliente {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (mioDcModel.ivlngCuitReceptor == 0)
                livstrError += $"Cuit Receptor {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (!ListHelper.ContainKey("MON", this.ivstrMoneda, mioContext))
                livstrError += $"Moneda {Resources.lioE_ObjectNoF}" + Environment.NewLine;
            if (!ListHelper.ContainKey("INCOT", ivstrIncoterms, mioContext))
                livstrError += $"Incoterms {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (!ListHelper.ContainKey("IDIO", this.ivnroIdioma.ToString(), mioContext))
                livstrError += $"Idioma {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            if (this.coItems.Count == 0)
                livstrError += $"Items {Resources.lioE_ObjectNoM}" + Environment.NewLine;
            short index = 1;
            if (this.coAsociados != null && this.coAsociados.Count > 0)
                foreach (DocumentAsociado item in this.coAsociados)
                {
                    if (!ListHelper.ContainKey("TCOMP", item.ivnroCbtetipo.ToString(), mioContext))
                        livstrError += string.Format(Resources.lioE_TipoAsoc_No, index) + Environment.NewLine;

                    if (item.ivnumCbtePuntovta < 1 || item.ivnumCbtePuntovta > 9999)
                        livstrError += string.Format(Resources.lioE_PvtaAsoc_No, index) + Environment.NewLine;
                    index++;
                }
            index = 1;

            if (this.coPermisos != null && this.coPermisos.Count > 0)
                foreach (DocumentPermisoExp item in this.coPermisos)
                {
                    if (string.IsNullOrEmpty(item.ivstrIdPermiso))
                        livstrError += string.Format(Resources.lioE_PermisoNo, index) + Environment.NewLine;
                    if (item.ivnumDestinoMercaderia == 0)
                        livstrError += string.Format(Resources.lioE_PermisoNo, index) + Environment.NewLine;
                }
            index = 1;
            if (this.coItems != null && this.coItems.Count > 0)
                foreach (DocumentItem item in this.coItems)
                {
                    if (string.IsNullOrEmpty(item.ivstrDescripcion))
                        livstrError += string.Format(Resources.lioE_Item_No, index) + Environment.NewLine;

                    index++;
                }
            index = 1;
            if (this.coOpcionales != null && this.coOpcionales.Count > 0)
                foreach (DocumentOpcional item in this.coOpcionales)
                {
                    if (!ListHelper.ContainKey("TOPC", item.ivstrId.ToString(), mioContext))
                        livstrError += string.Format(Resources.lioE_Opcional_No, index) + Environment.NewLine;
                    index++;
                }
            if (!string.IsNullOrEmpty(livstrError))
                throw new Exception(Resources.lioE_HeaderAuth + Environment.NewLine + livstrError);
            return;
        }
        public bool AuthDataModified(ITribDocument vioIDocument)
        {
            if (vioIDocument == null) return true;
            TribDocumentExp? lioCurrentDocument = vioIDocument as TribDocumentExp;
            if (lioCurrentDocument == null) return true;
            if (lioCurrentDocument.mioDcModel.ivnroStatus < 50) return false;
            if (lioCurrentDocument.mioDcModel.ivlngCuitEmisor != mioDcModel.ivlngCuitEmisor) return true;
            if (lioCurrentDocument.mioDcModel.ivlngCbte != mioDcModel.ivlngCbte) return true;
            if (lioCurrentDocument.mioDcModel.ivnroTipo != mioDcModel.ivnroTipo) return true;
            if (lioCurrentDocument.mioDcModel.ivnumPvta != mioDcModel.ivnumPvta) return true;
            if (lioCurrentDocument.mioDcModel.ivdtmEmision != mioDcModel.ivdtmEmision) return true;
            if (lioCurrentDocument.mioDcModel.ivdblImporte != mioDcModel.ivdblImporte) return true;
            if (lioCurrentDocument.ivstrPermisoExistente != ivstrPermisoExistente) return true;
            if (lioCurrentDocument.ivnroDestinoCmp != ivnroDestinoCmp) return true;
            if (lioCurrentDocument.ivstrRazonSocial != ivstrRazonSocial) return true;
            if (lioCurrentDocument.mioDcModel.ivlngCuitReceptor != mioDcModel.ivlngCuitReceptor) return true;
            if (lioCurrentDocument.ivstrDomicilioCliente != ivstrDomicilioCliente) return true;
            if (lioCurrentDocument.ivlngCuitPaisCliente != ivlngCuitPaisCliente) return true;
            if (lioCurrentDocument.ivstrMoneda != ivstrMoneda) return true;
            if (lioCurrentDocument.ivdblCotizacion != ivdblCotizacion) return true;
            if (lioCurrentDocument.ivstrObsComerciales != ivstrObsComerciales) return true;
            if (lioCurrentDocument.ivstrCondpago != ivstrCondpago) return true;
            if (lioCurrentDocument.ivstrIncoterms != ivstrIncoterms) return true;
            if (lioCurrentDocument.ivnroIdioma != ivnroIdioma) return true;
            if (lioCurrentDocument.coPermisos != null && lioCurrentDocument.coPermisos.Count > 0)
            {
                if (lioCurrentDocument.coPermisos.Count != coPermisos.Count) return true;
                foreach (DocumentPermisoExp lioO in lioCurrentDocument.coPermisos)
                    if (!coPermisos.Any(x => x.ivstrIdPermiso == lioO.ivstrIdPermiso && x.ivnumDestinoMercaderia == lioO.ivnumDestinoMercaderia)) return true;
            }
            if (lioCurrentDocument.coItems != null && lioCurrentDocument.coItems.Count > 0)
            {
                if (lioCurrentDocument.coItems.Count != coItems.Count) return true;
                foreach (DocumentItem lioO in lioCurrentDocument.coItems)
                    if (!coItems.Any(x => x.ivdblCantidad == lioO.ivdblCantidad && x.ivdblBonificaion == lioO.ivdblBonificaion && x.ivdblPrecioUnitario == lioO.ivdblPrecioUnitario && x.ivdblImporteTotal == lioO.ivdblImporteTotal && x.ivnroUM == lioO.ivnroUM && x.ivstrCodigo == lioO.ivstrCodigo && x.ivstrDescripcion == lioO.ivstrDescripcion)) return true;
            }
            if (lioCurrentDocument.coAsociados != null && lioCurrentDocument.coAsociados.Count > 0)
            {
                if (lioCurrentDocument.coAsociados.Count != coAsociados.Count) return true;
                foreach (DocumentAsociado lioO in lioCurrentDocument.coAsociados)
                    if (!coAsociados.Any(x => x.ivlngCbteCUIT == lioO.ivlngCbteCUIT && x.ivlngCbteNro == lioO.ivlngCbteNro && x.ivnumCbtePuntovta == lioO.ivnumCbtePuntovta && x.ivnroCbtetipo == lioO.ivnroCbtetipo && x.ivdtmFechaEmision == lioO.ivdtmFechaEmision)) return true;
            }
            return false;
        }
        #endregion
        #region PRIVATE METHODS

        #endregion
    }

}
