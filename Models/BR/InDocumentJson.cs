using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
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
                Cuit lioCuit = new Cuit(mivlngCuit, mioContext, null);
                ServiceMapper lioServiceMapper = GetMapper();
                string livstr = lioCuit.GetEncoding().GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty, lioCuit.GetEncoding()))),
                        livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
                DateTime livdtm;
                StringBuilder lioSbErrors = new StringBuilder();
                List<DocumentUser> lcoDocumentUser = JsonConvert.DeserializeObject<List<DocumentUser>>(livstr);
                short livnroI = 0;
                foreach (DocumentUser lioDocumentUser in lcoDocumentUser)
                {
                    lioDocumentUser.ivstrInputData = Format.Compress(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<DocumentUser> { lioDocumentUser }))));
                    lioDocumentUser.ivblnTaxInLines = lioServiceMapper.ivblnTaxInLines ?? false;
                    lioDocumentUser.ivblnCalcPermisoExistente = lioServiceMapper.ivblnCalcPermisoExistente ?? false;
                    lioDocumentUser.ivstrLoadErrors = string.Empty;
                    if (lioDocumentUser.ivnroTipoDoc == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Tipo de Documento {Resources.lioE_ObjectNoM}");
                    if (lioDocumentUser.ivnumPvta == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Punto de Venta {Resources.lioE_ObjectNoM}");
                    if (lioDocumentUser.ivlngCbte == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Numero de Comprobante {Resources.lioE_ObjectNoM}");
                    if (lioDocumentUser.ivstrFechaEmision == null || !DateTime.TryParseExact(lioDocumentUser.ivstrFechaEmision, livstrApiDtmFormat, null, DateTimeStyles.None, out livdtm))
                        lioSbErrors.AppendLine($"[{livnroI}] Fecha de Comprobante {Resources.lioE_ObjectNoF}");
                    if (lioDocumentUser.ivstrMoneda == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Moneda {Resources.lioE_ObjectNoF}");
                    if (lioDocumentUser.ivlngCuitEmisor == null)
                        lioSbErrors.AppendLine($"[{livnroI}] CUIT Emisor {Resources.lioE_ObjectNoF}");
                    if (lioDocumentUser.ivlngDocReceptor == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Numero de documento receptor {Resources.lioE_ObjectNoM}");
                    if (lioDocumentUser.ivnroTipoDocReceptor == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Tipo de documento receptor {Resources.lioE_ObjectNoM}");
                    if (lioDocumentUser.ivstrRazonSocial == null)
                        lioSbErrors.AppendLine($"[{livnroI}] Razon Social Receptor {Resources.lioE_ObjectNoF}");
                    if (lioSbErrors.Length > 0)
                        lioDocumentUser.ivstrLoadErrors = lioSbErrors.ToString();
                    livnroI++;
                }
                return lcoDocumentUser.ToArray();
        }
        public string ToPrint()
        {
            //DOCUMENTO
            DocumentUser[] lcoDocuments = GetDocuments();
            if (lcoDocuments == null || lcoDocuments.Length == 0)
                throw new Exception($"Documento {Resources.lioE_ObjectNoM}");
            //MAPEADOR
            ServiceMapper lioServiceMapper = GetMapper();
            //XML Crystal
            string livstr = ListHelper.GetValue("PATH", "template", this.mioContext),
                livstrPropertyValue;
            if (!File.Exists($"{livstr}\\print.xml"))
                throw new Exception($"Xml Impresion {Resources.lioE_ObjectNoM}");
            XmlDocument lioXmlToPrinter = new XmlDocument();
            XmlNode lioXmlNodeToPrinter, lioXmlNodeToClone, lioXmlParent;
            using (StreamReader lioRd = new StreamReader($"{livstr}\\print.xml", Encoding.UTF8))
            {
                lioXmlToPrinter.Load(lioRd);
            }
            XmlNamespaceManager lioNsMngr = new XmlNamespaceManager(lioXmlToPrinter.NameTable);
            lioNsMngr.AddNamespace("ns", "http://www.afip.com.ar/fe");
            StringBuilder lioSbErrors = new StringBuilder();
            //CAMPOS
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems)
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty)) continue;
                if (lioMapperItem.coXPaths==null) continue;
                //CAMPOS UNICOS
                switch (lioMapperItem.ivstrProperty.Substring(0, 2))
                {
                    case "iv":
                        livstrPropertyValue = lcoDocuments[0].GetType().GetProperty(lioMapperItem.ivstrProperty)?.GetValue(lcoDocuments[0], null)?.ToString() ?? string.Empty;
                        foreach (ServiceMapperItemXPath lioServiceMapperItemXPath in lioMapperItem.coXPaths)
                        {
                            try
                            {
                                lioXmlNodeToPrinter = lioXmlToPrinter.SelectSingleNode(lioServiceMapperItemXPath.ivstrData, lioNsMngr);
                            }
                            catch (Exception lioE)
                            {
                                lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} xpath {lioServiceMapperItemXPath.ivstrData} {Resources.lioE_ObjectNoM} Error: {lioE.Message}");
                                continue;
                            }
                            if (lioXmlNodeToPrinter == null)
                            {
                                lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} xpath {lioServiceMapperItemXPath.ivstrData} {Resources.lioE_ObjectNoM}");
                                continue;
                            }
                            lioXmlNodeToPrinter.InnerText = livstrPropertyValue;
                        }
                        break;
                    case "io":
                    case "co":
                        JArray lcoJArray = (JArray)JToken.FromObject(lcoDocuments[0].GetType().GetProperty(lioMapperItem.ivstrProperty)?.GetValue(lcoDocuments[0], null) ?? new JArray());
                        int livnumIndex = 1;
                        foreach (JObject lioJObject in lcoJArray)
                        {
                            foreach (ServiceMapperItemXPath lioServiceMapperItemXPath in lioMapperItem.coXPaths)
                            {
                                lioXmlNodeToPrinter = lioXmlToPrinter.SelectSingleNode(lioServiceMapperItemXPath.ivstrData.Replace("{N}", livnumIndex.ToString()), lioNsMngr);
                                if (lioXmlNodeToPrinter == null)
                                {
                                    lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} xpath {lioServiceMapperItemXPath.ivstrData} {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                livstrPropertyValue = lioJObject.GetType().GetProperty(lioMapperItem.ivstrProperty.Substring(3))?.GetValue(lioJObject, null)?.ToString() ?? string.Empty;
                                lioXmlNodeToPrinter.InnerText = livstrPropertyValue;
                            }
                            livnumIndex++;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (lioSbErrors.Length > 0)
            {
                throw new Exception(lioSbErrors.ToString());
            }
            return lioXmlToPrinter.OuterXml;
        }
        #endregion
        #region PRIVATE PROPS
        private long mivlngCuit;
        private NatContext mioContext;
        #endregion
        #region PRIVATE METHODS
        private ServiceMapper GetMapper()
        {
            Cuit lioCuit = new Cuit(mivlngCuit, mioContext, null);
            if (lioCuit.ioDcModel == null || lioCuit.ioDcModel.ivstrCnfg == null)
                throw new Exception($"CUIT invalido o {Resources.lioE_ObjectNoM}");
            if (string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                throw new Exception($"Configuracion de C.U.I.T. {Resources.lioE_ObjectNoF}");
            if (string.IsNullOrEmpty(ivstrName))
                throw new Exception($"Nombre de Archivo de Ingreso {Resources.lioE_ObjectNoM}");
            if (lioCuit.ioCnfg?.coServiceMappers?.Count() == 0)
                throw new Exception($"Mapeadores {Resources.lioE_ObjectNoM}s");
            ServiceMapper lioServiceMapper = lioCuit?.ioCnfg?.coServiceMappers?.FirstOrDefault(x => x.ivstrInputType == "json");
            if (lioServiceMapper == null)
                throw new Exception($"Mapeador JSON {Resources.lioE_ObjectNoM}s");
            return lioServiceMapper;
        }
        #endregion
    }
}
