using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("ROLEACTIONS")]
    public class RoleActionModel
    {
        [Key, Column("nroRole", Order = 0)]
        public short ivnroRole { get; set; }
        [Key, Column("nroAction", Order = 1)]
        public short ivnroAction { get; set; }
    }
}