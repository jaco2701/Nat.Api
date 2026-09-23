using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            ivstrKey = string.Empty;
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
                    livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext),
                    livstrPropName, livstrParentPropName, livstrRawPropValue;
            string[] lcvstrPropertyValues;
            DateTime livdtm;
            StringBuilder lioSbErrors = new StringBuilder();
            DocumentUser? lioDocumentUser;
            List<DocumentUser> lcoDocumentUser = new List<DocumentUser>();
            if (lioServiceMapper.ivblnMapping ?? false)
            {
                JToken lioRootToken = JToken.Parse(livstr);
                IEnumerable<JToken> lcoDocTokens;
                if (!string.IsNullOrEmpty(lioServiceMapper.ivstrSplitter))
                {
                    string livstrSplitterPath = lioServiceMapper.ivstrSplitter.Trim();
                    if (!livstrSplitterPath.StartsWith("$"))
                    {
                        livstrSplitterPath = "$." + livstrSplitterPath;
                    }
                    lcoDocTokens = lioRootToken.SelectTokens(livstrSplitterPath);
                }
                else if (lioRootToken is JArray lcoOs)
                    lcoDocTokens = lcoOs;
                else
                    lcoDocTokens = new List<JToken> { lioRootToken };

                Type lioType, lioParentType;
                PropertyInfo? lioPropInfo, lioParentPropInfo;
                bool livblnIsCollection;
                // carga el root para colecciones, para poder mapearlas luego
                Dictionary<string, string> lcoCollectionRootTokens = new Dictionary<string, string>();
                foreach (ServiceMapperItem lioO in lioServiceMapper.coItems?.Where(x =>
                                                                    !string.IsNullOrEmpty(x.ivstrProperty) &&
                                                                    !string.IsNullOrEmpty(x.ivstrCoord) &&
                                                                    x.ivstrProperty.Split('.').Length == 1 &&
                                                                    x.ivstrProperty.Split('.')[0].StartsWith("co")
                                                                    ) ?? Enumerable.Empty<ServiceMapperItem>())
                {
                    if (!lcoCollectionRootTokens.ContainsKey(lioO.ivstrProperty ?? string.Empty))
                        lcoCollectionRootTokens[lioO.ivstrProperty ?? string.Empty] = lioO.ivstrCoord ?? string.Empty;
                }
                //recorre cada documento y mapea sus propiedades
                foreach (JToken lioDocToken in lcoDocTokens)
                {
                    lioDocumentUser = new DocumentUser();
                    lioDocumentUser.ivblnSaveOnLoad = lioServiceMapper.ivblnSaveOnLoad ?? true;
                    lioSbErrors.Clear();
                    foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems?.Where(x => x != null && !string.IsNullOrEmpty(x.ivstrProperty) && !string.IsNullOrEmpty(x.ivstrCoord)) ?? Enumerable.Empty<ServiceMapperItem>())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(lioServiceMapperItem.ivstrProperty)) continue;
                            if (string.IsNullOrEmpty(lioServiceMapperItem.ivstrCoord)) continue;
                            lcvstrPropertyValues = (lioServiceMapperItem.ivstrProperty ?? string.Empty).Split('.');
                            if (lcvstrPropertyValues.Length == 1) // Propiedad en raíz
                            {
                                livstrPropName = lcvstrPropertyValues[0];
                                if (livstrPropName.StartsWith("co")) continue;
                                lioPropInfo = typeof(DocumentUser).GetProperty(livstrPropName);
                                if (lioPropInfo != null)
                                {
                                    JToken? lioTokenVal = SelectJsonToken(lioDocToken, lioServiceMapperItem.ivstrCoord);
                                    livstrRawPropValue = lioTokenVal?.ToString() ?? string.Empty;
                                    SetPropertyValue(lioDocumentUser, lioPropInfo, lioServiceMapperItem, livstrRawPropValue);
                                }
                            }
                            else if (lcvstrPropertyValues.Length == 2) // Objeto anidado o Colección
                            {
                                livstrParentPropName = lcvstrPropertyValues[0];
                                livstrPropName = lcvstrPropertyValues[1];
                                livblnIsCollection = lcoCollectionRootTokens.ContainsKey(livstrParentPropName);
                                lioParentPropInfo = typeof(DocumentUser).GetProperty(livstrParentPropName);
                                if (lioParentPropInfo == null) continue;
                                lioParentType = lioParentPropInfo.PropertyType;
                                lioType = lioParentType;

                                if (!livblnIsCollection)
                                {
                                    object? lioParentObj = lioParentPropInfo.GetValue(lioDocumentUser);
                                    if (lioParentObj == null)
                                    {
                                        lioParentObj = Activator.CreateInstance(lioParentType);
                                        lioParentPropInfo.SetValue(lioDocumentUser, lioParentObj);
                                    }
                                    if (lioParentObj != null)
                                    {
                                        lioPropInfo = lioParentType.GetProperty(livstrPropName);
                                        if (lioPropInfo != null)
                                        {
                                            JToken? lioTokenVal = SelectJsonToken(lioDocToken, lioServiceMapperItem.ivstrCoord);
                                            livstrRawPropValue = lioTokenVal?.ToString() ?? string.Empty;
                                            SetPropertyValue(lioParentObj, lioPropInfo, lioServiceMapperItem, livstrRawPropValue);
                                        }
                                    }
                                }
                                else
                                {
                                    if (lioParentType.IsGenericType && lioParentType.GetGenericTypeDefinition() == typeof(List<>))
                                        lioType = lioParentType.GetGenericArguments()[0];
                                    else if (lioParentType.IsArray)
                                        lioType = lioParentType.GetElementType()!;
                                    else if (typeof(IEnumerable).IsAssignableFrom(lioParentType) && lioParentType != typeof(string))
                                        if (lioParentType.IsGenericType)
                                            lioType = lioParentType.GetGenericArguments()[0];
                                    IEnumerable<JToken> lcoItemTokens = SelectJsonTokens(lioDocToken, lcoCollectionRootTokens[livstrParentPropName]);
                                    if (lcoItemTokens != null && lcoItemTokens.Any())
                                    {
                                        IList? lcoList = lioParentPropInfo.GetValue(lioDocumentUser) as IList;
                                        if (lcoList == null)
                                        {
                                            Type lioListType = typeof(List<>).MakeGenericType(lioType);
                                            lcoList = Activator.CreateInstance(lioListType) as IList;
                                            lioParentPropInfo.SetValue(lioDocumentUser, lcoList);
                                        }
                                        lioPropInfo = lioType.GetProperty(livstrPropName);
                                        if (lcoList != null)
                                        {
                                            int livnumIdx = 0;
                                            foreach (JToken lioItemToken in lcoItemTokens)
                                            {
                                                object? lioItemObj = null;
                                                if (livnumIdx < lcoList.Count)
                                                    lioItemObj = lcoList[livnumIdx];
                                                else
                                                {
                                                    lioItemObj = Activator.CreateInstance(lioType);
                                                    lcoList.Add(lioItemObj);
                                                }
                                                if (lioItemObj != null && lioPropInfo != null)
                                                {
                                                    livstrRawPropValue = string.Empty;
                                                    if (lioItemToken is JValue)
                                                        livstrRawPropValue = lioItemToken.ToString();
                                                    else
                                                    {
                                                        JToken? lioChildToken = lioItemToken.SelectToken(lioServiceMapperItem.ivstrCoord) ?? lioItemToken.SelectToken("$." + livstrPropName);
                                                        livstrRawPropValue = lioChildToken != null ? lioChildToken.ToString() : lioItemToken.ToString();
                                                    }
                                                    SetPropertyValue(lioItemObj, lioPropInfo, lioServiceMapperItem, livstrRawPropValue);
                                                }
                                                livnumIdx++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception lioE)
                        {
                            lioSbErrors.AppendLine($"{lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                        }
                    }
                    if (lioDocumentUser != null)
                    {
                        lioDocumentUser.ivstrLoadErrors = lioSbErrors.ToString();
                        lcoDocumentUser.Add(lioDocumentUser);
                    }
                }
            }
            else
                lcoDocumentUser = JsonConvert.DeserializeObject<List<DocumentUser>>(livstr) ?? new List<DocumentUser>();
            short livnroI = 0;
            foreach (DocumentUser lioDocUser in lcoDocumentUser.Where(x=>string.IsNullOrEmpty(x.ivstrLoadErrors)))
            {
                lioDocUser.ivstrInputData = Format.Compress(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<DocumentUser> { lioDocUser }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))));
                lioDocUser.ivblnTaxInLines = lioServiceMapper.ivblnTaxInLines ?? false;
                lioDocUser.ivblnCalcPermisoExistente = lioServiceMapper.ivblnCalcPermisoExistente ?? false;
                if (!string.IsNullOrEmpty(lioDocUser.ivstrCbteModo))
                {
                    lioDocUser.ivstrWs = "wscdc";
                    lioDocUser.ivstrIdCliente = "NatOrigen2";
                }
                if (lioDocUser.ivnroTipoDoc == null)
                    lioSbErrors.AppendLine($"[{livnroI}] Tipo de Documento {Resources.lioE_ObjectNoM}");
                if (lioDocUser.ivnumPvta == null)
                    lioSbErrors.AppendLine($"[{livnroI}] Punto de Venta {Resources.lioE_ObjectNoM}");
                if (lioDocUser.ivlngCbte == null && (lioDocUser.ivblnSaveOnLoad ?? true))
                    lioSbErrors.AppendLine($"[{livnroI}] Numero de Comprobante {Resources.lioE_ObjectNoM}");
                if (lioDocUser.ivstrFechaEmision == null || !DateTime.TryParseExact(lioDocUser.ivstrFechaEmision, livstrApiDtmFormat, null, DateTimeStyles.None, out livdtm))
                    lioSbErrors.AppendLine($"[{livnroI}] Fecha de Comprobante {Resources.lioE_ObjectNoF}");
                if (lioDocUser.ivlngCuitEmisor == null)
                    lioSbErrors.AppendLine($"[{livnroI}] CUIT Emisor {Resources.lioE_ObjectNoF}");
                if (lioDocUser.ivlngDocReceptor == null)
                    lioSbErrors.AppendLine($"[{livnroI}] Numero de documento receptor {Resources.lioE_ObjectNoM}");
                if (lioDocUser.ivnroTipoDocReceptor == null)
                    lioSbErrors.AppendLine($"[{livnroI}] Tipo de documento receptor {Resources.lioE_ObjectNoM}");
                if (lioSbErrors.Length > 0)
                    lioDocUser.ivstrLoadErrors += lioSbErrors.ToString();
                livnroI++;
                lioDocUser.FormatAmounts();
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
            string livstrPropName, livstrParentPropName, livstrChildPropName, livstrParentXPath;
            string? livstrPropRawValue;
            object? lioPropValue, lioParentValue;
            Type lioType, lioParentType;
            PropertyInfo? lioPropInfo, lioParentPropInfo;
            System.Collections.IEnumerable? lcoOs;
            List<object> lcoItemsList;
            XmlNodeList lcoNodes;
            XmlNode? lioNode, lioSourceNode, lioNewNode, lioParentContainerNode, lioCurrentParentNode;
            bool livblnIsCollection;
            // diccionario con padres de colecciones y sus xPaths
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems?? [])
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty)) continue;
                lcvstrPropertyValues = lioMapperItem.ivstrProperty.Split('.');
                if (lcvstrPropertyValues.Length > 1 || !lcvstrPropertyValues[0].StartsWith("co") || lioMapperItem.coXPaths == null || lioMapperItem.coXPaths.Length == 0 || string.IsNullOrEmpty(lioMapperItem.coXPaths[0].ivstrData)) continue;
                lcoParentXPaths[lcvstrPropertyValues[0]] = lioMapperItem.coXPaths[0].ivstrData;
            }
            // CAMPOS
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems?? [])
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty)) continue;
                if (lioMapperItem.coXPaths == null || lioMapperItem.coXPaths.Length == 0) continue;
                try
                {
                    lcvstrPropertyValues = lioMapperItem.ivstrProperty.Split('.');
                    if (lcvstrPropertyValues.Length == 1)  //propiedades en raiz
                    {
                        livstrPropName = lcvstrPropertyValues[0];
                        if (livstrPropName.StartsWith("co")) continue;
                        lioPropInfo = typeof(DocumentUser).GetProperty(livstrPropName);
                        if (lioPropInfo == null && (string.IsNullOrEmpty(lioMapperItem.ivstrCoord) || !lioMapperItem.ivstrCoord.Contains("FIX")))
                        {
                            lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                            continue;
                        }
                        if (lioPropInfo == null)
                            livstrPropRawValue = null;
                        else
                        {
                            lioPropValue = lioPropInfo.GetValue(lioDocumentUser);
                            livstrPropRawValue = lioMapperItem.FormatPropertyValue(lioPropValue?.ToString() ?? string.Empty);
                        }
                        if (lioMapperItem.ivblnRequired ?? false && string.IsNullOrEmpty(livstrPropRawValue))
                        {
                            lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                            continue;
                        }
                        foreach (ServiceMapperItemXPath lioXPath in lioMapperItem.coXPaths)
                        {
                            try
                            {
                                string livstrTargetXPath = lioXPath.ivstrData?.Replace("{N}", "1") ?? string.Empty;
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
                            if (lioPropInfo == null && (string.IsNullOrEmpty(lioMapperItem.ivstrCoord) || !lioMapperItem.ivstrCoord.Contains("FIX")))
                            {
                                lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                                continue;
                            }

                            for (int livnumIdx = 0; livnumIdx < lcoItemsList.Count; livnumIdx++)
                            {
                                if (lioPropInfo == null)
                                    livstrPropRawValue = null;
                                else
                                {
                                    lioPropValue = lcoItemsList[livnumIdx] != null ? lioPropInfo.GetValue(lcoItemsList[livnumIdx]) : null;
                                    livstrPropRawValue = lioMapperItem.FormatPropertyValue(lioPropValue?.ToString() ?? string.Empty);
                                }
                                foreach (ServiceMapperItemXPath lioXPath in lioMapperItem.coXPaths)
                                {
                                    try
                                    {
                                        lioNode = null;
                                        livstrParentXPath = string.Empty;
                                        if (lcoParentXPaths.TryGetValue(livstrParentPropName, out livstrParentXPath))
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

                                            lioCurrentParentNode = lcoNodes?.Item(livnumIdx);
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

                                        lioNewNode.InnerText = livstrPropRawValue??string.Empty;
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
                            if (lioPropInfo == null && (string.IsNullOrEmpty(lioMapperItem.ivstrCoord) || !lioMapperItem.ivstrCoord.Contains("FIX")))
                            {
                                lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoF}");
                                continue;
                            }
                            if (lioPropInfo == null)
                                livstrPropRawValue = null;
                            else
                            {
                                lioPropValue = lioPropInfo.GetValue(lioParentValue);
                                livstrPropRawValue = lioMapperItem.FormatPropertyValue(lioPropValue?.ToString() ?? string.Empty);
                            }

                            if (lioMapperItem.ivblnRequired ?? false && string.IsNullOrEmpty(livstrPropRawValue))
                            {
                                lioSbErrors.AppendLine($"Valor de propiedad {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                continue;
                            }

                            foreach (ServiceMapperItemXPath lioXPath in lioMapperItem.coXPaths)
                            {
                                try
                                {
                                    string livstrTargetXPath = lioXPath?.ivstrData?.Replace("{N}", "1")??string.Empty;
                                    lioNode = lioXmlToPrinter.SelectSingleNode(livstrTargetXPath, lioNsMngr);
                                    if (lioNode == null)
                                    {
                                        lioSbErrors.AppendLine($"XPath {lioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM}");
                                        continue;
                                    }
                                    lioNode.InnerText = livstrPropRawValue??string.Empty;
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
            ServiceMapper? lioServiceMapper = lioCuit?.ioCnfg?.coServiceMappers?.FirstOrDefault(x => x.ivstrInputType == "json");
            if (lioServiceMapper == null)
                throw new Exception($"Mapeador JSON {Resources.lioE_ObjectNoM}");
            return lioServiceMapper;
        }
        private JToken? SelectJsonToken(JToken vioToken, string? vivstrCoord)
        {
            if (string.IsNullOrEmpty(vivstrCoord)) return null;
            try
            {
                string livstrPath = vivstrCoord.Trim();
                if (!livstrPath.StartsWith("$"))
                {
                    livstrPath = "$." + livstrPath;
                }
                return vioToken.SelectToken(livstrPath);
            }
            catch
            {
                return null;
            }
        }
        private IEnumerable<JToken> SelectJsonTokens(JToken vioToken, string? vivstrCoord)
        {
            if (string.IsNullOrEmpty(vivstrCoord)) return Enumerable.Empty<JToken>();
            try
            {
                string livstrPath = vivstrCoord.Trim();
                if (!livstrPath.StartsWith("$"))
                {
                    livstrPath = "$." + livstrPath;
                }
                IEnumerable<JToken> lcoTokens = vioToken.SelectTokens(livstrPath);
                if (lcoTokens.Count() == 1 && lcoTokens.First() is JArray lioArr)
                {
                    return lioArr.Children();
                }
                return lcoTokens;
            }
            catch
            {
                return Enumerable.Empty<JToken>();
            }
        }
        private void SetPropertyValue(object vioTargetObj, PropertyInfo vioPropInfo, ServiceMapperItem vioMapperItem, string? vivstrRawValue)
        {
            string? livstrFormatted = vioMapperItem.FormatPropertyValue(vivstrRawValue??string.Empty);
            if (string.IsNullOrEmpty(livstrFormatted) && vioPropInfo.PropertyType != typeof(string)) return;

            Type lioTargetType = Nullable.GetUnderlyingType(vioPropInfo.PropertyType) ?? vioPropInfo.PropertyType;

            object? lioConvertedValue = null;
            if (lioTargetType == typeof(string))
            {
                lioConvertedValue = livstrFormatted;
            }
            else if (lioTargetType == typeof(short))
            {
                if (short.TryParse(livstrFormatted, NumberStyles.Any, CultureInfo.InvariantCulture, out short livnroVal))
                    lioConvertedValue = livnroVal;
            }
            else if (lioTargetType == typeof(int))
            {
                if (int.TryParse(livstrFormatted, NumberStyles.Any, CultureInfo.InvariantCulture, out int livnumVal))
                    lioConvertedValue = livnumVal;
            }
            else if (lioTargetType == typeof(long))
            {
                if (long.TryParse(livstrFormatted, NumberStyles.Any, CultureInfo.InvariantCulture, out long livlngVal))
                    lioConvertedValue = livlngVal;
            }
            else if (lioTargetType == typeof(double))
            {
                if (double.TryParse(livstrFormatted, NumberStyles.Any, CultureInfo.InvariantCulture, out double livdblVal))
                    lioConvertedValue = livdblVal;
            }
            else if (lioTargetType == typeof(bool))
            {
                if (bool.TryParse(livstrFormatted, out bool livblnVal))
                    lioConvertedValue = livblnVal;
                else if (livstrFormatted == "1" || livstrFormatted.Equals("S", StringComparison.OrdinalIgnoreCase) || livstrFormatted.Equals("True", StringComparison.OrdinalIgnoreCase))
                    lioConvertedValue = true;
                else if (livstrFormatted == "0" || livstrFormatted.Equals("N", StringComparison.OrdinalIgnoreCase) || livstrFormatted.Equals("False", StringComparison.OrdinalIgnoreCase))
                    lioConvertedValue = false;
            }
            else if (lioTargetType == typeof(DateTime))
            {
                if (DateTime.TryParse(livstrFormatted, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime livdtmVal))
                    lioConvertedValue = livdtmVal;
            }
            else
            {
                try
                {
                    lioConvertedValue = Convert.ChangeType(livstrFormatted, lioTargetType, CultureInfo.InvariantCulture);
                }
                catch { }
            }

            if (lioConvertedValue != null || (vioPropInfo.PropertyType == typeof(string) && vivstrRawValue != null))
            {
                vioPropInfo.SetValue(vioTargetObj, lioConvertedValue);
            }
        }
        #endregion
    }
}
