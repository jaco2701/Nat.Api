using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("CUITCUITS")]
    public class CuitCuitModel
    {
        [Key, Column("lngCuit", Order = 0)]
        public long ivlngCuit { get; set; }
        [Key, Column("lngCuitReceptor", Order = 1)]
        public long ivlngCuitReceptor { get; set; }
        [Column("strEmail")]
        public string? ivstrEmail { get; set; }
        [Column("strRs")]
        public string? ivstrRs { get; set; }
    }
}