using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Models.BR;
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
    public class InDocumentOpera : IRawDocument
    {
        #region CONS
        public InDocumentOpera(long vivlngCuit, NatContext vioContext)
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
            DocumentUser lioDocumentUser = new DocumentUser();
            StringBuilder lioSbErrors = new StringBuilder();
            int livnum; double livdbl = 0; long livlng = 0;
            string livstr = lioCuit.GetEncoding().GetString(Convert.FromBase64String(Format.UnCompress(ivstrRaw ?? string.Empty, lioCuit.GetEncoding())));
            OperaFiscalPayload lioOperaFiscalPayload = JsonConvert.DeserializeObject<OperaFiscalPayload>(livstr);
            if (lioOperaFiscalPayload == null)
                throw new Exception($"Documento Opera Fiscal {Resources.lioE_ObjectNoM}");
            livstr = $"{lioOperaFiscalPayload.DocumentInfo?.DocumentType ?? string.Empty}_{lioOperaFiscalPayload.DocumentInfo?.Suffix ?? string.Empty}";
            if (lioOperaFiscalPayload.DocumentInfo != null)
            {
                switch (lioOperaFiscalPayload.DocumentInfo.DocumentType)
                {
                    case "INV_A":
                        lioDocumentUser.ivnroTipoDoc = 1;
                        break;
                    default:
                        lioSbErrors.AppendLine($"Tipo de Documento {Resources.lioE_ObjectNoM}");
                        break;
                }
                if (!int.TryParse(lioOperaFiscalPayload.DocumentInfo.Prefix, out livnum))
                    lioSbErrors.AppendLine($"Prefijo {Resources.lioE_ObjectNoM}");
                else
                    lioDocumentUser.ivnumPvta = livnum;
                lioDocumentUser.ivstrFechaEmision = lioOperaFiscalPayload.DocumentInfo.BusinessDate.ToString();
                lioDocumentUser.ivnroConcepto = 2;

            }
            if (lioOperaFiscalPayload.ReservationInfo != null)
            {
                lioDocumentUser.ivstrFechaServdesde = lioOperaFiscalPayload.ReservationInfo.ArrivalDate.ToString();
                lioDocumentUser.ivstrFechaServhasta = lioOperaFiscalPayload.ReservationInfo.DepartureDate.ToString();
            }
            if (lioOperaFiscalPayload.FolioInfo != null)
            {

                lioDocumentUser.ivdblImporteNoGravado = lioOperaFiscalPayload.FolioInfo.TotalInfo?.NonTaxableAmount ?? 0;
                lioDocumentUser.ivdblImporteGravado = lioOperaFiscalPayload.FolioInfo.TotalInfo?.NetAmount ?? 0;
                lioDocumentUser.ivdblImporteTotal = lioOperaFiscalPayload.FolioInfo.TotalInfo?.GrossAmount ?? 0;
                lioDocumentUser.ivdblImporteExento = 0;
                lioDocumentUser.ivdblImporteIva = 0;
                lioDocumentUser.ivdblImporteOtrosTributos = 0;
                if (!string.IsNullOrEmpty(lioOperaFiscalPayload.FolioInfo.FolioHeaderInfo?.InvoiceCurrencyCode))
                    lioDocumentUser.ivstrMoneda = lioOperaFiscalPayload.FolioInfo.FolioHeaderInfo?.InvoiceCurrencyCode;
                else lioSbErrors.AppendLine($"Moneda {Resources.lioE_ObjectNoF}");
                if (double.TryParse(lioOperaFiscalPayload.FolioInfo.FolioHeaderInfo?.InvoiceCurrencyRate, out livdbl))
                    lioDocumentUser.ivdblCotizacion = livdbl;
                else
                    lioSbErrors.AppendLine($"Cotización {Resources.lioE_ObjectNoF}");
                if (lioOperaFiscalPayload.FolioInfo.PayeeInfo != null)
                {
                    switch (lioOperaFiscalPayload.FolioInfo.PayeeInfo.NameTaxType)
                    {
                        case "CUIT":
                            lioDocumentUser.ivnroTipoDocReceptor = 80;
                            break;
                        default:
                            lioSbErrors.AppendLine($"Tipo de Documento Receptor {Resources.lioE_ObjectNoM}");
                            break;
                    }
                }
                else
                    lioSbErrors.AppendLine($"Cliente {Resources.lioE_ObjectNoM}");
                if (string.IsNullOrEmpty(lioOperaFiscalPayload.FolioInfo.PayeeInfo?.Tax1No))
                    lioSbErrors.AppendLine($"CUIT Receptor {Resources.lioE_ObjectNoM}");
                else
                    if (!long.TryParse(lioOperaFiscalPayload.FolioInfo.PayeeInfo?.Tax1No, out livlng))
                        lioDocumentUser.ivlngDocReceptor = livlng;
                    else
                        lioDocumentUser.ivstrDocReceptor = lioOperaFiscalPayload.FolioInfo.PayeeInfo?.Tax1No;
                if (!string.IsNullOrEmpty(lioOperaFiscalPayload.FolioInfo.PayeeInfo?.PaymentDueDate))
                    lioDocumentUser.ivstrFechaVtopago = lioOperaFiscalPayload.FolioInfo.PayeeInfo?.PaymentDueDate;
                else
                    lioSbErrors.AppendLine($"Fecha Vencimiento Pago {Resources.lioE_ObjectNoF}");
            }
            lioDocumentUser.ivstrCanMisMonExt = "S";
            return [lioDocumentUser];
        }
        public string ToPrint() { return string.Empty; }
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
