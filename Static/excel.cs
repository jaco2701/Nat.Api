using System.Data;
using OfficeOpenXml;

namespace Applet.Nat.Api.Static
{
    public static class Excel
    {
        public static MemoryStream GenerateFormDataSet(DataSet vioDataSet, string vivstrTitle, string vivstrColswidth)
        {
            MemoryStream lioStream = new MemoryStream();
            using (ExcelPackage lioxlPackage = new ExcelPackage(lioStream))
            {
                foreach (DataTable lioDataTable in vioDataSet.Tables)
                {
                    ExcelWorksheet lioWs = lioxlPackage.Workbook.Worksheets.Add(lioDataTable.TableName);
                    int livnumRow = 1;
                    short livnro = 0;
                    lioWs.Cells[livnumRow, 3].Value = DateTime.Now.ToString("'Fecha:' dd/MM/yyyy");
                    lioWs.Cells[livnumRow, 4].Value = DateTime.Now.ToString("'Hora:' hh:mm");
                    lioWs.Cells[livnumRow, 1].Value = vivstrTitle;
                    livnumRow++;
                    if (lioDataTable.Rows.Count == 0)
                    {
                        lioWs.Cells[livnumRow, 1].Value = "No se encontraron Datos para mostrar";
                    }
                    else
                    {
                        string[] lcvstrColswidth = vivstrColswidth.Split(new char[] { ',' }, StringSplitOptions.TrimEntries);
                        for (int i = 1; i <= lioDataTable.Columns.Count; i++)
                        {
                            if (lcvstrColswidth.Length < i || !short.TryParse(lcvstrColswidth[i-1], out livnro))
                                lioWs.Column(i).Width = 30;
                            else
                                lioWs.Column(i).Width = livnro;
                            lioWs.Cells[livnumRow, i].Value = lioDataTable.Columns[i-1].ColumnName;
                        }
                        for (int j = livnumRow; j <= lioDataTable.Rows.Count; j++)
                            for (int k = 1; k <= lioDataTable.Columns.Count; k++)
                            {
                                lioWs.Cells[j + 1, k].Value = lioDataTable.Rows[j - 1].ItemArray[k - 1].ToString();

                            }
                    }
                    lioxlPackage.Save();
                }
            }
            lioStream.Position = 0;
            return lioStream;
        }

        private static double CellToLong(string vivstrlng)
        {
            vivstrlng = vivstrlng.Trim();
            int livnumDecimalPositions = vivstrlng.LastIndexOf(".");
            if (livnumDecimalPositions < 0)
                livnumDecimalPositions = vivstrlng.LastIndexOf(",");
            if (livnumDecimalPositions < 0) return -1;
            livnumDecimalPositions = vivstrlng.Length - livnumDecimalPositions - 1;
            vivstrlng = vivstrlng.Replace(".", string.Empty).Replace(",", string.Empty);
            return (long.Parse(vivstrlng) / Math.Pow(10, livnumDecimalPositions));
        }
    }
}
