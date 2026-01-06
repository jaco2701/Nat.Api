using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Nat.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
namespace Applet.Nat.Api.Br.Models
{
    public class InDocumentJSON : IRawDocument
    {
        #region CONS
        public InDocumentJSON(long vivlngCuit, NatContext vioContext)
        {
            mivlngCuit = vivlngCuit;
            mioContext = vioContext;
        }
        #endregion
        public string? ivstrRaw { get; set; }
        public string? ivstrName { get; set; }
        public string ivstrKey { get; set; }
        #region PUBLIC METHODS
        public DocumentUser[] GetDocuments()
        {
            DocumentUser lioDocumentUser = new DocumentUser();
            try
            {
                Cuit lioCuit = new Cuit(mivlngCuit, mioContext, null);
                if (string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                    throw new Exception("Mapeador no encontrado o con formato erroneo");
                string livstr = lioCuit.GetEncoding().GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty, lioCuit.GetEncoding()))),
                livstrWs,
                livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext),
                livstrXmlDtmFormat = ListHelper.GetValue("FORMAT", "XmlDtm", mioContext);
                DateTime livdtm;
                short livnro, livnroI;
                int livnum;
                long livlng;
                double livval;
                JObject lioJObject = JObject.Parse(livstr);
                if (lioJObject == null)
                    throw new Exception("Documento Original Invalido");
                livstrWs = lioJObject.SelectToken("DTE.Documento.Encabezado.IdDoc.WSRegimen")?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(livstrWs))
                    throw new Exception("Servicio no encontrado o con formato erroneo");
                ServiceMapper lioMapper = lioCuit.ioCnfg?.coServiceMappers.FirstOrDefault(x => x.ivstrInputType == "json" && x.ivstrWs == livstrWs);
                if (lioMapper == null || lioMapper.coItems == null)
                    throw new Exception("Mapeador no encontrado o con formato erroneo");
                UxDocumentAsociado lioUxDocumentAsociado;
                UxDocumentOtroTributo lioUxDocumentOtroTributo;
                UxDocumentOpcional lioUxDocumentOpcional = null;
                UxDocumentItem lioUxDocumentItem;
                UxDocumentIva lioUxDocumentIva;
                ServiceMapperItem lioMapperItem1;
                JToken lioJToken;
                StringBuilder lioSbErrors = new StringBuilder();
                lioDocumentUser = new DocumentUser();
                lioDocumentUser.ivstrWs = livstrWs;
                lioDocumentUser.ivstrInputData = ivstrRaw;
                lioDocumentUser.ioDomicilioReceptor = new UxDomicilio();

                foreach (ServiceMapperItem lioMapperItem in lioMapper.coItems.Where(x => !string.IsNullOrEmpty(x.coXPaths[0].ivstrData)))
                {
                    lioJToken = lioJObject.SelectToken(lioMapperItem.coXPaths[0].ivstrData);
                    switch (lioMapperItem.ivstrProperty)
                    {
                        #region Cabecera
                        case "ivnroTipoDoc":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                            {
                                lioSbErrors.AppendLine("Tipo de comprobante no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivnroTipoDoc = livnro;
                            break;
                        case "ivnumPvta":
                            if (lioJToken == null || !int.TryParse(lioJToken.ToString(), out livnum))
                            {
                                lioSbErrors.AppendLine("Punto de venta no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivnumPvta = livnum;
                            break;
                        case "ivlngCbte":
                            if (lioJToken == null || !long.TryParse(lioJToken.ToString(), out livlng))
                            {
                                lioSbErrors.AppendLine("Numero de comprobante no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivlngCbte = livlng;
                            break;
                        case "ivstrFechaEmision":
                            if (lioJToken == null || !DateTime.TryParse(lioJToken.ToString().Replace("T00:00:00", string.Empty), out livdtm))
                            {
                                lioSbErrors.AppendLine($"Fecha de Comprobante INVALIDA ({livstrXmlDtmFormat})");
                                continue;
                            }
                            lioDocumentUser.ivstrFechaEmision = livdtm.ToString(livstrApiDtmFormat);
                            break;
                        case "ivstrCondPago":
                            if (lioJToken == null)
                                lioDocumentUser.ivstrCondPago = string.Empty;
                            else
                                lioDocumentUser.ivstrCondPago = lioJToken.ToString();
                            break;
                        case "ivstrFechaVtopago":
                            if (lioJToken == null || !DateTime.TryParse(lioJToken.ToString().Replace("T00:00:00", string.Empty), out livdtm))
                            {
                                lioSbErrors.AppendLine($"FECHA de Vencimiento de Pago INVALIDA ({livstrXmlDtmFormat})");
                                continue;
                            }
                            lioDocumentUser.ivstrFechaVtopago = livdtm.ToString(livstrApiDtmFormat);
                            break;
                        case "ivstrFechaServdesde":
                            if (lioJToken != null)
                            {
                                if (!DateTime.TryParse(lioJToken.ToString().Replace("T00:00:00", string.Empty), out livdtm))
                                {
                                    lioSbErrors.AppendLine($"FECHA de Inicio de Servicios INVALIDA ({livstrXmlDtmFormat})");
                                    continue;
                                }
                                lioDocumentUser.ivstrFechaServdesde = livdtm.ToString(livstrApiDtmFormat);
                            }
                            break;
                        case "ivstrFechaServhasta":
                            if (lioJToken != null)
                            {
                                if (!DateTime.TryParse(lioJToken.ToString().Replace("T00:00:00", string.Empty), out livdtm))
                                {
                                    lioSbErrors.AppendLine($"FECHA de Finalizacion de Servicios INVALIDA ({livstrXmlDtmFormat})");
                                    continue;
                                }
                                lioDocumentUser.ivstrFechaServdesde = livdtm.ToString(livstrApiDtmFormat);
                            }
                            break;
                        case "ivstrMoneda":
                            if (lioJToken == null)
                            {
                                lioSbErrors.AppendLine("Moneda no encontrada o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivstrMoneda = lioJToken.ToString();
                            break;
                        case "ivnroConcepto":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                            {
                                lioSbErrors.AppendLine("Concepto no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivnroConcepto = livnro;
                            break;
                        case "ivdblCotizacion":
                            if (lioJToken == null || !double.TryParse(lioJToken.ToString(), out livval))
                                lioDocumentUser.ivdblCotizacion = 1;
                            else
                                lioDocumentUser.ivdblCotizacion = livval;
                            break;
                        case "ivstrObs":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrObs = lioJToken.ToString();
                            break;
                        case "ivstrIdCliente":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrIdCliente = lioJToken.ToString();
                            break;
                        case "ivstrIdSucursal":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrIdSucursal = lioJToken.ToString();
                            break;
                        case "ivstrCanMisMonExt":
                            if (lioJToken == null)
                            {
                                lioSbErrors.AppendLine("CanMisMonExt no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivstrCanMisMonExt = lioJToken.ToString();
                            break;
                        case "ivnroIdioma":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                                lioDocumentUser.ivnroIdioma = 1;
                            else
                                lioDocumentUser.ivnroIdioma = livnro;
                            break;
                        case "ivstrIncoterms":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrIncoterms = lioJToken.ToString();
                            break;
                        case "ivstrIncotermsDs":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrIncotermsDs = lioJToken.ToString();
                            break;
                        case "ivnroTipoExpo":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                                continue;
                            lioDocumentUser.ivnroTipoExpo = livnro;
                            break;
                        case "ivnroDestinoCmp":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                                continue;
                            lioDocumentUser.ivnroDestinoCmp = livnro;
                            break;
                        case "ivlngCuitPaisCliente":
                            if (lioJToken == null || !long.TryParse(lioJToken.ToString(), out livlng))
                                continue;
                            lioDocumentUser.ivlngCuitPaisCliente = livlng;
                            break;
                        case "ivlngIDImpositivo":
                            if (lioJToken == null || !long.TryParse(lioJToken.ToString(), out livlng))
                                continue;
                            lioDocumentUser.ivlngIDImpositivo = livlng;
                            break;
                        case "ivstrPermisoExistente":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrIncoterms = lioJToken.ToString();
                            break;

                        #endregion
                        #region Emisor
                        case "ivlngCuitEmisor":
                            if (lioJToken == null || !long.TryParse(lioJToken.ToString(), out livlng))
                            {
                                lioSbErrors.AppendLine("CUIT Emisor no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivlngCuitEmisor = livlng;
                            break;
                        #endregion
                        #region Receptor
                        case "ivlngDocReceptor":
                            if (lioJToken == null || !long.TryParse(lioJToken.ToString(), out livlng))
                            {
                                lioSbErrors.AppendLine("Numero de documento receptor no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivlngDocReceptor = livlng;
                            break;
                        case "ivnroTipoDocReceptor":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                            {
                                lioSbErrors.AppendLine("Tipo de documento receptor no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivnroTipoDocReceptor = livnro;
                            break;
                        case "ivstrRazonSocial":
                            if (lioJToken == null)
                            {
                                lioSbErrors.AppendLine("Razon Social Receptor no encontrada o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivstrRazonSocial = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrCalle":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrCalle = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrNro":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrNro = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrPiso":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrPiso = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrDepto":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrDepto = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrCuidad":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrCuidad = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrMunicipio":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrMunicipio = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivnroPcia":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivnroPcia = livnro;
                            break;
                        case "ioDomicilioReceptor.ivstrPais":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrPais = lioJToken.ToString();
                            break;
                        case "ioDomicilioReceptor.ivstrCP":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ioDomicilioReceptor.ivstrCP = lioJToken.ToString();
                            break;
                        case "ivnroTipoRespReceptor":
                            if (lioJToken == null || !short.TryParse(lioJToken.ToString(), out livnro))
                            {
                                lioSbErrors.AppendLine("Condicion Iva receptor no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivnroTipoRespReceptor = livnro;
                            break;
                        case "ivstrEmail":
                            if (lioJToken == null)
                                continue;
                            lioDocumentUser.ivstrEmail = lioJToken.ToString();
                            break;
                        #endregion
                        #region Importes
                        case "ivdblImporteTotal":
                            if (lioJToken == null || !double.TryParse(lioJToken.ToString(), out livval))
                            {
                                lioSbErrors.AppendLine("Importe Total no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivdblImporteTotal = livval;
                            break;
                        case "ivdblImporteGravado":
                            if (lioJToken == null || !double.TryParse(lioJToken.ToString(), out livval))
                            {
                                lioSbErrors.AppendLine("Importe Gravado no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivdblImporteGravado = livval;
                            break;
                        case "ivdblImporteNoGravado":
                            if (lioJToken == null || !double.TryParse(lioJToken.ToString(), out livval))
                            {
                                lioSbErrors.AppendLine("Importe NoGravado no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivdblImporteNoGravado = livval;
                            break;
                        case "ivdblImporteExento":
                            if (lioJToken == null || !double.TryParse(lioJToken.ToString(), out livval))
                            {
                                lioSbErrors.AppendLine("Importe Exento no encontrado o con formato erroneo");
                                continue;
                            }
                            lioDocumentUser.ivdblImporteExento = livval;
                            break;
                        #endregion
                        case "coAsociados":
                            livnroI = 1;
                            if (lioDocumentUser.coAsociados == null)
                                lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                            foreach (JToken lioO in lioJObject.SelectTokens("coAsociados").Children())
                            {
                                lioUxDocumentAsociado = new UxDocumentAsociado();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivnumCbtePuntovta");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Doc.Asociado {livnroI} Punto de Venta no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentAsociado.ivnumCbtePuntovta = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToObject<int>();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivnroCbtetipo");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Doc.Asociado {livnroI} Tipo de Comprobante no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentAsociado.ivnroCbtetipo = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToObject<short>();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivstrFechaEmision");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Doc.Asociado {livnroI} Fecha de Emision no encontrada o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentAsociado.ivstrFechaEmision = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToString();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivlngCbteCUIT");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Doc.Asociado {livnroI} CUIT no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentAsociado.ivlngCbteCUIT = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToObject<long>();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivlngCbteNro");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Doc.Asociado {livnroI} Nro Comprobante no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentAsociado.ivlngCbteNro = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToObject<long>();
                                lioDocumentUser.coAsociados.Add(lioUxDocumentAsociado);
                                livnroI++;
                            }
                            break;
                        case "coOtrosTributos":
                            lioDocumentUser.ivdblImporteOtrosTributos = 0;
                            livnroI = 1;
                            if (lioDocumentUser.coOtrosTributos == null)
                                lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                            foreach (JToken lioO in lioJObject.SelectTokens(lioMapperItem.coXPaths[0].ivstrData).Children())
                            {
                                lioUxDocumentOtroTributo = new UxDocumentOtroTributo();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivnroId");
                                if (lioMapperItem1 == null || !short.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livnro))
                                {
                                    lioSbErrors.AppendLine($"Tributo {livnroI} Id no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentOtroTributo.ivnroId = livnro;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivstrDesc");
                                if (lioMapperItem1 != null)
                                {
                                    lioUxDocumentOtroTributo.ivstrDesc = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToString();
                                    if (lioUxDocumentOtroTributo.ivstrDesc == "IVA")
                                        continue;
                                }
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivdblBaseImponible");
                                if (lioMapperItem1 == null || !double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                {
                                    lioSbErrors.AppendLine($"Tributo {livnroI} Base Imponible no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentOtroTributo.ivdblBaseImponible = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivdblAlicuota");
                                if (lioMapperItem1 == null || !double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                {
                                    lioSbErrors.AppendLine($"Tributo {livnroI} Alicuota no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentOtroTributo.ivdblAlicuota = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivdblImporte");
                                if (lioMapperItem1 == null || !double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                {
                                    lioSbErrors.AppendLine($"Tributo {livnroI} Importe no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentOtroTributo.ivdblImporte = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivstrId");
                                if (lioMapperItem1 == null || !short.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livnro))
                                {
                                    lioSbErrors.AppendLine($"Tributo {livnroI} tipo no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentOtroTributo.ivnroId = livnro;
                                lioDocumentUser.ivdblImporteOtrosTributos += lioUxDocumentOtroTributo.ivdblImporte;
                                lioDocumentUser.coOtrosTributos.Add(lioUxDocumentOtroTributo);
                                livnroI++;
                            }
                            break;
                        case "coIvas":
                            livnroI = 1;
                            if (lioDocumentUser.coIvas == null)
                                lioDocumentUser.coIvas = new List<UxDocumentIva>();
                            lioDocumentUser.ivdblImporteIva = 0;
                            foreach (JToken lioO in lioJObject.SelectTokens(lioMapperItem.coXPaths[0].ivstrData).Children())
                            {
                                lioUxDocumentIva = new UxDocumentIva();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivstrDesc");
                                if (lioMapperItem1 != null)
                                {
                                    livstr = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToString();
                                    if (livstr != "IVA")
                                        continue;
                                }
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas.ivnroTipo");
                                if (lioMapperItem1 == null || !short.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livnro))
                                {
                                    lioSbErrors.AppendLine($"Iva {livnroI} tipo no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentIva.ivnroTipo = livnro;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas.ivdblBaseImponible");
                                if (lioMapperItem1 == null || !double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                {
                                    lioSbErrors.AppendLine($"Iva {livnroI}  Base Imponible no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentIva.ivdblBaseImponible = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas.ivdblImporte");
                                if (lioMapperItem1 == null || !double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                {
                                    lioSbErrors.AppendLine($"Iva {livnroI} Importe no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentIva.ivdblImporte = livval;
                                lioDocumentUser.ivdblImporteIva += lioUxDocumentIva.ivdblImporte;
                                lioDocumentUser.coIvas.Add(lioUxDocumentIva);
                                livnroI++;
                            }
                            break;
                        case "coOpcionales":
                            for (short livnumOpcional = 1; livnumOpcional <= 10; livnumOpcional++)
                            {
                                if (lioDocumentUser.coOpcionales == null)
                                    lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                                foreach (JToken lioO in lioJObject.SelectTokens(lioMapperItem.coXPaths[0].ivstrData).Children())
                                {

                                    lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOpcionales.ivstrId");
                                    if (lioMapperItem1 != null)
                                    {
                                        livstr = $"{lioMapperItem1.coXPaths[0].ivstrData}{livnumOpcional}";
                                        if ((lioO.SelectToken("name")?.ToString() ?? string.Empty) != livstr)
                                            continue;
                                        if (lioUxDocumentOpcional == null)
                                            lioUxDocumentOpcional = new UxDocumentOpcional();
                                        lioUxDocumentOpcional.ivstrId = lioO.SelectToken("text")?.ToString();
                                        if (!string.IsNullOrEmpty(lioUxDocumentOpcional.ivstrValor))
                                            lioDocumentUser.coOpcionales.Add(lioUxDocumentOpcional);
                                        lioUxDocumentOpcional = null;
                                    }
                                    lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOpcionales.ivstrValor");
                                    if (lioMapperItem1 != null)
                                    {
                                        livstr = $"{lioMapperItem1.coXPaths[0].ivstrData}{livnumOpcional}";
                                        if ((lioO.SelectToken("name")?.ToString() ?? string.Empty) != livstr)
                                            continue;
                                        if (lioUxDocumentOpcional == null)
                                            lioUxDocumentOpcional = new UxDocumentOpcional();
                                        lioUxDocumentOpcional.ivstrValor = lioO.SelectToken("text")?.ToString();
                                        if (!string.IsNullOrEmpty(lioUxDocumentOpcional.ivstrId))
                                            lioDocumentUser.coOpcionales.Add(lioUxDocumentOpcional);
                                        lioUxDocumentOpcional = null;
                                    }
                                }
                            }
                            break;
                        case "coItems":
                            livnroI = 1;
                            if (lioDocumentUser.coItems == null)
                                lioDocumentUser.coItems = new List<UxDocumentItem>();
                            foreach (JToken lioO in lioJObject.SelectTokens(lioMapperItem.coXPaths[0].ivstrData).Children())
                            {
                                lioUxDocumentItem = new UxDocumentItem();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivstrId");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Linea Detalle {livnroI}: Id no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentItem.ivstrId = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToString();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivstrDescripcion");
                                if (lioMapperItem1 == null)
                                {
                                    lioSbErrors.AppendLine($"Linea Detalle {livnroI}: Descripcion no encontrado o con formato erroneo");
                                    continue;
                                }
                                lioUxDocumentItem.ivstrDescripcion = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToString();
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblCantidad");
                                if (lioMapperItem1 != null && double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                    lioUxDocumentItem.ivdblCantidad = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblPrecioUnitario");
                                if (lioMapperItem1 != null && double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                    lioUxDocumentItem.ivdblPrecioUnitario = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblBonificaion");
                                if (lioMapperItem1 != null && double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                    lioUxDocumentItem.ivdblBonificaion = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblImporteTotal");
                                if (lioMapperItem1 != null && double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                    lioUxDocumentItem.ivdblImporteTotal = livval;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivnroUM");
                                if (lioMapperItem1 != null && short.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livnro))
                                    lioUxDocumentItem.ivnroUM = lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData)?.ToObject<short>() ?? 0;
                                else
                                    lioUxDocumentItem.ivnroUM = 0;
                                lioMapperItem1 = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblImporteIVA");
                                if (lioMapperItem1 != null && double.TryParse(lioO.SelectToken(lioMapperItem1.coXPaths[0].ivstrData).ToString(), out livval))
                                    lioUxDocumentItem.ivdblImporteIVA = livval;
                                lioDocumentUser.coItems.Add(lioUxDocumentItem);
                                livnroI++;
                            }
                            break;
                    }
                }
                if (lioSbErrors.Length > 0)
                    throw new Exception(lioSbErrors.ToString());
                lioDocumentUser.ivstrLoadErrors = string.Empty;
                return [lioDocumentUser];
            }
            catch (Exception lioEx)
            {
                lioDocumentUser.ivstrLoadErrors = lioEx.Message;
                return [lioDocumentUser];
            }

        }


        public string ToPrint()
        {
            if (string.IsNullOrEmpty(ivstrRaw))
                return string.Empty;
            Cuit lioCuit = new Cuit(mivlngCuit, mioContext,null);
            if (string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                throw new Exception("Mapeador no encontrado o con formato erroneo");
            string livstr = lioCuit.GetEncoding().GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw, lioCuit.GetEncoding()))), livstrExtNode;
            JObject lioJObject = JObject.Parse(livstr);

            livstrExtNode = string.Empty;
            foreach (JToken lioO in lioJObject.SelectTokens("DTE.Extensions.Extension.StringField").Children())
                livstrExtNode += $"<StringField name=\"{lioO.SelectToken("name") ?? "".ToString()}\">{lioO.SelectToken("text") ?? "".ToString()}</StringField>";
            foreach (JToken lioO in lioJObject.SelectTokens("DTE.Extensions.Extension.NumericField").Children())
                livstrExtNode += $"<NumericField name=\"{lioO.SelectToken("name") ?? "".ToString()}\">{lioO.SelectToken("text") ?? "".ToString()}</NumericField>";
            JToken? lioJt = lioJObject.SelectToken("DTE.Extensions.Extension.StringField");
            if (lioJt != null)
                lioJt.Parent?.Remove();
            lioJt = lioJObject.SelectToken("DTE.Extensions.Extension.NumericField");
            if (lioJt != null)
                lioJt.Parent?.Remove();
            livstr = lioJObject.ToString();
            XmlDocument lioXmlDocument = JsonConvert.DeserializeXmlNode(livstr, "Root");
            livstr = $"DTE{Format.GenerateRandomHex(30)}";
            return lioXmlDocument.OuterXml
                .Replace("<Root>", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                .Replace("<DTE>", "<DTE version=\"1.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.afip.com.ar/fe\">")
                .Replace("<Documento>", $"<Documento Id=\"{livstr}\">")
                .Replace("<Extension>", $"<Extension dteId=\"{livstr}\">{livstrExtNode}")
                .Replace("</Root>", string.Empty);
        }
        #endregion
        #region PRIVATE PROPS
        private long mivlngCuit;
        private NatContext mioContext;
        #endregion
    }
}
