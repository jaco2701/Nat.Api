using Applet.Nat.Afip.Mtxca;
using Applet.Nat.Api.AFIP.Model;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.Afip;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Microsoft.IdentityModel.Tokens;
using Nat.Api.Properties;
using Newtonsoft.Json;

namespace Applet.Nat.BR
{
    public class DocumentMTXCA : IDocument
    {
        #region CONSTRUCT
        public DocumentMTXCA(DocumentModel vioDocumentModel, NatContext vioContext)
        {
            mioDcModel = vioDocumentModel;
            mioContext = vioContext;
            ivstrDocWs = "wsmtxca";
        }
        #endregion
        #region PRIVATE PROPS
        private NatContext mioContext { get; set; }
        private DocumentModel mioDcModel { get; set; }
        #endregion
        #region PUBLIC PROPS
        public short ivnroConcepto { get; set; }
        public double ivdblImporteNoGravado { get; set; }
        public double ivdblImporteGravado { get; set; }
        public double ivdblImporteExento { get; set; }
        public double ivdblImporteIva { get; set; }
        public double ivdblImporteOtrosTributos { get; set; }
        public DateTime? ivdtmServdesde { get; set; }
        public DateTime? ivdtmServhasta { get; set; }
        public DateTime? ivdtmVtopago { get; set; }
        public String ivstrMoneda { get; set; }
        public String ivstrObservaciones { get; set; }
        public double ivdblCotizacion { get; set; }
        public short ivnroTipoResp { get; set; }
        public short ivnroTipoReceptor { get; set; }
        public string ivstrNroReceptor { get; set; }
        public string ivstrDocWs { get; set; }
        public string? ivstrCanMisMonExt { get; set; }
        public List<DocumentAsociado> coAsociados { get; set; }
        public List<DocumentOtroTributo> coOtrosTributos { get; set; }
        public List<DocumentIva> coIvas { get; set; }
        public List<DocumentOpcional> coOpcionales { get; set; }
        public List<DocumentComprador> coCompradores { get; set; }
        public List<DocumentAdicional> coAdicionales { get; set; }
        private List<DocumentItem> coItems { get; set; }
        #endregion
        #region PUBLICS METHODS
        public void SetData(DocumentUser vioDocumentUser)
        {
            string livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
            ivnroConcepto = vioDocumentUser.ivnroConcepto ?? 0;
            ivdblImporteNoGravado = vioDocumentUser.ivdblImporteNoGravado ?? 0;
            ivdblImporteGravado = vioDocumentUser.ivdblImporteGravado ?? 0;
            ivdblImporteExento = vioDocumentUser.ivdblImporteExento ?? 0;
            ivdblImporteIva = vioDocumentUser.ivdblImporteIva ?? 0;
            ivdblImporteOtrosTributos = vioDocumentUser.ivdblImporteOtrosTributos ?? 0;
            ivdblCotizacion = vioDocumentUser.ivdblCotizacion ?? 0;
            ivstrMoneda = vioDocumentUser.ivstrMoneda ?? string.Empty;
            ivnroTipoReceptor = vioDocumentUser.ivnroTipoDocReceptor ?? 0;
            ivnroTipoResp = vioDocumentUser.ivnroTipoRespReceptor ?? 0;
            ivstrNroReceptor = vioDocumentUser.ivlngDocReceptor.ToString() ?? string.Empty;
            ivdtmServdesde = Format.DateFromUX(vioDocumentUser.ivdtmServdesde, livstrApiDtmFormat);
            ivdtmServhasta = Format.DateFromUX(vioDocumentUser.ivdtmServhasta, livstrApiDtmFormat);
            ivdtmVtopago = Format.DateFromUX(vioDocumentUser.ivdtmVtopago, livstrApiDtmFormat);
            ivstrCanMisMonExt = vioDocumentUser.ivstrCanMisMonExt ?? string.Empty;
            if (vioDocumentUser.coAsociados != null && vioDocumentUser.coAsociados.Count > 0)
            {
                coAsociados = new List<DocumentAsociado>();
                foreach (UxDocumentAsociado lioO in vioDocumentUser.coAsociados)
                    coAsociados.Add(
                        new DocumentAsociado
                        {
                            ivdtmFechaEmision = Format.DateFromUX(lioO.ivdtmFechaEmision, livstrApiDtmFormat),
                            ivlngCbteCUIT = lioO.ivlngCbteCUIT,
                            ivlngCbteNro = lioO.ivlngCbteNro,
                            ivnumCbtePuntovta = lioO.ivnumCbtePuntovta,
                            ivnroCbtetipo = lioO.ivnroCbtetipo
                        });
            }
            if (vioDocumentUser.coOtrosTributos != null && vioDocumentUser.coOtrosTributos.Count > 0)
            {
                coOtrosTributos = new List<DocumentOtroTributo>();
                foreach (UxDocumentOtroTributo lioO in vioDocumentUser.coOtrosTributos)
                    coOtrosTributos.Add(
                         new DocumentOtroTributo
                         {
                             ivdblAlicuota = lioO.ivdblAlicuota,
                             ivdblBaseImp = lioO.ivdblBaseImponible,
                             ivdblImporte = lioO.ivdblImporte,
                             ivnroId = lioO.ivnroId,
                             ivstrDesc = lioO.ivstrDesc
                         });
            }
            if (vioDocumentUser.coIvas != null && vioDocumentUser.coIvas.Count > 0)
            {
                coIvas = new List<DocumentIva>();
                foreach (UxDocumentIva lioO in vioDocumentUser.coIvas)
                    coIvas.Add(
                         new DocumentIva
                         {
                             ivdblBaseImponible = lioO.ivdblBaseImponible,
                             ivnroTipo = lioO.ivnroTipo,
                             ivdblImporte = lioO.ivdblImporte
                         });
            }
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
            if (vioDocumentUser.coCompradores != null && vioDocumentUser.coCompradores.Count > 0)
            {
                coCompradores = new List<DocumentComprador>();
                foreach (UxDocumentComprador lioO in vioDocumentUser.coCompradores)
                    coCompradores.Add(
                         new DocumentComprador
                         {
                             ivdblPorcentaje = lioO.ivdblPorcentaje,
                             ivnroDocTipo = lioO.ivnroDocTipo,
                             ivlngDocNro = lioO.ivlngDocNro
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
                             ivdblImporteIVA = lioO.ivdblImporteIVA,
                             ivdblImporteTotal = lioO.ivdblImporteTotal,
                             ivdblPrecioUnitario = lioO.ivdblPrecioUnitario,
                             ivnroTipoIVA = lioO.ivnroTipoIVA,
                             ivnroUM = lioO.ivnroUM,
                             ivstrDescripcion = lioO.ivstrDescripcion
                         });
            }
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
            if (!ListHelper.ContainKey("TCONS", this.ivnroConcepto.ToString(), mioContext))
                livstrError += Resources.lioE_Concepto_No + Environment.NewLine;
            if (!ListHelper.ContainKey("MON", this.ivstrMoneda, mioContext))
                livstrError += Resources.lioE_Moneda_No + Environment.NewLine;
            if (!ListHelper.ContainKey("TRESP", this.ivnroTipoResp.ToString(), mioContext))
                livstrError += Resources.lioE_Tresp_No + Environment.NewLine;
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
        public void SetContext(NatContext vioContext)
        {
            mioContext = vioContext;
        }
        public async Task<short> Auth()
        {
            //TOKEN
            short livnroNextStatus = 0;
            DocumentTracking lioDocumentTracking = new DocumentTracking(mioContext, mioDcModel.ivlngDoc);
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            AuthRequestType lioAutRequest = new AuthRequestType()
            {
                cuitRepresentada = this.mioDcModel.ivlngCuitEmisor,
                sign = lioAfipLoginResponse.ivstrSign,
                token = lioAfipLoginResponse.ivstrToken
            };
            //CONSULTA ULTIMO COMPROBANTE AUTORIZADO
            MTXCAServicePortType lioService = new MTXCAServicePortTypeClient(
                MTXCAServicePortTypeClient.EndpointConfiguration.MTXCAServiceHttpSoap11Endpoint,
                lioAfipService.ivstrUrl
                );
            consultarUltimoComprobanteAutorizadoRequest lioconsultarUltimoComprobanteAutorizadoRequest = new consultarUltimoComprobanteAutorizadoRequest(
                lioAutRequest,
                new ConsultaUltimoComprobanteAutorizadoRequestType
                {
                    codigoTipoComprobante = mioDcModel.ivnroTipo,
                    numeroPuntoVenta = mioDcModel.ivnumPvta
                }
            );
            consultarUltimoComprobanteAutorizadoResponse lioconsultarUltimoComprobanteAutorizadoResponse = await lioService.consultarUltimoComprobanteAutorizadoAsync(lioconsultarUltimoComprobanteAutorizadoRequest);
            //EL DOCUMENTO ES MAYOR AL ULTIMO AUTORIZADO ==> EsperaPredecesor
            if (lioconsultarUltimoComprobanteAutorizadoResponse != null && lioconsultarUltimoComprobanteAutorizadoResponse.numeroComprobante + 1 < this.mioDcModel.ivlngCbte)
            {
                livnroNextStatus = 35;
                lioDocumentTracking.addTrack(
                    livnroNextStatus,
                    JsonConvert.SerializeObject(
                        new
                        {
                            Request = lioconsultarUltimoComprobanteAutorizadoRequest,
                            Response = lioconsultarUltimoComprobanteAutorizadoResponse
                        }
                    )
                );
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES MENOR AL ULTIMO AUTORIZADO  ==> CONSULTAR CAE
            if (lioconsultarUltimoComprobanteAutorizadoResponse != null && lioconsultarUltimoComprobanteAutorizadoResponse.numeroComprobante + 1 > this.mioDcModel.ivlngCbte)
            {
                consultarComprobanteRequest lioFECompConsultaReq = new consultarComprobanteRequest
                {
                    authRequest = new AuthRequestType
                    {
                        cuitRepresentada = this.mioDcModel.ivlngCuitEmisor,
                        sign = lioAfipLoginResponse.ivstrSign,
                        token = lioAfipLoginResponse.ivstrToken
                    },
                    consultaComprobanteRequest = new ConsultaComprobanteRequestType
                    {
                        numeroComprobante = this.mioDcModel.ivlngCbte,
                        codigoTipoComprobante = this.mioDcModel.ivnroTipo,
                        numeroPuntoVenta = this.mioDcModel.ivnumPvta,
                    }
                };
                livnroNextStatus = 40;
                consultarComprobanteResponse lioFECompConsultarResponse = await lioService.consultarComprobanteAsync(lioFECompConsultaReq);
                if (lioFECompConsultarResponse != null && lioFECompConsultarResponse.comprobante != null && lioFECompConsultarResponse.comprobante.codigoAutorizacion != 0)
                    livnroNextStatus =50;
                lioDocumentTracking.addTrack(
                    livnroNextStatus,
                    JsonConvert.SerializeObject(
                        new
                        {
                            Request = lioFECompConsultaReq,
                            Response = lioconsultarUltimoComprobanteAutorizadoResponse
                        }
                    )
                );
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES EL SIGUIENTE  ==> AUTORIZAR
            ComprobanteType lioComprobanteType = new ComprobanteType
            {
                codigoTipoComprobante = this.mioDcModel.ivnroTipo,
                numeroPuntoVenta = this.mioDcModel.ivnumPvta,
                numeroComprobante = this.mioDcModel.ivlngCbte,
                fechaEmision = this.mioDcModel.ivdtmEmision ?? DateTime.MinValue,
                codigoTipoAutorizacion = CodigoTipoAutorizacionSimpleType.A,
                codigoTipoDocumento = short.Parse(this.ivnroTipoReceptor.ToString()),
                numeroDocumento = long.Parse(this.ivstrNroReceptor.ToString()),
                importeGravado = Convert.ToDecimal(this.ivdblImporteGravado),
                importeNoGravado = Convert.ToDecimal(this.ivdblImporteNoGravado),
                importeExento = Convert.ToDecimal(this.ivdblImporteExento),
                codigoMoneda = this.ivstrMoneda,
                cotizacionMoneda = Convert.ToDecimal(this.ivdblCotizacion),
                observaciones = this.ivstrObservaciones,
                codigoConcepto = this.ivnroConcepto,
                cancelaEnMismaMonedaExtranjera= string.IsNullOrEmpty(ivstrCanMisMonExt) 
                                                   ?
                                                   ivstrCanMisMonExt=="S"
                                                       ?
                                                       SiNoSimpleType.S
                                                       :
                                                       SiNoSimpleType.N
                                                   : SiNoSimpleType.N
            };
            int lionum = 0;
            if (this.coAsociados != null && this.coAsociados.Count > 0)
            {
                lioComprobanteType.arrayComprobantesAsociados = new ComprobanteAsociadoType[this.coAsociados.Count];
                foreach (DocumentAsociado lioAsociado in this.coAsociados)
                {
                    lioComprobanteType.arrayComprobantesAsociados[lionum] = new ComprobanteAsociadoType();
                    lioComprobanteType.arrayComprobantesAsociados[lionum].codigoTipoComprobante = lioAsociado.ivnroCbtetipo ?? 0;
                    lioComprobanteType.arrayComprobantesAsociados[lionum].numeroPuntoVenta = lioAsociado.ivnumCbtePuntovta ?? 0;
                    lioComprobanteType.arrayComprobantesAsociados[lionum].numeroComprobante = lioAsociado.ivlngCbteNro ?? 0;
                    lioComprobanteType.arrayComprobantesAsociados[lionum].cuit = lioAsociado.ivlngCbteCUIT ?? 0;
                    lioComprobanteType.arrayComprobantesAsociados[lionum].fechaEmision = lioAsociado.ivdtmFechaEmision ?? DateTime.MinValue;
                    lionum++;
                }
            }
            if (this.coItems != null && this.coItems.Count > 0)
            {
                lionum = 0;
                lioComprobanteType.arrayItems = new ItemType[this.coItems.Count];
                foreach (DocumentItem lioItem in this.coItems)
                {
                    lioComprobanteType.arrayItems[lionum] = new ItemType();
                    lioComprobanteType.arrayItems[lionum].codigo = lioItem.ivstrCodigo;
                    lioComprobanteType.arrayItems[lionum].cantidad = Convert.ToDecimal(lioItem.ivdblCantidad);
                    lioComprobanteType.arrayItems[lionum].codigoCondicionIVA = lioItem.ivnroTipoIVA ?? 0;
                    lioComprobanteType.arrayItems[lionum].codigoUnidadMedida = (short)(lioItem.ivnroUM ?? 0);
                    lioComprobanteType.arrayItems[lionum].descripcion = lioItem.ivstrDescripcion;
                    lioComprobanteType.arrayItems[lionum].importeBonificacion = Convert.ToDecimal(lioItem.ivdblBonificaion);
                    lioComprobanteType.arrayItems[lionum].importeItem = Convert.ToDecimal(lioItem.ivdblImporteTotal);
                    lioComprobanteType.arrayItems[lionum].importeIVA = Convert.ToDecimal(lioItem.ivdblImporteIVA);
                    lioComprobanteType.arrayItems[lionum].precioUnitario = Convert.ToDecimal(lioItem.ivdblPrecioUnitario);
                    lionum++;
                }
            }
            if (this.coOtrosTributos != null && this.coOtrosTributos.Count > 0)
            {
                lionum = 0;
                lioComprobanteType.arrayOtrosTributos = new OtroTributoType[this.coOtrosTributos.Count];
                foreach (DocumentOtroTributo lioTributo in this.coOtrosTributos)
                {
                    lioComprobanteType.arrayOtrosTributos[lionum] = new OtroTributoType();
                    lioComprobanteType.arrayOtrosTributos[lionum].codigo = lioTributo.ivnroId ?? 0;
                    lioComprobanteType.arrayOtrosTributos[lionum].descripcion = lioTributo.ivstrDesc;
                    lioComprobanteType.arrayOtrosTributos[lionum].baseImponible = Convert.ToDecimal(lioTributo.ivdblBaseImp);
                    lioComprobanteType.arrayOtrosTributos[lionum].importe = Convert.ToDecimal(lioTributo.ivdblImporte);
                    lionum++;
                }
            }
            if (this.coCompradores != null && this.coCompradores.Count > 0)
            {
                lionum = 0;
                lioComprobanteType.arrayCompradores = new CompradorType[this.coCompradores.Count];
                foreach (DocumentComprador lioComprador in this.coCompradores)
                {
                    lioComprobanteType.arrayCompradores[lionum] = new CompradorType();
                    lioComprobanteType.arrayCompradores[lionum].numeroDocumento = lioComprador.ivlngDocNro ?? 0;
                    lioComprobanteType.arrayCompradores[lionum].codigoTipoDocumento = lioComprador.ivnroDocTipo ?? 0;
                    lioComprobanteType.arrayCompradores[lionum].porcentaje = Convert.ToDecimal(lioComprador.ivdblPorcentaje);
                    lionum++;
                }
            }
            if (this.coAdicionales != null && this.coAdicionales.Count > 0)
            {
                lionum = 0;
                lioComprobanteType.arrayDatosAdicionales = new DatoAdicionalType[this.coAdicionales.Count];
                foreach (DocumentAdicional lioAdicional in this.coAdicionales)
                {
                    lioComprobanteType.arrayDatosAdicionales[lionum] = new DatoAdicionalType();
                    lioComprobanteType.arrayDatosAdicionales[lionum].t = lioAdicional.ivnroTipo;
                    lioComprobanteType.arrayDatosAdicionales[lionum].c1 = lioAdicional.ivstrValor1;
                    lioComprobanteType.arrayDatosAdicionales[lionum].c2 = lioAdicional.ivstrValor2;
                    lioComprobanteType.arrayDatosAdicionales[lionum].c3 = lioAdicional.ivstrValor3;
                    lioComprobanteType.arrayDatosAdicionales[lionum].c4 = lioAdicional.ivstrValor4;
                    lioComprobanteType.arrayDatosAdicionales[lionum].c5 = lioAdicional.ivstrValor5;
                    lioComprobanteType.arrayDatosAdicionales[lionum].c6 = lioAdicional.ivstrValor6;
                    lionum++;
                }
            }
            autorizarComprobanteRequest lioautorizarComprobanteRequest = new autorizarComprobanteRequest(lioAutRequest, lioComprobanteType);
            autorizarComprobanteResponse lioautorizarComprobanteResponse = await lioService.autorizarComprobanteAsync(lioautorizarComprobanteRequest);
            livnroNextStatus = 40;
            if (lioautorizarComprobanteResponse != null && lioautorizarComprobanteResponse?.comprobanteResponse != null && lioautorizarComprobanteResponse.comprobanteResponse.CAE != 0)
                livnroNextStatus =50;
            lioDocumentTracking.addTrack(
                livnroNextStatus,
                JsonConvert.SerializeObject(
                    new
                    {
                        Request = lioautorizarComprobanteRequest,
                        Response = lioautorizarComprobanteResponse
                    }
                )
            );
            return livnroNextStatus;
        }
        public UxAuth GetAuth()
        {
            DocumentTrackingModel lioTrack = mioContext.DocumentTrackings.OrderByDescending(x => x.ivdtmTrack).FirstOrDefault(x => x.ivlngDoc == mioDcModel.ivlngDoc && (x.ivnroStatus ==50 || x.ivnroStatus == 40));
            if (lioTrack == null || string.IsNullOrEmpty(lioTrack.ivstrData))
                throw new Exception(Resources.lioE_CAEQry_Err);
            dynamic lioTrackData = JsonConvert.DeserializeObject(lioTrack.ivstrData);
            autorizarComprobanteResponse lioautorizarComprobanteResponse = JsonConvert.DeserializeObject<autorizarComprobanteResponse>(lioTrackData.Response.ToString()); ;
            if (lioautorizarComprobanteResponse != null && lioautorizarComprobanteResponse.comprobanteResponse != null && lioautorizarComprobanteResponse.comprobanteResponse.CAE != 0)
            {
                return new UxAuth
                {
                    ivdtmNode = lioTrack.ivdtmTrack,
                    ivnumtrack = lioTrack.ivnumTrack,
                    ivstrAuthCode = lioautorizarComprobanteResponse.comprobanteResponse.CAE.ToString(),
                    ivdtmAuthVenc = lioautorizarComprobanteResponse.comprobanteResponse.fechaVencimientoCAE.ToString(),
                    ivtrStatusDesc = lioautorizarComprobanteResponse != null && lioautorizarComprobanteResponse?.comprobanteResponse != null && lioautorizarComprobanteResponse.comprobanteResponse.CAE != 0 ? "Rechazado" : "Autorizado",
                    ivstrAuthType = "CAE",
                    ivstrErrors = lioautorizarComprobanteResponse.arrayErrores != null ? string.Join(", ", lioautorizarComprobanteResponse.arrayErrores.Select(x => $"{x.codigo}:{x.descripcion}")) : string.Empty,
                    ivstrObs =  lioautorizarComprobanteResponse.arrayObservaciones != null ? string.Join(", ", lioautorizarComprobanteResponse.arrayObservaciones.Select(x => $"{x.codigo}:{x.descripcion}")) : string.Empty
                };
            }
            consultarComprobanteResponse lioconsultarComprobanteResponse = JsonConvert.DeserializeObject<consultarComprobanteResponse>(lioTrackData.Response.ToString());
            if (lioconsultarComprobanteResponse != null && lioconsultarComprobanteResponse.comprobante != null || lioconsultarComprobanteResponse.comprobante.codigoAutorizacion != 0)
                throw new Exception(Resources.lioE_CAEQry_Err);
            return new UxAuth
            {
                ivdtmNode = lioTrack.ivdtmTrack,
                ivnumtrack = lioTrack.ivnumTrack,
                ivstrAuthCode = lioconsultarComprobanteResponse.comprobante.codigoAutorizacion.ToString(),
                ivdtmAuthVenc = lioconsultarComprobanteResponse.comprobante.fechaVencimiento.ToString(),
                ivstrAuthType = "CAE",
                ivtrStatusDesc = lioconsultarComprobanteResponse != null && lioconsultarComprobanteResponse?.comprobante != null && lioconsultarComprobanteResponse.comprobante.codigoAutorizacion != 0 ? "Rechazado" : "Autorizado",
                ivstrErrors = lioconsultarComprobanteResponse.arrayErrores != null ? string.Join(", ", lioconsultarComprobanteResponse.arrayErrores.Select(x => $"{x.codigo}:{x.descripcion}")) : string.Empty,
                ivstrObs =  lioconsultarComprobanteResponse.arrayObservaciones != null ? string.Join(", ", lioconsultarComprobanteResponse.arrayObservaciones.Select(x => $"{x.codigo}:{x.descripcion}")) : string.Empty
            };
        }
        public bool AuthDataModified(IDocument vioIDocument)
        {
            if (vioIDocument == null) return true;
            DocumentMTXCA? vioCurrentDocument = vioIDocument as DocumentMTXCA;
            if (vioCurrentDocument.mioDcModel.ivnroStatus <50) return false;
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
            if (vioCurrentDocument.ivdblImporteIva != ivdblImporteIva) return true;
            if (vioCurrentDocument.ivstrMoneda != ivstrMoneda) return true;
            if (vioCurrentDocument.ivdblCotizacion != ivdblCotizacion) return true;
            if (vioCurrentDocument.ivnroConcepto != ivnroConcepto) return true;
            if (vioCurrentDocument.ivnroConcepto == 2 || vioCurrentDocument.ivnroConcepto == 3)
            {
                if (vioCurrentDocument.ivdtmServdesde != this.ivdtmServdesde) return true;
                if (vioCurrentDocument.ivdtmServhasta != this.ivdtmServhasta) return true;
                if (vioCurrentDocument.ivdtmVtopago != this.ivdtmVtopago) return true;
            }
            if (vioCurrentDocument.mioDcModel.ivnroTipo >= 201 && vioCurrentDocument.mioDcModel.ivnroTipo <= 213 && vioCurrentDocument.mioDcModel.ivnroTipo != 203)
                if (vioCurrentDocument.ivdtmVtopago != this.ivdtmVtopago) return true;
            if (vioCurrentDocument.coAsociados != null && vioCurrentDocument.coAsociados.Count > 0)
            {
                if (vioCurrentDocument.coAsociados.Count != coAsociados.Count) return true;
                foreach (DocumentAsociado lioDocumentAsociado in vioCurrentDocument.coAsociados)
                    if (!coAsociados.Any(x => x.ivlngCbteNro == lioDocumentAsociado.ivlngCbteNro && x.ivnroCbtetipo == lioDocumentAsociado.ivnroCbtetipo && x.ivnumCbtePuntovta == lioDocumentAsociado.ivnumCbtePuntovta && x.ivlngCbteCUIT == lioDocumentAsociado.ivlngCbteCUIT && x.ivdtmFechaEmision == lioDocumentAsociado.ivdtmFechaEmision)) return true;
            }
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
            if (vioCurrentDocument.coOpcionales != null && vioCurrentDocument.coOpcionales.Count > 0)
            {
                if (vioCurrentDocument.coOpcionales.Count != coOpcionales.Count) return true;
                foreach (DocumentOpcional lioDocumentOpcional in vioCurrentDocument.coOpcionales)
                    if (!coOpcionales.Any(x => x.ivstrId == lioDocumentOpcional.ivstrId && x.ivstrValor == lioDocumentOpcional.ivstrValor)) return true;
            }
            if (vioCurrentDocument.coCompradores != null && vioCurrentDocument.coCompradores.Count > 0)
            {
                if (vioCurrentDocument.coCompradores.Count != coCompradores.Count) return true;
                foreach (DocumentComprador lioDocumentComprador in vioCurrentDocument.coCompradores)
                    if (!coCompradores.Any(x => x.ivnroDocTipo == lioDocumentComprador.ivnroDocTipo && x.ivlngDocNro == lioDocumentComprador.ivlngDocNro && x.ivdblPorcentaje == lioDocumentComprador.ivdblPorcentaje)) return true;
            }
            return false;
        }
        public async Task<double> GetCotizacion(string vivstrMoneda, DateTime vivdtm)
        {
            AfipService lioAfipService = new AfipService { ivstrName = ivstrDocWs, ioContext = mioContext };
            AfipLoginResponse lioAfipLoginResponse = await lioAfipService.GetAfipLogin();
            AuthRequestType lioAutRequest = new AuthRequestType()
            {
                cuitRepresentada = this.mioDcModel.ivlngCuitEmisor,
                sign = lioAfipLoginResponse.ivstrSign,
                token = lioAfipLoginResponse.ivstrToken
            };
            MTXCAServicePortType lioService = new MTXCAServicePortTypeClient(
               MTXCAServicePortTypeClient.EndpointConfiguration.MTXCAServiceHttpSoap11Endpoint,
               lioAfipService.ivstrUrl
               );
            consultarCotizacionMonedaRequest lioconsultarCotizacionMonedaRequest = new consultarCotizacionMonedaRequest
            {
                authRequest = lioAutRequest,
                codigoMoneda = vivstrMoneda,
                fechaCotizacion = vivdtm
            };
            LogHelper.writeinfo(
                $"Request: {JsonConvert.SerializeObject(lioconsultarCotizacionMonedaRequest)}",
                ListHelper.GetValue("FORMAT", "VERBOSE", mioContext) == "1"
            );
            consultarCotizacionMonedaResponse lioconsultarCotizacionMonedaResponse = await lioService.consultarCotizacionMonedaAsync(lioconsultarCotizacionMonedaRequest);
            if (lioconsultarCotizacionMonedaResponse == null)
                throw new Exception(Resources.lioE_Cotiz_No);
            if (lioconsultarCotizacionMonedaResponse.arrayErrores != null && lioconsultarCotizacionMonedaResponse.arrayErrores.Count() != 0)
                throw new Exception($"{Resources.lioE_Cotiz_No} => {lioconsultarCotizacionMonedaResponse.arrayErrores[0].codigo}:{lioconsultarCotizacionMonedaResponse.arrayErrores[0].descripcion}");
            return (double)lioconsultarCotizacionMonedaResponse.cotizacionMoneda;
        }
        #endregion
    }
}


