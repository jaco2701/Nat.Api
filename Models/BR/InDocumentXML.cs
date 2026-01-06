using Applet.Nat.Api.Static;
using Applet.Nat.Api.Ifaces;
using System.Text;
using System.Xml;
using Applet.Nat.Api.DC;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Protocols.WsTrust;
using Nat.API.Properties;
namespace Applet.Nat.Api.Br.Models
{

    public class InDocumentXML : IRawDocument
    {
        #region CONSTRUCTORS
        public InDocumentXML() { }
        public InDocumentXML(long vivlngCuit, NatContext vioContext)
        {
            mivlngCuit = vivlngCuit;
            mioContext = vioContext;
        }
        #endregion
        #region PUBLIC PROPS
        public string? ivstrRaw { get; set; }
        public string? ivstrName { get; set; }
        public string ivstrKey { get; set; }
        #endregion
        #region PUBLIC METHODS
        public DocumentUser[] GetDocuments()
        {
            DocumentUser lioDocumentUser = new DocumentUser();
            try
            {
                if (string.IsNullOrEmpty(ivstrRaw)) return [lioDocumentUser];
                string livstr = Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty,Encoding.UTF8))),
                    livstrPath,
                    livstrXmlDtmFormat = ListHelper.GetValue("FORMAT", "XmlDtm", mioContext),
                    livstrApiDtmFormat = ListHelper.GetValue("FORMAT", "ApiDtm", mioContext);
                StringBuilder lioSbErrors = new StringBuilder();
                XmlDocument mioXmlDocument = new XmlDocument();
                mioXmlDocument.LoadXml(livstr);
                XmlNamespaceManager lioNsMngr = new XmlNamespaceManager(mioXmlDocument.NameTable);
                lioNsMngr.AddNamespace("ns", "http://www.afip.com.ar/fe");
                //SERVICIO
                XmlNode xmlDocumentNode = mioXmlDocument.SelectSingleNode("//ns:DTE/ns:Documento/ns:Encabezado/ns:IdDoc/ns:WSRegimen", lioNsMngr);
                if (xmlDocumentNode == null || string.IsNullOrEmpty(xmlDocumentNode.InnerXml))
                    throw new Exception($"Servicio {Resources.lioE_ObjectNoM}");
                lioDocumentUser.ivstrWs = xmlDocumentNode.InnerXml;
                lioDocumentUser.ivstrInputData = ivstrRaw;
                //MAPEADOR               
                Cuit lioCuit = new Cuit(mivlngCuit, mioContext, null);
                if (string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                    throw new Exception($"Mapeador {Resources.lioE_ObjectNoM}");
                ServiceMapper lioMapper = lioCuit.ioCnfg?.coServiceMappers.FirstOrDefault(x => x.ivstrInputType=="xml" && x.ivstrWs == lioDocumentUser.ivstrWs);
                if (lioMapper == null || lioMapper.coItems == null)
                    throw new Exception($"Mapeador {Resources.lioE_ObjectNoM}");
                xmlDocumentNode = mioXmlDocument.SelectSingleNode("//ns:DTE/ns:Documento", lioNsMngr);
                short livnro;
                long livlng = 0;
                double livval=0;
                DateTime livdtm;
                int livnum;
                System.Xml.XmlNode lioxmlNodeField;
                livstr = string.Empty;
                livlng = 0;
                livdtm = DateTime.MinValue;
                livnro = 0;
                livnum = 0;
                #region Encabezado
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroTipoDoc")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioSbErrors.AppendLine($"Tipo de comprobante {Resources.lioE_ObjectNoM}");
                    else
                    lioDocumentUser.ivnroTipoDoc = livnro;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnumPvta")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !int.TryParse(lioxmlNodeField.InnerXml, out livnum))
                        lioSbErrors.AppendLine($"Punto de venta {Resources.lioE_ObjectNoM}");
                    else
                        lioDocumentUser.ivnumPvta = livnum;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivlngCbte")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                        lioSbErrors.AppendLine($"Numero de comprobante {Resources.lioE_ObjectNoM}");
                    else
                        lioDocumentUser.ivlngCbte = livlng;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrFechaEmision")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !DateTime.TryParseExact(lioxmlNodeField.InnerXml.Replace("T00:00:00", string.Empty), livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                        lioSbErrors.AppendLine($"Fecha de Comprobante INVALIDA ({livstrXmlDtmFormat}");
                    else
                        lioDocumentUser.ivstrFechaEmision = livdtm.ToString(livstrApiDtmFormat);
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrCondPago")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null)
                        lioDocumentUser.ivstrCondPago = string.Empty;
                    else
                        lioDocumentUser.ivstrCondPago = lioxmlNodeField.InnerXml;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrFechaVtopago")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null && !String.IsNullOrEmpty(lioxmlNodeField.InnerXml))
                    {
                        if (!DateTime.TryParseExact(lioxmlNodeField.InnerXml, livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                            lioSbErrors.AppendLine($"FECHA de Vencimiento de Pago {Resources.lioE_ObjectNoF} ({livstrXmlDtmFormat})");
                        else
                            lioDocumentUser.ivstrFechaVtopago = livdtm.ToString(livstrApiDtmFormat);
                    }
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrMoneda")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null)
                        lioSbErrors.AppendLine($"Moneda {Resources.lioE_ObjectNoF}");
                    else
                        lioDocumentUser.ivstrMoneda = lioxmlNodeField.InnerXml;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroConcepto")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioSbErrors.AppendLine($"Concepto {Resources.lioE_ObjectNoM}");
                    else
                        lioDocumentUser.ivnroConcepto = livnro;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrFechaServdesde")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null && !String.IsNullOrEmpty(lioxmlNodeField.InnerXml))
                    {
                        if (!DateTime.TryParseExact(lioxmlNodeField.InnerXml, livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                            lioSbErrors.AppendLine($"FECHA de Inicio de Servicios {Resources.lioE_ObjectNoF} ({livstrXmlDtmFormat})");
                        else
                            lioDocumentUser.ivstrFechaServdesde = livdtm.ToString(livstrApiDtmFormat); ;
                    }
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrFechaServhasta")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null && !String.IsNullOrEmpty(lioxmlNodeField.InnerXml))
                    {
                        if (!DateTime.TryParseExact(lioxmlNodeField.InnerXml, livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                            lioSbErrors.AppendLine($"FECHA de Finalizacion de Servicios {Resources.lioE_ObjectNoF} ({livstrXmlDtmFormat})");
                        else
                            lioDocumentUser.ivstrFechaServhasta = livdtm.ToString(livstrApiDtmFormat);
                    }
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivdblCotizacion")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null)
                        lioDocumentUser.ivdblCotizacion = 1;
                    else if (!double.TryParse(lioxmlNodeField.InnerXml, out livval))
                        lioSbErrors.AppendLine($"Cotizacion {Resources.lioE_ObjectNoF} ");
                    else
                        lioDocumentUser.ivdblCotizacion = livval;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrCanMisMonExt")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrCanMisMonExt = lioxmlNodeField.InnerXml;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrObs")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrObs = lioxmlNodeField.InnerXml;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrIdCliente")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrIdCliente = lioxmlNodeField.InnerXml;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrIdSucursal")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrIdSucursal = lioxmlNodeField.InnerXml;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrRazonSocial")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrRazonSocial = lioxmlNodeField.InnerXml;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrIncoterms")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrIncoterms = lioxmlNodeField.InnerXml;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrIncotermsDs")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrIncotermsDs = lioxmlNodeField.InnerXml;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroIdioma")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        livnro = 1; 
                    lioDocumentUser.ivnroIdioma = livnro;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroTipoExpo")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioSbErrors.AppendLine($"Tipo Exportacion {Resources.lioE_ObjectNoF}");
                    else
                        lioDocumentUser.ivnroTipoExpo = livnro;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroDestinoCmp")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioSbErrors.AppendLine($"Pais Destino {Resources.lioE_ObjectNoM}");
                    else
                        lioDocumentUser.ivnroDestinoCmp = livnro;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivlngCuitPaisCliente")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                        lioSbErrors.AppendLine($"Cuit Pais Cliente {Resources.lioE_ObjectNoF}");
                    else
                        lioDocumentUser.ivlngCuitPaisCliente = livlng;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivlngIDImpositivo")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                        lioSbErrors.AppendLine($"Id Impositivo {Resources.lioE_ObjectNoF}");
                    else
                        lioDocumentUser.ivlngIDImpositivo = livlng;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrPermisoExistente")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null)
                        lioDocumentUser.ivstrPermisoExistente = lioxmlNodeField.InnerXml;
                }
                #endregion
                #region Emisor
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivlngCuitEmisor")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                        lioSbErrors.AppendLine($"CUIT Emisor {Resources.lioE_ObjectNoM}");
                    else if (livlng != mivlngCuit)
                        lioSbErrors.AppendLine($"CUIT Emisor {Resources.lioE_ObjectNoF}");
                    else
                        lioDocumentUser.ivlngCuitEmisor = livlng;
                }
                #endregion
                #region Receptor
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivlngDocReceptor")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                        lioSbErrors.AppendLine($"Numero de documento receptor {Resources.lioE_ObjectNoM}");
                    lioDocumentUser.ivlngDocReceptor = livlng;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroTipoDocReceptor")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioSbErrors.AppendLine($"Tipo de documento receptor {Resources.lioE_ObjectNoM}");
                    lioDocumentUser.ivnroTipoDocReceptor = livnro;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrRazonSocial")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null)
                        lioSbErrors.AppendLine($"Razon Social Receptor {Resources.lioE_ObjectNoF}");
                    lioDocumentUser.ivstrRazonSocial = lioxmlNodeField.InnerXml;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroTipoRespReceptor")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioSbErrors.AppendLine($"Condicion Iva receptor {Resources.lioE_ObjectNoF}");
                    lioDocumentUser.ivnroTipoRespReceptor = livnro;
                }
                //
                lioDocumentUser.ioDomicilioReceptor = new UxDomicilio();
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrCalle")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrCalle = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : string.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrNro")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrNro = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrPiso")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrPiso = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrDepto")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrDepto = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrCuidad")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrCuidad = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrMunicipio")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrMunicipio = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivnroPcia")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField != null && !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                        lioDocumentUser.ioDomicilioReceptor.ivnroPcia = livnro;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrPais")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrPais = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ioDomicilioReceptor.ivstrCP")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ioDomicilioReceptor.ivstrCP = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivstrEmail")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    lioDocumentUser.ivstrEmail = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                }
                //
                #endregion
                #region Importes
                //
                livstr = String.Empty;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivdblImporteGravado")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                        lioSbErrors.AppendLine($"Importe neto gravado {Resources.lioE_ObjectNoM} ");
                    else
                        lioDocumentUser.ivdblImporteGravado = livval;
                }
                livstr = String.Empty;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivdblImporteNoGravado")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                        lioSbErrors.AppendLine($"Importe neto no gravado {Resources.lioE_ObjectNoM} ");
                    else
                        lioDocumentUser.ivdblImporteNoGravado = livval;
                }
                livstr = String.Empty;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivdblImporteExento")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                        lioSbErrors.AppendLine($"Importe Exento {Resources.lioE_ObjectNoF} ");
                    else
                        lioDocumentUser.ivdblImporteExento = livval;
                }
                livstr = String.Empty;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivdblImporteTotal")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioxmlNodeField = xmlDocumentNode.SelectSingleNode(livstrPath, lioNsMngr);
                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                        lioSbErrors.AppendLine($"Importe total {Resources.lioE_ObjectNoF} ");
                    else
                        lioDocumentUser.ivdblImporteTotal = livval;
                }
                #endregion
                #region Asociados
                livstr = String.Empty;
                short livnroI = 1;
                XmlNodeList lioXmlNodeList;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioXmlNodeList = xmlDocumentNode.SelectNodes(livstrPath, lioNsMngr);
                    if (lioXmlNodeList != null && lioXmlNodeList.Count > 0)
                    {
                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                        foreach (System.Xml.XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            if (!String.IsNullOrEmpty(lioXmlNode.InnerXml))
                            {
                                UxDocumentAsociado lioUxDocumentAsociado = new UxDocumentAsociado();
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivnumCbtePuntovta")?.coXPaths[0].ivstrData;
                                lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                                    lioSbErrors.AppendLine($"Punto de venta de Comprobante Asociado {livnroI} {Resources.lioE_ObjectNoM}");
                                else
                                    lioUxDocumentAsociado.ivnumCbtePuntovta = livnro;
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivnroCbtetipo")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                                        lioSbErrors.AppendLine($" Tipo de comprobante de Comprobante Asociado {livnroI} {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentAsociado.ivnroCbtetipo = livnro;
                                }
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivlngCbteNro")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                                        lioSbErrors.AppendLine($"Numero de Comprobante Asociado {livnroI} {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentAsociado.ivlngCbteNro = livlng;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivlngCbteCUIT")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField != null)
                                    {
                                        if (!long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                                            lioSbErrors.AppendLine($"CUIT de Comprobante Asociado {livnroI} {Resources.lioE_ObjectNoM}");
                                        else
                                            lioUxDocumentAsociado.ivlngCbteCUIT = livlng;
                                    }
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados.ivstrFechaEmision")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField != null && !String.IsNullOrEmpty(lioxmlNodeField.InnerXml))
                                    {
                                        if (!DateTime.TryParseExact(lioxmlNodeField.InnerXml, livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                                            lioSbErrors.AppendLine($"FECHA de Emision Comprobante Asociado {livnroI} {Resources.lioE_ObjectNoF} ({livstrXmlDtmFormat})");
                                        else
                                            lioUxDocumentAsociado.ivstrFechaEmision = livdtm.ToString(livstrApiDtmFormat);
                                    }
                                }
                                lioDocumentUser.coAsociados.Add(lioUxDocumentAsociado);
                                livnroI++;
                            }
                        }
                    }
                }
                #endregion
                #region Tributos
                livnroI = 1;
                lioDocumentUser.ivdblImporteOtrosTributos = 0;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos")?.coXPaths[0].ivstrData;
                lioXmlNodeList = xmlDocumentNode.SelectNodes(livstrPath, lioNsMngr);
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    if (lioXmlNodeList != null && lioXmlNodeList.Count > 0)
                    {
                        lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            if (!String.IsNullOrEmpty(lioXmlNode.InnerXml))
                            {
                                UxDocumentOtroTributo lioUxDocumentOtroTributo = new UxDocumentOtroTributo();
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivnroId")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                                        lioSbErrors.AppendLine($"Id Tributo {livnroI} {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentOtroTributo.ivnroId = livnro;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivstrDesc")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    lioUxDocumentOtroTributo.ivstrDesc = (lioxmlNodeField != null) ? lioxmlNodeField.InnerXml : String.Empty;
                                }
                                //
                                livstr = String.Empty;
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivdblBaseImp")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Base Imponible Tributo {livnroI} {Resources.lioE_ObjectNoF}");
                                    else
                                        lioUxDocumentOtroTributo.ivdblBaseImponible = livval;
                                }
                                //
                                livstr = String.Empty;
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivdblAlicuota")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Alicuota Tributo {livnroI} {Resources.lioE_ObjectNoF}");
                                    else
                                        lioUxDocumentOtroTributo.ivdblAlicuota = livval;
                                }
                                //
                                livstr = String.Empty;
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos.ivdblImporte")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Importe Tributo {livnroI} {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentOtroTributo.ivdblImporte = livval;
                                }
                                //
                                lioDocumentUser.ivdblImporteOtrosTributos += lioUxDocumentOtroTributo.ivdblImporte;
                                lioDocumentUser.coOtrosTributos.Add(lioUxDocumentOtroTributo);
                                livnroI++;
                            }
                        }
                    }
                }
                lioDocumentUser.ivdblImporteOtrosTributos = double.Round(lioDocumentUser.ivdblImporteOtrosTributos??0, 2);
                #endregion
                #region Alicuota
                livnroI = 1;
                lioDocumentUser.ivdblImporteIva = 0;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas")?.coXPaths[0].ivstrData;
                    lioXmlNodeList = xmlDocumentNode.SelectNodes(livstrPath, lioNsMngr);
                    if (lioXmlNodeList != null && lioXmlNodeList.Count > 0)
                    {
                        lioDocumentUser.coIvas = new List<UxDocumentIva>();
                        foreach (System.Xml.XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            if (!String.IsNullOrEmpty(lioXmlNode.InnerXml))
                            {
                                UxDocumentIva lioUxDocumentIva = new UxDocumentIva();
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas.ivnroTipo")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                                        lioSbErrors.AppendLine($"Alicuota {livnroI} Id IVA {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentIva.ivnroTipo = livnro;
                                }
                                //
                                livstr = String.Empty;
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas.ivdblBaseImponible")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Alicuota {livnroI} Base Imponible IVA {Resources.lioE_ObjectNoF}");
                                    else
                                        lioUxDocumentIva.ivdblBaseImponible = livval;
                                }
                                //
                                livstr = String.Empty;
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas.ivdblImporte")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Alicuota {livnroI}: Importe IVA {Resources.lioE_ObjectNoM}");
                                    else
                                    {
                                        lioUxDocumentIva.ivdblImporte = livval;
                                        lioDocumentUser.ivdblImporteIva += lioUxDocumentIva.ivdblImporte;
                                    }
                                }
                                //
                                lioDocumentUser.coIvas.Add(lioUxDocumentIva);
                                livnroI++;
                            }
                        }
                    }
                }
                #endregion
                #region Opcionales
                livnroI = 1;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOpcionales")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioXmlNodeList = xmlDocumentNode.SelectNodes(livstrPath, lioNsMngr);
                    if (lioXmlNodeList != null && lioXmlNodeList.Count > 0)
                    {
                        UxDocumentOpcional lioUxDocumentOpcional = null;
                        lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                        while (livnroI < 10)
                        {
                            foreach (XmlNode lioXmlNode in lioXmlNodeList)
                            {
                                if (!String.IsNullOrEmpty(lioXmlNode.InnerXml))
                                {
                                    livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOpcionales.ivstrId")?.coXPaths[0].ivstrData;
                                    if (!string.IsNullOrEmpty(livstrPath))
                                    {
                                        if (livstrPath.Contains("{"))
                                            livstrPath = string.Format(livstrPath, livnroI);
                                        if (lioXmlNode.Attributes?[0].Value == livstrPath)
                                        {
                                            if (lioUxDocumentOpcional == null)
                                                lioUxDocumentOpcional = new UxDocumentOpcional();
                                            if (string.IsNullOrEmpty(lioUxDocumentOpcional.ivstrId))
                                            {
                                                lioUxDocumentOpcional.ivstrId = lioXmlNode.InnerText;
                                                if (!string.IsNullOrEmpty(lioUxDocumentOpcional.ivstrValor))
                                                {
                                                    if (!lioDocumentUser.coOpcionales.Contains(lioUxDocumentOpcional))
                                                    {
                                                        lioDocumentUser.coOpcionales.Add(lioUxDocumentOpcional);
                                                        lioUxDocumentOpcional = null;
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                    livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOpcionales.ivstrValor")?.coXPaths[0].ivstrData;
                                    if (!string.IsNullOrEmpty(livstrPath))
                                    {
                                        if (livstrPath.Contains("{"))
                                            livstrPath = string.Format(livstrPath, livnroI);
                                        if (lioXmlNode.Attributes?[0].Value == livstrPath)
                                        {
                                            if (lioUxDocumentOpcional == null)
                                                lioUxDocumentOpcional = new UxDocumentOpcional();
                                            if (string.IsNullOrEmpty(lioUxDocumentOpcional.ivstrValor))
                                            {
                                                lioUxDocumentOpcional.ivstrValor = lioXmlNode.InnerText;
                                                if (!string.IsNullOrEmpty(lioUxDocumentOpcional.ivstrId))
                                                {
                                                    if (!lioDocumentUser.coOpcionales.Contains(lioUxDocumentOpcional))
                                                    {
                                                        lioDocumentUser.coOpcionales.Add(lioUxDocumentOpcional);
                                                        lioUxDocumentOpcional = null;
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                }
                            }
                            livnroI++;
                        }
                    }
                }
                #endregion
                #region Compradores
                livnroI = 1;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coCompradores")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioXmlNodeList = xmlDocumentNode.SelectNodes(livstrPath, lioNsMngr);
                    if (lioXmlNodeList != null && lioXmlNodeList.Count > 0)
                    {
                        lioDocumentUser.coCompradores = new List<UxDocumentComprador>();
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            if (!String.IsNullOrEmpty(lioXmlNode.InnerXml))
                            {
                                UxDocumentComprador lioUxDocumentComprador = new UxDocumentComprador();
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coCompradores.ivnroDocTipo")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                                        lioSbErrors.AppendLine($"Comprador {livnroI} Tipo de documento del comprador {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentComprador.ivnroDocTipo = livnro;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coCompradores.ivlngDocNro")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !long.TryParse(lioxmlNodeField.InnerXml, out livlng))
                                        lioSbErrors.AppendLine($"Comprador {livnroI} Nro de documento del comprador {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentComprador.ivlngDocNro = livlng;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coCompradores.ivdblPorcentaje")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Comprador {livnroI} Porcentaje titularidad de compra {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentComprador.ivdblPorcentaje = livval;
                                }
                                //
                                lioDocumentUser.coCompradores.Add(lioUxDocumentComprador);
                                livnroI++;
                            }
                        }
                    }
                }
                #endregion
                #region Detalle
                livnroI = 1;
                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems")?.coXPaths[0].ivstrData;
                if (!string.IsNullOrEmpty(livstrPath))
                {
                    lioXmlNodeList = xmlDocumentNode.SelectNodes(livstrPath, lioNsMngr);
                    if (lioXmlNodeList != null && lioXmlNodeList.Count > 0)
                    {
                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                        foreach (System.Xml.XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            if (!String.IsNullOrEmpty(lioXmlNode.InnerXml))
                            {
                                UxDocumentItem lioUxDocumentItem = new UxDocumentItem();
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivstrId")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null)
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Id {Resources.lioE_ObjectNoM}");
                                    else
                                        lioUxDocumentItem.ivstrId = lioxmlNodeField.InnerXml;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivstrDescripcion")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null)
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Descripcion {Resources.lioE_ObjectNoF}");
                                    else
                                        lioUxDocumentItem.ivstrDescripcion = lioxmlNodeField.InnerXml;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblCantidad")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Cantidad {Resources.lioE_ObjectNoF}");
                                    lioUxDocumentItem.ivdblCantidad = livval;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblPrecioUnitario")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Precio Unitario Bruto {Resources.lioE_ObjectNoM}");
                                    lioUxDocumentItem.ivdblPrecioUnitario = livval;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblBonificaion")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Importe Bonificacion {Resources.lioE_ObjectNoM}");
                                    lioUxDocumentItem.ivdblBonificaion = livval;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblImporteTotal")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Importe Total {Resources.lioE_ObjectNoM}");
                                    lioUxDocumentItem.ivdblImporteTotal = livval;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivnroUM")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !short.TryParse(lioxmlNodeField.InnerXml, out livnro))
                                        livnro = 0;
                                    else
                                        lioUxDocumentItem.ivnroUM = livnro;
                                }
                                //
                                livstrPath = lioMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems.ivdblImporteIVA")?.coXPaths[0].ivstrData;
                                if (!string.IsNullOrEmpty(livstrPath))
                                {
                                    lioxmlNodeField = lioXmlNode.SelectSingleNode(livstrPath, lioNsMngr);
                                    if (lioxmlNodeField == null || !double.TryParse(lioxmlNodeField.InnerXml, out livval))
                                        lioSbErrors.AppendLine($"Linea Detalle {livnroI} Importe IVA Total {Resources.lioE_ObjectNoM}");
                                    lioUxDocumentItem.ivdblImporteIVA = livval;
                                }
                                //
                                lioDocumentUser.coItems.Add(lioUxDocumentItem);
                                livnroI++;
                            }
                        }
                    }
                }
                #endregion
                if (lioSbErrors.Length > 0)
                    throw new Exception(lioSbErrors.ToString());
                lioDocumentUser.ivstrLoadErrors = string.Empty;
                return [lioDocumentUser];
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                lioDocumentUser.ivstrLoadErrors = lioE.Message;
                return [lioDocumentUser];
            }
        }
        public string ToPrint()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw,Encoding.UTF8)));
        }
        #endregion
        #region PRIVATE PROPS
        private long mivlngCuit;
        private NatContext mioContext;
        #endregion  
    }
}

