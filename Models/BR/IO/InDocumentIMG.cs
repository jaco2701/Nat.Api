using Applet.Nat.Afip.ServicesV1;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Mvc;
using Nat.API.Models.Afip;
using Nat.API.Properties;
using PdfiumViewer;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Web;
using ZXing;
using ZXing.SkiaSharp;
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
                MemoryStream lioMS;
                Result lioResult;
                if (ivstrName.EndsWith(".pdf"))
                {
                    using PdfDocument lioPdfDocument = PdfDocument.Load(new MemoryStream(Convert.FromBase64String(ivstrRaw ?? string.Empty)));
                    using var lioBMP = lioPdfDocument.Render(0, 300, 300, false);
                    lioMS = new MemoryStream();
                    lioBMP.Save(lioMS, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                {
                    lcoBytes = Convert.FromBase64String(ivstrRaw ?? string.Empty);
                    lioMS = new MemoryStream(lcoBytes);
                }
                SKBitmap lioSKBitmap = SKBitmap.Decode(lioMS);
                if (lioSKBitmap == null) throw new Exception("No se pudo decodificar la imagen"); ;
                BarcodeReader lioBarcodeReader = new BarcodeReader
                {
                    AutoRotate = true,
                    Options = new ZXing.Common.DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                    }
                };
                var S = Convert.ToBase64String(lioSKBitmap.Bytes);
                lioResult = lioBarcodeReader.Decode(lioSKBitmap);
                short livnroCuadrante = 0; // 0 = superior izquierdo, 1 = superior derecho, 2 = inferior izquierdo, 3 = inferior derecho
                Rectangle lioCenterRectangle;
                while (true)
                {
                    //divide en 4 la image y le hace zoom a la parte central para intentar leer el QR
                    if (lioResult != null || livnroCuadrante >= 4) break;
                    switch (livnroCuadrante)
                    {
                        case 0:
                            lioCenterRectangle = new Rectangle(0, 0, (lioSKBitmap.Width / 2) - 1, (lioSKBitmap.Height / 2) - 1);
                            break;
                        case 1:
                            lioCenterRectangle = new Rectangle(lioSKBitmap.Width / 2, 0, (lioSKBitmap.Width / 2) - 1, (lioSKBitmap.Height / 2) - 1);
                            break;
                        case 2:
                            lioCenterRectangle = new Rectangle(0, lioSKBitmap.Height / 2, (lioSKBitmap.Width / 2) - 1, (lioSKBitmap.Height / 2) - 1);
                            break;
                        case 3:
                            lioCenterRectangle = new Rectangle(lioSKBitmap.Width / 2, lioSKBitmap.Height / 2, (lioSKBitmap.Width / 2) - 1, (lioSKBitmap.Height / 2) - 1);
                            break;
                        default:
                            throw new Exception("Cuadrante inválido");
                    }
                    using SKBitmap lioZoomedImage = new SKBitmap(lioCenterRectangle.Width, lioCenterRectangle.Height);
                    using var canvas = new SKCanvas(lioZoomedImage);
                    // Set high-quality scaling algorithms to prevent pixelation blurring
                    using var paint = new SKPaint { FilterQuality = SKFilterQuality.High };

                    var sourceRect = new SKRect(lioCenterRectangle.X, lioCenterRectangle.Y, lioCenterRectangle.X + lioCenterRectangle.Width, lioCenterRectangle.Y + lioCenterRectangle.Height);
                    var destRect = new SKRect(0, 0, lioZoomedImage.Width, lioZoomedImage.Height);

                    canvas.DrawBitmap(lioSKBitmap, sourceRect, destRect, paint);
                    using var ms = new MemoryStream();
                    S = Convert.ToBase64String(lioZoomedImage.Bytes);
                    lioResult = lioBarcodeReader.Decode(lioZoomedImage);
                    livnroCuadrante++;
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
        #endregion
    }
}
