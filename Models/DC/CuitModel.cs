using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("CUITS")]
    public class CuitModel
    {
        [Key, Column("lngCuit", Order = 0)]
        public long ivlngCuit { get; set; }
        [Column("strCnfg")]
        public string? ivstrCnfg { get; set; }
        [Column("strCuitRS")]
        public string? ivstrCuitRS { get; set; }
    }
    public class CuitPassRequest
    {
        public string ivstrPass { get; set; }
    }
}