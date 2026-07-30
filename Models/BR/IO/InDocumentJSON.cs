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
                if (!string.IsNullOrEmpty(lioDocumentUser.ivstrCbteModo))
                {
                    lioDocumentUser.ivstrWs = "wscdc";
                    lioDocumentUser.ivstrIdCliente = "NatOrigen2";
                }
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
            string livstr = ListHelper.GetValue("PATH", "template", this.mioContext);
            if (!File.Exists($"{livstr}\\print.xml"))
                throw new Exception($"Xml Impresion {Resources.lioE_ObjectNoM}");
            XmlDocument lioXmlToPrinter = new XmlDocument();
            using (StreamReader lioRd = new StreamReader($"{livstr}\\print.xml", Encoding.UTF8))
            {
                lioXmlToPrinter.Load(lioRd);
            }
            XmlNamespaceManager lioNsMngr = new XmlNamespaceManager(lioXmlToPrinter.NameTable);
            lioNsMngr.AddNamespace("ns", "http://www.afip.com.ar/fe");
            StringBuilder lioSbErrors = new StringBuilder();
            Dictionary<string, XmlNode> lcoXmlNodesToClone = new Dictionary<string, XmlNode>();
            Dictionary<string, string> lcoParentXPaths = new Dictionary<string, string>();
            DocumentUser lioDocumentUser = lcoDocuments[0];
            string[] lcvstrPropertyValues;
            string livstrPropName, livstrPropRawValue, livstrParentPropName, livstrChildPropName, livstrParentXPath;
            object? lioPropValue, lioParentValue;
            Type lioType, lioParentType;
            PropertyInfo? lioPropInfo, lioParentPropInfo;
            System.Collections.IEnumerable? lcoOs;
            List<object> lcoItemsList;
            XmlNodeList lcoNodes;
            XmlNode? lioNode, lioSourceNode, lioNewNode, lioParentContainerNode, lioCurrentParentNode;
            bool livblnIsCollection;
            // diccionario con padres de colecciones y sus xPaths
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems)
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty)) continue;
                lcvstrPropertyValues = lioMapperItem.ivstrProperty.Split('.');
                if (lcvstrPropertyValues.Length > 1 || !lcvstrPropertyValues[0].StartsWith("co") || lioMapperItem.coXPaths == null && lioMapperItem.coXPaths.Length == 0) continue;
                lcoParentXPaths[lcvstrPropertyValues[0]] = lioMapperItem.coXPaths[0].ivstrData;
            }

            // CAMPOS
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems)
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty)) continue;
                if (lioMapperItem.coXPaths == null || lioMapperItem.coXPaths.Length == 0) continue;
                try
                {
                    lcvstrPropertyValues = lioMapperItem.ivstrProperty.Split('.');
                    if (lcvstrPropertyValues.Length == 0) continue;
                    if (lcvstrPropertyValues.Length == 1)  //propiedades en raiz
                    {
                        livstrPropName = lcvstrPropertyValues[0];
                        if (livstrPropName.StartsWith("co")) continue;
                        lioPropInfo = typeof(DocumentUser).GetProperty(livstrPropName);
                        if (lioPropInfo == null)
                        {
                            lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                            continue;
                        }
                        lioPropValue = lioPropInfo.GetValue(lioDocumentUser);
                        livstrPropRawValue = FormatValue(lioMapperItem, lioPropValue?.ToString());
                        if (lioMapperItem.ivblnRequired ?? false && string.IsNullOrEmpty(livstrPropRawValue))
                        {
                            lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                            continue;
                        }
                        foreach (ServiceMapperItemXPath lioXPath in lioMapperItem.coXPaths)
                        {
                            try
                            {
                                string livstrTargetXPath = lioXPath.ivstrData.Replace("{N}", "1");
                                lioNode = lioXmlToPrinter.SelectSingleNode(livstrTargetXPath, lioNsMngr);
                                if (lioNode == null)
                                {
                                    lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioNode.InnerText = livstrPropRawValue;
                            }
                            catch (Exception lioE)
                            {
                                lioSbErrors.AppendLine($"Property {lioMapperItem.ivstrProperty} XPath {lioXPath.ivstrData} Error: {lioE.Message}");
                            }
                        }
                    } //propiedades dentro de objeto o coleccion
                    else if (lcvstrPropertyValues.Length == 2)
                    {
                        livstrParentPropName = lcvstrPropertyValues[0];
                        livstrChildPropName = lcvstrPropertyValues[1];

                        lioParentPropInfo = typeof(DocumentUser).GetProperty(livstrParentPropName);
                        if (lioParentPropInfo == null)
                        {
                            lioSbErrors.AppendLine($"Propiedad {livstrParentPropName} {Resources.lioE_ObjectNoF}");
                            continue;
                        }

                        lioParentValue = lioParentPropInfo.GetValue(lioDocumentUser);
                        lioParentType = lioParentPropInfo.PropertyType;

                        livblnIsCollection = false;
                        lioType = lioParentType;

                        if (lioParentType.IsGenericType && lioParentType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            lioType = lioParentType.GetGenericArguments()[0];
                            livblnIsCollection = true;
                        }
                        else if (lioParentType.IsArray)
                        {
                            lioType = lioParentType.GetElementType()!;
                            livblnIsCollection = true;
                        }
                        else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(lioParentType) && lioParentType != typeof(string))
                        {
                            livblnIsCollection = true;
                            if (lioParentType.IsGenericType)
                                lioType = lioParentType.GetGenericArguments()[0];
                        }

                        if (livblnIsCollection)
                        {
                            lcoOs = lioParentValue as System.Collections.IEnumerable;
                            lcoItemsList = new List<object>();
                            if (lcoOs != null)
                            {
                                foreach (object lioO in lcoOs)
                                    lcoItemsList.Add(lioO);
                            }

                            if (lcoItemsList.Count == 0)
                            {
                                if (lioMapperItem.ivblnRequired ?? false)
                                {
                                    lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                }
                                continue;
                            }

                            lioPropInfo = lioType.GetProperty(livstrChildPropName);
                            if (lioPropInfo == null)
                            {
                                lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                                continue;
                            }

                            for (int livnumIdx = 0; livnumIdx < lcoItemsList.Count; livnumIdx++)
                            {
                                lioPropValue = lcoItemsList[livnumIdx] != null ? lioPropInfo.GetValue(lcoItemsList[livnumIdx]) : null;
                                foreach (ServiceMapperItemXPath lioXPath in lioMapperItem.coXPaths)
                                {
                                    try
                                    {
                                        lioNode = null;
                                        livstrParentXPath = lcoParentXPaths.ContainsKey(livstrParentPropName) ? lcoParentXPaths[livstrParentPropName] : null;
                                        if (!string.IsNullOrEmpty(livstrParentXPath))
                                        {
                                            lcoNodes = lioXmlToPrinter.SelectNodes(livstrParentXPath, lioNsMngr);
                                            if (lcoNodes == null || lcoNodes.Count == 0)
                                            {
                                                lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                                continue;
                                            }
                                            if (!lcoXmlNodesToClone.ContainsKey(livstrParentXPath))
                                                lcoXmlNodesToClone[livstrParentXPath] = lcoNodes.Item(0)!;

                                            if (lcoNodes.Count < livnumIdx + 1)
                                            {
                                                lioSourceNode = lcoXmlNodesToClone[livstrParentXPath];
                                                lioNewNode = lioXmlToPrinter.ImportNode(lioSourceNode, true);
                                                lioParentContainerNode = lcoNodes.Item(0)?.ParentNode;

                                                if (!string.IsNullOrEmpty(lioXPath.ivstrEnum))
                                                {
                                                    XmlNode? lioEnumNode = lioNewNode.SelectSingleNode(lioXPath.ivstrEnum, lioNsMngr);
                                                    if (lioEnumNode != null)
                                                        lioEnumNode.InnerText = (livnumIdx + 1).ToString();
                                                }

                                                lioParentContainerNode?.AppendChild(lioNewNode);
                                                lcoNodes = lioXmlToPrinter.SelectNodes(livstrParentXPath, lioNsMngr);
                                            }

                                            lioCurrentParentNode  = lcoNodes?.Item(livnumIdx);
                                            string livstrDataPath = lioXPath.ivstrData.Replace("{N}", (livnumIdx + 1).ToString());

                                            lioNewNode = lioCurrentParentNode?.SelectSingleNode(livstrDataPath, lioNsMngr);
                                            if (lioNewNode == null)
                                                lioNewNode = lioXmlToPrinter.SelectSingleNode(livstrDataPath, lioNsMngr);
                                        }
                                        else
                                        {
                                            string livstrDataPath = lioXPath.ivstrData.Replace("{N}", (livnumIdx + 1).ToString());
                                            lioNewNode = lioXmlToPrinter.SelectSingleNode(livstrDataPath, lioNsMngr);
                                        }

                                        if (lioNewNode == null)
                                        {
                                            lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                            continue;
                                        }

                                        lioNewNode.InnerText = FormatValue(lioMapperItem, lioPropValue?.ToString()); 
                                    }
                                    catch (Exception lioE)
                                    {
                                        lioSbErrors.AppendLine($"Property {lioMapperItem.ivstrProperty} XPath {lioXPath.ivstrData} Error: {lioE.Message}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (lioParentValue == null)
                            {
                                if (lioMapperItem.ivblnRequired ?? false)
                                {
                                    lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                }
                                continue;
                            }

                            lioPropInfo = lioParentType.GetProperty(livstrChildPropName);
                            if (lioPropInfo == null)
                            {
                                lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                                continue;
                            }

                            lioPropValue = lioPropInfo.GetValue(lioParentValue);
                            livstrPropRawValue = FormatValue(lioMapperItem, lioPropValue?.ToString());

                            if (lioMapperItem.ivblnRequired ?? false && string.IsNullOrEmpty(livstrPropRawValue))
                            {
                                lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                continue;
                            }

                            foreach (ServiceMapperItemXPath lioXPath in lioMapperItem.coXPaths)
                            {
                                try
                                {
                                    string livstrTargetXPath = lioXPath.ivstrData.Replace("{N}", "1");
                                    lioNode = lioXmlToPrinter.SelectSingleNode(livstrTargetXPath, lioNsMngr);
                                    if (lioNode == null)
                                    {
                                        lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                        continue;
                                    }
                                    lioNode.InnerText = livstrPropRawValue;
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Property {lioMapperItem.ivstrProperty} XPath {lioXPath.ivstrData} Error: {lioE.Message}");
                                }
                            }
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

        private string FormatValue(ServiceMapperItem vioMapperItem, string? vivstrValue)
        {
            if (vioMapperItem.ivstrCoord != null && vioMapperItem.ivstrCoord.Contains("FIX"))
            {
                return vioMapperItem.ivstrformat ?? string.Empty;
            }

            string livstrValue = vivstrValue ?? string.Empty;

            if (vioMapperItem.coConversion != null && vioMapperItem.coConversion.ContainsKey(livstrValue))
            {
                livstrValue = vioMapperItem.coConversion[livstrValue];
            }

            if (string.IsNullOrEmpty(livstrValue) && !string.IsNullOrEmpty(vioMapperItem.ivstrDefault))
            {
                livstrValue = vioMapperItem.ivstrDefault;
            }

            if (!string.IsNullOrEmpty(vioMapperItem.ivstrformat))
            {
                if (vioMapperItem.ivstrformat.Contains("{"))
                {
                    livstrValue = string.Format(vioMapperItem.ivstrformat, livstrValue);
                }
                else if (vioMapperItem.ivstrformat.Contains("=>"))
                {
                    string[] lcvstrFmt = vioMapperItem.ivstrformat.Split("=>");
                    if (DateTime.TryParseExact(livstrValue, lcvstrFmt[0], null, DateTimeStyles.None, out DateTime livdtm))
                    {
                        livstrValue = livdtm.ToString(lcvstrFmt[1], CultureInfo.InvariantCulture);
                    }
                }
            }

            return livstrValue;
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
