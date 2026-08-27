using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("CLIENTS")]
    public class ClientModel
    {
        [Key, Column("codClient", Order = 0)]
        public string? ivcodClient { get; set; }
        [Column("strClient")]
        public string? ivstrClient { get; set; }
        [Column("strClientSecret")]
        public string? ivstrClientSecret { get; set; }
        [Column("strToken")]
        public string? ivstrToken { get; set; }
        [Column("numUser")]
        public int? ivnumUser { get; set; }
    }
}