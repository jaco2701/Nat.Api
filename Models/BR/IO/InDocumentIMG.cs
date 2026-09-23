using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Nat.API.Models.Afip;
using Nat.API.Properties;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;
using System.Web;
using ZXing;
using ZXing.Windows.Compatibility;
namespace Applet.Nat.Api.Br.Models
{
    public class InDocumentIMG : IRawDocument
    {
        #region CONS
        public InDocumentIMG(long vivlngCuit, NatContext vioContext)
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
            StringBuilder lioSbErrors = new StringBuilder();
            List<DocumentUser> lcoDocumentUser = new List<DocumentUser>();
            DocumentUser lioDocumentUser = new DocumentUser();
            try
            {
                Cuit lioCuit = new Cuit(mivlngCuit, mioContext, null);
                byte[] lcoBytes = [];
                Bitmap lioBMPVanilla;
                if ((ivstrName ?? string.Empty).EndsWith(".pdf"))
                {
                    PdfDocument lioPdfDocument = PdfDocument.Load(new MemoryStream(Convert.FromBase64String(ivstrRaw ?? string.Empty)));
                    lioBMPVanilla = (Bitmap)lioPdfDocument.Render(0, 300, 300, PdfRenderFlags.CorrectFromDpi);
                }
                else
                    lioBMPVanilla = (Bitmap)Image.FromStream(new MemoryStream(Convert.FromBase64String(ivstrRaw ?? string.Empty)));
                Result? lioResult;

                using (lioBMPVanilla)
                {
                    BarcodeReader lioBarcodeReader = new BarcodeReader();
                    lioResult = null;
                    double livdblScale;
                    // zoom livdblScale de 100% a 500%
                    for (int livnumZoomPercent = 100; livnumZoomPercent <= 500; livnumZoomPercent += 100)
                    {
                        livdblScale = livnumZoomPercent / 100.0;
                        if (livnumZoomPercent == 100)
                            lioResult = lioBarcodeReader.Decode(lioBMPVanilla);
                        else
                            using (Bitmap lioBMPZommed = ResizeImage(lioBMPVanilla, livdblScale))
                            {
                                lioResult = lioBarcodeReader.Decode(lioBMPZommed);
                            }
                        if (lioResult != null) break;
                    }
                }
                if (lioResult == null)
                    throw new Exception("QR no encontrado ");
                Uri uri = new Uri(lioResult?.Text ?? string.Empty);
                string livstr = HttpUtility.ParseQueryString(uri.Query).Get("p");
                if (string.IsNullOrEmpty(livstr))
                    throw new Exception("No se pudo obtener el parámetro 'p' de la URL del código QR.");
                livstr = Encoding.UTF8.GetString(Convert.FromBase64String(Format.SanitizeBase64String(livstr)));
                QRData lioQRData = Newtonsoft.Json.JsonConvert.DeserializeObject<QRData>(livstr);
                if (lioQRData == null)
                    throw new Exception("No se pudo deserializar el contenido del código QR.");
                lioDocumentUser = new DocumentUser
                {
                    ivnroTipoDoc = lioQRData.tipoCmp,
                    ivnumPvta = lioQRData.ptoVta,
                    ivlngCbte = lioQRData.nroCmp,
                    ivstrFechaEmision = lioQRData.fecha,
                    ivlngCuitEmisor = lioQRData.cuit,
                    ivlngDocReceptor = lioQRData.nroDocRec,
                    ivnroTipoDocReceptor = lioQRData.tipoDocRec,
                    ivstrCodAutorizacion = lioQRData.codAut.ToString(),
                    ivstrMoneda = lioQRData.moneda,
                    ivdblImporteTotal = lioQRData.importe,
                    ivstrWs = "wscdc",
                    ivblnAuth = true,
                    ivstrIdCliente = "NatOrigen3"
                };
                lioDocumentUser.ivstrLoadErrors = string.Empty;
                if (lioDocumentUser.ivnroTipoDoc == null)
                    lioSbErrors.AppendLine($"Tpo de Documento {Resources.lioE_ObjectNoM}");
                if (lioDocumentUser.ivnumPvta == null)
                    lioSbErrors.AppendLine($"Punto de Venta {Resources.lioE_ObjectNoM}");
                if (lioDocumentUser.ivlngCbte == null)
                    lioSbErrors.AppendLine($"Numero de Comprobante {Resources.lioE_ObjectNoM}");
                DateTime livdtm = DateTime.MinValue;
                if (lioDocumentUser.ivstrFechaEmision == null || !DateTime.TryParseExact(lioDocumentUser.ivstrFechaEmision, ListHelper.GetValue("Format", "XmlDtm", mioContext), null, DateTimeStyles.None, out livdtm))
                    lioSbErrors.AppendLine($"Fecha de Comprobante {Resources.lioE_ObjectNoF}");
                lioDocumentUser.ivstrFechaEmision = livdtm.ToString(ListHelper.GetValue("Format", "ApiDtm", mioContext));
                if (lioDocumentUser.ivlngCuitEmisor == null)
                    lioSbErrors.AppendLine($"CUIT Emisor {Resources.lioE_ObjectNoF}");
                if (lioDocumentUser.ivlngDocReceptor == null)
                    lioSbErrors.AppendLine($"Numero de documento receptor {Resources.lioE_ObjectNoM}");
                if (lioDocumentUser.ivnroTipoDocReceptor == null)
                    lioSbErrors.AppendLine($"Tipo de documento receptor {Resources.lioE_ObjectNoM}");
                if (lioSbErrors.Length > 0)
                    lioDocumentUser.ivstrLoadErrors = lioSbErrors.ToString();
                lcoDocumentUser.Add(lioDocumentUser);
                return lcoDocumentUser.ToArray();
            }
            catch (Exception lioE)
            {
                lioSbErrors.AppendLine($"{Resources.lioE_DocRNo} : {lioE.Message}");
            }
            finally
            {
                if (lioSbErrors.Length > 0)
                {
                    lioDocumentUser.ivstrLoadErrors = lioSbErrors.ToString();
                    lcoDocumentUser.Add(lioDocumentUser);
                }

            }
            return lcoDocumentUser.ToArray();
        }
        public string ToPrint()
        {
            return string.Empty;
        }
        #endregion
        #region PRIVATE PROPS
        private long mivlngCuit;
        private NatContext mioContext;
        #endregion
        #region PRIVATE METHODS
        private static Bitmap ResizeImage(Bitmap vioBMP, double vivdblScale)
        {
            int newWidth = (int)(vioBMP.Width * vivdblScale);
            int newHeight = (int)(vioBMP.Height * vivdblScale);
            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics lioGraphic = Graphics.FromImage(newImage))
            {
                lioGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                lioGraphic.SmoothingMode = SmoothingMode.HighQuality;
                lioGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                lioGraphic.CompositingQuality = CompositingQuality.HighQuality;
                lioGraphic.DrawImage(vioBMP, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }
        #endregion
    }
}
