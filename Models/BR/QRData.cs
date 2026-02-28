namespace Nat.Api.Models.BR
{
    public class QRData
    {
        public short? ver { get; set; }
        public string? fecha { get; set; }
        public long? cuit { get; set; }
        public int? ptoVta { get; set; }
        public short? tipoCmp { get; set; }
        public long? nroCmp { get; set; }
        public double? importe { get; set; }
        public string? moneda { get; set; }
        public double?  ctz { get; set; }
        public short? tipoDocRec { get; set; }
        public long? nroDocRec { get; set; }
        public string? tipoCodAut { get; set; }
        public long? codAut { get; set; }
    }
}
