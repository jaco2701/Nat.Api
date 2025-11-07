using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("IDENTITYPROVIDERS")]
    public class IdentityProviderModel
    {
        [Key, Column("numIdentityProvider", Order = 0)]
        public int ivnumIdentityProvider { get; set; }
        [Column("strIdentityProvider")]
        public string ivstrIdentityProvider { get; set; }
        [Column("strIdentityProviderId")]
        public string ivstrIdentityProviderId { get; set; }
        [Column("strIdentityProviderUrl")]
        public string ivstrIdentityProviderUrl { get; set; }
        [Column("strClientSecret")]
        public string? ivstrClientSecret { get; set; }
        [Column("blnEnable")]
        public bool? ivblnEnable { get; set; }
    }
}