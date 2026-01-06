using Newtonsoft.Json;
namespace Applet.Nat.Api.Br.Models
{
    public class DocumentUser
    {
        public long? ivlngCuitEmisor { get; set; }  // CUIT del emisor del documento
        public short? ivnroTipoDocReceptor { get; set; } // Tipo de documento del receptor (ej. CUIT, CUIL, DNI, etc.)
        public long? ivlngDocReceptor { get; set; } // Número de documento del receptor
        public short? ivnroTipoRespReceptor { get; set; } // Tipo de responsable del receptor (ej. Responsable Inscripto, Monotributista, etc.)
        public int? ivnumPvta { get; set; } // Punto de venta del documento
        public short? ivnroTipoDoc { get; set; } // Tipo de documento (ej. Factura A, Factura B, etc.)
        public long? ivlngCbte { get; set; } // Número del comprobante
        public long? ivlngInt { get; set; } // Número interno del documento
        public short? ivnroConcepto { get; set; } // Concepto del documento (1: Productos, 2: Servicios, 3: Productos y Servicios)
        public string? ivstrFechaEmision { get; set; } // Fecha de emisión del documento
        public string? ivstrFechaServdesde { get; set; } // Fecha de inicio del servicio (si aplica)
        public string? ivstrFechaServhasta { get; set; } // Fecha de fin del servicio (si aplica)
        public string? ivstrFechaVtopago { get; set; } // Fecha de vencimiento del pago
        public double? ivdblImporteTotal { get; set; } // Importe total del documento
        public double? ivdblImporteGravado { get; set; } // Importe gravado del documento
        public double? ivdblImporteNoGravado { get; set; } // Importe no gravado del documento
        public double? ivdblImporteExento { get; set; } // Importe exento del documento
        public double? ivdblImporteOtrosTributos { get; set; } // Importe de otros tributos del documento 
        public double? ivdblImporteIva { get; set; } // Importe total del IVA del documento
        public string? ivstrMoneda { get; set; } // Moneda del documento (ej. ARS, USD, etc.)
        public double? ivdblCotizacion { get; set; } // Cotización de la moneda del documento
        public string? ivstrRazonSocial { get; set; } // Razón social del receptor  del documento
        public string? ivstrCondPago { get; set; } // Condiciones de pago del documento (ej. Contado, Crédito, etc.)
        public string? ivstrObs { get; set; } // Observaciones del documento
        public UxDomicilio? ioDomicilioReceptor { get; set; }  // Domicilio del receptor del documento 
        public string? ivstrWs { get; set; } // Web service utilizado para la emisión del documento
        public string? ivstrIdCliente { get; set; } // Identificador interno del cliente asociado al documento
        public string? ivstrIdSucursal { get; set; } // Identificador interno de la sucursal
        public List<UxDocumentAsociado>? coAsociados { get; set; } // Lista de documentos asociados (ej. notas de crédito, débito, etc.)
        public List<UxDocumentOtroTributo>? coOtrosTributos { get; set; } // Lista de otros tributos aplicables al documento
        public List<UxDocumentIva>? coIvas { get; set; } // Lista de alícuotas de IVA aplicables al documento
        public List<UxDocumentOpcional>? coOpcionales { get; set; } // Lista de campos opcionales del documento
        public List<UxDocumentAdicional>? coAdicionales { get; set; } // Lista de campos adicionales del documento
        public List<UxDocumentComprador>? coCompradores { get; set; } // Lista de compradores del documento (en caso de ser un documento compartido)
        public List<UxDocumentItem>? coItems { get; set; } // Lista de ítems del documento (productos o servicios facturados)
        public List<UxDocumentItemCT>? coItemsCT { get; set; } // Lista de ítems de tipo "Código de Turismo" (CT) del documento
        public string? ivstrCanMisMonExt { get; set; } // Cantidad de monedas extranjeras utilizadas en el documento
        public string? ivstrPermisoExistente { get; set; } // Permiso existente para la emisión del documento (si aplica)
        public string? ivstrCliente { get; set; } // Nombre del cliente asociado al documento
        public long? ivlngCuitPaisCliente { get; set; } // CUIT del país del cliente asociado al documento
        public string? ivstrObsComerciales { get; set; } // Observaciones comerciales del documento
        public string? ivstrIncoterms { get; set; } // Incoterms aplicables al documento (ej. FOB, CIF, etc.)
        public short? ivnroIdioma { get; set; } // Idioma del documento (ej. Español, Inglés, etc.)
        public string? ivstrIncotermsDs { get; set; } // Descripción de los Incoterms aplicables al documento
        public string? ivstrEmail { get; set; } // Email del receptor del documento
        public short? ivnroTipoExpo { get; set; } // Tipo de exportación del documento (ej. Definitiva, Temporal, etc.)
        public short? ivnroDestinoCmp { get; set; } // Destino del comprobante (ej. Mercado Interno, Exportación, etc.)
        public List<UxDocumentPermisoExp>? coPermisosExp { get; set; } // Lista de permisos de exportación asociados al documento
        public string? ivstrLoadErrors { get; set; }
        public string? ivstrInputData { get; set; }
        public long? ivlngIDImpositivo { get; set; } // Id Impositivo Expo
        public UxDocumentIntegracion ioIntegracion { get; set; } // Datos de integración del documento
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
        #region Propiedades
        public short? ivnroCbtetipo { get; set; } // Tipo de documento asociado (ej. Factura A, B, C, etc.)
        public int? ivnumCbtePuntovta { get; set; } // Punto de venta del documento asociado
        public long? ivlngCbteNro { get; set; } // Número del documento asociado
        public long? ivlngCbteCUIT { get; set; } // CUIT del emisor del documento asociado
        public string? ivstrFechaEmision { get; set; } // Fecha de emisión del documento asociado
        #endregion
    }
    public class UxDocumentOtroTributo
    {
        #region Propiedades
        public short? ivnroId { get; set; } // Id del otro tributo
        public string? ivstrDesc { get; set; } // Descripción del otro tributo
        public Double? ivdblBaseImponible { get; set; }  // Base imponible del otro tributo
        public Double? ivdblAlicuota { get; set; } // Alícuota del otro tributo
        public Double? ivdblImporte { get; set; } // Importe total del otro tributo
        #endregion
    }
    public class UxDocumentIva
    {
        #region Propiedades
        public short? ivnroTipo { get; set; } // Tipo de IVA (ej. IVA General, IVA Reducido, etc.)
        public Double? ivdblBaseImponible { get; set; } // Base imponible sobre la cual se calcula el IVA
        public Double? ivdblImporte { get; set; } // Importe total del IVA calculado
        #endregion
    }
    public class UxDocumentOpcional
    {
        #region Propiedades
        public string? ivstrId { get; set; } // Identificador del campo opcional
        public string? ivstrValor { get; set; } // Valor del campo opcional
        #endregion
    }
    public class UxDocumentAdicional
    {
        #region Propiedades
        public short? ivnroTipo { get; set; } // Tipo de campo adicional (ej. Texto, Numérico, Fecha, etc.)
        public string? ivstrValor1 { get; set; }    // Valor del primer campo adicional
        public string? ivstrValor2 { get; set; }    // Valor del segundo campo adicional
        public string? ivstrValor3 { get; set; }   // Valor del tercer campo adicional
        public string? ivstrValor4 { get; set; }    // Valor del cuarto campo adicional
        public string? ivstrValor5 { get; set; }   // Valor del quinto campo adicional
        #endregion
    }
    public class UxDocumentComprador
    {
        #region Propiedades
        public short? ivnroDocTipo { get; set; } // Tipo de documento del comprador (ej. CUIT, CUIL, DNI, etc.)
        public long? ivlngDocNro { get; set; } // Número de documento del comprador
        public Double? ivdblPorcentaje { get; set; }   // Porcentaje de participación del comprador en el documento
        #endregion
    }
    public class UxDocumentItem
    {
        #region Propiedades
        public string? ivstrId { get; set; } // Identificador del ítem
        public string? ivstrDescripcion { get; set; } // Descripción del ítem
        public double? ivdblCantidad { get; set; }  // Cantidad del ítem
        public double? ivdblPrecioUnitario { get; set; } // Precio unitario del ítem
        public double? ivdblBonificaion { get; set; } // Bonificación aplicada al ítem (si aplica)
        public short? ivnroUM { get; set; } // Unidad de medida del ítem (ej. Kilos, Litros, Unidades, etc.)
        public double? ivdblImporteTotal { get; set; }  // Importe total del ítem (Cantidad * Precio Unitario - Bonificación)
        public short? ivnroTipoIVA { get; set; } // Tipo de IVA aplicable al ítem (ej. IVA General, IVA Reducido, etc.)
        public double? ivdblImporteIVA { get; set; } // Importe total del IVA calculado para el ítem
        #endregion
    }
    public class UxDocumentItemCT
    {
        #region Propiedades
        public short? ivnroTipo { get; set; } // Tipo de ítem (ej. Producto, Servicio, Código de Turismo, etc.)
        public short? ivnroCodigoTurismo { get; set; } // Código de turismo asociado al ítem (si aplica)
        public string? ivstrCodigo { get; set; } // Código del ítem (ej. SKU, Código de barras, etc.)
        public string? ivstrDesc { get; set; } // Descripción del ítem
        public short? ivnroTipoIVA { get; set; } // Tipo de IVA aplicable al ítem (ej. IVA General, IVA Reducido, etc.)
        public double? ivdblImporteItem { get; set; } // Importe total del ítem (Cantidad * Precio Unitario - Bonificación)
        #endregion
    }
    public class UxDocumentPermisoExp
    {
        public int? ivnumDestMerc { get; set; } // Destino del mercaderia del permiso de exportación (ej. Mercado Interno, Exportación, etc.)
        public string? ivstrId { get; set; } // Identificador del permiso de exportación
    }
    public class UxAuth
    {
        #region Propiedades
        public string? ivstrAuthCode { get; set; }
        public string? ivdtmAuthVenc { get; set; }
        public string? ivstrAuthType { get; set; }
        public string? ivtrStatusDesc { get; set; }
        public string? ivstrErrors { get; set; }
        public string? ivstrObs { get; set; }
        public DateTime? ivdtmNode { get; set; }
        public int? ivnumtrack { get; set; }

        #endregion
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

