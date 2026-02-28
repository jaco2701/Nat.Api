using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("USERCUITS")]
    public class UserCuitModel
    {
        [Key, Column("numUser", Order = 0)]
        public int ivnumUser { get; set; }
        [Key, Column("lngCuit", Order = 1)]
        public long ivlngCuit { get; set; }
        [Column("blnDefaut")]
        public bool ivblnDefaut { get; set; }
        [NotMapped]
        public string? ivstrRS { get; set; }
        [NotMapped]
        public string? ivstrEncoding { get; set; }
    }
}