using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("DOCUMENTS")]
    public class DocumentModel
    {
        [Key, Column("lngDoc", Order = 0)]
        public long ivlngDoc { get; set; }
        [Column("lngCuitEmisor")]
        public long ivlngCuitEmisor { get; set; }
        [Column("nroTipo")]
        public short ivnroTipo { get; set; }
        [Column("numPvta")]
        public int ivnumPvta { get; set; }
        [Column("lngCbte")]
        public long ivlngCbte { get; set; }
        [Column("dtmEmision")]
        public DateTime? ivdtmEmision { get; set; }
        [Column("lngCuitReceptor")]
        public long ivlngCuitReceptor { get; set; }
        [Column("strWs")]
        public string? ivstrWs { get; set; }
        [Column("strInData")]
        public string? ivstrInData { get; set; }
        [Column("dblImporte")]
        public double? ivdblImporte { get; set; }
        [Column("nroStatus")]
        public short ivnroStatus { get; set; }
        [Column("strInType")]
        public string? ivstrInType { get; set; }
        [Column("strIdCliente")]
        public string? ivstrIdCliente { get; set; }
        [Column("nroTemplateVersion")]
        public short ivnroTemplateVersion { get; set; }
        [Column("strRazonSocial")]
        public string? ivstrRazonSocial { get; set; }
        [Column("strMoneda")]
        public string? ivstrMoneda { get; set; }
        [Column("strSR")]
        public string? ivstrSR { get; set; }
    }
}