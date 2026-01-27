using Applet.Nat.Afip.ServicesV1;
using Applet.Nat.Api.AFIP.Model;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Models.Afip;
using Applet.Nat.Api.Models.BR;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Net;

namespace Applet.Nat.Api.Br.Models
{
    public class TribDocumentV1 : ITribDocument
    {
        #region CONSTRUCT
        public TribDocumentV1(DocumentModel vioDocumentModel, NatContext vioContext)
        {
            mioDcModel = vioDocumentModel;
            mioContext = vioContext;
            ivstrDocWs = "wsfe";
        }
        #endregion
        #region PUBLIC PROPS
        public short ivnroConcepto { get; set; }
        public double ivdblImporteNoGravado { get; set; }
        public double ivdblImporteGravado { get; set; }
        public double ivdblImporteExento { get; set; }
        public double ivdblImporteIva { get; set; }
        public double ivdblImporteOtrosTributos { get; set; }
        public double ivdblCotizacion { get; set; }
        public string ivstrMoneda { get; set; }
        public short ivnroTipoResp { get; set; }
        public short ivnroTipoReceptor { get; set; }
        public string ivstrNroReceptor { get; set; }
        public string ivstrDocWs { get; set; }
        public DateTime? ivdtmServdesde { get; set; }
        public DateTime? ivdtmServhasta { get; set; }
        public DateTime? ivdtmVtopago { get; set; }
        public string? ivstrCanMisMonExt { get; set; }
        public List<DocumentAsociado> coAsociados { get; set; }
        public List<DocumentOtroTributo> coOtrosTributos { get; set; }
        public List<DocumentIva> coIvas { get; set; }
        public List<DocumentOpcional> coOpcionales { get; set; }
        public List<DocumentComprador> coCompradores { get; set; }
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
            vioDocumentUser.FormatAmounts();
            ivnroConcepto = vioDocumentUser.ivnroConcepto ?? 0;
            ivdblImporteNoGravado = Math.Abs(vioDocumentUser.ivdblImporteNoGravado ?? 0);
            ivdblImporteGravado = Math.Abs(vioDocumentUser.ivdblImporteGravado ?? 0);
            ivdblImporteExento = Math.Abs(vioDocumentUser.ivdblImporteExento ?? 0);
            ivdblImporteIva = Math.Abs(vioDocumentUser.ivdblImporteIva ?? 0);
            ivdblImporteOtrosTributos = Math.Abs(vioDocumentUser.ivdblImporteOtrosTributos ?? 0);
            ivdblCotizacion = vioDocumentUser.ivdblCotizacion ?? 0;
            ivstrMoneda = vioDocumentUser.ivstrMoneda ?? string.Empty;
            ivnroTipoReceptor = vioDocumentUser.ivnroTipoDocReceptor ?? 0;
            ivnroTipoResp = vioDocumentUser.ivnroTipoRespReceptor ?? 0;
            ivstrNroReceptor = vioDocumentUser.ivlngDocReceptor.ToString() ?? string.Empty;
            ivdtmServdesde = Format.DateFromUX(vioDocumentUser.ivstrFechaServdesde, livstrApiDtmFormat);
            ivdtmServhasta = Format.DateFromUX(vioDocumentUser.ivstrFechaServhasta, livstrApiDtmFormat);
            ivdtmVtopago = Format.DateFromUX(vioDocumentUser.ivstrFechaVtopago, livstrApiDtmFormat);
            ivstrCanMisMonExt = vioDocumentUser.ivstrCanMisMonExt ?? string.Empty;
            if (vioDocumentUser.coAsociados != null && vioDocumentUser.coAsociados.Count > 0)
            {
                coAsociados = new List<DocumentAsociado>();
                foreach (UxDocumentAsociado lioO in vioDocumentUser.coAsociados)
                {
                    if (lioO.ivnroCbtetipo == 0) continue;
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
            }
            short livnro;
       
            if (vioDocumentUser.coOpcionales != null && vioDocumentUser.coOpcionales.Count > 0)
            {
                coOpcionales = new List<DocumentOpcional>();
                foreach (UxDocumentOpcional lioO in vioDocumentUser.coOpcionales)
                {
                    if (string.IsNullOrEmpty(lioO.ivstrId)) continue;
                    coOpcionales.Add(
                         new DocumentOpcional
                         {
                             ivstrId = lioO.ivstrId,
                             ivstrValor = lioO.ivstrValor
                         });
                }
            }
            if (vioDocumentUser.coCompradores != null && vioDocumentUser.coCompradores.Count > 0)
            {
                coCompradores = new List<DocumentComprador>();
                foreach (UxDocumentComprador lioO in vioDocumentUser.coCompradores)
                {
                    if (lioO.ivnroDocTipo == 0) continue;
                    coCompradores.Add(
                         new DocumentComprador
                         {
                             ivdblPorcentaje = lioO.ivdblPorcentaje,
                             ivnroDocTipo = lioO.ivnroDocTipo,
                             ivlngDocNro = lioO.ivlngDocNro
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
                             ivdblBaseImponible = lioO.ivdblBaseImponible ??0,
                             ivdblImporte = lioO.ivdblImporte ?? 0,
                             ivnroTipo = lioO.ivnroTipo
                         });
                }
            }
            // borrado de iva no gravado y exento
            coIvas.RemoveAll(x => x.ivnroTipo == 1 || x.ivnroTipo == 2);
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
            FEAuthRequest lioAutRequest = new FEAuthRequest()
            {
                Cuit = this.mioDcModel.ivlngCuitEmisor,
                Sign = lioAfipLoginResponse.ivstrSign,
                Token = lioAfipLoginResponse.ivstrToken
            };
            //CONSULTA ULTIMO COMPROBANTE AUTORIZADO
            ServiceSoapClient lioService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            lioService.Endpoint.Address = new System.ServiceModel.EndpointAddress(lioAfipService.ivstrUrl);
            FECompUltimoAutorizadoResponse lioFECompUltimoAutorizadoResponse = null;
            short livnroIntento = 0;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioFECompUltimoAutorizadoResponse = await lioService.FECompUltimoAutorizadoAsync(lioAutRequest, mioDcModel.ivnumPvta, mioDcModel.ivnroTipo);
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
                               Response = lioE.Message
                           }
                       )
                   );
                    LogHelper.write(lioE);
                    return livnroNextStatus;
                }
            }
            if (lioFECompUltimoAutorizadoResponse == null || lioFECompUltimoAutorizadoResponse.Body == null || lioFECompUltimoAutorizadoResponse.Body.FECompUltimoAutorizadoResult == null)
                throw new Exception($"{Resources.lioE_HeaderAuth}:{Resources.lioE_AfipRespNo}");
            //EL DOCUMENTO ES MAYOR AL ULTIMO AUTORIZADO ==> EsperaPredecesor
            if (lioFECompUltimoAutorizadoResponse.Body.FECompUltimoAutorizadoResult.CbteNro + 1 < this.mioDcModel.ivlngCbte)
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
                                Response = lioFECompUltimoAutorizadoResponse
                            }
                        )
                    );
                }
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES MENOR AL ULTIMO AUTORIZADO  ==> CONSULTAR CAE
            if (lioFECompUltimoAutorizadoResponse.Body.FECompUltimoAutorizadoResult.CbteNro + 1 > this.mioDcModel.ivlngCbte)
            {
                FECompConsultaReq lioFECompConsultaReq = new FECompConsultaReq
                {
                    CbteNro = this.mioDcModel.ivlngCbte,
                    CbteTipo = this.mioDcModel.ivnroTipo,
                    PtoVta = this.mioDcModel.ivnumPvta
                };
                FECompConsultarResponse lioFECompConsultarResponse = null;
                while (true)
                {
                    livnroIntento++;
                    try
                    {
                        lioFECompConsultarResponse = await lioService.FECompConsultarAsync(lioAutRequest, lioFECompConsultaReq);
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
                                       lioFECompConsultaReq
                                   },
                                   Response = lioE.Message
                               }
                           )
                       );
                        LogHelper.write(lioE);
                        return livnroNextStatus;
                    }
                }
                if (lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet != null && !string.IsNullOrEmpty(lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet.CodAutorizacion))
                    livnroNextStatus = 50;
                lioDocumentTracking.addTrack(
                    livnroNextStatus,
                    JsonConvert.SerializeObject(
                        new
                        {
                            Request = new
                            {
                                lioAutRequest,
                                lioFECompConsultaReq
                            },
                            Response = lioFECompConsultarResponse
                        }
                    )
                );
                return livnroNextStatus;
            }
            //EL DOCUMENTO ES EL SIGUIENTE  ==> AUTORIZAR
            FECAECabRequest lioCAECabRequest = new FECAECabRequest
            {
                CantReg = 1,
                CbteTipo = this.mioDcModel.ivnroTipo,
                PtoVta = this.mioDcModel.ivnumPvta
            };
            FECAEDetRequest lioCAEDetRequest = new FECAEDetRequest
            {
                DocTipo = this.ivnroTipoReceptor,
                DocNro = long.Parse(this.ivstrNroReceptor),
                CbteDesde = this.mioDcModel.ivlngCbte,
                CbteHasta = this.mioDcModel.ivlngCbte,
                CbteFch = this.mioDcModel.ivdtmEmision?.ToString(lioAfipService.ivstrDateformat),
                ImpTotal = this.mioDcModel.ivdblImporte ?? 0,
                ImpTotConc = this.ivdblImporteNoGravado,
                ImpNeto = this.ivdblImporteGravado,
                ImpOpEx = this.ivdblImporteExento,
                ImpTrib = this.ivdblImporteOtrosTributos,
                ImpIVA = this.ivdblImporteIva,
                MonId = this.ivstrMoneda,
                MonCotiz = this.ivdblCotizacion,
                Concepto = this.ivnroConcepto,
                CondicionIVAReceptorId = this.ivnroTipoResp,
                CanMisMonExt = string.IsNullOrEmpty(ivstrCanMisMonExt) ? null : ivstrCanMisMonExt,
            };
            if (this.ivnroConcepto == 2 || this.ivnroConcepto == 3)
            {
                lioCAEDetRequest.FchServDesde = this.ivdtmServdesde?.ToString(lioAfipService.ivstrDateformat);
                lioCAEDetRequest.FchServHasta = this.ivdtmServhasta?.ToString(lioAfipService.ivstrDateformat);
            }
            // Fecha de pago
            if (new short[] { 202, 203, 207, 208 }.Contains(this.mioDcModel.ivnroTipo))
                lioCAEDetRequest.FchVtoPago = null;
            else if (this.mioDcModel.ivnroTipo < 201 && this.ivnroConcepto == 1)
                lioCAEDetRequest.FchVtoPago = null;
            else
                lioCAEDetRequest.FchVtoPago = this.ivdtmVtopago?.ToString(lioAfipService.ivstrDateformat);
            //
            int lionum = 0;
            if (this.coAsociados != null && this.coAsociados.Count > 0)
            {
                lioCAEDetRequest.CbtesAsoc = new CbteAsoc[this.coAsociados.Count];
                foreach (DocumentAsociado lioDocumentAsociado in this.coAsociados)
                {
                    lioCAEDetRequest.CbtesAsoc[lionum] = new CbteAsoc();
                    lioCAEDetRequest.CbtesAsoc[lionum].Tipo = (int)lioDocumentAsociado.ivnroCbtetipo;
                    lioCAEDetRequest.CbtesAsoc[lionum].PtoVta = lioDocumentAsociado.ivnumCbtePuntovta ?? 0;
                    lioCAEDetRequest.CbtesAsoc[lionum].Nro = lioDocumentAsociado.ivlngCbteNro ?? 0;
                    lioCAEDetRequest.CbtesAsoc[lionum].Cuit = lioDocumentAsociado.ivlngCbteCUIT.ToString();
                    lioCAEDetRequest.CbtesAsoc[lionum].CbteFch = lioDocumentAsociado.ivdtmFechaEmision?.ToString(lioAfipService.ivstrDateformat);
                    lionum++;
                }
            }
            if (this.coOtrosTributos != null && this.coOtrosTributos.Count > 0)
            {
                lionum = 0;
                lioCAEDetRequest.Tributos = new Tributo[this.coOtrosTributos.Count];
                foreach (DocumentOtroTributo lioDocumentOtroTributo in this.coOtrosTributos)
                {
                    lioCAEDetRequest.Tributos[lionum] = new Tributo();
                    lioCAEDetRequest.Tributos[lionum].Id = lioDocumentOtroTributo.ivnroId ?? 0;
                    lioCAEDetRequest.Tributos[lionum].Desc = lioDocumentOtroTributo.ivstrDesc;
                    lioCAEDetRequest.Tributos[lionum].BaseImp = lioDocumentOtroTributo.ivdblBaseImp ?? 0;
                    lioCAEDetRequest.Tributos[lionum].Alic = lioDocumentOtroTributo.ivdblAlicuota ?? 0;
                    lioCAEDetRequest.Tributos[lionum].Importe = lioDocumentOtroTributo.ivdblImporte ?? 0;
                    lionum++;
                }
            }
            if (this.coIvas != null && this.coIvas.Count > 0)
            {
                lionum = 0;
                lioCAEDetRequest.Iva = new AlicIva[this.coIvas.Count];
                foreach (DocumentIva lioDocumentIva in this.coIvas)
                {
                    lioCAEDetRequest.Iva[lionum] = new AlicIva();
                    lioCAEDetRequest.Iva[lionum].Id = lioDocumentIva.ivnroTipo ?? 0;
                    lioCAEDetRequest.Iva[lionum].BaseImp = lioDocumentIva.ivdblBaseImponible ?? 0;
                    lioCAEDetRequest.Iva[lionum].Importe = lioDocumentIva.ivdblImporte ?? 0;
                    lionum++;
                }
            }
            //
            if (this.coOpcionales != null && this.coOpcionales.Count > 0)
            {
                lionum = 0;
                lioCAEDetRequest.Opcionales = new Opcional[this.coOpcionales.Count];
                foreach (DocumentOpcional lioDocumentOpcional in this.coOpcionales)
                {
                    lioCAEDetRequest.Opcionales[lionum] = new Opcional();
                    lioCAEDetRequest.Opcionales[lionum].Id = lioDocumentOpcional.ivstrId;
                    lioCAEDetRequest.Opcionales[lionum].Valor = lioDocumentOpcional.ivstrValor;
                    lionum++;
                }
            }
            //
            if (this.coCompradores != null && this.coCompradores.Count > 0)
            {
                lionum = 0;
                lioCAEDetRequest.Compradores = new Comprador[this.coCompradores.Count];
                foreach (DocumentComprador lioDocumentComprador in this.coCompradores)
                {
                    lioCAEDetRequest.Compradores[lionum] = new Comprador();
                    lioCAEDetRequest.Compradores[lionum].DocTipo = lioDocumentComprador.ivnroDocTipo ?? 0;
                    lioCAEDetRequest.Compradores[lionum].DocNro = lioDocumentComprador.ivlngDocNro ?? 0;
                    lioCAEDetRequest.Compradores[lionum].Porcentaje = lioDocumentComprador.ivdblPorcentaje ?? 0;
                    lionum++;
                }
            }
            FECAERequest lioFECAERequest = new FECAERequest
            {
                FeCabReq = lioCAECabRequest,
                FeDetReq = new FECAEDetRequest[] { lioCAEDetRequest }
            };
            FECAESolicitarResponse lioFECAESolicitarResponse = null;
            while (true)
            {
                livnroIntento++;
                try
                {
                    lioFECAESolicitarResponse = await lioService.FECAESolicitarAsync(lioAutRequest, lioFECAERequest);
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
                                   lioFECAERequest
                               },
                               Response = lioE.Message
                           }
                       )
                    );
                    LogHelper.write(lioE);
                    return 40;
                }
            }
            if (lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp != null && lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp.Length > 0)
                foreach (FECAEDetResponse lioDetResp in lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp)
                {
                    if (!string.IsNullOrEmpty(lioDetResp.CAE))
                    {
                        livnroNextStatus = 50;
                        break;
                    }
                }
            lioDocumentTracking.addTrack(
                livnroNextStatus,
                JsonConvert.SerializeObject(
                    new
                    {
                        Request = new
                        {
                            lioAutRequest,
                            lioFECAERequest
                        },
                        Response = lioFECAESolicitarResponse
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
        public bool AuthDataModified(ITribDocument vioIDocument)
        {
            if (vioIDocument == null) return true;
            TribDocumentV1? vioCurrentDocument = vioIDocument as TribDocumentV1;
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
            FEAuthRequest lioAutRequest = new FEAuthRequest()
            {
                Cuit = this.mioDcModel.ivlngCuitEmisor,
                Sign = lioAfipLoginResponse.ivstrSign,
                Token = lioAfipLoginResponse.ivstrToken
            };
            ServiceSoapClient lioService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);
            short livnroIntento = 0;
            FEParamGetCotizacionResponse lioFEParamGetCotizacionResponse = null;
            while (true)
            {
                livnroIntento++;
                try
                {
                    LogHelper.writeinfo(
                       $"FEParamGetCotizacionAsync: Url:{lioAfipService.ivstrUrl} Auth:{JsonConvert.SerializeObject(lioAutRequest)}, Moneda: {vivstrMoneda}, Fecha:{vivdtm.ToString(lioAfipService.ivstrDateformat)}",
                       ListHelper.GetValue("FORMAT", "VERBOSE", mioContext) == "1"
                    );
                    lioFEParamGetCotizacionResponse = await lioService.FEParamGetCotizacionAsync(lioAutRequest, vivstrMoneda, vivdtm.ToString(lioAfipService.ivstrDateformat));
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
            if (lioFEParamGetCotizacionResponse == null)
                throw new Exception(Resources.lioE_Cotiz_No);
            if (lioFEParamGetCotizacionResponse.Body.FEParamGetCotizacionResult.Errors != null && lioFEParamGetCotizacionResponse.Body.FEParamGetCotizacionResult.Errors.Count() != 0)
                throw new Exception($"{Resources.lioE_Cotiz_No} => {lioFEParamGetCotizacionResponse.Body.FEParamGetCotizacionResult.Errors[0].Code}:{lioFEParamGetCotizacionResponse.Body.FEParamGetCotizacionResult.Errors[0].Msg}");
            return (double)lioFEParamGetCotizacionResponse.Body.FEParamGetCotizacionResult.ResultGet.MonCotiz;
        }
        public UxAuth GetAuth()
        {
            DocumentTrackingModel lioTrack = mioContext.DocumentTrackings.OrderByDescending(x => x.ivdtmTrack).FirstOrDefault(x => x.ivlngDoc == mioDcModel.ivlngDoc && (x.ivnroStatus == 50 || x.ivnroStatus == 40));
            if (lioTrack == null || string.IsNullOrEmpty(lioTrack.ivstrData))
                throw new Exception(Resources.lioE_CAEQry_Err);
            dynamic lioTrackData = JsonConvert.DeserializeObject(lioTrack.ivstrData);
            UxAuth lioUxAuth;
            string livstr;
            FECAESolicitarResponse lioFECAESolicitarResponse = JsonConvert.DeserializeObject<FECAESolicitarResponse>(lioTrackData?.Response?.ToString());
            if (lioFECAESolicitarResponse != null && lioFECAESolicitarResponse.Body != null && lioFECAESolicitarResponse.Body.FECAESolicitarResult != null)
            {
                lioUxAuth = new UxAuth();
                lioUxAuth.ivdtmNode = lioTrack.ivdtmTrack;
                lioUxAuth.ivnumtrack = lioTrack.ivnumTrack;
                lioUxAuth.ivstrAuthCode = lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp[0].CAE;
                lioUxAuth.ivdtmAuthVenc = lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp[0].CAEFchVto;
                lioUxAuth.ivstrAuthType = "CAE";
                lioUxAuth.ivtrStatusDesc = string.IsNullOrEmpty(lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp[0].CAE) ? "Rechazado" : "Autorizado";
                livstr = string.Empty;
                if (lioFECAESolicitarResponse.Body.FECAESolicitarResult.Errors != null)
                    livstr = string.Join(", ", lioFECAESolicitarResponse.Body.FECAESolicitarResult.Errors.Select(x => $"{x.Code}:{x.Msg}"));
                lioUxAuth.ivstrErrors = livstr;
                livstr = string.Empty;
                if (lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp != null && lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp.Length > 0)
                    for (int i = 0; i < lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp.Length; i++)
                        if (lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp[i].Observaciones != null)
                            livstr += string.Join(", ", lioFECAESolicitarResponse.Body.FECAESolicitarResult.FeDetResp[i].Observaciones.Select(x => $"{x.Code}:{x.Msg}"));
                lioUxAuth.ivstrErrors += livstr;
                if (lioFECAESolicitarResponse.Body.FECAESolicitarResult.Events != null)
                    livstr += string.Join(", ", lioFECAESolicitarResponse.Body.FECAESolicitarResult.Events.Select(x => $"{x.Code}:{x.Msg}"));
                lioUxAuth.ivstrObs = livstr;
                return lioUxAuth;
            }
            FECompConsultarResponse lioFECompConsultarResponse = JsonConvert.DeserializeObject<FECompConsultarResponse>(lioTrackData.Response.ToString());
            if (lioFECompConsultarResponse == null || lioFECompConsultarResponse.Body == null || lioFECompConsultarResponse.Body.FECompConsultarResult == null)
                throw new Exception(Resources.lioE_CAEQry_Err);
            lioUxAuth = new UxAuth();
            lioUxAuth.ivdtmNode = lioTrack.ivdtmTrack;
            lioUxAuth.ivnumtrack = lioTrack.ivnumTrack;
            lioUxAuth.ivstrAuthCode = lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet.CodAutorizacion;
            lioUxAuth.ivdtmAuthVenc = lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet.FchVto;
            lioUxAuth.ivstrAuthType = "CAE";
            lioUxAuth.ivtrStatusDesc = string.IsNullOrEmpty(lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet.CodAutorizacion) ? "Rechazado" : "Autorizado";
            livstr = string.Empty;
            if (lioFECompConsultarResponse.Body.FECompConsultarResult.Errors != null)
                livstr = string.Join(", ", lioFECompConsultarResponse.Body.FECompConsultarResult.Errors.Select(x => $"{x.Code}:{x.Msg}"));
            lioUxAuth.ivstrErrors = livstr;
            livstr = string.Empty;
            if (lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet.Observaciones != null)
                livstr = string.Join(", ", lioFECompConsultarResponse.Body.FECompConsultarResult.ResultGet.Observaciones.Select(x => $"{x.Code}:{x.Msg}"));
            lioUxAuth.ivstrErrors += livstr;
            if (lioFECompConsultarResponse.Body.FECompConsultarResult.Events != null)
                livstr += string.Join(", ", lioFECompConsultarResponse.Body.FECompConsultarResult.Events.Select(x => $"{x.Code}:{x.Msg}"));
            lioUxAuth.ivstrObs = livstr;
            return lioUxAuth;
        }
        #endregion
        #region PRIVATE METHODS

        #endregion
    }

}
