using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Microsoft.IdentityModel.Tokens;
using Nat.API.Properties;
using System.Globalization;
using System.Text;
using System.Xml;
namespace Applet.Nat.Api.Br.Models
{
    public class InDocumentTXT : IRawDocument
    {
        #region CONS
        public InDocumentTXT(long vivlngCuit, NatContext vioContext)
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
                string livstr,
                livstrApiDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext),
                livstrXmlDtmFormat = ListHelper.GetValue("FORMAT", "XmlDtm", mioContext),
                livstrIdInicio,
                livstrLine,
                livstrPropertyValue;
                DateTime livdtm;
                short livnro, livnroI;
                int livnum, livnumLine, livnumOffset, livnumFrom, livnumLen;
                long livlng;
                double livval = 0;
                bool livblnOK;
                #region Configuracion desde CUIT
                Cuit lioCuit = new Cuit(mivlngCuit, mioContext, null);
                if (lioCuit.ioDcModel == null || lioCuit.ioDcModel.ivstrCnfg == null)
                    throw new Exception($"CUIT invalido o {Resources.lioE_ObjectNoM}");
                if (string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                    throw new Exception("Configuracion de C.U.I.T. {Resources.lioE_ObjectNoF}");
                if (string.IsNullOrEmpty(ivstrName) || ivstrName.Split('_').Length < 2 || !short.TryParse(ivstrName.Split('_')[0], out livnro))
                    throw new Exception("Nombre de Archivo de Ingreso invalido (N_...");
                ServiceMapper lioServiceMapper = lioCuit.ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrInputType == "txt" && x.cvnroDocTypes.Contains(livnro));
                if (lioServiceMapper == null)
                    throw new Exception($"Mapeador {Resources.lioE_ObjectNoM}");
                if (string.IsNullOrEmpty(lioServiceMapper.ivstrWs))
                    throw new Exception($"Servicio {Resources.lioE_ObjectNoM} en configuracion del CUIT");
                #endregion
                livstr = lioCuit.GetEncoding().GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty, lioCuit.GetEncoding())));
                string[] cvstrInDocumentLines = livstr.Split("\n");
                UxDocumentAsociado lioUxDocumentAsociado;
                UxDocumentOtroTributo lioUxDocumentOtroTributo;
                UxDocumentOpcional lioUxDocumentOpcional;
                UxDocumentItem lioUxDocumentItem;
                UxDocumentIva lioUxDocumentIva = null;
                UxDocumentComprador lioUxDocumentComprador;
                UxDocumentItemCT lioUxDocumentItemCT;
                StringBuilder lioSbErrors = new StringBuilder();
                lioDocumentUser = new DocumentUser();
                lioDocumentUser.ivstrWs = lioServiceMapper.ivstrWs;
                lioDocumentUser.ivstrInputData = ivstrRaw;
                lioDocumentUser.ioDomicilioReceptor = new UxDomicilio();
                foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems)
                {
                    //busca coordenadas desde el xpath
                    if (!GetCoordinates(lioMapperItem.ivstrCoord, out livstrIdInicio, out livnum, out livnumOffset, out livnumLen))
                    {
                        lioSbErrors.AppendLine($"{lioMapperItem.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                        continue;
                    }
                    //busca linea de inicio
                    livstr = cvstrInDocumentLines.FirstOrDefault(x => x.StartsWith(livstrIdInicio)) ?? string.Empty;
                    if (string.IsNullOrEmpty(livstr))
                        continue;
                    //se para en la linea del identificador de inicio
                    livnumLine = cvstrInDocumentLines.ToList().IndexOf(livstr);
                    #region Campos de tipo U (unico)
                    if (lioMapperItem.ivstrCoord.StartsWith("U"))
                    {
                        // se para en la linea relativa
                        livnumLine += livnum;
                        if (cvstrInDocumentLines[livnumLine].Length < livnumOffset) continue;
                        if (lioMapperItem.ivstrCoord.Contains("FIX"))
                            livstrPropertyValue = lioMapperItem.ivstrformat;
                        else if (livnumLen > 0)
                            livstrPropertyValue = cvstrInDocumentLines[livnumLine].Substring(livnumOffset - 1, livnumLen).Trim();
                        else
                            livstrPropertyValue = cvstrInDocumentLines[livnumLine].Substring(livnumOffset - 1).Trim();
                        livstrPropertyValue = Format.Property(lioMapperItem.ivstrProperty, livstrPropertyValue);
                        if (string.IsNullOrEmpty(livstrPropertyValue) && (!string.IsNullOrEmpty(lioMapperItem.ivstrDefault)))
                            livstrPropertyValue = lioMapperItem.ivstrDefault;
                        switch (lioMapperItem.ivstrProperty)
                        {
                            case "ivnroTipoDoc":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                {
                                    lioSbErrors.AppendLine($"Tipo de comprobante {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnroTipoDoc = livnro;
                                break;
                            case "ivnumPvta":
                                if (!int.TryParse(livstrPropertyValue, out livnum))
                                {
                                    lioSbErrors.AppendLine($"Punto de venta {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnumPvta = livnum;
                                break;
                            case "ivlngCbte":
                                if (!long.TryParse(livstrPropertyValue, out livlng))
                                {
                                    lioSbErrors.AppendLine($"Numero de comprobante {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivlngCbte = livlng;
                                break;
                            case "ivdtmEmision":
                            case "ivstrFechaEmision":
                                if (!GetDateFromProp(lioMapperItem, livstrPropertyValue, out livdtm))
                                {
                                    lioSbErrors.AppendLine($"Fecha de Comprobante INVALIDA ({livstrXmlDtmFormat})");
                                    continue;
                                }
                                lioDocumentUser.ivstrFechaEmision = livdtm.ToString(livstrApiDtmFormat);
                                break;
                            case "ivstrCondPago":
                                lioDocumentUser.ivstrCondPago = livstrPropertyValue;
                                break;
                            case "ivstrFechaVtopago":
                                if (!GetDateFromProp(lioMapperItem, livstrPropertyValue, out livdtm))
                                {
                                    lioSbErrors.AppendLine($"FECHA de Vencimiento de Pago INVALIDA ({livstrXmlDtmFormat})");
                                    continue;
                                }
                                lioDocumentUser.ivstrFechaVtopago = livdtm.ToString(livstrApiDtmFormat);
                                break;
                            case "ivstrFechaServdesde":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                {
                                    if (!GetDateFromProp(lioMapperItem, livstrPropertyValue, out livdtm))
                                    {
                                        lioSbErrors.AppendLine($"FECHA de Inicio de Servicios INVALIDA ({livstrXmlDtmFormat})");
                                        continue;
                                    }
                                    lioDocumentUser.ivstrFechaServdesde = livdtm.ToString(livstrApiDtmFormat);
                                }
                                break;
                            case "ivstrFechaServhasta":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                {
                                    if (!GetDateFromProp(lioMapperItem, livstrPropertyValue, out livdtm))
                                    {
                                        lioSbErrors.AppendLine($"FECHA de Finalizacion de Servicios INVALIDA ({livstrXmlDtmFormat})");
                                        continue;
                                    }
                                    lioDocumentUser.ivstrFechaServhasta = livdtm.ToString(livstrApiDtmFormat);
                                }
                                break;
                            case "ivstrMoneda":
                                if (string.IsNullOrEmpty(livstrPropertyValue))
                                {
                                    lioSbErrors.AppendLine("Moneda {Resources.lioE_ObjectNoF}");
                                    continue;
                                }
                                lioDocumentUser.ivstrMoneda = livstrPropertyValue;
                                break;
                            case "ivnroConcepto":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                {
                                    lioSbErrors.AppendLine($"Concepto {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnroConcepto = livnro;
                                break;
                            case "ivdblCotizacion":
                                if (!double.TryParse(livstrPropertyValue, out livval))
                                    lioDocumentUser.ivdblCotizacion = 1;
                                else
                                    lioDocumentUser.ivdblCotizacion = livval;
                                break;
                            case "ivstrObs":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrObs = livstrPropertyValue;
                                break;
                            case "ivstrIdCliente":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrIdCliente = livstrPropertyValue;
                                break;
                            case "ivstrIdSucursal":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrIdSucursal = livstrPropertyValue;
                                break;
                            case "ivstrCanMisMonExt":
                                if (string.IsNullOrEmpty(livstrPropertyValue))
                                {
                                    lioSbErrors.AppendLine($"CanMisMonExt {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivstrCanMisMonExt = livstrPropertyValue;
                                break;
                            case "ivnroIdioma":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                    lioDocumentUser.ivnroIdioma = 1;
                                else
                                    lioDocumentUser.ivnroIdioma = livnro;
                                break;
                            case "ivnroTipoExpo":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                {
                                    lioSbErrors.AppendLine($"Tipo Exportacion {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnroTipoExpo = livnro;
                                break;
                            case "ivstrIncoterms":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrIncoterms = livstrPropertyValue;
                                break;
                            case "ivstrIncotermsDs":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrIncotermsDs = livstrPropertyValue;
                                break;
                            case "ivnroDestinoCmp":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                {
                                    lioSbErrors.AppendLine($"Pais Destino {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnroDestinoCmp = livnro;
                                break;
                            case "ivlngCuitPaisCliente":
                                if (!long.TryParse(livstrPropertyValue, out livlng))
                                {
                                    lioSbErrors.AppendLine($"Cuit Pais Cliente {Resources.lioE_ObjectNoF}");
                                    continue;
                                }
                                lioDocumentUser.ivlngCuitPaisCliente = livlng;
                                break;
                            case "ivlngIDImpositivo":
                                if (long.TryParse(livstrPropertyValue, out livlng))
                                {
                                    lioDocumentUser.ivlngIDImpositivo = livlng;
                                }
                                break;
                            case "ivstrPermisoExistente":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrPermisoExistente = livstrPropertyValue;
                                break;
                            #region Emisor
                            case "ivlngCuitEmisor":
                                if (!long.TryParse(livstrPropertyValue, out livlng))
                                {
                                    lioSbErrors.AppendLine($"CUIT Emisor {Resources.lioE_ObjectNoF}");
                                    continue;
                                }
                                if (livlng != mivlngCuit)
                                {
                                    lioSbErrors.AppendLine($"CUIT Emisor {Resources.lioE_ObjectNoF}");
                                    continue;
                                }
                                lioDocumentUser.ivlngCuitEmisor = livlng;
                                break;
                            #endregion
                            #region Receptor
                            case "ivlngDocReceptor":
                                if (!long.TryParse(livstrPropertyValue, out livlng))
                                {
                                    lioSbErrors.AppendLine($"Numero de documento receptor {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivlngDocReceptor = livlng;
                                break;
                            case "ivnroTipoDocReceptor":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                {
                                    lioSbErrors.AppendLine($"Tipo de documento receptor {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnroTipoDocReceptor = livnro;
                                break;
                            case "ivstrRazonSocial":
                                if (string.IsNullOrEmpty(livstrPropertyValue))
                                {
                                    lioSbErrors.AppendLine($"Razon Social Receptor {Resources.lioE_ObjectNoF}");
                                    continue;
                                }
                                lioDocumentUser.ivstrRazonSocial = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrCalle":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrCalle = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrNro":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrNro = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrPiso":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrPiso = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrDepto":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrDepto = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrCuidad":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrCuidad = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrMunicipio":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrMunicipio = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivnroPcia":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                    continue;
                                lioDocumentUser.ioDomicilioReceptor.ivnroPcia = livnro;
                                break;
                            case "ioDomicilioReceptor.ivstrPais":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrPais = livstrPropertyValue;
                                break;
                            case "ioDomicilioReceptor.ivstrCP":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ioDomicilioReceptor.ivstrCP = livstrPropertyValue;
                                break;
                            case "ivnroTipoRespReceptor":
                                if (!short.TryParse(livstrPropertyValue, out livnro))
                                {
                                    lioSbErrors.AppendLine($"Condicion Iva receptor {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivnroTipoRespReceptor = livnro;
                                break;
                            case "ivstrEmail":
                                if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    lioDocumentUser.ivstrEmail = livstrPropertyValue;
                                break;
                            #endregion
                            #region Importes
                            case "ivdblImporteTotal":
                                if (!double.TryParse(livstrPropertyValue, out livval))
                                {
                                    lioSbErrors.AppendLine($"Importe Total {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivdblImporteTotal = livval;
                                break;
                            case "ivdblImporteGravado":
                                if (!double.TryParse(livstrPropertyValue, out livval))
                                {
                                    lioSbErrors.AppendLine($"Importe Gravado {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivdblImporteGravado = livval;
                                break;
                            case "ivdblImporteNoGravado":
                                if (!double.TryParse(livstrPropertyValue, out livval))
                                {
                                    lioSbErrors.AppendLine($"Importe NoGravado {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivdblImporteNoGravado = livval;
                                break;
                            case "ivdblImporteExento":
                                if (!double.TryParse(livstrPropertyValue, out livval))
                                {
                                    lioSbErrors.AppendLine($"Importe Exento {Resources.lioE_ObjectNoM}");
                                    continue;
                                }
                                lioDocumentUser.ivdblImporteExento = livval;
                                break;
                            case "ivdblImporteIva":
                                if (!double.TryParse(livstrPropertyValue, out livval))
                                    continue;
                                lioDocumentUser.ivdblImporteIva = livval;
                                break;
                                #endregion
                        }
                        if (lioMapperItem.ivstrProperty.StartsWith("coIvas"))  // caso iva unico 
                        {
                            if (lioDocumentUser.coIvas == null)
                            {
                                lioDocumentUser.coIvas = new List<UxDocumentIva>();
                                lioUxDocumentIva = new UxDocumentIva();
                                lioDocumentUser.coIvas.Add(lioUxDocumentIva);
                            }
                            switch (lioMapperItem.ivstrProperty)
                            {
                                case "coIvas.ivnroTipo":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo Iva {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    lioUxDocumentIva.ivnroTipo = livnro;
                                    break;
                                case "coIvas.ivdblBaseImponible":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Base Imponible IVA {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    lioUxDocumentIva.ivdblBaseImponible = livval;
                                    break;
                                case "coIvas.ivdblImporte":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe IVA {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    lioUxDocumentIva.ivdblImporte = livval;
                                    lioDocumentUser.ivdblImporteIva = livval;
                                    break;
                            }
                        }
                    }
                    #endregion
                    #region Campos de tipo R (repetitivos en fila)
                    if (lioMapperItem.ivstrCoord.StartsWith("R"))
                    {
                        #region Asociados
                        if (lioMapperItem.ivstrProperty.StartsWith("coAsociados"))
                        {
                            if (lioDocumentUser.coAsociados != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentAsociado = new UxDocumentAsociado();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coAsociados.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($"{lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumOffset) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coAsociados.ivnroCbtetipo":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo de comprobante asociado {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentAsociado.ivnroCbtetipo = livnro;
                                            break;
                                        case "coAsociados.ivnumCbtePuntovta":
                                            if (!int.TryParse(livstrPropertyValue, out livnum))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Punto de Venta asociado {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentAsociado.ivnumCbtePuntovta = livnum;
                                            break;
                                        case "coAsociados.ivlngCbteNro":
                                            if (!long.TryParse(livstrPropertyValue, out livlng))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Nro de Comprobante asociado {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentAsociado.ivlngCbteNro = livlng;
                                            break;
                                        case "coAsociados.ivlngCbteCUIT":
                                            if (!long.TryParse(livstrPropertyValue, out livlng))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: CUIT asociado {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentAsociado.ivlngCbteCUIT = livlng;
                                            break;
                                        case "coAsociados.ivstrFechaEmision":
                                            livdtm = DateTime.MinValue;
                                            if (!GetDateFromProp(lioMapperItemR, livstrPropertyValue, out livdtm))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Fecha de Comprobante asociado INVALIDA");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentAsociado.ivstrFechaEmision = livdtm.ToString(livstrApiDtmFormat);
                                            break;
                                    }
                                }
                                if (livblnOK)
                                    lioDocumentUser.coAsociados.Add(lioUxDocumentAsociado);
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                        #region OtroTributo
                        else if (lioMapperItem.ivstrProperty.StartsWith("coOtrosTributos"))
                        {
                            if (lioDocumentUser.coOtrosTributos != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentOtroTributo = new UxDocumentOtroTributo();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coOtrosTributos.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($"{lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumFrom) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coOtrosTributos.ivnroId":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Id Otros Tributos {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentOtroTributo.ivnroId = livnro;
                                            break;
                                        case "coOtrosTributos.ivstrDesc":
                                            if (!string.IsNullOrEmpty(livstrPropertyValue))
                                                lioUxDocumentOtroTributo.ivstrDesc = livstrPropertyValue;
                                            break;
                                        case "coOtrosTributos.ivdblBaseImp":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Base Imponible Otros Tributos {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentOtroTributo.ivdblBaseImponible = livval;
                                            break;
                                        case "coOtrosTributos.ivdblAlicuota":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Alicuota Otros Tributos {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentOtroTributo.ivdblAlicuota = livval;
                                            break;
                                        case "coOtrosTributos.ivdblImporte":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe Otros Tributos {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentOtroTributo.ivdblImporte = livval;
                                            break;
                                    }
                                }
                                if (livblnOK)
                                {
                                    lioDocumentUser.coOtrosTributos.Add(lioUxDocumentOtroTributo);
                                }
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                        #region Ivas
                        else if (lioMapperItem.ivstrProperty.StartsWith("coIvas"))
                        {
                            if (lioDocumentUser.coIvas != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coIvas = new List<UxDocumentIva>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentIva = new UxDocumentIva();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coIvas.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($" {lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumFrom) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coIvas.ivnroTipo":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo Iva {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentIva.ivnroTipo = livnro;
                                            break;
                                        case "coIvas.ivdblBaseImponible":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Base Imponible IVA {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentIva.ivdblBaseImponible = livval;
                                            break;
                                        case "coIvas.ivdblImporte":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe IVA {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentIva.ivdblImporte = livval;
                                            break;
                                    }
                                }
                                if (livblnOK)
                                {
                                    lioDocumentUser.coIvas.Add(lioUxDocumentIva);
                                }
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                        #region Opcionales
                        else if (lioMapperItem.ivstrProperty.StartsWith("coOpcionales"))
                        {
                            if (lioDocumentUser.coOpcionales != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentOpcional = new UxDocumentOpcional();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coOpcionales.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($" {lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumFrom) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coOpcionales.ivstrId":
                                            if (string.IsNullOrEmpty(livstrPropertyValue))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Id Opcional {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentOpcional.ivstrId = livstrPropertyValue;
                                            break;
                                        case "coOpcionales.ivstrValor":
                                            if (string.IsNullOrEmpty(livstrPropertyValue))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Valor Opcional {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentOpcional.ivstrValor = livstrPropertyValue;
                                            break;
                                    }
                                }
                                if (livblnOK)
                                    lioDocumentUser.coOpcionales.Add(lioUxDocumentOpcional);
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                        #region Compradores
                        else if (lioMapperItem.ivstrProperty.StartsWith("coCompradores"))
                        {
                            if (lioDocumentUser.coCompradores != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coCompradores = new List<UxDocumentComprador>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentComprador = new UxDocumentComprador();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coCompradores.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($" {lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumFrom) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coCompradores.ivnroDocTipo":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo Comprador {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentComprador.ivnroDocTipo = livnro;
                                            break;
                                        case "coCompradores.ivlngDocNro":
                                            if (!long.TryParse(livstrPropertyValue, out livlng))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Nro Comprador {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentComprador.ivlngDocNro = livlng;
                                            break;
                                        case "coCompradores.ivdblPorcentaje":
                                            if (!double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Procentaje Comprador {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentComprador.ivdblPorcentaje = livval;
                                            break;
                                    }
                                }
                                if (livblnOK)
                                    lioDocumentUser.coCompradores.Add(lioUxDocumentComprador);
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                        #region Items
                        else if (lioMapperItem.ivstrProperty.StartsWith("coItems"))
                        {
                            if (lioDocumentUser.coItems != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coItems = new List<UxDocumentItem>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentItem = new UxDocumentItem();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coItems.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($" {lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumFrom) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coItems.ivstrId":
                                            if (string.IsNullOrEmpty(livstrPropertyValue))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Id Item {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivstrId = livstrPropertyValue;
                                            break;
                                        case "coItems.ivstrDescripcion":
                                            if (string.IsNullOrEmpty(livstrPropertyValue))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Descripcion Item {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivstrDescripcion = livstrPropertyValue;
                                            break;

                                        case "coItems.ivdblCantidad":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Cantidad Imponible {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivdblCantidad = livval;
                                            break;
                                        case "coItems.ivdblPrecioUnitario":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Precio Unitario {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivdblPrecioUnitario = livval;
                                            break;
                                        case "coItems.ivdblBonificaion":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Boinificacion {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivdblBonificaion = livval;
                                            break;
                                        case "coItems.ivnroUM":
                                            if (short.TryParse(livstrPropertyValue, out livnro))
                                                lioUxDocumentItem.ivnroUM = livnro;
                                            else
                                                lioUxDocumentItem.ivnroUM = 0;

                                            //{
                                            //    lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Unidad de Medida {Resources.lioE_ObjectNoF}");
                                            //    livblnOK = false;
                                            //    continue;
                                            //}
                                            break;
                                        case "coItems.ivdblImporteTotal":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe Total {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivdblImporteTotal = livval;
                                            break;
                                        case "coItems.ivnroTipoIVA":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo IVA {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivnroTipoIVA = livnro;
                                            break;

                                        case "coItems.ivdblImporteIVA":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe IVA {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItem.ivdblImporteIVA = livval;
                                            break;
                                    }
                                }
                                if (livblnOK)
                                    lioDocumentUser.coItems.Add(lioUxDocumentItem);
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                        #region ItemsCT
                        else if (lioMapperItem.ivstrProperty.StartsWith("coItemsCT"))
                        {
                            if (lioDocumentUser.coItemsCT != null) // si ya tiene asociados, no los vuelve a cargar
                                continue;
                            lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                            livnumLine++; //saltea linea de titulos
                            while (true)
                            {
                                livnumLine++; //recorre las lineas hasta el porximo identificador "*"
                                livstrLine = cvstrInDocumentLines[livnumLine];
                                if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                                    continue;
                                if (livstrLine.Trim().StartsWith("*") || livstrLine.Trim().StartsWith("XXXFINDOC"))
                                    break;
                                lioUxDocumentItemCT = new UxDocumentItemCT();
                                livblnOK = true;
                                foreach (ServiceMapperItem lioMapperItemR in lioServiceMapper.coItems.Where(x => x.ivstrProperty.StartsWith("coItemsCT.")))
                                {
                                    if (!GetCoordinates(lioMapperItemR.coXPaths[0].ivstrData, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                                    {
                                        lioSbErrors.AppendLine($" {lioMapperItemR.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (livstrLine.Length < livnumFrom) continue;
                                    if (livstrLine.Length < livnumFrom - 1 + livnumOffset)
                                        livnumOffset = livstrLine.Length - livnumFrom + 1;
                                    if (lioMapperItemR.ivstrCoord.Contains("FIX"))
                                        livstrPropertyValue = lioMapperItemR.ivstrformat;
                                    else
                                        livstrPropertyValue = livstrLine.Substring(livnumFrom - 1, livnumOffset).Trim();
                                    livstrPropertyValue = Format.Property(lioMapperItemR.ivstrProperty, livstrPropertyValue);
                                    switch (lioMapperItemR.ivstrProperty)
                                    {
                                        case "coItemsCT.ivnroTipo":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo Documento {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItemCT.ivnroTipo = livnro;
                                            break;
                                        case "coItemsCT.ivnroCodigoTurismo":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Codigo Turismo {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItemCT.ivnroCodigoTurismo = livnro;
                                            break;
                                        case "coItemsCT.ivstrCodigo":
                                            if (string.IsNullOrEmpty(livstrPropertyValue))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Codigo {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItemCT.ivstrCodigo = livstrPropertyValue;
                                            break;
                                        case "coItemsCT.ivstrDesc":
                                            if (string.IsNullOrEmpty(livstrPropertyValue))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Descripcion {Resources.lioE_ObjectNoF}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItemCT.ivstrDesc = livstrPropertyValue;
                                            break;
                                        case "coItemsCT.ivnroTipoIVA":
                                            if (!short.TryParse(livstrPropertyValue, out livnro))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo IVA {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItemCT.ivnroTipoIVA = livnro;
                                            break;
                                        case "coItemsCT.ivdblImporteItem":
                                            if (!Double.TryParse(livstrPropertyValue, out livval))
                                            {
                                                lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe {Resources.lioE_ObjectNoM}");
                                                livblnOK = false;
                                                continue;
                                            }
                                            lioUxDocumentItemCT.ivdblImporteItem = livval;
                                            break;
                                    }
                                }
                                if (livblnOK)
                                    lioDocumentUser.coItemsCT.Add(lioUxDocumentItemCT);
                                if (lioSbErrors.Length > 0)
                                    throw new Exception(lioSbErrors.ToString());
                            }
                        }
                        #endregion
                    }
                    #endregion
                    #region Campos de tipo C (repetitivos en columnas)
                    if (lioMapperItem.ivstrCoord.StartsWith("C"))
                    {
                        // livnumFrom: nro de fila
                        // livnumOffset: cada cuanto se repite,
                        // livnumLen: cuanto ocupa
                        if (!GetCoordinates(lioMapperItem.ivstrCoord, out livstrIdInicio, out livnumFrom, out livnumOffset, out livnumLen))
                        {
                            lioSbErrors.AppendLine($" {lioMapperItem.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                            livblnOK = false;
                            continue;
                        }
                        livstrLine = cvstrInDocumentLines[livnumLine + livnumFrom]; //linea real a leer
                        if (string.IsNullOrEmpty(livstrLine.Trim()) || livstrLine.Trim() == "\r")
                            continue;
                        int livnumColumn = 0;
                        while (livnumOffset < livstrLine.Length)
                        {
                            livnumColumn++;
                            if (livstrLine.Length < livnumLen + livnumOffset)
                                livnumLen = livstrLine.Length - livnumOffset; // ajusta el offset al final de la linea
                            if (lioMapperItem.ivstrCoord.Contains("FIX"))
                                livstrPropertyValue = lioMapperItem.ivstrformat;
                            else
                                livstrPropertyValue = livstrLine.Substring(livnumOffset, livnumLen).Trim();
                            livstrPropertyValue = Format.Property(lioMapperItem.ivstrProperty, livstrPropertyValue);
                            livnumOffset += livnumLen;
                            switch (lioMapperItem.ivstrProperty)
                            {
                                case "coAsociados.ivnroCbtetipo":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo de comprobante asociado {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coAsociados == null)
                                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                                    if (lioDocumentUser.coAsociados.Count() < livnumColumn)
                                        lioDocumentUser.coAsociados.Add(new UxDocumentAsociado());
                                    lioDocumentUser.coAsociados[livnumColumn - 1].ivnroCbtetipo = livnro;
                                    break;
                                case "coAsociados.ivnumCbtePuntovta":
                                    if (!int.TryParse(livstrPropertyValue, out livnum))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Punto de Venta asociado {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coAsociados == null)
                                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                                    if (lioDocumentUser.coAsociados.Count() < livnumColumn)
                                        lioDocumentUser.coAsociados.Add(new UxDocumentAsociado());
                                    lioDocumentUser.coAsociados[livnumColumn - 1].ivnumCbtePuntovta = livnum;
                                    break;
                                case "coAsociados.ivlngCbteNro":
                                    if (!long.TryParse(livstrPropertyValue, out livlng))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Nro de Comprobante asociado {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coAsociados == null)
                                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                                    if (lioDocumentUser.coAsociados.Count() < livnumColumn)
                                        lioDocumentUser.coAsociados.Add(new UxDocumentAsociado());
                                    lioDocumentUser.coAsociados[livnumColumn - 1].ivlngCbteNro = livlng;
                                    break;
                                case "coAsociados.ivlngCbteCUIT":
                                    if (!long.TryParse(livstrPropertyValue, out livlng))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: CUIT asociado {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coAsociados == null)
                                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                                    if (lioDocumentUser.coAsociados.Count() < livnumColumn)
                                        lioDocumentUser.coAsociados.Add(new UxDocumentAsociado());
                                    lioDocumentUser.coAsociados[livnumColumn - 1].ivlngCbteCUIT = livlng;
                                    break;
                                case "coAsociados.ivstrFechaEmision":
                                    if (!GetDateFromProp(lioMapperItem, livstrPropertyValue, out livdtm))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Fecha de Comprobante Asociado INVALIDA");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coAsociados == null)
                                        lioDocumentUser.coAsociados = new List<UxDocumentAsociado>();
                                    if (lioDocumentUser.coAsociados.Count() < livnumColumn)
                                        lioDocumentUser.coAsociados.Add(new UxDocumentAsociado());
                                    lioDocumentUser.coAsociados[livnumColumn - 1].ivstrFechaEmision = livdtm.ToString(livstrApiDtmFormat);
                                    break;
                                case "coOtrosTributos.ivnroId":
                                    if (string.IsNullOrEmpty(livstrPropertyValue)) continue;
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Id Otros Tributos {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coOtrosTributos == null)
                                        lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                                    if (lioDocumentUser.coOtrosTributos.Count() < livnumColumn)
                                        lioDocumentUser.coOtrosTributos.Add(new UxDocumentOtroTributo());
                                    lioDocumentUser.coOtrosTributos[livnumColumn - 1].ivnroId = livnro;
                                    break;
                                case "coOtrosTributos.ivstrDesc":
                                    if (!string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        if (lioDocumentUser.coOtrosTributos == null)
                                            lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                                        if (lioDocumentUser.coOtrosTributos.Count() < livnumColumn)
                                            lioDocumentUser.coOtrosTributos.Add(new UxDocumentOtroTributo());
                                        lioDocumentUser.coOtrosTributos[livnumColumn - 1].ivstrDesc = livstrPropertyValue;
                                    }
                                    break;
                                case "coOtrosTributos.ivdblBaseImp":
                                    if (string.IsNullOrEmpty(livstrPropertyValue)) continue;
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Base Imponible Otros Tributos {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coOtrosTributos == null)
                                        lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                                    if (lioDocumentUser.coOtrosTributos.Count() < livnumColumn)
                                        lioDocumentUser.coOtrosTributos.Add(new UxDocumentOtroTributo());
                                    lioDocumentUser.coOtrosTributos[livnumColumn - 1].ivdblBaseImponible = livval;
                                    break;
                                case "coOtrosTributos.ivdblAlicuota":
                                    if (string.IsNullOrEmpty(livstrPropertyValue)) continue;
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Alicuota Otros Tributos {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coOtrosTributos == null)
                                        lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                                    if (lioDocumentUser.coOtrosTributos.Count() < livnumColumn)
                                        lioDocumentUser.coOtrosTributos.Add(new UxDocumentOtroTributo());
                                    lioDocumentUser.coOtrosTributos[livnumColumn - 1].ivdblAlicuota = livval;
                                    break;
                                case "coOtrosTributos.ivdblImporte":
                                    if (string.IsNullOrEmpty(livstrPropertyValue)) continue;
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe Otros Tributos {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coOtrosTributos == null)
                                        lioDocumentUser.coOtrosTributos = new List<UxDocumentOtroTributo>();
                                    if (lioDocumentUser.coOtrosTributos.Count() < livnumColumn)
                                        lioDocumentUser.coOtrosTributos.Add(new UxDocumentOtroTributo());
                                    lioDocumentUser.coOtrosTributos[livnumColumn - 1].ivdblImporte = livval;
                                    break;
                                case "coIvas.ivnroTipo":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo IVA {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coIvas == null)
                                        lioDocumentUser.coIvas = new List<UxDocumentIva>();
                                    if (lioDocumentUser.coIvas.Count() < livnumColumn)
                                        lioDocumentUser.coIvas.Add(new UxDocumentIva());
                                    lioDocumentUser.coIvas[livnumColumn - 1].ivnroTipo = livnro;
                                    break;
                                case "coIvas.ivdblBaseImponible":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Base Imponible IVA {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coIvas == null)
                                        lioDocumentUser.coIvas = new List<UxDocumentIva>();
                                    if (lioDocumentUser.coIvas.Count() < livnumColumn)
                                        lioDocumentUser.coIvas.Add(new UxDocumentIva());
                                    lioDocumentUser.coIvas[livnumColumn - 1].ivdblBaseImponible = livval;
                                    break;
                                case "coIvas.ivdblImporte":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe IVA {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coIvas == null)
                                        lioDocumentUser.coIvas = new List<UxDocumentIva>();
                                    if (lioDocumentUser.coIvas.Count() < livnumColumn)
                                        lioDocumentUser.coIvas.Add(new UxDocumentIva());
                                    lioDocumentUser.coIvas[livnumColumn - 1].ivdblImporte = livval;
                                    break;
                                case "coOpcionales.ivstrId":
                                    if (string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Id Opcional {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coOpcionales == null)
                                        lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                                    if (lioDocumentUser.coOpcionales.Count() < livnumColumn)
                                        lioDocumentUser.coOpcionales.Add(new UxDocumentOpcional());
                                    lioDocumentUser.coOpcionales[livnumColumn - 1].ivstrId = livstrPropertyValue;
                                    break;
                                case "coOpcionales.ivstrValor":
                                    if (string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Valor Opcional {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coOpcionales == null)
                                        lioDocumentUser.coOpcionales = new List<UxDocumentOpcional>();
                                    if (lioDocumentUser.coOpcionales.Count() < livnumColumn)
                                        lioDocumentUser.coOpcionales.Add(new UxDocumentOpcional());
                                    lioDocumentUser.coOpcionales[livnumColumn - 1].ivstrValor = livstrPropertyValue;
                                    break;
                                case "coCompradores.ivnroDocTipo":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo Comprador {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coCompradores == null)
                                        lioDocumentUser.coCompradores = new List<UxDocumentComprador>();
                                    if (lioDocumentUser.coCompradores.Count() < livnumColumn)
                                        lioDocumentUser.coCompradores.Add(new UxDocumentComprador());
                                    lioDocumentUser.coCompradores[livnumColumn - 1].ivnroDocTipo = livnro;
                                    break;
                                case "coCompradores.ivlngDocNro":
                                    if (!long.TryParse(livstrPropertyValue, out livlng))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Nro Comprador {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coCompradores == null)
                                        lioDocumentUser.coCompradores = new List<UxDocumentComprador>();
                                    if (lioDocumentUser.coCompradores.Count() < livnumColumn)
                                        lioDocumentUser.coCompradores.Add(new UxDocumentComprador());
                                    lioDocumentUser.coCompradores[livnumColumn - 1].ivlngDocNro = livlng;
                                    break;
                                case "coCompradores.ivdblPorcentaje":
                                    if (!double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Porcentaje Comprador {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coCompradores == null)
                                        lioDocumentUser.coCompradores = new List<UxDocumentComprador>();
                                    if (lioDocumentUser.coCompradores.Count() < livnumColumn)
                                        lioDocumentUser.coCompradores.Add(new UxDocumentComprador());
                                    lioDocumentUser.coCompradores[livnumColumn - 1].ivdblPorcentaje = livval;
                                    break;
                                case "coItems.ivstrId":
                                    if (string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Id Item {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivstrId = livstrPropertyValue;
                                    break;
                                case "coItems.ivstrDescripcion":
                                    if (string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Descripcion Item {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivstrDescripcion = livstrPropertyValue;
                                    break;
                                case "coItems.ivdblCantidad":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Cantidad Imponible {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivdblCantidad = livval;
                                    break;
                                case "coItems.ivdblPrecioUnitario":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Precio Unitario {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivdblPrecioUnitario = livval;
                                    break;
                                case "coItems.ivdblBonificaion":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Bonificacion {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivdblBonificaion = livval;
                                    break;
                                case "coItems.ivnroUM":
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    if (short.TryParse(livstrPropertyValue, out livnro))
                                        lioDocumentUser.coItems[livnumColumn - 1].ivnroUM = livnro;
                                    else
                                        lioDocumentUser.coItems[livnumColumn - 1].ivnroUM = 0;
                                    break;
                                case "coItems.ivdblImporteTotal":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe Total {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivdblImporteTotal = livval;
                                    break;
                                case "coItems.ivnroTipoIVA":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo IVA {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivnroTipoIVA = livnro;
                                    break;
                                case "coItems.ivdblImporteIVA":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe IVA {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItems == null)
                                        lioDocumentUser.coItems = new List<UxDocumentItem>();
                                    if (lioDocumentUser.coItems.Count() < livnumColumn)
                                        lioDocumentUser.coItems.Add(new UxDocumentItem());
                                    lioDocumentUser.coItems[livnumColumn - 1].ivdblImporteIVA = livval;
                                    break;
                                case "coItemsCT.ivnroTipo":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo Documento {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItemsCT == null)
                                        lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                                    if (lioDocumentUser.coItemsCT.Count() < livnumColumn)
                                        lioDocumentUser.coItemsCT.Add(new UxDocumentItemCT());
                                    lioDocumentUser.coItemsCT[livnumColumn - 1].ivnroTipo = livnro;
                                    break;
                                case "coItemsCT.ivnroCodigoTurismo":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Codigo Turismo {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItemsCT == null)
                                        lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                                    if (lioDocumentUser.coItemsCT.Count() < livnumColumn)
                                        lioDocumentUser.coItemsCT.Add(new UxDocumentItemCT());
                                    lioDocumentUser.coItemsCT[livnumColumn - 1].ivnroCodigoTurismo = livnro;
                                    break;
                                case "coItemsCT.ivstrCodigo":
                                    if (string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Codigo {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItemsCT == null)
                                        lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                                    if (lioDocumentUser.coItemsCT.Count() < livnumColumn)
                                        lioDocumentUser.coItemsCT.Add(new UxDocumentItemCT());
                                    lioDocumentUser.coItemsCT[livnumColumn - 1].ivstrCodigo = livstrPropertyValue;
                                    break;
                                case "coItemsCT.ivstrDesc":
                                    if (string.IsNullOrEmpty(livstrPropertyValue))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Descripcion {Resources.lioE_ObjectNoF}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItemsCT == null)
                                        lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                                    if (lioDocumentUser.coItemsCT.Count() < livnumColumn)
                                        lioDocumentUser.coItemsCT.Add(new UxDocumentItemCT());
                                    lioDocumentUser.coItemsCT[livnumColumn - 1].ivstrDesc = livstrPropertyValue;
                                    break;
                                case "coItemsCT.ivnroTipoIVA":
                                    if (!short.TryParse(livstrPropertyValue, out livnro))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Tipo IVA {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItemsCT == null)
                                        lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                                    if (lioDocumentUser.coItemsCT.Count() < livnumColumn)
                                        lioDocumentUser.coItemsCT.Add(new UxDocumentItemCT());
                                    lioDocumentUser.coItemsCT[livnumColumn - 1].ivnroTipoIVA = livnro;
                                    break;
                                case "coItemsCT.ivdblImporteItem":
                                    if (!Double.TryParse(livstrPropertyValue, out livval))
                                    {
                                        lioSbErrors.AppendLine($"Linea {livnumLine + 1}: Importe {Resources.lioE_ObjectNoM}");
                                        livblnOK = false;
                                        continue;
                                    }
                                    if (lioDocumentUser.coItemsCT == null)
                                        lioDocumentUser.coItemsCT = new List<UxDocumentItemCT>();
                                    if (lioDocumentUser.coItemsCT.Count() < livnumColumn)
                                        lioDocumentUser.coItemsCT.Add(new UxDocumentItemCT());
                                    lioDocumentUser.coItemsCT[livnumColumn - 1].ivdblImporteItem = livval;
                                    break;
                            }
                        }
                        if (lioSbErrors.Length > 0)
                            throw new Exception(lioSbErrors.ToString());
                    }
                    #endregion
                }
                if (lioSbErrors.Length > 0)
                    throw new Exception(lioSbErrors.ToString());
                lioDocumentUser.ivstrLoadErrors = string.Empty;
                if (lioDocumentUser.coOtrosTributos != null)
                    lioDocumentUser.ivdblImporteOtrosTributos = double.Round(lioDocumentUser.coOtrosTributos.Sum(x => x.ivdblImporte) ?? 0, 2);
                if (lioDocumentUser.coIvas != null)
                    lioDocumentUser.ivdblImporteIva = double.Round(lioDocumentUser.coIvas.Sum(x => x.ivdblImporte) ?? 0, 2); ;
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
            string livstrIdInicio, livstrPropertyValue, livstr, livstrXPath;
            XmlNodeList lcoXmlNodes;
            XmlNode lioXmlNode, lioXmlNodeToClone, lioXmlParent;
            int livnumInicio, livnumLine, livnumOffset, livnumEfectiveOffset, livnumRepeticion, livnumExtendido, livnumLen;
            double livval;
            StringBuilder lioSbErrors = new StringBuilder();
            Cuit lioCuit = new Cuit(long.Parse(ivstrKey.Split('_')[0]), this.mioContext, null);
            if (string.IsNullOrEmpty(lioCuit.ioDcModel.ivstrCnfg))
                throw new Exception($"Configuracion de C.U.I.T. {Resources.lioE_ObjectNoF}");
            ServiceMapper lioServiceMapper = lioCuit.ioCnfg.coServiceMappers.FirstOrDefault(x => x.ivstrInputType == "txt" && x.cvnroDocTypes.Contains(short.Parse(ivstrKey.Split('_')[1])));
            if (lioServiceMapper == null)
                throw new Exception($"Mapeador {Resources.lioE_ObjectNoM}");
            //XML Crystal
            livstr = ListHelper.GetValue("PATH", "template", this.mioContext);
            if (!File.Exists($"{livstr}\\print.xml"))
                throw new Exception($"Xml Impresion {Resources.lioE_ObjectNoM}");
            XmlDocument lioXmlToPrinter = new XmlDocument();
            using (StreamReader lioRd = new StreamReader($"{livstr}\\print.xml", Encoding.UTF8))
            {
                lioXmlToPrinter.Load(lioRd);
            }
            XmlNamespaceManager lioNsMngr = new XmlNamespaceManager(lioXmlToPrinter.NameTable);
            lioNsMngr.AddNamespace("ns", "http://www.afip.com.ar/fe");
            //Lectura del archivo de texto
            livstr = lioCuit.GetEncoding().GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw, lioCuit.GetEncoding())));
            string[] cvstrInDocumentLines = livstr.Split("\n");
            lioXmlNode = lioXmlToPrinter.SelectSingleNode(".//ns:Encabezado/ns:IdDoc/ns:WSRegimen", lioNsMngr);
            if (lioXmlNode == null)
            {
                lioSbErrors.AppendLine($"WSRegimen {Resources.lioE_ObjectNoM}");
                return null;
            }
            lioXmlNode.InnerText = lioServiceMapper.ivstrWs;
            foreach (ServiceMapperItem lioMapperItem in lioServiceMapper.coItems)
            {
                if (string.IsNullOrEmpty(lioMapperItem.ivstrProperty) || string.IsNullOrEmpty(lioMapperItem.ivstrCoord) || string.IsNullOrEmpty(lioMapperItem.coXPaths[0].ivstrData)) continue;
                //busca coordenadas desde el xpath
                if (!GetCoordinates(lioMapperItem.ivstrCoord, out livstrIdInicio, out livnumInicio, out livnumOffset, out livnumLen))
                    continue;
                //busca linea de inicio
                livstr = cvstrInDocumentLines.FirstOrDefault(x => x.StartsWith(livstrIdInicio)) ?? string.Empty;
                if (string.IsNullOrEmpty(livstr))
                    continue;
                //se para en la linea del identificador de inicio
                livnumLine = cvstrInDocumentLines.ToList().IndexOf(livstr);
                #region Campos de tipo U (unico)
                if (lioMapperItem.ivstrCoord.StartsWith("U"))
                {
                    // se para en la linea relativa
                    livnumLine += livnumInicio;
                    if (cvstrInDocumentLines[livnumLine].Length < livnumOffset) continue;
                    if (lioMapperItem.ivstrCoord.Contains("FIX"))
                        livstrPropertyValue = lioMapperItem.ivstrformat;
                    else
                    {
                        if (livnumLen > 0)
                            livstrPropertyValue = cvstrInDocumentLines[livnumLine].Substring(livnumOffset - 1, livnumLen).Trim();
                        else
                            livstrPropertyValue = cvstrInDocumentLines[livnumLine].Substring(livnumOffset - 1).Trim();
                    }
                    if (string.IsNullOrEmpty(livstrPropertyValue))
                        continue;
                    livstrPropertyValue = Format.Property(lioMapperItem.ivstrProperty, livstrPropertyValue);
                    foreach (ServiceMapperItemXPath lioServiceMapperItemXPath in lioMapperItem.coXPaths)
                    {
                        lioXmlNode = lioXmlToPrinter.SelectSingleNode(lioServiceMapperItemXPath.ivstrData.Replace("{N}", "1"), lioNsMngr);
                        if (lioXmlNode == null)
                        {
                            lioSbErrors.AppendLine($"Propiedad {lioMapperItem.ivstrProperty} xpath {lioServiceMapperItemXPath.ivstrData} {Resources.lioE_ObjectNoM}");
                            continue;
                        }
                        lioXmlNode.InnerText = FormatPropertyValue(lioMapperItem, livstrPropertyValue);
                        if (lioXmlNode.InnerText == null)
                        {
                            lioSbErrors.AppendLine($"Error en conversion {lioMapperItem.ivstrProperty}");
                            continue;
                        }
                    }
                }

                #endregion
                #region Campos de tipo R (repetitivos)
                else if (lioMapperItem.ivstrCoord.StartsWith("R"))
                {
                    // saltea la linea de titulo
                    livnumLine++;
                    // recupera la linea de datos
                    livnumRepeticion = 1; livnumExtendido = 1;
                    lioXmlNodeToClone = null;
                    lcoXmlNodes = null;
                    livstrXPath = null;
                    while (true)
                    {
                        livnumLine++;
                        livstr = cvstrInDocumentLines[livnumLine] ?? string.Empty;
                        if (string.IsNullOrEmpty(livstr.Trim()) || livstr.Trim() == "\r")
                        {
                            livnumExtendido++;
                            continue;
                        }
                        if (livstr.Trim().StartsWith("*") || livstr.Trim().StartsWith("XXXFINDOC"))
                            break;
                        // si la linea es mas corta que el offset, ajusta el offset al final de la linea
                        if (lioMapperItem.ivstrCoord.Contains("FIX"))
                            livstrPropertyValue = lioMapperItem.ivstrformat;
                        else
                        {
                            if (livstr.Length < livnumInicio) continue;
                            if (livstr.Length < livnumInicio + livnumOffset)
                                livnumEfectiveOffset = livstr.Length - livnumInicio + 1;
                            else
                                livnumEfectiveOffset = livnumOffset;
                            //recupera el valor de la propiedad
                            livstrPropertyValue = livstr.Substring(livnumInicio - 1, livnumEfectiveOffset).Trim();
                        }
                        if (string.IsNullOrEmpty(livstrPropertyValue))
                            continue;
                        livstrPropertyValue = Format.Property(lioMapperItem.ivstrProperty, livstrPropertyValue);
                        //recupera xpaths a cargar
                        foreach (ServiceMapperItemXPath lioServiceMapperItemXPath in lioMapperItem.coXPaths)
                        {
                            if (!string.IsNullOrEmpty(lioServiceMapperItemXPath.ivstrParent))
                            {
                                //livnumIdx = livstrXp.Split("=>")[0].Trim().IndexOf("[");  // busca el nodo listador
                                //if (livnumIdx > 0)
                                //    livstrXPathNodeList = livstrXp.Substring(0, livnumIdx);
                                //else
                                //    livstrXPathNodeList = livstrXp;
                                lcoXmlNodes = lioXmlToPrinter.SelectNodes(lioServiceMapperItemXPath.ivstrParent, lioNsMngr);
                                if (lcoXmlNodes == null || lcoXmlNodes.Count == 0)
                                {
                                    lioSbErrors.AppendLine($"{lioMapperItem.ivstrProperty} xpath de nodo repetitivo {lioServiceMapperItemXPath.ivstrParent} {Resources.lioE_ObjectNoM} ");
                                    continue;
                                }
                                if (lioXmlNodeToClone == null)
                                    lioXmlNodeToClone = lcoXmlNodes.Item(0); // guarda el primer nodo repetitivo para clonarlo
                                if (lcoXmlNodes.Count < livnumRepeticion)
                                {
                                    //agrega un nodo nuevo al padre
                                    lioXmlNode = lioXmlToPrinter.ImportNode(lioXmlNodeToClone, true);
                                    if (lioXmlNode == null)
                                    {
                                        lioSbErrors.AppendLine($"Error en clonado de nodo para nueva linea: {lioMapperItem.ivstrProperty} xpath {lioXmlNodeToClone} {Resources.lioE_ObjectNoM}");
                                        continue;
                                    }
                                    //clona el nodo y lo agrega al padre
                                    lioXmlParent = lioXmlNodeToClone.ParentNode;  // obtiene el padre del nodo repetitivo
                                    if (!string.IsNullOrEmpty(lioServiceMapperItemXPath.ivstrEnum)) // si tiene un numerador, lo actualiza
                                        lioXmlNode.SelectSingleNode(lioServiceMapperItemXPath.ivstrEnum, lioNsMngr).InnerText = livnumRepeticion.ToString();
                                    lioXmlParent.AppendChild(lioXmlNode);
                                }
                                lcoXmlNodes = lioXmlToPrinter.SelectNodes(lioServiceMapperItemXPath.ivstrParent, lioNsMngr);
                                livstrXPath = lioServiceMapperItemXPath.ivstrData.Replace("{N}", livnumExtendido.ToString());
                                lioXmlNode = lcoXmlNodes.Item(livnumRepeticion - 1).SelectSingleNode(livstrXPath, lioNsMngr);
                            }
                            else
                            {
                                livstrXPath = lioServiceMapperItemXPath.ivstrData.Replace("{N}", livnumExtendido.ToString());
                                lioXmlNode = lioXmlToPrinter.SelectSingleNode(livstrXPath, lioNsMngr);
                            }
                            if (lioXmlNode == null)
                            {
                                lioSbErrors.AppendLine($"{lioMapperItem.ivstrProperty} xpath {livstrXPath} {Resources.lioE_ObjectNoM}");
                                continue;
                            }
                            lioXmlNode.InnerText = FormatPropertyValue(lioMapperItem, livstrPropertyValue);
                            if (lioXmlNode.InnerText == null)
                            {
                                lioSbErrors.AppendLine($"Error en conversion {lioMapperItem.ivstrProperty}");
                                continue;
                            }
                        }
                        livnumRepeticion++;
                        livnumExtendido++;

                    }
                }
                #endregion
                #region Campos de tipo C (repetitivos)
                else if (lioMapperItem.ivstrCoord.StartsWith("C"))
                {
                    // livnumFrom: nro de fila
                    // livnumOffset: cada cuanto se repite,
                    // livnumLen: cuanto ocupa
                    if (!GetCoordinates(lioMapperItem.ivstrCoord, out livstrIdInicio, out livnumInicio, out livnumOffset, out livnumLen))
                    {
                        lioSbErrors.AppendLine($"{lioMapperItem.ivstrProperty} identificador de propiedad invalido (****XXXX:N,N)");
                        continue;
                    }
                    livstr = cvstrInDocumentLines[livnumLine + livnumInicio]; //linea real a leer
                    if (string.IsNullOrEmpty(livstr) || livstr == "\r")
                        continue;
                    int livnumColumn = 1;
                    lioXmlNodeToClone = null;
                    while (livnumOffset < livstr.Length)
                    {
                        if (livstr.Length < livnumLen + livnumOffset)
                            livnumLen = livstr.Length - livnumOffset; // ajusta el offset al final de la linea
                        if (lioMapperItem.ivstrCoord.Contains("FIX"))
                            livstrPropertyValue = lioMapperItem.ivstrformat;
                        else
                            livstrPropertyValue = livstr.Substring(livnumOffset, livnumLen).Trim();
                        livstrPropertyValue = Format.Property(lioMapperItem.ivstrProperty, livstrPropertyValue);
                        foreach (ServiceMapperItemXPath lioServiceMapperItemXPath in lioMapperItem.coXPaths)
                        {
                            if (!string.IsNullOrEmpty(lioServiceMapperItemXPath.ivstrParent))
                            {
                                //livnumIdx = livstrXp.Split("=>")[0].Trim().IndexOf("[");  // busca el nodo listador
                                //if (livnumIdx > 0)
                                //    livstrXPathNodeList = livstrXp.Substring(0, livnumIdx);
                                //else
                                //    livstrXPathNodeList = livstrXp;
                                lcoXmlNodes = lioXmlToPrinter.SelectNodes(lioServiceMapperItemXPath.ivstrParent, lioNsMngr);
                                if (lcoXmlNodes == null || lcoXmlNodes.Count == 0)
                                {
                                    lioSbErrors.AppendLine($"{lioMapperItem.ivstrProperty} xpath de nodo repetitivo {lioServiceMapperItemXPath.ivstrParent} {Resources.lioE_ObjectNoM} ");
                                    continue;
                                }
                                if (lioXmlNodeToClone == null)
                                    lioXmlNodeToClone = lcoXmlNodes.Item(0); // guarda el primer nodo repetitivo para clonarlo
                                if (lcoXmlNodes.Count < livnumColumn)
                                {
                                    //agrega un nodo nuevo al padre
                                    lioXmlNode = lioXmlToPrinter.ImportNode(lioXmlNodeToClone, true);
                                    if (lioXmlNode == null)
                                    {
                                        lioSbErrors.AppendLine($"Error en clonado de nodo para nueva linea: {lioMapperItem.ivstrProperty} xpath {lioXmlNodeToClone} {Resources.lioE_ObjectNoM}");
                                        continue;
                                    }
                                    //clona el nodo y lo agrega al padre
                                    lioXmlParent = lioXmlNodeToClone.ParentNode;  // obtiene el padre del nodo repetitivo
                                    if (!string.IsNullOrEmpty(lioServiceMapperItemXPath.ivstrEnum)) // si tiene un numerador, lo actualiza
                                        lioXmlNode.SelectSingleNode(lioServiceMapperItemXPath.ivstrEnum, lioNsMngr).InnerText = livnumColumn.ToString();
                                    lioXmlParent.AppendChild(lioXmlNode);
                                }
                                lcoXmlNodes = lioXmlToPrinter.SelectNodes(lioServiceMapperItemXPath.ivstrParent, lioNsMngr);
                                livstrXPath = lioServiceMapperItemXPath.ivstrData.Replace("{N}", livnumColumn.ToString());
                                lioXmlNode = lcoXmlNodes.Item(livnumColumn - 1).SelectSingleNode(livstrXPath, lioNsMngr);
                            }
                            else
                            {
                                livstrXPath = lioServiceMapperItemXPath.ivstrData.Replace("{N}", livnumColumn.ToString());
                                lioXmlNode = lioXmlToPrinter.SelectSingleNode(livstrXPath, lioNsMngr);
                            }
                            if (lioXmlNode == null)
                            {
                                lioSbErrors.AppendLine($"{lioMapperItem.ivstrProperty} xpath {livstrXPath} {Resources.lioE_ObjectNoM} ");
                                continue;
                            }
                            lioXmlNode.InnerText = FormatPropertyValue(lioMapperItem, livstrPropertyValue);
                            if (lioXmlNode.InnerText == null)
                            {
                                lioSbErrors.AppendLine($"Error en conversion {lioMapperItem.ivstrProperty}");
                                continue;
                            }
                        }
                        livnumOffset += livnumLen;
                        livnumColumn++;
                    }
                }
                #endregion
            }
            if (lioSbErrors.Length > 0)
                throw new Exception(lioSbErrors.ToString());
            return lioXmlToPrinter.OuterXml;
        }
        #endregion
        #region PRIVATE METHODS
        private bool GetCoordinates(string vivstrXpath, out string rivstrInicio, out int rivnumRelativeline, out int rivnumOffset, out int rivnumLen)
        {
            rivnumRelativeline = -1;
            rivnumOffset = -1;
            rivnumLen = -1;
            rivstrInicio = string.Empty;
            if (string.IsNullOrEmpty(vivstrXpath))
                return false;
            string[] cvstr = vivstrXpath.Split(':');
            if (cvstr.Length != 2)
                return false;
            rivstrInicio = cvstr[0].Substring(1).Trim();
            if (string.IsNullOrEmpty(rivstrInicio) || !rivstrInicio.StartsWith("*"))
                return false;
            string livstr = cvstr[1].Trim();
            if (livstr.Contains("FIX"))
            {
                rivnumRelativeline = 0;
                rivnumOffset = 0;
                rivnumLen = 0;
                return true;
            }
            if (livstr.Split(',').Length < 2)
                return false;
            if (!int.TryParse(livstr.Split(',')[0], out rivnumRelativeline) || !int.TryParse(livstr.Split(',')[1], out rivnumOffset))
                return false;
            if (livstr.Split(',').Length == 3 && !int.TryParse(livstr.Split(',')[2], out rivnumLen))
                return false;
            if (rivnumRelativeline <= 0 || rivnumOffset <= 0)
                return false;
            return true;
        }
        private string FormatPropertyValue(ServiceMapperItem vioMapperItem, string vivstrPropertyValue)
        {
            if (vioMapperItem.ivstrCoord.Contains("FIX"))
                return vivstrPropertyValue;

            if (string.IsNullOrEmpty(vivstrPropertyValue))
            {
                if (vioMapperItem.ivblnIsNumeric)
                    return "0";
                return vivstrPropertyValue;
            }
            if (vioMapperItem.ivstrProperty.Contains("dbl"))
            {
                if (vivstrPropertyValue.Contains("-"))
                    vivstrPropertyValue = "-" + vivstrPropertyValue.Replace("-", string.Empty);
                Double livval;
                if (!Double.TryParse(vivstrPropertyValue, out livval))
                    return null;
                return livval.ToString();
            }
            if (string.IsNullOrEmpty(vioMapperItem.ivstrformat))
                return vivstrPropertyValue;
            if (vioMapperItem.ivstrformat.Contains("{"))
                return string.Format(vioMapperItem.ivstrformat, vivstrPropertyValue);
            if (vioMapperItem.ivstrProperty.Contains("dtm"))
            {
                if (vioMapperItem.ivstrformat.Contains("=>"))
                {
                    if (!DateTime.TryParseExact(vivstrPropertyValue, vioMapperItem.ivstrformat.Split("=>")[0], null, DateTimeStyles.None, out DateTime livdtm))
                        return null;
                    return livdtm.ToString(vioMapperItem.ivstrformat.Split("=>")[1], null);
                }
                return DateTime.Parse(vivstrPropertyValue).ToString(vioMapperItem.ivstrformat, null);

            }
            return null;
        }
        private bool GetDateFromProp(ServiceMapperItem vioMapperItem, string vivstrPropertyValue, out DateTime vivdtm)
        {
            string livstrFormat;
            if (string.IsNullOrEmpty(vioMapperItem.ivstrformat))
                return DateTime.TryParse(vivstrPropertyValue, out vivdtm);
            if (vioMapperItem.ivstrformat.Contains("=>"))
                livstrFormat = vioMapperItem.ivstrformat.Split("=>")[0];
            else
                livstrFormat = vioMapperItem.ivstrformat;
            return DateTime.TryParseExact(vivstrPropertyValue, livstrFormat, null, DateTimeStyles.None, out vivdtm);
        }

        #endregion
        #region PRIVATE PROPS
        private long mivlngCuit;
        private NatContext mioContext;
        #endregion
    }
}
