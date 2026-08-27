using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [Column("nroRole")]
        public short? ivnroRole { get; set; }
        [Column("blnEnable")]
        public bool? ivblnEnable { get; set; }
        [Column("nrologonFails")]
        public short? ivnrologonFails { get; set; }
        [JsonIgnore]
        [Column("strUserPwd")]
        public string? ivstrUserPwd { get; set; }

    }
}
