namespace Applet.Nat.Api.Models.BR
{
    public class DocumentFormaDePago
    {
        #region Propiedades
        public short ivnroCodigo { get; set; }
        public string ivstrSwiftCode { get; set; }
        public short ivnroTipoCuenta { get; set; }
        public string ivstrNroCuenta { get; set; }
        public string ivstrNroTarjeta { get; set; }
        public Double ivvalImporte { get; set; }
        #endregion
    }

}
