using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
                lioDocumentUser.ivstrInputData = Format.Compress(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<DocumentUser> { lioDocumentUser }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))));
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
                if (lioDocumentUser.ivlngCuitEmisor == null)
                    lioSbErrors.AppendLine($"[{livnroI}] CUIT Emisor {Resources.lioE_ObjectNoF}");
                if (lioDocumentUser.ivlngDocReceptor == null)
                    lioSbErrors.AppendLine($"[{livnroI}] Numero de documento receptor {Resources.lioE_ObjectNoM}");
                if (lioDocumentUser.ivnroTipoDocReceptor == null)
                    lioSbErrors.AppendLine($"[{livnroI}] Tipo de documento receptor {Resources.lioE_ObjectNoM}");
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
            // Convert the document to JObject for JSON path queries
            JObject lioDocumentJson = JObject.FromObject(lcoDocuments[0]);
            string[] lcvstrPropertyValues;
            PropertyInfo lioPropertyInfo, lioParentPropertyInfo;
            Type lioPropertyType;
            string livstrValue;
            object lioTargetObject;
            bool livblnIsCollection;
            XmlNodeList lioXmlNodeList;
            int livnumNodesToClone = 0;
            // CAMPOS
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems)
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty)) continue;
                if (lioMapperItem.coXPaths == null || lioMapperItem.coXPaths.Length == 0) continue;
                try
                {
                    lcvstrPropertyValues = lioMapperItem.ivstrProperty.Split('.');
                    if (lcvstrPropertyValues.Length == 0) continue;
                    if (lcvstrPropertyValues.Length == 1)
                    {
                        if (lcvstrPropertyValues[0].StartsWith("iv")) //propiedad directa de DocumentUser
                        {
                            lioPropertyInfo = typeof(DocumentUser).GetProperty(lioMapperItem.ivstrProperty);
                            lioTargetObject = lcoDocuments[0];
                        }
                        else
                        {
                            if (!lcvstrPropertyValues[0].StartsWith("co")) continue;
                            //padres de coleccion solo se setea el nodo padre del xml
                            lioXmlNodeList = lioXmlToPrinter.SelectNodes(lioMapperItem.coXPaths[0].ivstrData, lioNsMngr);
                            if (lioXmlNodeList == null || lioXmlNodeList.Count == 0)
                                lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                            // se deben crear tantos nodos como elementos tenga la coleccion, pero como no se especifica la propiedad de la coleccion no se puede acceder a ella, por lo que se clona el nodo por cada elemento de la coleccion sin setearle valor alguno
                            lioPropertyInfo = typeof(DocumentUser).GetProperty(lcvstrPropertyValues[0]);
                            lioPropertyType = lioPropertyInfo.PropertyType;
                            livnumNodesToClone = (int)lioPropertyInfo.GetValue(lcoDocuments[0])?.GetType().GetProperty("Count")?.GetValue(lioPropertyInfo.GetValue(lcoDocuments[0]));
                            //for( int livnum lioNode in lioXmlNodeList)
                            //{
                            //    lioXmlNodeToClone = lioNode.Clone();
                            //    lioNode.ParentNode.InsertBefore(lioXmlNodeToClone, lioNode);
                            //}
                            continue;
                        }
                    }
                    else if (lcvstrPropertyValues.Length == 2)
                    {
                        lioParentPropertyInfo = typeof(DocumentUser).GetProperty(lcvstrPropertyValues[0]);
                        if (lioParentPropertyInfo == null)
                        {
                            lioSbErrors.AppendLine($"Propiedad {lcvstrPropertyValues[0]} {Resources.lioE_ObjectNoF}");
                            continue;
                        }



                        lioTargetObject = lioParentPropertyInfo.GetValue(lcoDocuments[0]);
                        if (lioTargetObject == null)
                            continue;
                        lioPropertyType = lioParentPropertyInfo.PropertyType;
                        livblnIsCollection = false;
                        if (lioPropertyType.IsGenericType && lioPropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            lioPropertyType = lioPropertyType.GetGenericArguments()[0];
                            livblnIsCollection = true;
                        }
                        else if (lioPropertyType.IsArray)
                        {
                            lioPropertyType = lioPropertyType.GetElementType();
                            livblnIsCollection = true;
                        }
                        if (livblnIsCollection)
                        {
                            var lioEnumerable = lioTargetObject as System.Collections.IEnumerable;
                            if (lioEnumerable != null)
                            {
                                var lioEnumerator = lioEnumerable.GetEnumerator();
                                if (lioEnumerator.MoveNext())
                                    lioTargetObject = lioEnumerator.Current;
                                else
                                    continue;
                            }
                        }

                        lioPropertyInfo = lioPropertyType.GetProperty(lcvstrPropertyValues[1]);
                    }
                    else continue;
                    if (lioPropertyInfo == null)
                    {
                        lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                        continue;
                    }
                    livstrValue = lioPropertyInfo.GetValue(lioTargetObject)?.ToString() ?? string.Empty;
                    if (lioMapperItem.ivblnRequired ?? false && string.IsNullOrEmpty(livstrValue))
                    {
                        lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                        continue;
                    }
                    foreach (ServiceMapperItemXPath lioServiceMapperItemXPath in lioMapperItem.coXPaths)
                    {
                        try
                        {
                            lioXmlNodeToPrinter = lioXmlToPrinter.SelectSingleNode(lioServiceMapperItemXPath.ivstrData, lioNsMngr);
                            if (lioXmlNodeToPrinter == null)
                            {
                                lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                continue;
                            }
                            lioXmlNodeToPrinter.InnerText = livstrValue;
                        }
                        catch (Exception lioE)
                        {
                            lioSbErrors.AppendLine($"Property {lioMapperItem.ivstrProperty} XPath {lioServiceMapperItemXPath.ivstrData} Error: {lioE.Message}");
                            continue;
                        }
                    }
                }
                catch (Exception lioE)
                {
                    lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} Error: {lioE.Message}");
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
                throw new Exception($"Mapeadores {Resources.lioE_ObjectNoM}");
            ServiceMapper lioServiceMapper = lioCuit?.ioCnfg?.coServiceMappers?.FirstOrDefault(x => x.ivstrInputType == "json");
            if (lioServiceMapper == null)
                throw new Exception($"Mapeador JSON {Resources.lioE_ObjectNoM}");
            return lioServiceMapper;
        }
        #endregion
    }
}
