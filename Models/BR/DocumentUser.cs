using Applet.Nat.Api.Models.BR;
using Newtonsoft.Json;
namespace Applet.Nat.Api.Br.Models
{
    public class DocumentUser
    {
        [JsonProperty("CuitEmisor")] public long? ivlngCuitEmisor { get; set; }  // CUIT del emisor del documento
        [JsonProperty("TipoDocReceptor")] public short? ivnroTipoDocReceptor { get; set; } // Tipo de documento del receptor (ej. CUIT, CUIL, DNI, etc.)
        [JsonProperty("DocReceptor")] public long? ivlngDocReceptor { get; set; } // Número de documento del receptor
        [JsonProperty("TipoRespReceptor")] public short? ivnroTipoRespReceptor { get; set; } // Tipo de responsable del receptor (ej. Responsable Inscripto, Monotributista, etc.)
        [JsonProperty("PuntoVenta")] public int? ivnumPvta { get; set; } // Punto de venta del documento
        [JsonProperty("TipoDocumento")] public short? ivnroTipoDoc { get; set; } // Tipo de documento (ej. Factura A, Factura B, etc.)
        [JsonProperty("NroCbte")] public long? ivlngCbte { get; set; } // Número del comprobante
        [JsonProperty("IdInterno")] public long? ivlngInt { get; set; } // Número interno del documento
        [JsonProperty("Concepto")] public short? ivnroConcepto { get; set; } // Concepto del documento (1: Productos, 2: Servicios, 3: Productos y Servicios)
        [JsonProperty("FechaEmision")] public string? ivstrFechaEmision { get; set; } // Fecha de emisión del documento
        [JsonProperty("FechaServDesde")] public string? ivstrFechaServdesde { get; set; } // Fecha de inicio del servicio (si aplica)
        [JsonProperty("FechaServHasta")] public string? ivstrFechaServhasta { get; set; } // Fecha de fin del servicio (si aplica)
        [JsonProperty("FechaVtoPago")] public string? ivstrFechaVtopago { get; set; } // Fecha de vencimiento del pago
        [JsonProperty("ImporteTotal")] public double? ivdblImporteTotal { get; set; } // Importe total del documento
        [JsonProperty("ImporteGravado")] public double? ivdblImporteGravado { get; set; } // Importe gravado del documento
        [JsonProperty("ImporteNoGravado")] public double? ivdblImporteNoGravado { get; set; } // Importe no gravado del documento
        [JsonProperty("ImporteExento")] public double? ivdblImporteExento { get; set; } // Importe exento del documento
        [JsonProperty("ImporteOtrosTributos")] public double? ivdblImporteOtrosTributos { get; set; } // Importe de otros tributos del documento
        [JsonProperty("ImporteIva")] public double? ivdblImporteIva { get; set; } // Importe total del IVA del documento
        [JsonProperty("Moneda")] public string? ivstrMoneda { get; set; } // Moneda del documento (ej. ARS, USD, etc.)
        [JsonProperty("Cotizacion")] public double? ivdblCotizacion { get; set; } // Cotización de la moneda del documento
        [JsonProperty("RazonSocial")] public string? ivstrRazonSocial { get; set; } // Razón social del receptor  del documento
        [JsonProperty("CondPago")] public string? ivstrCondPago { get; set; } // Condiciones de pago del documento (ej. Contado, Crédito, etc.)
        [JsonProperty("Obs")] public string? ivstrObs { get; set; } // Observaciones del documento
        [JsonProperty("DomicilioReceptor")] public UxDomicilio? ioDomicilioReceptor { get; set; }  // Domicilio del receptor del documento
        [JsonProperty("Ws")] public string? ivstrWs { get; set; } // Web service utilizado para la emisión del documento
        [JsonProperty("IdCliente")] public string? ivstrIdCliente { get; set; } // Identificador interno del cliente asociado al documento
        [JsonProperty("IdSucursal")] public string? ivstrIdSucursal { get; set; } // Identificador interno de la sucursal
        [JsonProperty("DocumentosAsociados")] public List<UxDocumentAsociado>? coAsociados { get; set; } // Lista de documentos asociados (ej. notas de crédito, débito, etc.)
        [JsonProperty("PeriodoAsociado")] public UxPeriodoAsociado ioPeriodoAsociado { get; set; } // Periodo Asociado al documento
        [JsonProperty("OtrosTributos")] public List<UxDocumentOtroTributo>? coOtrosTributos { get; set; } // Lista de otros tributos aplicables al documento
        [JsonProperty("Ivas")] public List<UxDocumentIva>? coIvas { get; set; } // Lista de alícuotas de IVA aplicables al documento
        [JsonProperty("Opcionales")] public List<UxDocumentOpcional>? coOpcionales { get; set; } // Lista de campos opcionales del documento
        [JsonProperty("Adicionales")] public List<UxDocumentAdicional>? coAdicionales { get; set; } // Lista de campos adicionales del documento
        [JsonProperty("Compradores")] public List<UxDocumentComprador>? coCompradores { get; set; } // Lista de compradores del documento (en caso de ser un documento compartido)
        [JsonProperty("Items")] public List<UxDocumentItem>? coItems { get; set; } // Lista de ítems del documento (productos o servicios facturados)
        [JsonProperty("ItemsCT")] public List<UxDocumentItemCT>? coItemsCT { get; set; } // Lista de ítems de tipo "Código de Turismo" (CT) del documento
        [JsonProperty("CanMisMonExt")] public string? ivstrCanMisMonExt { get; set; } // Cantidad de monedas extranjeras utilizadas en el documento
        [JsonProperty("PermisoExistente")] public string? ivstrPermisoExistente { get; set; } // Permiso existente para la emisión del documento (si aplica)
        [JsonProperty("Cliente")] public string? ivstrCliente { get; set; } // Nombre del cliente asociado al documento
        [JsonProperty("CuitPaisCliente")] public long? ivlngCuitPaisCliente { get; set; } // CUIT del país del cliente asociado al documento
        [JsonProperty("ObsComerciales")] public string? ivstrObsComerciales { get; set; } // Observaciones comerciales del documento
        [JsonProperty("Incoterms")] public string? ivstrIncoterms { get; set; } // Incoterms aplicables al documento (ej. FOB, CIF, etc.)
        [JsonProperty("Idioma")] public short? ivnroIdioma { get; set; } // Idioma del documento (ej. Español, Inglés, etc.)
        [JsonProperty("IncotermsDs")] public string? ivstrIncotermsDs { get; set; } // Descripción de los Incoterms aplicables al documento
        [JsonProperty("Email")] public string? ivstrEmail { get; set; } // Email del receptor del documento
        [JsonProperty("TipoExpo")] public short? ivnroTipoExpo { get; set; } // Tipo de exportación del documento (ej. Definitiva, Temporal, etc.)
        [JsonProperty("DestinoCmp")] public short? ivnroDestinoCmp { get; set; } // Destino del comprobante (ej. Mercado Interno, Exportación, etc.)
        [JsonProperty("PermisosExp")] public List<UxDocumentPermisoExp>? coPermisosExp { get; set; } // Lista de permisos de exportación asociados al documento
        [JsonProperty("LoadErrors")] public string? ivstrLoadErrors { get; set; }
        [JsonProperty("InputData")] public string? ivstrInputData { get; set; }
        [JsonProperty("IDImpositivo")] public long? ivlngIDImpositivo { get; set; } // Id Impositivo Expo
        [JsonProperty("CBU")] public string? ivstrCBU { get; set; }
        [JsonProperty("Transferencia")] public string? ivstrTransferencia { get; set; }
        [JsonProperty("Anulacion")] public string? ivstrAnulacion { get; set; }
        [JsonProperty("IdPermisoEmbarque")] public string? ivstrPEId { get; set; }
        [JsonProperty("DestinoMercaderia")] public int? ivnumPEDestMerc { get; set; }
        [JsonIgnore] public bool? ivblnTaxInLines { get; set; }
        [JsonIgnore] public bool? ivblnCalcPermisoExistente { get; set; }
        [JsonIgnore] public UxDocumentIntegracion ioIntegracion { get; set; } // Datos de integración del documento
        public void FormatAmounts()
        {
            short livnro;
            if (coIvas != null && coIvas.Count > 0)
            {
                //agrupacion de codigos de iva
                livnro = 0;
                List<UxDocumentIva> lcoUxDocumentIvas = new List<UxDocumentIva>();
                foreach (UxDocumentIva lioO in coIvas.OrderBy(x => x.ivnroTipo))
                {
                    lioO.ivdblBaseImponible = lioO.ivdblBaseImponible ?? 0;
                    lioO.ivdblImporte = lioO.ivdblImporte ?? 0;
                    if (lioO.ivnroTipo == null || lioO.ivnroTipo == 0) continue;
                    if (livnro != lioO.ivnroTipo)
                    {
                        lcoUxDocumentIvas.Add(
                             new UxDocumentIva
                             {
                                 ivdblBaseImponible = 0,
                                 ivnroTipo = lioO.ivnroTipo,
                                 ivdblImporte = 0
                             }
                        );
                        livnro = lioO.ivnroTipo ?? 0;
                    }
                    lcoUxDocumentIvas.Last().ivdblImporte += lioO.ivdblImporte;
                    lcoUxDocumentIvas.Last().ivdblBaseImponible += lioO.ivdblBaseImponible;
                }
                coIvas = lcoUxDocumentIvas;
            }
            if (coOtrosTributos != null && coOtrosTributos.Count > 0)
            {
                //agrupacion de codigos de otros tributos
                string livstrJurisdiccion = "999999999";
                List<UxDocumentOtroTributo> lcoOtrosTributos = new List<UxDocumentOtroTributo>();
                foreach (UxDocumentOtroTributo lioO in coOtrosTributos.OrderBy(x => x.ivstrJurisdiccion))
                {
                    lioO.ivdblBaseImponible = lioO.ivdblBaseImponible ?? 0;
                    lioO.ivdblAlicuota = lioO.ivdblAlicuota ?? 0;
                    lioO.ivdblImporte = lioO.ivdblImporte ?? 0;
                    if (lioO.ivnroId == null || lioO.ivnroId == 0) continue;
                    if (livstrJurisdiccion != lioO.ivstrJurisdiccion)
                    {
                        lcoOtrosTributos.Add(
                             new UxDocumentOtroTributo
                             {
                                 ivdblAlicuota = lioO.ivdblAlicuota ?? 0,
                                 ivdblBaseImponible = 0,
                                 ivdblImporte = 0,
                                 ivnroId = lioO.ivnroId,
                                 ivstrDesc = lioO.ivstrDesc,
                                 ivstrJurisdiccion = lioO.ivstrJurisdiccion
                             }
                        );
                        livstrJurisdiccion = lioO.ivstrJurisdiccion ?? string.Empty;
                    }
                    lcoOtrosTributos.Last().ivdblBaseImponible += lioO.ivdblBaseImponible ?? 0;
                    lcoOtrosTributos.Last().ivdblImporte += lioO.ivdblImporte ?? 0;
                }
                coOtrosTributos = lcoOtrosTributos;
            }
            if (ivblnTaxInLines ?? false)
            {
                // Obtension de montos desde los impuestos
                if (coIvas != null && coIvas.Count > 0)
                {
                    ivdblImporteNoGravado = 0;
                    ivdblImporteGravado = 0;
                    ivdblImporteExento = 0;
                    ivdblImporteOtrosTributos = 0;
                    ivdblImporteIva = 0;
                    foreach (UxDocumentIva lioO in coIvas.OrderBy(x => x.ivnroTipo))
                    {
                        switch (lioO.ivnroTipo)
                        {
                            case 1:
                                ivdblImporteNoGravado += lioO.ivdblBaseImponible ?? 0;
                                break;
                            case 2:
                                ivdblImporteExento += lioO.ivdblBaseImponible ?? 0;
                                break;
                            default:
                                ivdblImporteGravado += lioO.ivdblBaseImponible ?? 0;
                                ivdblImporteIva += lioO.ivdblImporte ?? 0;
                                break;
                        }
                    }
                }
                if (coOtrosTributos != null && coOtrosTributos.Count > 0)
                    foreach (UxDocumentOtroTributo lioO in coOtrosTributos)
                        ivdblImporteOtrosTributos += lioO.ivdblImporte ?? 0;
            }
            //redondeos y valor absoluto
            ivdblImporteTotal = Math.Abs(Math.Round(ivdblImporteTotal ?? 0, 2));
            ivdblImporteGravado = Math.Abs(Math.Round(ivdblImporteGravado ?? 0, 2));
            ivdblImporteNoGravado = Math.Abs(Math.Round(ivdblImporteNoGravado ?? 0, 2));
            ivdblImporteExento = Math.Abs(Math.Round(ivdblImporteExento ?? 0, 2));
            ivdblImporteOtrosTributos = Math.Abs(Math.Round(ivdblImporteOtrosTributos ?? 0, 2));
            ivdblImporteIva = Math.Abs(Math.Round(ivdblImporteIva ?? 0, 2));
            //
            if (coIvas != null && coIvas.Count > 0)
                foreach (UxDocumentIva lioO in coIvas)
                {
                    lioO.ivdblBaseImponible = Math.Abs(Math.Round(lioO.ivdblBaseImponible ?? 0, 2));
                    lioO.ivdblImporte = Math.Abs(Math.Round(lioO.ivdblImporte ?? 0, 2));
                }
            //
            if (coOtrosTributos != null && coOtrosTributos.Count > 0)
                foreach (UxDocumentOtroTributo lioO in coOtrosTributos)
                {
                    lioO.ivdblBaseImponible = Math.Abs(Math.Round(lioO.ivdblBaseImponible ?? 0, 2));
                    lioO.ivdblAlicuota = Math.Abs(Math.Round(lioO.ivdblAlicuota ?? 0, 2));
                    lioO.ivdblImporte = Math.Abs(Math.Round(lioO.ivdblImporte ?? 0, 2));
                }
            //
            if (coItems != null && coItems.Count > 0)
                foreach (UxDocumentItem lioO in coItems)
                {
                    lioO.ivdblCantidad = Math.Round(lioO.ivdblCantidad ?? 0, 6);
                    lioO.ivdblImporteIVA = Math.Round(lioO.ivdblImporteIVA ?? 0, 2);
                    lioO.ivdblImporteTotal = Math.Round(lioO.ivdblImporteTotal ?? 0, 2);
                    lioO.ivdblPrecioUnitario = Math.Abs(Math.Round(lioO.ivdblPrecioUnitario ?? 0, 6));
                    if (new short[] { 97, 99 }.Contains(lioO.ivnroUM ?? 0))
                    {
                        if (lioO.ivdblCantidad > 0)
                            lioO.ivdblCantidad = -lioO.ivdblCantidad;
                        if (lioO.ivdblImporteIVA > 0)
                            lioO.ivdblImporteIVA = -lioO.ivdblImporteIVA;
                        if (lioO.ivdblImporteTotal > 0)
                            lioO.ivdblImporteTotal = -lioO.ivdblImporteTotal;
                    }
                    else
                    {
                        lioO.ivdblCantidad = Math.Abs(lioO.ivdblCantidad ?? 0);
                        lioO.ivdblImporteIVA = Math.Abs(lioO.ivdblImporteIVA ?? 0);
                        lioO.ivdblImporteTotal = Math.Abs(lioO.ivdblImporteTotal ?? 0);
                    }
                }
            // inversion de signo para notas de credito
            //if (new short[] { 3, 8, 13, 21, 203, 208 }.Contains(ivnroTipoDoc ?? 0))
            //{
            //    ivdblImporteTotal = -(ivdblImporteTotal ?? 0);
            //    ivdblImporteGravado = -(ivdblImporteGravado ?? 0);
            //    ivdblImporteNoGravado = -(ivdblImporteNoGravado ?? 0);
            //    ivdblImporteExento = -(ivdblImporteExento ?? 0);
            //    ivdblImporteOtrosTributos = -(ivdblImporteOtrosTributos ?? 0);
            //    ivdblImporteIva = -(ivdblImporteIva ?? 0);
            //    if (coIvas != null && coIvas.Count > 0)
            //        foreach (UxDocumentIva lioO in coIvas)
            //        {
            //            lioO.ivdblBaseImponible = -lioO.ivdblBaseImponible;
            //            lioO.ivdblImporte = -lioO.ivdblImporte;
            //        }
            //    if (coOtrosTributos != null && coOtrosTributos.Count > 0)
            //        foreach (UxDocumentOtroTributo lioO in coOtrosTributos)
            //        {
            //            lioO.ivdblBaseImponible = -lioO.ivdblBaseImponible;
            //            lioO.ivdblAlicuota = -lioO.ivdblAlicuota;
            //            lioO.ivdblImporte = -lioO.ivdblImporte;
            //        }
            //    if (coItems != null && coItems.Count > 0)
            //        foreach (UxDocumentItem lioO in coItems)
            //        {
            //            if (new short[] { 97, 99 }.Contains(lioO.ivnroUM ?? 0))
            //            {
            //                lioO.ivdblCantidad = -lioO.ivdblCantidad;
            //                lioO.ivdblImporteIVA = -lioO.ivdblImporteIVA;
            //                lioO.ivdblImporteTotal = -lioO.ivdblImporteTotal;
            //            }
            //            else
            //            {
            //                lioO.ivdblCantidad = Math.Abs(lioO.ivdblCantidad ?? 0);
            //                lioO.ivdblImporteIVA = Math.Abs(lioO.ivdblImporteIVA ?? 0);
            //                lioO.ivdblImporteTotal = Math.Abs(lioO.ivdblImporteTotal ?? 0);
            //            }
            //            lioO.ivdblPrecioUnitario = Math.Abs(lioO.ivdblPrecioUnitario ?? 0);
            //        }
            //}
        }
    }
    public class UxDomicilio
    {
        [JsonProperty("Calle")]
        public string? ivstrCalle { get; set; } // Calle del domicilio
        [JsonProperty("Nro")]
        public string? ivstrNro { get; set; } // Número del domicilio
        [JsonProperty("Piso")]
        public string? ivstrPiso { get; set; } // Piso del domicilio
        [JsonProperty("Depto")]
        public string? ivstrDepto { get; set; } // Depto del domicilio
        [JsonProperty("Cuidad")]
        public string? ivstrCuidad { get; set; } // Ciudad del domicilio
        [JsonProperty("Municipio")]
        public string? ivstrMunicipio { get; set; } // Municipio del domicilio
        [JsonProperty("Pcia")]
        public short? ivnroPcia { get; set; } // Provincia del domicilio (código numérico)
        [JsonProperty("Pais")]
        public string? ivstrPais { get; set; } // País del domicilio (código ISO 3166-1 alpha-2)
        [JsonProperty("CP")]
        public string? ivstrCP { get; set; }// Código postal del domicilio
        [JsonProperty("Referencia")]
        public string? ivstrReferencia { get; set; } // Referencia adicional del domicilio (opcional)
    }
    public class UxDocumentAsociado
    {
        [JsonProperty("TipoDoc")] public short? ivnroCbtetipo { get; set; } // Tipo de documento asociado (ej. Factura A, B, C, etc.)
        [JsonProperty("PuntoVenta")] public int? ivnumCbtePuntovta { get; set; } // Punto de venta del documento asociado
        [JsonProperty("NroCbte")] public long? ivlngCbteNro { get; set; } // Número del documento asociado
        [JsonProperty("CbteCUIT")] public long? ivlngCbteCUIT { get; set; } // CUIT del emisor del documento asociado
        [JsonProperty("FechaEmision")] public string? ivstrFechaEmision { get; set; } // Fecha de emisión del documento asociado
    }
    public class UxDocumentOtroTributo
    {
        [JsonProperty("idTributo")] public short? ivnroId { get; set; } // Id del otro tributo
        [JsonProperty("DescTributo")] public string? ivstrDesc { get; set; } // Descripción del otro tributo
        [JsonProperty("BaseImponible")] public Double? ivdblBaseImponible { get; set; }  // Base imponible del otro tributo
        [JsonProperty("Alicuota")] public Double? ivdblAlicuota { get; set; } // Alícuota del otro tributo
        [JsonProperty("Importe")] public Double? ivdblImporte { get; set; } // Importe total del otro tributo
        [JsonProperty("Jurisdiccion")] public string? ivstrJurisdiccion { get; set; } // Id del otro tributo

    }
    public class UxDocumentIva
    {
        [JsonProperty("Tipo")] public short? ivnroTipo { get; set; } // Tipo de IVA (ej. IVA General, IVA Reducido, etc.)
        [JsonProperty("BaseImponible")] public Double? ivdblBaseImponible { get; set; } // Base imponible sobre la cual se calcula el IVA
        [JsonProperty("Importe")] public Double? ivdblImporte { get; set; } // Importe total del IVA calculado
    }
    public class UxDocumentOpcional
    {
        [JsonProperty("Id")] public string? ivstrId { get; set; } // Identificador del campo opcional
        [JsonProperty("Valor")] public string? ivstrValor { get; set; } // Valor del campo opcional
    }
    public class UxDocumentAdicional
    {
        [JsonProperty("Tipo")] public short? ivnroTipo { get; set; } // Tipo de campo adicional (ej. Texto, Numérico, Fecha, etc.)
        [JsonProperty("Valor1")] public string? ivstrValor1 { get; set; }    // Valor del primer campo adicional
        [JsonProperty("Valor2")] public string? ivstrValor2 { get; set; }    // Valor del segundo campo adicional
        [JsonProperty("Valor3")] public string? ivstrValor3 { get; set; }   // Valor del tercer campo adicional
        [JsonProperty("Valor4")] public string? ivstrValor4 { get; set; }    // Valor del cuarto campo adicional
        [JsonProperty("Valor5")] public string? ivstrValor5 { get; set; }   // Valor del quinto campo adicional
    }
    public class UxDocumentComprador
    {
        [JsonProperty("Tipo")] public short? ivnroDocTipo { get; set; } // Tipo de documento del comprador (ej. CUIT, CUIL, DNI, etc.)
        [JsonProperty("Numero")] public long? ivlngDocNro { get; set; } // Número de documento del comprador
        [JsonProperty("Porcentaje")] public Double? ivdblPorcentaje { get; set; }   // Porcentaje de participación del comprador en el documento
    }
    public class UxDocumentItem
    {
        [JsonProperty("Id")] public string? ivstrId { get; set; } // Identificador del ítem
        [JsonProperty("Descripcion")] public string? ivstrDescripcion { get; set; } // Descripción del ítem
        [JsonProperty("Cantidad")] public double? ivdblCantidad { get; set; }  // Cantidad del ítem
        [JsonProperty("PrecioUnitario")] public double? ivdblPrecioUnitario { get; set; } // Precio unitario del ítem
        [JsonProperty("Bonificacion")] public double? ivdblBonificaion { get; set; } // Bonificación aplicada al ítem (si aplica)
        [JsonProperty("UM")] public short? ivnroUM { get; set; } // Unidad de medida del ítem (ej. Kilos, Litros, Unidades, etc.)
        [JsonProperty("ImporteTotal")] public double? ivdblImporteTotal { get; set; }  // Importe total del ítem (Cantidad * Precio Unitario - Bonificación)
        [JsonProperty("TipoIVA")] public short? ivnroTipoIVA { get; set; } // Tipo de IVA aplicable al ítem (ej. IVA General, IVA Reducido, etc.)
        [JsonProperty("ImporteIVA")] public double? ivdblImporteIVA { get; set; } // Importe total del IVA calculado para el ítem
    }
    public class UxDocumentItemCT
    {
        [JsonProperty("Tipo")] public short? ivnroTipo { get; set; } // Tipo de ítem (ej. Producto, Servicio, Código de Turismo, etc.)
        [JsonProperty("CodigoTurismo")] public short? ivnroCodigoTurismo { get; set; } // Código de turismo asociado al ítem (si aplica)
        [JsonProperty("Codigo")] public string? ivstrCodigo { get; set; } // Código del ítem (ej. SKU, Código de barras, etc.)
        [JsonProperty("Desc")] public string? ivstrDesc { get; set; } // Descripción del ítem
        [JsonProperty("TipoIVA")] public short? ivnroTipoIVA { get; set; } // Tipo de IVA aplicable al ítem (ej. IVA General, IVA Reducido, etc.)
        [JsonProperty("ImporteItem")] public double? ivdblImporteItem { get; set; } // Importe total del ítem (Cantidad * Precio Unitario - Bonificación)
    }
    public class UxPeriodoAsociado
    {
        [JsonProperty("FechaDesde")] public string? ivstrFechaDesde { get; set; } // Fecha de inicio del período asociado
        [JsonProperty("FechaHasta")] public string ivstrFechaHasta { get; set; } // Fecha de fin del período asociado
    }
    public class UxDocumentPermisoExp
    {
        [JsonProperty("DestMerc")] public int? ivnumDestMerc { get; set; } // Destino del mercaderia del permiso de exportación (ej. Mercado Interno, Exportación, etc.)
        [JsonProperty("Id")] public string? ivstrId { get; set; } // Identificador del permiso de exportación
    }
    public class UxAuth
    {
        public string? ivstrAuthCode { get; set; }
        public string? ivdtmAuthVenc { get; set; }
        public string? ivstrAuthType { get; set; }
        public string? ivtrStatusDesc { get; set; }
        public string? ivstrErrors { get; set; }
        public string? ivstrObs { get; set; }
        public DateTime? ivdtmNode { get; set; }
        public int? ivnumtrack { get; set; }

    }
    public class UxDocumentIntegracion
    {
        public string? ivstrAttributeCategory { get; set; }
        public string? ivstrEfdKeyNumber { get; set; }
        public string? ivstrEfdKeyDate { get; set; }
        public string? ivstrEfdStatus { get; set; }
        public string? ivstrEfdMessage { get; set; }
        public string? ivstrEfdKeyNumberAtt { get; set; }
        public string? ivstrEfdKeyDateAtt { get; set; }
        public string? ivstrEfdStatusAtt { get; set; }
        public string? ivstrEfdMessageAtt { get; set; }
        public string? ivstrInvoiceNumber { get; set; }
        public string? ivstrInvoiceId { get; set; }
    }

    public class DocumentUploadResponse
    {
        [JsonProperty("Id.Nat")]
        public long? ivlngDoc { get; set; }
        [JsonProperty("CuitEmisor")]
        public long? ivlngCuitEmisor { get; set; }
        [JsonProperty("PuntoDeVenta")]
        public int? ivnumPvta { get; set; }
        [JsonProperty("Tipo")]
        public short? ivnroTipoDoc { get; set; }
        [JsonProperty("Numero")]
        public long? ivlngCbte { get; set; }
        [JsonProperty("Estado")]
        public short? ivnroStatus { get; set; }
        [JsonProperty("Descripcion")]
        public string? ivstrDescStatus { get; set; }
        [JsonProperty("Integracion")]
        public string? ivstrIntegracion { get; set; }
    }
    public class DocumentUploadRequest
    {
        [JsonProperty("Documento")]
        public string? ivstrData { get; set; }
        [JsonProperty("Nombre")]
        public string? ivstrName { get; set; }
        [JsonProperty("Comp")]
        public bool? ivblnComp { get; set; }
        [JsonProperty("CuitEmisor")]
        public long ivlngCuit { get; set; }
    }
}

