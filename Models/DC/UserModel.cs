using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("USERS")]
    public class UserModel
    {
        [Key, Column("numUser", Order = 0)]
        public int ivnumUser { get; set; }
        [Column("strUserEmail")]
        public string? ivstrUserEmail { get; set; }
        [Column("strUserId")]
        public string? ivstrUserId { get; set; }
        [Column("strUserName")]
        public string? ivstrUserName { get; set; }
        [Column("blnAdmin")]
        public bool? ivblnAdmin { get; set; }
        [Column("blnEnable")]
        public bool? ivblnEnable { get; set; }
        [Column("nrologonFails")]
        public short? ivnrologonFails { get; set; }
    }
    public class UserPassRequest
    {
        public string ivstrPass { get; set; }
    }
}
