using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Microsoft.IdentityModel.Tokens;
using Nat.API.Properties;
using System.Linq;
using System.Text;
using System.Xml;
namespace Applet.Nat.Api.Br.Models
{

    public class InDocumentXMLNew : IRawDocument
    {
        #region CONSTRUCTORS
        public InDocumentXMLNew() { }
        public InDocumentXMLNew(long vivlngCuit, NatContext vioContext)
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

            if (string.IsNullOrEmpty(ivstrRaw)) return [];
            string livstr = Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty, Encoding.UTF8))),
                livstrApiDtmFormat = ListHelper.GetValue("FORMAT", "ApiDtm", mioContext);
            string[] cvstr;
            int livnumIdx;
            Object lioObj;
            StringBuilder lioSbErrors = new StringBuilder();
            ServiceMapper lioServiceMapper;
            List<DocumentUser> lcoDocumentsUser = new List<DocumentUser>();
            try
            {
                lioServiceMapper = GetMapper();
            }
            catch (Exception lioE)
            {
                LogHelper.writeinfo($"Documento Exceptuado: {mioXmlDocument.OuterXml}", ListHelper.Verbose(mioContext));
                return lcoDocumentsUser.ToArray();
            }
            if (string.IsNullOrEmpty(lioServiceMapper.ivstrSplitter))
                throw new Exception($"Separador de Documentos {Resources.lioE_ObjectNoM}");
            XmlDocument lioXmlDocument, lioXmlNodeDocument;
            DocumentUser lioDocumentUser = null;
            XmlNodeList lcoNodes = mioXmlDocument.SelectNodes(lioServiceMapper.ivstrSplitter);
            XmlNode lioCurrentNode, lioImportedNode;
            XmlNode? lioParentNode = null;
            if (lcoNodes == null || lcoNodes.Count == 0)
                lcoNodes = mioXmlDocument.SelectNodes("/");
            foreach (XmlNode lioXmlNodeDoc in lcoNodes)
            {
                try
                {
                    //Creo el XML copia del original
                    lioXmlDocument = new XmlDocument();
                    lioXmlDocument.LoadXml(mioXmlDocument.OuterXml);
                    //borro todos los nodos excepto el actual 
                    foreach (XmlNode lioXmlNode in lioXmlDocument.SelectNodes(lioServiceMapper.ivstrSplitter))
                    {
                        if (lioXmlNode.InnerXml == lioXmlNodeDoc.InnerXml)
                            continue;
                        lioXmlNode.ParentNode?.RemoveChild(lioXmlNode);
                    }
                    lioDocumentUser = new DocumentUser();
                    lioDocumentUser.ivstrWs = lioServiceMapper.ivstrWs;
                    lioDocumentUser.ivblnTaxInLines = lioServiceMapper.ivblnTaxInLines ?? false;
                    lioDocumentUser.ivblnCalcPermisoExistente = lioServiceMapper.ivblnCalcPermisoExistente ?? false;
                    lioDocumentUser.ivstrInputData = Format.Compress(Convert.ToBase64String(Encoding.UTF8.GetBytes(lioXmlDocument.OuterXml)));
                    //Mapeo de valores
                    lioSbErrors.Clear();
                    #region Campos Individuales (iv)
                    foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("iv")))
                    {
                        try
                        {
                            lioObj = GetItemValue(lioServiceMapperItem, lioXmlDocument, string.Empty);
                            if (lioObj != null)
                                typeof(DocumentUser).GetProperty(lioServiceMapperItem.ivstrProperty)?.SetValue(lioDocumentUser, lioObj);
                        }
                        catch (Exception lioE)
                        {
                            lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                        }
                    }
                    #endregion
                    #region Receptor
                    lioDocumentUser.ioDomicilioReceptor = new UxDomicilio();
                    foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("ioDomicilioReceptor")))
                    {
                        try
                        {
                            lioObj = GetItemValue(lioServiceMapperItem, lioXmlDocument, string.Empty);
                            if (lioObj != null)
                                typeof(UxDomicilio).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioDocumentUser.ioDomicilioReceptor, lioObj);
                        }
                        catch (Exception lioE)
                        {
                            lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                        }
                    }
                    #endregion
                    #region Asociados
                    bool livblnLoadChild;
                    XmlNodeList lioXmlNodeList;
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coAsociados")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        UxDocumentAsociado lioUxDocumentAsociado;
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                        livnumIdx = 1;
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentAsociado = new UxDocumentAsociado();
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coAsociados.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentAsociado).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentAsociado, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                                lioDocumentUser.coAsociados.Add(lioUxDocumentAsociado);
                            livnumIdx++;
                        }
                    }
                    lioDocumentUser.ioPeriodoAsociado = null;
                    foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("ioPeriodoAsociado.")))
                    {
                        try
                        {
                            if (lioDocumentUser.ioPeriodoAsociado == null)
                                lioDocumentUser.ioPeriodoAsociado = new UxPeriodoAsociado();
                            lioObj = GetItemValue(lioServiceMapperItem, lioXmlDocument, string.Empty);
                            if (lioObj != null)
                            {
                                typeof(UxPeriodoAsociado).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioDocumentUser.ioPeriodoAsociado, lioObj);
                            }
                        }
                        catch (Exception lioE)
                        {
                            lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                        }
                    }
                    #endregion
                    #region Otros Tributos
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOtrosTributos")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        UxDocumentOtroTributo lioUxDocumentOtroTributo;
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                        livnumIdx = 1;
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentOtroTributo = new UxDocumentOtroTributo { ivstrJurisdiccion = string.Empty };
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coOtrosTributos.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentOtroTributo).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentOtroTributo, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                                lioDocumentUser.coOtrosTributos.Add(lioUxDocumentOtroTributo);
                            livnumIdx++;
                        }
                    }
                    #endregion
                    #region IVA
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coIvas")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        UxDocumentIva lioUxDocumentIva;
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coIvas = new List<UxDocumentIva>();
                        livnumIdx = 1;

                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentIva = new UxDocumentIva();
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coIvas.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentIva).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentIva, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                            {
                                lioDocumentUser.coIvas.Add(lioUxDocumentIva);
                            }
                            livnumIdx++;
                        }
                    }
                    #endregion
                    #region Opcionales
                    UxDocumentOpcional lioUxDocumentOpcional;
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coOpcionales")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                        livnumIdx = 1;
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentOpcional = new UxDocumentOpcional();
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coOpcionales.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentOpcional).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentOpcional, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                                lioDocumentUser.coOpcionales.Add(lioUxDocumentOpcional);
                            livnumIdx++;
                        }
                    }
                    #endregion
                    #region Compradores
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coCompradores")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        UxDocumentComprador lioUxDocumentComprador;
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coCompradores = new List<UxDocumentComprador>();
                        livnumIdx = 1;
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentComprador = new UxDocumentComprador();
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coCompradores.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentComprador).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentComprador, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                                lioDocumentUser.coCompradores.Add(lioUxDocumentComprador);
                            livnumIdx++;
                        }
                    }
                    #endregion
                    #region Detalle
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coItems")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        UxDocumentItem lioUxDocumentItem;
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                        livnumIdx = 1;
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentItem = new UxDocumentItem();
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coItems.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentItem).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentItem, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                            {
                                lioDocumentUser.coItems.Add(lioUxDocumentItem);
                            }
                            livnumIdx++;
                        }
                    }
                    #endregion
                    #region Permisos
                    livstr = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "coPermisosExp")?.coXPaths[0].ivstrData;
                    if (!string.IsNullOrEmpty(livstr))
                    {
                        UxDocumentPermisoExp lioUxDocumentPermisoExp;
                        lioXmlNodeList = lioXmlDocument.SelectNodes(livstr);
                        lioDocumentUser.coPermisosExp = new List<UxDocumentPermisoExp>();
                        livnumIdx = 1;
                        foreach (XmlNode lioXmlNode in lioXmlNodeList)
                        {
                            lioXmlNodeDocument = new XmlDocument();
                            lioXmlNodeDocument.LoadXml(lioXmlNode.OuterXml);
                            lioUxDocumentPermisoExp = new UxDocumentPermisoExp();
                            livblnLoadChild = false;
                            foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coPermisosExp.")))
                            {
                                try
                                {
                                    lioObj = GetItemValue(lioServiceMapperItem, lioXmlNodeDocument, livnumIdx.ToString());
                                    if (lioObj != null)
                                    {
                                        typeof(UxDocumentPermisoExp).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioUxDocumentPermisoExp, lioObj);
                                        livblnLoadChild = true;
                                    }
                                }
                                catch (Exception lioE)
                                {
                                    lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                                }
                            }
                            if (livblnLoadChild)
                                lioDocumentUser.coPermisosExp.Add(lioUxDocumentPermisoExp);
                            livnumIdx++;
                        }
                    }
                    #endregion
                    #region Integracion
                    lioDocumentUser.ioIntegracion = new UxDocumentIntegracion();
                    foreach (ServiceMapperItem lioServiceMapperItem in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("ioIntegracion")))
                    {
                        try
                        {
                            lioObj = GetItemValue(lioServiceMapperItem, lioXmlDocument, string.Empty);
                            if (lioObj != null)
                                typeof(UxDocumentIntegracion).GetProperty(lioServiceMapperItem.ivstrProperty.Split(".").Last())?.SetValue(lioDocumentUser.ioIntegracion, lioObj);
                        }
                        catch (Exception lioE)
                        {
                            lioSbErrors.AppendLine($"Prop: {lioServiceMapperItem.ivstrProperty}: {lioE.Message}");
                        }
                    }
                    #endregion
                    #region Opcionales para FCredito
                    if (lioDocumentUser.ivnroTipoDoc == 201)
                    {
                        if (!string.IsNullOrEmpty(lioDocumentUser.ivstrCBU))
                        {
                            if (lioDocumentUser.coOpcionales == null)
                                lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                            lioUxDocumentOpcional = lioDocumentUser.coOpcionales.FirstOrDefault(x => x.ivstrId == "2101");
                            if (lioUxDocumentOpcional == null)
                            {
                                lioDocumentUser.coOpcionales.Add(new UxDocumentOpcional()
                                {
                                    ivstrId = "2101",
                                    ivstrValor = lioDocumentUser.ivstrCBU
                                });
                            }
                            else
                                lioUxDocumentOpcional.ivstrValor = lioDocumentUser.ivstrCBU;
                        }
                        if (!string.IsNullOrEmpty(lioDocumentUser.ivstrTransferencia))
                        {
                            if (lioDocumentUser.coOpcionales == null)
                                lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                            lioUxDocumentOpcional = lioDocumentUser.coOpcionales.FirstOrDefault(x => x.ivstrId == "27");
                            if (lioUxDocumentOpcional == null)
                            {
                                lioDocumentUser.coOpcionales.Add(new UxDocumentOpcional()
                                {
                                    ivstrId = "27",
                                    ivstrValor = lioDocumentUser.ivstrTransferencia
                                });
                            }
                            else
                                lioUxDocumentOpcional.ivstrValor = lioDocumentUser.ivstrTransferencia;
                        }
                    }
                    if (new short[] { 201, 202, 203 }.Contains(lioDocumentUser.ivnroTipoDoc ?? 0))
                    {
                        if (!string.IsNullOrEmpty(lioDocumentUser.ivstrAnulacion))
                        {
                            if (lioDocumentUser.coOpcionales == null)
                                lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                            lioUxDocumentOpcional = lioDocumentUser.coOpcionales.FirstOrDefault(x => x.ivstrId == "22");
                            if (lioUxDocumentOpcional == null)
                            {
                                lioDocumentUser.coOpcionales.Add(new UxDocumentOpcional()
                                {
                                    ivstrId = "22",
                                    ivstrValor = lioDocumentUser.ivstrAnulacion
                                });
                            }
                            else
                                lioUxDocumentOpcional.ivstrValor = lioDocumentUser.ivstrAnulacion;
                        }
                    }
                    #endregion
                    #region Permisos no iterativos
                    if (new short[] { 19, 20, 21 }.Contains(lioDocumentUser.ivnroTipoDoc ?? 0))
                    {
                        if (lioDocumentUser.coPermisosExp == null || lioDocumentUser.coPermisosExp.Count() == 0)
                            // no hay estructura iterativa de permisos, la crea con los valores de campos no repetitivos si los hubiera
                            lioDocumentUser.coPermisosExp = new List<UxDocumentPermisoExp>
                            {
                                new UxDocumentPermisoExp
                                    {
                                        ivstrId = lioDocumentUser.ivstrPEId ?? string.Empty,
                                        ivnumDestMerc = lioDocumentUser.ivnumPEDestMerc ?? 0,
                                    }
                            };
                        foreach (UxDocumentPermisoExp lioO in lioDocumentUser.coPermisosExp)
                        {
                            // para cada permiso completa campos con los datos no repetitivos si los hubiera
                            if (string.IsNullOrEmpty(lioO.ivstrId?.Trim()))
                                lioO.ivstrId = lioDocumentUser.ivstrPEId ?? string.Empty;
                            if (lioO.ivnumDestMerc == null || lioO.ivnumDestMerc == 0)
                                lioO.ivnumDestMerc = lioDocumentUser.ivnumPEDestMerc ?? 0;
                        }
                        //borra permisos duplicados y en blanco
                        List<UxDocumentPermisoExp> lcoPermisosExp = new List<UxDocumentPermisoExp>();
                        string livstrCurrPEId = string.Empty;
                        foreach (UxDocumentPermisoExp lioO in lioDocumentUser.coPermisosExp.OrderBy(x => x.ivstrId))
                        {
                            if (string.IsNullOrEmpty(lioO.ivstrId?.Trim())) continue;
                            if (lioO.ivnumDestMerc == null || lioO.ivnumDestMerc == 0) continue;
                            if (lioO.ivstrId == livstrCurrPEId) continue;
                            livstrCurrPEId = lioO.ivstrId;
                            lcoPermisosExp.Add(lioO);
                        }
                        lioDocumentUser.coPermisosExp = lcoPermisosExp;
                    }
                    if (lioSbErrors.Length > 0)
                    {
                        LogHelper.writeinfo(lioDocumentUser.ivstrKey,true);
                        LogHelper.writeinfo(lioSbErrors.ToString(), true);
                    }
                    #endregion
                    lcoDocumentsUser.Add(lioDocumentUser);
                }
                catch (Exception lioE)
                {
                    LogHelper.write(lioE);
                    lcoDocumentsUser.Add(lioDocumentUser);
                }
            }
            return lcoDocumentsUser.ToArray();
        }
        public string ToPrint()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw, Encoding.UTF8)));
        }
        #endregion
        #region PRIVATE PROPS
        private long mivlngCuit;
        private NatContext mioContext;
        private XmlDocument mioXmlDocument
        {
            get
            {
                if (_mioXmlDocument == null)
                {
                    _mioXmlDocument = new XmlDocument();
                    string livstr = Encoding.UTF8.GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty, Encoding.UTF8)));
                    _mioXmlDocument.LoadXml(livstr);
                }
                return _mioXmlDocument;
            }
        }
        private XmlDocument _mioXmlDocument;
        #endregion
        #region PRIVATE METHODS  
        private object GetItemValue(ServiceMapperItem vioMapperItem, XmlDocument vioXmlDocument, string vivstrIndex)
        {
            string livstr = string.Empty,
                livstrValue = string.Empty,
                livstrXmlDtmFormat = ListHelper.GetValue("FORMAT", "XmlDtm", mioContext),
                livstrApiDtmFormat = ListHelper.GetValue("FORMAT", "ApiDtm", mioContext);
            int livnumIdx,
                livnumLen,
                livnum;
            short livnro;
            long livlng;
            double livval;
            DateTime livdtm;
            XmlNode lioXmlNode;
            if (!string.IsNullOrEmpty(vioMapperItem.ivstrCoord) && vioMapperItem.ivstrCoord.Contains("FIX"))
                livstrValue = vioMapperItem.ivstrformat ?? string.Empty;
            else
            {
                foreach (ServiceMapperItemXPath lioMapperItemXPath in vioMapperItem.coXPaths)
                {
                    if (string.IsNullOrEmpty(lioMapperItemXPath?.ivstrData))
                        continue;
                    lioXmlNode = vioXmlDocument.SelectSingleNode(lioMapperItemXPath.ivstrData);
                    if (lioXmlNode == null || string.IsNullOrEmpty(lioXmlNode.InnerXml))
                        continue;
                    livstrValue = lioXmlNode.InnerXml;
                    if (!string.IsNullOrEmpty(lioMapperItemXPath.ivstrCoord))
                    { //Si tiene coordenadas, corto el string
                        livnumIdx = Convert.ToInt32(lioMapperItemXPath.ivstrCoord.Split(',')[0]);
                        livnumLen = Convert.ToInt32(lioMapperItemXPath.ivstrCoord.Split(',')[1]);
                        if (livstrValue.Length < livnumIdx)
                            throw new Exception($"{vioMapperItem.ivstrProperty} {Resources.lioE_ObjectNoM} en Mapeador");
                        if (livstrValue.Length < (livnumIdx + livnumLen))
                            livnumLen = livstrValue.Length - livnumIdx;
                        livstrValue = livstrValue.Substring(livnumIdx, livnumLen);
                    }
                    break;
                }
                if (vioMapperItem.coConversion != null && vioMapperItem.coConversion.ContainsKey(livstrValue))
                    livstrValue = vioMapperItem.coConversion[livstrValue];
            }
            if (string.IsNullOrEmpty(livstrValue) && (!string.IsNullOrEmpty(vioMapperItem.ivstrDefault)))
                livstrValue = vioMapperItem.ivstrDefault;
            if (string.IsNullOrEmpty(livstrValue))
            {
                if (vioMapperItem.ivblnRequired ?? false)
                    throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                else
                    return null;
            }
            if (vioMapperItem.ivstrProperty.Split(".").Last().StartsWith("ivstrFecha"))
            {
                //Formato de fecha especial
                if (!DateTime.TryParseExact(livstrValue, livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                    throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                return livdtm.ToString(livstrApiDtmFormat);
            }
            switch (vioMapperItem.ivstrProperty.Split(".").Last().Substring(2, 3))
            {
                case "nro":
                    if (!short.TryParse(livstrValue, out livnro))
                        throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                    return livnro;
                case "num":
                    if (!int.TryParse(livstrValue, out livnum))
                        throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                    return livnum;
                case "lng":
                    if (!long.TryParse(livstrValue, out livlng))
                        throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                    return livlng;
                case "dbl":
                case "val":
                    if (!double.TryParse(livstrValue, out livval))
                        throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                    return livval;
                case "dtm":
                    if (!DateTime.TryParseExact(livstrValue, livstrXmlDtmFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                        throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
                    return livdtm;
                case "str":
                    return livstrValue;
                default:
                    throw new Exception($"{Resources.ResourceManager.GetString("lioP_" + vioMapperItem.ivstrProperty)} [{vioMapperItem.ivstrProperty}] {Resources.lioE_ObjectNoM}");
            }
        }
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
            //Extraigo el tipo de comprobante del primer mapeador asumiendo que es para todos iguales
            ServiceMapper lioServiceMapper = lioCuit.ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrInputType == "xml");
            ServiceMapperItem lioServiceMapperItem = lioServiceMapper.coItems.FirstOrDefault(x => x.ivstrProperty == "ivnroTipoDoc");
            if (lioServiceMapperItem == null || lioServiceMapperItem.coXPaths == null || lioServiceMapperItem.coXPaths.Count() == 0)
                throw new Exception($"Tipo de Comprobante {Resources.lioE_ObjectNoM} en Mapeador");
            short livnroTipoDoc = (short)GetItemValue(lioServiceMapperItem, mioXmlDocument, string.Empty);
            lioServiceMapper = lioCuit.ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrInputType == "xml" && x.cvnroDocTypes.Contains(livnroTipoDoc));
            if (lioServiceMapper == null)
                throw new Exception($"Mapeador {Resources.lioE_ObjectNoM}");
            if (string.IsNullOrEmpty(lioServiceMapper.ivstrWs))
                throw new Exception($"Servicio {Resources.lioE_ObjectNoM} en configuracion del CUIT");
            return lioServiceMapper;
        }
        #endregion
    }
}

