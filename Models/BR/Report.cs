using iText.Barcodes;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Nat.Api.Properties;
using Newtonsoft.Json;
using System.Reflection;

namespace Applet.Nat.Api.Br.Models
{
    public class Report
    {
        #region CONSTRUCT
        public Report(Object vioSource, string vivstrPathTemplate, string vivstrOutputName)
        {
            if (!File.Exists(vivstrPathTemplate))
                throw new Exception(Resources.lioE_ReportTemplate_No);
            ivstrTemplateFolder = new FileInfo(vivstrPathTemplate).Directory.FullName;
            ioTemplate = JsonConvert.DeserializeObject<ReportTemplate>(File.ReadAllText(vivstrPathTemplate));
            if (ioTemplate == null)
                throw new Exception(Resources.lioE_ReportTemplate_No);
            ioSource = vioSource;
            if (ioSource == null)
                throw new Exception(Resources.lioE_ReportSource_No);
            if (ioTemplate.ivstrAssembly == null)
                throw new Exception(Resources.lioE_ReportAssembly_No);
            if (ioSourceType.FullName != ioTemplate.ivstrAssembly)
                throw new Exception(string.Format(Resources.lioE_ReportAssembly_NoMatch, ioTemplate.ivstrAssembly, ioSourceType.FullName));
            this.ivstrOutputName = vivstrOutputName;
        }
        #endregion
        #region PRIVATE PROPS
        private PdfFont mioFont;
        //private string ivstrTempFile { get { return ivstrOutputName.Replace(".pdf", "_tmp.pdf"); } }
        #endregion
        #region PUBLIC PROPS
        public List<Tuple<int, int, int, double>> coRowBreak { get; set; } //pagina filadesde filahasta
        public string ivstrTemplateFolder { get; set; }
        public ReportTemplate ioTemplate { get; set; }
        public object ioSource { get; set; }
        public Type ioSourceType { get { return ioSource.GetType(); } }
        public PdfFont ioDftFont
        {
            get
            {
                if (mioFont == null)
                    if (ioTemplate.ioDftTextStyle?.ivstrFont != null)
                        return PdfFontFactory.CreateFont(ivstrTemplateFolder + "/" + ioTemplate.ioDftTextStyle?.ivstrFont);
                    else
                        return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                return mioFont;
            }
        }
        public string ivstrOutputName { get; set; }
        #endregion
        #region PUBLIC METHODS
        public void Build()
        {
            PageSize lioPageSize;
            Paragraph lioParagraph;
            PropertyInfo lioProperty;
            PropertyInfo[] lcoProperties;
            Table lioTable;
            Rectangle lioRectangle;
            PdfFont lioFont;
            Type lioChildType;
            Cell lioCell;
            //if (File.Exists(ivstrTempFile))
            //    File.Delete(ivstrTempFile);
            //File.Move(ivstrOutputName, ivstrOutputName.Replace(".pdf", $"_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.pdf"));
            PdfDocument lioPdfDocument = new PdfDocument(new PdfWriter(ivstrOutputName));
            iText.Layout.Document lioDocument = new iText.Layout.Document(lioPdfDocument);
            try
            {
                PdfCanvasProcessor parser = new PdfCanvasProcessor(new LocationTextExtractionStrategy());
                lioPdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageEventHandler(this));
                lioPageSize = lioPdfDocument.GetDefaultPageSize();
                switch (ioTemplate.ivstrPageSize)
                {
                    case "A4":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A4);
                        break;
                    case "A5":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A5);
                        break;
                    case "A6":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A6);
                        break;
                    case "A7":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A7);
                        break;
                    case "A8":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A8);
                        break;
                    case "A9":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A9);
                        break;
                    case "A10":
                        lioPdfDocument.SetDefaultPageSize(PageSize.A10);
                        break;
                    case "B4":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B4);
                        break;
                    case "B5":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B5);
                        break;
                    case "B6":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B6);
                        break;
                    case "B7":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B7);
                        break;
                    case "B8":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B8);
                        break;
                    case "B9":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B9);
                        break;
                    case "B10":
                        lioPdfDocument.SetDefaultPageSize(PageSize.B10);
                        break;
                    case "LEGAL":
                        lioPdfDocument.SetDefaultPageSize(PageSize.LEGAL);
                        break;
                    case "LETTER":
                        lioPdfDocument.SetDefaultPageSize(PageSize.LETTER);
                        break;
                    case "TABLOID":
                        lioPdfDocument.SetDefaultPageSize(PageSize.TABLOID);
                        break;
                    default:
                        lioPdfDocument.SetDefaultPageSize(PageSize.A4);
                        break;
                }
                if (ioTemplate.coFields == null)
                    throw new Exception(Resources.lioE_Report_NoFields);
                ReportField lioTableField = ioTemplate.coFields.FirstOrDefault(x => x.ieType == eReportFieldType.Table);
                if (lioTableField == null)
                    throw new Exception(Resources.lioE_ReportLines_No);
                lioProperty = ioSourceType.GetProperty(lioTableField.ivstrValue);
                lioChildType = lioProperty.PropertyType?.GetGenericArguments()[0];  //las colecciones deben ser listas no arrays
                var lcoItems = (System.Collections.IList)lioProperty.GetValue(ioSource);
                //se calcula la altura de cada fila para generar las paginas
                int livnumPage = 0, livnumItemDesde = 0;
                double lioRowHeight, lioTableRealHeight = 0;
                System.Drawing.SizeF lioSize;
                coRowBreak = new List<Tuple<int, int, int, double>>();
                for (int livnumItem = 0; livnumItem < lcoItems.Count; livnumItem++)
                {
                    lioRowHeight = 0;
                    foreach (ReportField lioColumn in lioTableField.coColumns)
                    {
                        lioFont = ioDftFont;
                        if (lioColumn.ioStyle?.ivstrFont != null)
                            lioFont = PdfFontFactory.CreateFont(ivstrTemplateFolder + "/" + lioColumn.ioStyle?.ivstrFont);

                        lioProperty = lioChildType.GetProperty(lioColumn.ivstrValue);
                        if (lioProperty == null)
                            continue;
                        //busca la maxima altura de la fila
                        lioSize = GetTextSize($"{lioProperty.GetValue(lcoItems[livnumItem])}", lioFont.ToString(), lioColumn.ioStyle?.ivnumSize ?? ioTemplate.ioDftTextStyle.ivnumSize ?? 10);
                        lioRowHeight = Math.Max(
                            lioRowHeight,
                            (Math.Floor(lioSize.Width / lioColumn.ivnumWidth) * lioSize.Height) + lioSize.Height
                        );
                    }
                    //sumo el margen de la fila
                    lioRowHeight += lioTableField.ivnumRowMargin;
                    lioTableRealHeight += lioRowHeight;
                    if (lioTableRealHeight > lioTableField.ivnumHeight)
                    {
                        coRowBreak.Add(
                            new Tuple<int, int, int, double>(livnumPage, livnumItemDesde, livnumItem - 1, 0)
                            );
                        lioPdfDocument.AddNewPage();
                        livnumItemDesde = livnumItem - 1;
                        livnumPage++;
                        lioTableRealHeight = lioRowHeight;
                    }
                }
                if (livnumItemDesde > 0 || coRowBreak.Count() == 0)
                {
                    coRowBreak.Add(
                        new Tuple<int, int, int, double>(livnumPage, livnumItemDesde, lcoItems.Count, lioTableRealHeight)
                        );
                    lioPdfDocument.AddNewPage();
                }
                AddPageNumbers(lioPdfDocument);
            }
            catch (Exception lioEx)
            {
                throw lioEx;
            }
            finally
            {
                lioDocument.Close();
            }
        }
        private void AddPageNumbers(PdfDocument vioPdfDocument)
        {
            iText.Layout.Document lioDocument = new iText.Layout.Document(vioPdfDocument);
            int livnumPages = vioPdfDocument.GetNumberOfPages();
            vioPdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageNumberHandler(livnumPages, this));
            lioDocument.Close();
        }
        static System.Drawing.SizeF GetTextSize(string vivstrText, string vivnumFont, int vivnumFontSize)
        {
            // Assuming font size of 12pt
            using (var lioFont = new System.Drawing.Font("vivnumFont", vivnumFontSize))
            using (var lioImage = new System.Drawing.Bitmap(1, 1))
            using (var lioGraphics = System.Drawing.Graphics.FromImage(lioImage))
            {
                return lioGraphics.MeasureString(vivstrText, lioFont);
            }
        }
        #endregion

    }
    internal class PageEventHandler : IEventHandler
    {
        public PageEventHandler(Report vioReport)
        {
            mioReport = vioReport;
        }
        private Report mioReport { get; set; }
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent lioPdfDocumentEvent = (PdfDocumentEvent)@event;
            PdfDocument lioPdfDocument = lioPdfDocumentEvent.GetDocument();
            PropertyInfo lioProperty;
            Table lioTable;
            Type lioChildType;
            Text lioTextLabel, lioTextField;
            Cell lioCell;
            Paragraph lioParagraph;
            PdfFont lioFont;
            PdfPage lioPage = lioPdfDocumentEvent.GetPage();  //A$ 595*842
            PdfCanvas lioPdfCanvas = new PdfCanvas(lioPage.NewContentStreamBefore(), lioPage.GetResources(), lioPdfDocument);
            foreach (ReportField lioField in mioReport.ioTemplate.coFields)
            {
                if (lioField.ivnumWidth > lioPage.GetPageSize().GetWidth())
                    lioField.ivnumWidth = (int)lioPage.GetPageSize().GetWidth() - 1;
                if (lioField.ivnumHeight > lioPage.GetPageSize().GetHeight())
                    lioField.ivnumHeight = (int)lioPage.GetPageSize().GetHeight() - 1;
                //     lioField.ivnumX = (int)lioPage.GetPageSize().GetWidth() - lioField.ivnumX;
                switch (lioField.ieType)
                {
                    case eReportFieldType.Line:
                        int livnumY = (int)lioPage.GetPageSize().GetHeight() - lioField.ivnumY;
                        if (lioField.ivnumWidth == 0)
                            lioPdfCanvas
                                .SaveState()
                                .SetLineWidth(lioField.ioStyle.ioBorder.ivvalWidth ?? 1)
                                .SetStrokeColor(Color.FromHtml(lioField.ioStyle.ioBorder.ivstrColor ?? "000000"))
                                .MoveTo(lioField.ivnumX, livnumY)
                                .LineTo(lioField.ivnumX, livnumY - lioField.ivnumHeight)
                                .ClosePathStroke()
                                .RestoreState();
                        else if (lioField.ivnumHeight == 0)
                            lioPdfCanvas
                                .SaveState()
                                .SetLineWidth(lioField.ioStyle.ioBorder.ivvalWidth ?? 1)
                                .SetStrokeColor(Color.FromHtml(lioField.ioStyle.ioBorder.ivstrColor ?? "000000"))
                                .MoveTo(lioField.ivnumX, livnumY)
                                .LineTo(lioField.ivnumX + lioField.ivnumWidth, livnumY)
                                .ClosePathStroke()
                                .RestoreState();
                        break;
                    case eReportFieldType.Rectangle:
                        lioPdfCanvas
                            .SaveState()
                            .SetLineWidth(lioField.ioStyle.ioBorder.ivvalWidth ?? 1)
                            .SetStrokeColor(Color.FromHtml(lioField.ioStyle.ioBorder.ivstrColor ?? "000000"))
                            .MoveTo(lioField.ivnumX, lioField.ivnumY)
                            .Rectangle(lioField.ivnumX, lioField.ivnumY, lioField.ivnumWidth, lioField.ivnumHeight)
                            .ClosePathStroke()
                            .RestoreState();
                        break;
                    case eReportFieldType.Image:
                        lioPdfCanvas
                            .SaveState()
                            .MoveTo(lioField.ivnumX, lioField.ivnumY)
                            .ClosePathStroke()
                            .RestoreState()
                            .AddImageFittedIntoRectangle(ImageDataFactory.Create(mioReport.ivstrTemplateFolder + "/" + lioField.ivstrValue), new Rectangle(lioField.ivnumX, lioField.ivnumY, lioField.ivnumWidth, lioField.ivnumHeight), false);
                        break;
                    case eReportFieldType.Text:
                        lioFont = mioReport.ioDftFont;
                        if (lioField.ioStyle?.ivstrFont != null)
                            lioFont = PdfFontFactory.CreateFont(mioReport.ivstrTemplateFolder + "/" + lioField.ioStyle?.ivstrFont);
                        lioParagraph = new Paragraph().SetTextAlignment(lioField.ioStyle?.ieTextAlignment);
                        lioParagraph.SetFont(lioFont);
                        lioParagraph.SetFixedPosition(lioField.ivnumX, lioPage.GetPageSize().GetHeight() - lioField.ivnumY, lioField.ivnumWidth);
                        if (lioField.ioStyle?.ivnumMargin != null || mioReport.ioTemplate.ioDftTextStyle?.ivnumMargin != null)
                        {
                            lioParagraph.SetPaddingLeft(lioField.ioStyle?.ivnumMargin ?? mioReport.ioTemplate.ioDftTextStyle?.ivnumMargin ?? 0);
                            lioParagraph.SetPaddingRight(lioField.ioStyle?.ivnumMargin ?? mioReport.ioTemplate.ioDftTextStyle?.ivnumMargin ?? 0);
                        }
                        if ((lioField.ioStyle?.ivblnBold ?? false) || (mioReport.ioTemplate.ioDftTextStyle?.ivblnBold ?? false))
                            lioParagraph.SetBold();
                        if (lioField.ioStyle?.ivnumSize != null || mioReport.ioTemplate.ioDftTextStyle?.ivnumSize != null)
                            lioParagraph.SetFontSize(lioField.ioStyle?.ivnumSize ?? (mioReport.ioTemplate.ioDftTextStyle?.ivnumSize ?? 12));
                        if (lioField.ioStyle?.ivstrColor != null || (mioReport.ioTemplate.ioDftTextStyle?.ivstrColor != null))
                            lioParagraph.SetFontColor(Color.FromHtml(lioField.ioStyle?.ivstrColor ?? mioReport.ioTemplate.ioDftTextStyle?.ivstrColor ?? "000000"));
                        if (lioField.ioStyle?.ioBorder != null)
                        {
                            lioParagraph.SetBorder(
                                new SolidBorder(Color.FromHtml(lioField.ioStyle?.ioBorder?.ivstrColor ?? "000000"),
                                    lioField.ioStyle?.ioBorder?.ivvalWidth ?? 1)
                                );
                        }
                        lioTextLabel = new Text(string.Empty);
                        lioTextField = new Text(string.Empty);

                        if (lioField.ivstrLabel != null)
                        {
                            lioTextLabel = new Text($"{lioField.ivstrLabel}: ");
                            if ((lioField.ioStyle?.ivblnBoldLabel ?? false))
                                lioTextLabel.SetBold();
                        }
                        if (lioField.ivstrValue != null)
                        {
                            if (lioField.ioStyle?.ivblnCaps ?? false)
                                lioField.ivstrValue = lioField.ivstrValue.ToUpper();
                            if (lioField.ivstrValue.StartsWith("$$"))
                                lioTextField = new Text($"{lioField.ivstrValue.Replace("$$", string.Empty)}");
                            else
                            {
                                lioProperty = mioReport.ioSourceType.GetProperty(lioField.ivstrValue);
                                if (lioProperty == null)
                                    throw new Exception(string.Format(Resources.lioE_ReportProperty_No, lioField.ivstrValue));
                                lioTextField = new Text($"{lioProperty.GetValue(mioReport.ioSource)}");
                            }
                        }
                        lioParagraph.Add(lioTextLabel);
                        lioParagraph.Add(lioTextField);
                        new Canvas(lioPdfCanvas, lioPdfDocument.GetDefaultPageSize()).Add(lioParagraph);
                        break;
                    case eReportFieldType.QRCode:
                        lioProperty = mioReport.ioSourceType.GetProperty(lioField.ivstrValue);
                        if (lioProperty == null)
                            throw new Exception(string.Format(Resources.lioE_ReportProperty_No, lioField.ivstrValue));
                        BarcodeQRCode lioBarcodeQRCode = new BarcodeQRCode($"{lioProperty.GetValue(mioReport.ioSource)}");
                        Image lioQrCodeImage = new Image(lioBarcodeQRCode.CreateFormXObject(lioPdfDocument));
                        lioQrCodeImage.SetWidth(lioField.ivnumWidth);
                        lioQrCodeImage.SetHeight(lioField.ivnumHeight);
                        lioQrCodeImage.SetFixedPosition(lioField.ivnumX, lioPage.GetPageSize().GetHeight() - lioField.ivnumY);
                        new Canvas(lioPdfCanvas, lioPdfDocument.GetDefaultPageSize()).Add(lioQrCodeImage);
                        break;
                    case eReportFieldType.Table:
                        if (lioField.coColumns == null || lioField.coColumns.Count() == 0)
                            throw new Exception(Resources.lioE_ReportField_NoColumns);
                        lioProperty = mioReport.ioSourceType.GetProperty(lioField.ivstrValue);
                        if (lioProperty == null)
                            throw new Exception(string.Format(Resources.lioE_ReportProperty_No, lioField.ivstrValue));
                        lioTable = new Table(lioField.coColumns.Count());
                        lioTable.SetBorder(Border.NO_BORDER);
                        if (lioField.ioStyle?.ioBorder != null)
                            lioTable.SetBorder(new SolidBorder(Color.FromHtml(lioField.ioStyle?.ioBorder?.ivstrColor ?? "0000000"), lioField.ioStyle?.ioBorder?.ivvalWidth ?? 1));
                        lioTable.SetFixedPosition(lioField.ivnumX, lioPage.GetPageSize().GetHeight() - lioField.ivnumY, lioField.ivnumWidth);
                        if (lioField.ivnumWidth > lioPage.GetPageSize().GetWidth())
                            lioTable.SetWidth(lioField.ivnumWidth);
                        lioTable.SetHeight(lioField.ivnumHeight);
                        lioTable.SetWidth(UnitValue.CreatePercentValue(100));
                        //HEADER
                        lioChildType = lioProperty.PropertyType?.GetGenericArguments()[0];  //las colecciones deben ser listas no arrays
                        var lcoItems = (System.Collections.IList)lioProperty.GetValue(mioReport.ioSource);
                        foreach (ReportField lioColumn in lioField.coColumns)
                        {
                            if (lioColumn == null)
                                throw new Exception(Resources.lioE_ReportField_No);
                            if (lioColumn.ivstrValue == null)
                                throw new Exception(Resources.lioE_ReportField_No);
                            lioProperty = lioChildType.GetProperty(lioColumn.ivstrValue);
                            if (lioProperty == null)
                                throw new Exception(string.Format(Resources.lioE_ReportProperty_No, lioColumn.ivstrValue));
                            lioFont = mioReport.ioDftFont;
                            if (lioColumn.ioStyle?.ivstrFont != null)
                                lioFont = PdfFontFactory.CreateFont(mioReport.ivstrTemplateFolder + "/" + lioColumn.ioStyle?.ivstrFont);
                            lioCell = new Cell();
                            lioCell.SetTextAlignment(lioColumn.ioStyle?.ieTextAlignment);
                            lioCell.SetFont(lioFont);
                            if ((lioColumn.ioStyle?.ivblnBold ?? false) || (mioReport.ioTemplate.ioDftTextStyle?.ivblnBold ?? false))
                                lioCell.SetBold();
                            if (lioColumn.ioStyle?.ivnumSize != null || mioReport.ioTemplate.ioDftTextStyle?.ivnumSize != null)
                                lioCell.SetFontSize(lioColumn.ioStyle?.ivnumSize ?? mioReport.ioTemplate.ioDftTextStyle?.ivnumSize ?? 12);

                            if (lioColumn.ioStyle?.ivstrColor != null || mioReport.ioTemplate.ioDftTextStyle?.ivstrColor != null)
                                lioCell.SetFontColor(Color.FromHtml(lioColumn.ioStyle?.ivstrColor ?? mioReport.ioTemplate.ioDftTextStyle?.ivstrColor ?? "000000"));
                            if (lioColumn.ioStyle?.ioBorder != null)
                                lioCell.SetBorder(new SolidBorder(Color.FromHtml(lioColumn.ioStyle?.ioBorder?.ivstrColor ?? "0000000"), lioColumn.ioStyle?.ioBorder?.ivvalWidth ?? 1));
                            else
                                lioCell.SetBorder(Border.NO_BORDER);
                            lioCell.Add(new Paragraph(lioColumn.ivstrLabel).SetWidth(lioColumn.ivnumWidth));
                            lioTable.AddHeaderCell(lioCell);
                        }
                        //ROWS
                        Tuple<int, int, int, double> lioRowBreak = mioReport.coRowBreak.FirstOrDefault(x => x.Item1 == lioPdfDocument.GetPageNumber(lioPage) - 1);
                        for (int i = lioRowBreak.Item2; i < lioRowBreak.Item3; i++)
                        {
                            var lioItem = lcoItems[i];
                            foreach (ReportField lioColumn in lioField.coColumns)
                            {
                                lioProperty = lioChildType.GetProperty(lioColumn.ivstrValue);
                                if (lioColumn.ioStyle?.ivblnCaps ?? false)
                                    lioColumn.ivstrLabel = lioColumn.ivstrLabel;
                                lioParagraph = new Paragraph($"{lioProperty.GetValue(lioItem)}");
                                lioCell = new Cell().Add(lioParagraph);
                                if (lioColumn.ioStyle?.ioBorder != null)
                                    lioCell.SetBorder(new SolidBorder(Color.FromHtml(lioColumn.ioStyle?.ioBorder?.ivstrColor ?? "0000000"), lioColumn.ioStyle?.ioBorder?.ivvalWidth ?? 1));
                                else
                                    lioCell.SetBorder(Border.NO_BORDER);
                                lioCell.SetKeepTogether(false);
                                if (lioColumn.ivnumWidth != null)
                                {
                                    lioCell.SetWidth(lioColumn.ivnumWidth);
                                    lioCell.SetMaxWidth(lioColumn.ivnumWidth);
                                }
                                lioTable.AddCell(lioCell);
                            }
                        }
                        if (lioRowBreak.Item4 > 0)  //si es la ultima fila se ajusta la altura
                        {
                            lioTable.AddCell(new Cell().SetHeight((float)(lioField.ivnumHeight - lioRowBreak.Item4)));
                        }
                        new Canvas(lioPdfCanvas, lioPdfDocument.GetDefaultPageSize()).Add(lioTable);
                        break;
                }

            }
        }
    }
    internal class PageNumberHandler : IEventHandler
    {
        public PageNumberHandler(int vivnumPages, Report vioReport)
        {
            ivnumPages = vivnumPages;
            mioReport = vioReport;
        }
        private int ivnumPages { get; set; }
        private Report mioReport { get; set; }
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent lioDocEvent = (PdfDocumentEvent)@event;
            PdfDocument lioPdfDocument = lioDocEvent.GetDocument();
            if (lioPdfDocument == null)
                return;
            lioPdfDocument.GetPageNumber(lioDocEvent.GetPage());
            iText.Layout.Document lioDocument = new iText.Layout.Document(lioPdfDocument);
            ReportField lioField = mioReport.ioTemplate.coFields.FirstOrDefault(x => x.ieType == eReportFieldType.PageNumber);
            if (lioField == null)
                return;
            PdfFont lioFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            lioFont = mioReport.ioDftFont;
            if (lioField.ioStyle?.ivstrFont != null)
                lioFont = PdfFontFactory.CreateFont(mioReport.ivstrTemplateFolder + "/" + lioField.ioStyle?.ivstrFont);
            Paragraph lioParagraph = new Paragraph().SetTextAlignment(lioField.ioStyle?.ieTextAlignment);
            lioParagraph.SetFont(lioFont);
            lioParagraph.SetFixedPosition(lioField.ivnumX, lioPdfDocument.GetDefaultPageSize().GetHeight() - lioField.ivnumY, lioField.ivnumWidth);
            if ((lioField.ioStyle?.ivblnBold ?? false) || (mioReport.ioTemplate.ioDftTextStyle?.ivblnBold ?? false))
                lioParagraph.SetBold();
            if (lioField.ioStyle?.ivnumSize != null || mioReport.ioTemplate.ioDftTextStyle?.ivnumSize != null)
                lioParagraph.SetFontSize(lioField.ioStyle?.ivnumSize ?? (mioReport.ioTemplate.ioDftTextStyle?.ivnumSize ?? 12));
            if (lioField.ioStyle?.ivstrColor != null || (mioReport.ioTemplate.ioDftTextStyle?.ivstrColor != null))
                lioParagraph.SetFontColor(Color.FromHtml(lioField.ioStyle?.ivstrColor ?? mioReport.ioTemplate.ioDftTextStyle?.ivstrColor ?? "000000"));
            if (lioField.ioStyle?.ioBorder != null)
            {
                lioParagraph.SetBorder(
                    new SolidBorder(Color.FromHtml(lioField.ioStyle?.ioBorder?.ivstrColor ?? "000000"),
                    lioField.ioStyle?.ioBorder?.ivvalWidth ?? 1)
                    );
            }
            lioParagraph.Add(lioField.ioStyle?.ivstrPageFormat?.Replace("{page}", lioPdfDocument.GetPageNumber(lioDocEvent.GetPage()).ToString()).Replace("{total}", ivnumPages.ToString()));
            lioDocument.Add(lioParagraph);

        }
    }
    public class ReportTemplate
    {
        public string? ivstrAssembly { get; set; }
        public string? ivstrPageSize { get; set; }
        public PageBackground? ioPageBackground { get; set; }
        public ReportField[]? coFields { get; set; }
        public FieldStyle? ioDftTextStyle { get; set; }
    }
    public class ReportField
    {
        public eReportFieldType ieType { get; set; }
        public int ivnumX { get; set; }
        public int ivnumY { get; set; }
        public int ivnumWidth { get; set; }
        public int ivnumRowMargin { get; set; }
        public int ivnumHeight { get; set; }
        public string? ivstrLabel { get; set; }
        public string? ivstrValue { get; set; }
        public FieldStyle? ioStyle { get; set; }
        public ReportField[]? coColumns { get; set; }

    }
    public class FieldBorder
    {
        public float? ivvalWidth { get; set; }
        public string? ivstrColor { get; set; }
    }
    public class PageBackground
    {
        public FieldBorder? ioBorder { get; set; }
        public ReportField[] coLines { get; set; }
    }
    public class FieldStyle
    {
        public string? ivstrFont { get; set; }
        public bool? ivblnBold { get; set; }
        public bool? ivblnBoldLabel { get; set; }
        public bool? ivblnCaps { get; set; }
        public string? ivstrColor { get; set; }
        public int? ivnumSize { get; set; }
        public int? ivnumMargin { get; set; }
        public FieldBorder? ioBorder { get; set; }
        public TextAlignment ieTextAlignment { get; set; }
        public string? ivstrPageFormat { get; set; }

    }
    public enum eReportFieldType : short
    {
        Text = 1,
        Image = 2,
        Line = 3,
        Rectangle = 4,
        Table = 5,
        PageNumber = 6,
        QRCode = 7
    }
    internal static class Color
    {
        public static DeviceRgb FromHtml(string vivstrColor)
        {
            return new DeviceRgb(int.Parse(vivstrColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                                     int.Parse(vivstrColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                                     int.Parse(vivstrColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));

        }
    }
}

