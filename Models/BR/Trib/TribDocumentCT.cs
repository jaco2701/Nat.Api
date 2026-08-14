using Applet.Nat.Afip.ServicesCT;
using Applet.Nat.Api.AFIP.Model;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.Afip;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Microsoft.IdentityModel.Tokens;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Net;

namespace Applet.Nat.Api.Br.Models
{
    public class TribDocumentCT : ITribDocument
    {
        #region CONSTRUCT
        public TribDocumentCT(DocumentModel vioDocumentModel, NatContext vioContext)
        {
            mioDcModel = vioDocumentModel;
            mioContext = vioContext;
            ivstrDocWs = "wsct";
        }
        #endregion
        #region PUBLIC PROPS
        public short ivnroPais { get; set; }
        public double ivdblImporteNoGravado { get; set; }
        public double ivdblImporteGravado { get; set; }
        public double ivdblImporteExento { get; set; }
        public double ivdblImporteReintegro { get; set; }
        public double ivdblImporteOtrosTributos { get; set; }
        public double ivdblCotizacion { get; set; }
        public string ivstrMoneda { get; set; }
        public short ivnroTipoResp { get; set; }
        public short ivnroTipoReceptor { get; set; }
        public string ivstrNroReceptor { get; set; }
        public string ivstrDocWs { get; set; }
        public string ivstrDomicilio { get; set; }
        public string ivstrObservaciones { get; set; }
        public DateTime? ivdtmVtopago { get; set; }
        public string? ivstrCanMisMonExt { get; set; }
        public List<DocumentOtroTributo> coOtrosTributos { get; set; }
        public List<DocumentIva> coIvas { get; set; }
        public List<DocumentAdicional> coAdicionales { get; set; }
        public bool? ivblnTaxInLines { get; set; }
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext { get; set; }
        private DocumentModel mioDcModel { get; set; }
        #endregion
        #region PUBLICS METHODS
        public void SetData(DocumentUser vioDocumentUser)
        {
            string livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
            ivdblImporteNoGravado = vioDocumentUser.ivdblImporteNoGravado ?? 0;
            ivdblImporteGravado = vioDocumentUser.ivdblImporteGravado ?? 0;
            ivdblImporteExento = vioDocumentUser.ivdblImporteExento ?? 0;
            ivdblImporteReintegro = vioDocumentUser.ivdblImporteReintegro ?? 0;
            ivdblImporteOtrosTributos = vioDocumentUser.ivdblImporteOtrosTributos ?? 0;
            ivdblCotizacion = vioDocumentUser.ivdblCotizacion ?? 0;
            ivstrMoneda = vioDocumentUser.ivstrMoneda ?? string.Empty;
            ivnroTipoReceptor = vioDocumentUser.ivnroTipoDocReceptor ?? 0;
            ivnroTipoResp = vioDocumentUser.ivnroTipoRespReceptor ?? 0;
            ivstrNroReceptor = vioDocumentUser.ivlngDocReceptor.ToString() ?? string.Empty;
            ivdtmVtopago = Format.DateFromUX(vioDocumentUser.ivstrFechaVtopago, livstrApiDtmFormat);
            ivstrCanMisMonExt = vioDocumentUser.ivstrCanMisMonExt ?? string.Empty;
            short livnro;

            if (vioDocumentUser.coAdicionales != null && vioDocumentUser.coAdicionales.Count > 0)
            {
                coAdicionales = new List<DocumentAdicional>();
                foreach (UxDocumentAdicional lioO in vioDocumentUser.coAdicionales)
                {
                    if (lioO.ivnroTipo == 0) continue;
                    coAdicionales.Add(
                         new DocumentAdicional
                         {
                             ivnroTipo = lioO.ivnroTipo ?? 0,
                             ivstrValor1 = lioO.ivstrValor1,
                             ivstrValor2 = lioO.ivstrValor2,
                             ivstrValor3 = lioO.ivstrValor3,
                             ivstrValor4 = lioO.ivstrValor4,
                             ivstrValor5 = lioO.ivstrValor5,
                             ivstrValor6 = lioO.ivstrValor6
                         });
                }
            }
            if (vioDocumentUser.coIvas != null && vioDocumentUser.coIvas.Count > 0)
            {
                coIvas = new List<DocumentIva>();
                foreach (UxDocumentIva lioO in vioDocumentUser.coIvas)
                {
                    coIvas.Add(
                         new DocumentIva
                         {
                             ivdblBaseImponible = lioO.ivdblBaseImponible ?? 0,
                             ivdblImporte = lioO.ivdblImporte ?? 0,
                             ivnroTipo = lioO.ivnroTipo
                         });
                }
                // borrado de iva no gravado y exento
                coIvas.RemoveAll(x => x.ivnroTipo == 1 || x.ivnroTipo == 2);
            }
            if (vioDocumentUser.coOtrosTributos != null && vioDocumentUser.coOtrosTributos.Count > 0)
            {
                coOtrosTributos = new List<DocumentOtroTributo>();
                foreach (UxDocumentOtroTributo lioO in vioDocumentUser.coOtrosTributos)
                {
                    coOtrosTributos.Add(
                         new DocumentOtroTributo
                         {
                             ivdblBaseImp = lioO.ivdblBaseImponible ?? 0,
                             ivdblImporte = lioO.ivdblImporte ?? 0,
                             ivnroId = lioO.ivnroId ?? 0,
                             ivdblAlicuota = lioO.ivdblAlicuota,
                             ivstrDesc = lioO.ivstrDesc
                         });
                }
            }
        }
        public async Task<short> Auth()
        {
            short livnroNextStatus = 40;
            DocumentTracking lioDocumentTracking = new DocumentTracking(mioContext, mioDcModel.ivlngDoc);
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)int.Parse(ListHelper.GetValue("FORMAT", "TLS", mioContext));
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            AuthRequestType lioAutRequest = new AuthRequestType
            {
                cuitRepresentada = ivCuitAutorizante,
                sign = lioAfipLoginResponse.ivstrSign,
                token = lioAfipLoginResponse.ivstrToken
            };
            //CONSULTA ULTIMO COMPROBANTE AUTORIZADO
            CTServicePortTypeClient lioService = new CTServicePortTypeClient(CTServicePortTypeClient.EndpointConfiguration.CTServiceSOAP);
            lioService.Endpoint.Address = new System.ServiceModel.EndpointAddress(lioAfipService.ivstrUrl);
            consultarUltimoComprobanteAutorizadoResponse lioconsultarUltimoComprobanteAutorizadoResponse = null;
            short livnroIntento = 0;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioconsultarUltimoComprobanteAutorizadoResponse = await lioService.consultarUltimoComprobanteAutorizadoAsync(lioAutRequest, mioDcModel.ivnroTipo, (short)mioDcModel.ivnumPvta);
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
                                   lioAutRequest
                               },
                               Response = ExceptionToResponse(lioE)
                           }
                       )
                   );
                    LogHelper.write(lioE);
                    return livnroNextStatus;
                }
            }
            if (lioconsultarUltimoComprobanteAutorizadoResponse == null || lioconsultarUltimoComprobanteAutorizadoResponse.consultarUltimoComprobanteAutorizadoReturn == null)
                throw new Exception($"{Resources.lioE_HeaderAuth}:{Resources.lioE_AfipRespNo}");
            //EL DOCUMENTO ES MAYOR AL ULTIMO AUTORIZADO ==> EsperaPredecesor
            if (lioconsultarUltimoComprobanteAutorizadoResponse.consultarUltimoComprobanteAutorizadoReturn.numeroComprobante + 1 < this.mioDcModel.ivlngCbte)
            {
                livnroNextStatus = 35;
                if (this.mioDcModel.ivnroStatus != 35)
                {
                    lioDocumentTracking.addTrack(
                        livnroNextStatus,
                        JsonConvert.SerializeObject(
                            new
                            {
                                Request = new
                                {
                                    AutRequest = lioAutRequest,
                                    numPvta = mioDcModel.ivnumPvta,
                                    nroTipo = mioDcModel.ivnroTipo
                                },
                                Response = lioconsultarUltimoComprobanteAutorizadoResponse
                            }
                        )
                    );
                }
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES MENOR AL ULTIMO AUTORIZADO  ==> CONSULTAR CAE
            if (lioconsultarUltimoComprobanteAutorizadoResponse.consultarUltimoComprobanteAutorizadoReturn.numeroComprobante + 1 > this.mioDcModel.ivlngCbte)
            {
                consultarComprobanteTipoPVentaNroResponse lioConsultarResponse = null;
                while (true)
                {
                    livnroIntento++;
                    try
                    {
                        lioConsultarResponse = await lioService.consultarComprobanteTipoPVentaNroAsync(lioAutRequest, mioDcModel.ivnroTipo, (short)mioDcModel.ivnumPvta, mioDcModel.ivlngCbte);
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
                                       mioDcModel.ivnroTipo,
                                       mioDcModel.ivnumPvta,
                                       mioDcModel.ivlngCbte
                                   },
                                   Response = ExceptionToResponse(lioE)
                               }
                           )
                       );
                        LogHelper.write(lioE);
                        return livnroNextStatus;
                    }
                }
                if (lioConsultarResponse.consultarComprobanteReturn != null && lioConsultarResponse.consultarComprobanteReturn.comprobante != null && lioConsultarResponse.consultarComprobanteReturn.comprobante.codigoAutorizacion != 0)
                    livnroNextStatus = 50;
                lioDocumentTracking.addTrack(
                    livnroNextStatus,
                    JsonConvert.SerializeObject(
                        new
                        {
                            Request = new
                            {
                                lioAutRequest,
                                mioDcModel.ivnroTipo,
                                mioDcModel.ivnumPvta,
                                mioDcModel.ivlngCbte
                            },
                            Response = lioConsultarResponse
                        }
                    )
                );
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES EL SIGUIENTE  ==> AUTORIZAR
            ComprobanteType lioComprobanteType = new ComprobanteType
            {
                numeroPuntoVenta = (short)mioDcModel.ivnumPvta,
                numeroComprobante = mioDcModel.ivlngCbte,
                codigoTipoComprobante = mioDcModel.ivnroTipo,
                codigoTipoDocumento = this.ivnroTipoReceptor,
                numeroDocumento = this.ivstrNroReceptor.ToString(),
                idImpositivo = null, //ver
                fechaEmision = mioDcModel.ivdtmEmision ?? DateTime.MinValue,
                fechaVencimiento = ivdtmVtopago ?? DateTime.MinValue,
                importeTotal = (decimal)(mioDcModel.ivdblImporte ?? 0),
                importeExento = (decimal)this.ivdblImporteExento,
                importeGravado = (decimal)this.ivdblImporteGravado,
                importeOtrosTributos = (decimal)this.ivdblImporteOtrosTributos,
                importeNoGravado = (decimal)this.ivdblImporteNoGravado,
                importeReintegro = (decimal)this.ivdblImporteReintegro,
                codigoMoneda = this.ivstrMoneda,
                cotizacionMoneda = (decimal)this.ivdblCotizacion,
                cancelaEnMismaMonedaExtranjera = string.IsNullOrEmpty(ivstrCanMisMonExt) ? SiNoSimpleType.S : SiNoSimpleType.N,
                codigoPais = this.ivnroPais,
                domicilioReceptor = ivstrDomicilio,
                observaciones = ivstrObservaciones,
                arrayComprobantesAsociados = [],
                arrayDatosAdicionales = [],
                arrayFormasPago = [],
                arrayItems = [],
                arrayOtrosTributos = [],
                arraySubtotalesIVA = [],

            };

            int livnum = 0;

            if (this.coOtrosTributos != null && this.coOtrosTributos.Count > 0)
            {
                livnum = 0;
                lioComprobanteType.arrayOtrosTributos = new OtroTributoType[this.coOtrosTributos.Count];
                foreach (DocumentOtroTributo lioDocumentOtroTributo in this.coOtrosTributos)
                {
                    lioComprobanteType.arrayOtrosTributos[livnum] = new OtroTributoType
                    {
                        codigo = lioDocumentOtroTributo.ivnroId ?? 0,
                        descripcion = lioDocumentOtroTributo.ivstrDesc,
                        baseImponible = (decimal)(lioDocumentOtroTributo.ivdblBaseImp ?? 0),
                        importe = (decimal)(lioDocumentOtroTributo.ivdblImporte ?? 0)
                    };
                    livnum++;
                }
            }
            if (this.coIvas != null && this.coIvas.Count > 0)
            {
                livnum = 0;
                lioComprobanteType.arraySubtotalesIVA = new SubtotalIVAType[this.coIvas.Count];
                foreach (DocumentIva lioDocumentIva in this.coIvas)
                {
                    lioComprobanteType.arraySubtotalesIVA[livnum] = new SubtotalIVAType
                    {
                        codigo = lioDocumentIva.ivnroTipo ?? 0,
                        importe = (decimal)(lioDocumentIva.ivdblImporte ?? 0)
                    };
                    livnum++;
                }
            }
            //
            if (this.coAdicionales != null && this.coAdicionales.Count > 0)
            {
                livnum = 0;
                lioComprobanteType.arrayDatosAdicionales = new TipoDatoAdicionalType[this.coAdicionales.Count];
                foreach (DocumentAdicional lioDocumentOpcional in coAdicionales)
                {
                    lioComprobanteType.arrayDatosAdicionales[livnum] = new TipoDatoAdicionalType
                    {
                        t = lioDocumentOpcional.ivnroTipo,
                        c1 = lioDocumentOpcional.ivstrValor1,
                        c2 = lioDocumentOpcional.ivstrValor2,
                        c3 = lioDocumentOpcional.ivstrValor3,
                        c4 = lioDocumentOpcional.ivstrValor4,
                        c5 = lioDocumentOpcional.ivstrValor5,
                        c6 = lioDocumentOpcional.ivstrValor6
                    };
                    livnum++;
                }
            }
            //
            autorizarComprobanteResponse lioautorizarComprobanteResponse = null;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioautorizarComprobanteResponse = await lioService.autorizarComprobanteAsync(lioAutRequest, lioComprobanteType);
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
                                   lioComprobanteType
                               },
                               Response = ExceptionToResponse(lioE)
                           }
                       )
                    );
                    LogHelper.write(lioE);
                    return 40;
                }
            }
            if (lioautorizarComprobanteResponse.autorizarComprobanteReturn != null && lioautorizarComprobanteResponse.autorizarComprobanteReturn.comprobanteResponse != null && lioautorizarComprobanteResponse.autorizarComprobanteReturn.comprobanteResponse.CAE != 0)
                livnroNextStatus = 50;
            lioDocumentTracking.addTrack(
                livnroNextStatus,
                JsonConvert.SerializeObject(
                    new
                    {
                        Request = new
                        {
                            lioAutRequest,
                            lioComprobanteType
                        },
                        Response = lioautorizarComprobanteResponse
                    }
                )
            );
            return livnroNextStatus;
        }
        public void SetContext(NatContext vioContext)
        {
            mioContext = vioContext;
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
            if (!ListHelper.ContainKey("TDOC", this.ivnroTipoReceptor.ToString(), mioContext))
                livstrError += Resources.lioE_TipoReceptor_No + Environment.NewLine;
            if (!ListHelper.ContainKey("MON", this.ivstrMoneda, mioContext))
                livstrError += Resources.lioE_Moneda_No + Environment.NewLine;
            if (!ListHelper.ContainKey("TRESP", this.ivnroTipoResp.ToString(), mioContext))
                livstrError += Resources.lioE_Tresp_No + Environment.NewLine;
            short index = 1;

            if (this.coOtrosTributos != null && this.coOtrosTributos.Count > 0)
                foreach (DocumentOtroTributo item in this.coOtrosTributos)
                {
                    if (!ListHelper.ContainKey("TTRIB", item.ivnroId.ToString(), mioContext))
                        livstrError += string.Format(Resources.lioE_Tributo_No, index) + Environment.NewLine;
                    index++;
                }
            index = 1;
            if (this.coIvas != null && this.coIvas.Count > 0)
                foreach (DocumentIva item in this.coIvas)
                {
                    if (!ListHelper.ContainKey("TIVA", item.ivnroTipo.ToString(), mioContext))
                        livstrError += string.Format(Resources.lioE_Tiva_No, index) + Environment.NewLine;
                    index++;
                }
            if (!string.IsNullOrEmpty(livstrError))
                throw new Exception(Resources.lioE_HeaderAuth + Environment.NewLine + livstrError);
            return;
        }
        public bool AuthDataModified(ITribDocument vioIDocument)
        {
            if (vioIDocument == null) return true;
            TribDocumentCT? vioCurrentDocument = vioIDocument as TribDocumentCT;
            if (vioCurrentDocument.mioDcModel.ivnroStatus < 50) return false;
            if (vioCurrentDocument.mioDcModel.ivlngCuitEmisor != mioDcModel.ivlngCuitEmisor) return true;
            if (vioCurrentDocument.mioDcModel.ivlngCbte != mioDcModel.ivlngCbte) return true;
            if (vioCurrentDocument.mioDcModel.ivnroTipo != mioDcModel.ivnroTipo) return true;
            if (vioCurrentDocument.mioDcModel.ivnumPvta != mioDcModel.ivnumPvta) return true;
            if (vioCurrentDocument.mioDcModel.ivdtmEmision != mioDcModel.ivdtmEmision) return true;
            if (vioCurrentDocument.mioDcModel.ivdblImporte != mioDcModel.ivdblImporte) return true;
            if (vioCurrentDocument.ivnroTipoReceptor != ivnroTipoReceptor) return true;
            if (vioCurrentDocument.ivstrNroReceptor != ivstrNroReceptor) return true;
            if (vioCurrentDocument.ivdblImporteNoGravado != ivdblImporteNoGravado) return true;
            if (vioCurrentDocument.ivdblImporteGravado != ivdblImporteGravado) return true;
            if (vioCurrentDocument.ivdblImporteExento != ivdblImporteExento) return true;
            if (vioCurrentDocument.ivdblImporteOtrosTributos != ivdblImporteOtrosTributos) return true;
            if (vioCurrentDocument.ivstrMoneda != ivstrMoneda) return true;
            if (vioCurrentDocument.ivdblCotizacion != ivdblCotizacion) return true;
            if (vioCurrentDocument.ivdtmVtopago != this.ivdtmVtopago) return true;
            if (vioCurrentDocument.coOtrosTributos != null && vioCurrentDocument.coOtrosTributos.Count > 0)
            {
                if (vioCurrentDocument.coOtrosTributos.Count != coOtrosTributos.Count) return true;
                foreach (DocumentOtroTributo DocumentOtroTributo in vioCurrentDocument.coOtrosTributos)
                    if (!coOtrosTributos.Any(x => x.ivnroId == DocumentOtroTributo.ivnroId && x.ivstrDesc == DocumentOtroTributo.ivstrDesc && x.ivdblBaseImp == DocumentOtroTributo.ivdblBaseImp && x.ivdblAlicuota == DocumentOtroTributo.ivdblAlicuota && x.ivdblImporte == DocumentOtroTributo.ivdblImporte)) return true;
            }
            if (vioCurrentDocument.coIvas != null && vioCurrentDocument.coIvas.Count > 0)
            {
                if (vioCurrentDocument.coIvas.Count != coIvas.Count) return true;
                foreach (DocumentIva lioDocumentIva in vioCurrentDocument.coIvas)
                    if (!coIvas.Any(x => x.ivnroTipo == lioDocumentIva.ivnroTipo && x.ivdblBaseImponible == lioDocumentIva.ivdblBaseImponible && x.ivdblImporte == lioDocumentIva.ivdblImporte)) return true;
            }
            if (vioCurrentDocument.coAdicionales != null && vioCurrentDocument.coAdicionales.Count > 0)
            {
                if (vioCurrentDocument.coAdicionales.Count != coAdicionales.Count) return true;
                foreach (DocumentAdicional lioDocumentAdicional in vioCurrentDocument.coAdicionales)
                    if (!coAdicionales.Any(x => x.ivnroTipo == lioDocumentAdicional.ivnroTipo && (x.ivstrValor1 == lioDocumentAdicional.ivstrValor1 || x.ivstrValor2 == lioDocumentAdicional.ivstrValor2 || x.ivstrValor3 == lioDocumentAdicional.ivstrValor3 || x.ivstrValor4 == lioDocumentAdicional.ivstrValor4 || x.ivstrValor5 == lioDocumentAdicional.ivstrValor5))) return true;
            }
            return false;
        }
        public async Task<double> GetCotizacion(string vivstrMoneda, DateTime vivdtm)
        {
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            AuthRequestType lioAutRequest = new AuthRequestType
            {
                cuitRepresentada = ivCuitAutorizante,
                sign = lioAfipLoginResponse.ivstrSign,
                token = lioAfipLoginResponse.ivstrToken
            };
            CTServicePortTypeClient lioService = new CTServicePortTypeClient(CTServicePortTypeClient.EndpointConfiguration.CTServiceSOAP);
            lioService.Endpoint.Address = new System.ServiceModel.EndpointAddress(lioAfipService.ivstrUrl);
            short livnroIntento = 0;
            consultarCotizacionResponse lioconsultarCotizacionResponse = null;
            while (true)
            {
                livnroIntento++;
                try
                {
                    LogHelper.writeinfo(
                       $"FEParamGetCotizacionAsync: Url:{lioAfipService.ivstrUrl} Auth:{JsonConvert.SerializeObject(lioAutRequest)}, Moneda: {vivstrMoneda}, Fecha:{vivdtm.ToString(lioAfipService.ivstrDateformat)}",
                       ListHelper.GetValue("FORMAT", "VERBOSE", mioContext) == "1"
                    );
                    lioconsultarCotizacionResponse = await lioService.consultarCotizacionAsync(lioAutRequest, vivstrMoneda, vivdtm);
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
            if (lioconsultarCotizacionResponse == null)
                throw new Exception(Resources.lioE_Cotiz_No);
            if (lioconsultarCotizacionResponse.consultarCotizacionReturn.arrayErrores != null && lioconsultarCotizacionResponse.consultarCotizacionReturn.arrayErrores.Count() != 0)
                throw new Exception($"{Resources.lioE_Cotiz_No} => {lioconsultarCotizacionResponse.consultarCotizacionReturn.arrayErrores[0].codigo}:{lioconsultarCotizacionResponse.consultarCotizacionReturn.arrayErrores[0].descripcion}");
            return (double)lioconsultarCotizacionResponse.consultarCotizacionReturn.cotizacionMoneda;
        }
        public long ivCuitAutorizante { get { return mioDcModel.ivlngCuitEmisor; } }
        public string ivstrSR { get; set; } = "S";
        public UxAuth GetAuth()
        {
            short[] lcvnroStatusRTA = new short[] { 20, 35, 40, 50 };
            DocumentTrackingModel[] lcoTracks = mioContext.DocumentTrackings.OrderByDescending(x => x.ivdtmTrack).Where(x => x.ivlngDoc == mioDcModel.ivlngDoc).ToArray();
            if (lcoTracks == null || lcoTracks.Length == 0 || !lcoTracks.Any(x => lcvnroStatusRTA.Contains(x.ivnroStatus)))
                throw new Exception($"{Resources.lioE_CAENoSts}: ivlngDoc {mioDcModel.ivlngDoc}");
            DocumentTrackingModel lioTrack = lcoTracks.FirstOrDefault(x => lcvnroStatusRTA.Contains(x.ivnroStatus));
            if (lioTrack == null || string.IsNullOrEmpty(lioTrack.ivstrData))
                throw new Exception($"{Resources.lioE_CAERespErr}: ivlngDoc {mioDcModel.ivlngDoc}");
            dynamic lioTrackData = JsonConvert.DeserializeObject(lioTrack.ivstrData);
            UxAuth lioUxAuth;
            string livstr;
            consultarUltimoComprobanteAutorizadoResponse lioconsultarUltimoComprobanteAutorizadoResponse = JsonConvert.DeserializeObject<consultarUltimoComprobanteAutorizadoResponse>(lioTrackData.Response.ToString());
            if (lioconsultarUltimoComprobanteAutorizadoResponse != null && lioconsultarUltimoComprobanteAutorizadoResponse.consultarUltimoComprobanteAutorizadoReturn != null)

                return new UxAuth
                {
                    ivdtmNode = lioTrack.ivdtmTrack,
                    ivnumtrack = lioTrack.ivnumTrack,
                    ivstrAuthCode = string.Empty,
                    ivdtmAuthVenc = string.Empty,
                    ivstrAuthType = "CAE",
                    ivtrStatusDesc = "Rechazado",
                    ivstrErrors = $"No Correlativo, Ultimo Autorizado: {lioconsultarUltimoComprobanteAutorizadoResponse.consultarUltimoComprobanteAutorizadoReturn.numeroComprobante.ToString()}",
                    ivstrObs = string.Empty
                };
            consultarComprobanteTipoPVentaNroResponse lioconsultarComprobanteTipoPVentaNroResponse = JsonConvert.DeserializeObject<consultarComprobanteTipoPVentaNroResponse>(lioTrackData?.Response?.ToString());
            if (lioconsultarComprobanteTipoPVentaNroResponse != null && lioconsultarComprobanteTipoPVentaNroResponse.consultarComprobanteReturn != null && lioconsultarComprobanteTipoPVentaNroResponse?.consultarComprobanteReturn.comprobante != null && lioconsultarComprobanteTipoPVentaNroResponse?.consultarComprobanteReturn.comprobante.codigoAutorizacion > 0)
                return new UxAuth
                {
                    ivdtmNode = lioTrack.ivdtmTrack,
                    ivnumtrack = lioTrack.ivnumTrack,
                    ivstrAuthCode = string.Empty,
                    ivdtmAuthVenc = string.Empty,
                    ivstrAuthType = "CAE",
                    ivtrStatusDesc = "Rechazado",
                    ivstrErrors = $"No Correlativo, Ultimo Autorizado: {lioconsultarComprobanteTipoPVentaNroResponse.consultarComprobanteReturn.comprobante.codigoAutorizacion}",
                    ivstrObs = string.Empty
                };
            autorizarComprobanteResponse lioautorizarComprobanteResponse = JsonConvert.DeserializeObject<autorizarComprobanteResponse>(lioTrackData?.Response?.ToString());
            if (lioautorizarComprobanteResponse == null || lioautorizarComprobanteResponse.autorizarComprobanteReturn == null || lioautorizarComprobanteResponse.autorizarComprobanteReturn.comprobanteResponse == null)
                throw new Exception($"{Resources.lioE_CAERespErr}: ivlngDoc {mioDcModel.ivlngDoc}");
            lioUxAuth = new UxAuth();
            lioUxAuth.ivdtmNode = lioTrack.ivdtmTrack;
            lioUxAuth.ivnumtrack = lioTrack.ivnumTrack;
            lioUxAuth.ivstrAuthCode = lioautorizarComprobanteResponse.autorizarComprobanteReturn.comprobanteResponse.CAE.ToString();
            lioUxAuth.ivdtmAuthVenc = lioautorizarComprobanteResponse.autorizarComprobanteReturn.comprobanteResponse.fechaVencimientoCAE.ToString();
            lioUxAuth.ivstrAuthType = "CAE";
            lioUxAuth.ivtrStatusDesc = lioautorizarComprobanteResponse.autorizarComprobanteReturn.comprobanteResponse.CAE > 0 ? "Autorizado" : "Rechazado";
            livstr = string.Empty;
            if (lioautorizarComprobanteResponse.autorizarComprobanteReturn != null && lioautorizarComprobanteResponse.autorizarComprobanteReturn.arrayErrores != null)
                livstr = string.Join(", ", lioautorizarComprobanteResponse.autorizarComprobanteReturn.arrayErrores.Select(x => $"{x.codigo}:{x.descripcion}"));
            lioUxAuth.ivstrErrors = livstr;
            livstr = string.Empty;
            if (lioautorizarComprobanteResponse.autorizarComprobanteReturn != null && lioautorizarComprobanteResponse.autorizarComprobanteReturn.arrayObservaciones != null)
                livstr = string.Join(", ", lioautorizarComprobanteResponse.autorizarComprobanteReturn.arrayObservaciones.Select(x => $"{x.codigo}:{x.descripcion}"));
            lioUxAuth.ivstrErrors += livstr;
            lioUxAuth.ivstrObs = livstr;
            return lioUxAuth;
        }
        #endregion
        #region PRIVATE METHODS
        private consultarComprobanteTipoPVentaNroResponse ExceptionToResponse(Exception lioE)
        {
            consultarComprobanteTipoPVentaNroResponse lioO = new consultarComprobanteTipoPVentaNroResponse
            {
                consultarComprobanteReturn = new ConsultarComprobanteReturnType
                {
                    comprobante = new ComprobanteType { codigoAutorizacion = 0, fechaVencimiento = DateTime.MinValue },
                    arrayErrores = [new CodigoDescripcionType { codigo = 999, descripcion = lioE.Message }],
                    arrayObservaciones = []
                }
            };
            return lioO;
        }
        #endregion
    }

}
