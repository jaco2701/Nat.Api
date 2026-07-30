namespace Applet.Nat.Api.Models.BR
{
    public class DocumentItem
    {
        #region Propiedades
        public string? ivstrCodigo { get; set; }
        public string? ivstrDescripcion { get; set; }
        public double? ivdblCantidad{ get; set; }
        public double? ivdblPrecioUnitario { get; set; }
        public double? ivdblBonificaion { get; set; }
        public int? ivnroUM { get; set; }
        public double? ivdblImporteTotal { get; set; }
        public short? ivnroTipoIVA { get; set; }
        public double? ivdblImporteIVA { get; set; }
        #endregion
    }
  }
