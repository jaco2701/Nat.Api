using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.DC
{
    [Table("OIDCSTATE")]
    public class OidcStateModel
    {
        [Key, Column("strState", Order = 0)]
        public string ivstrState { get; set; }
        [Column("numIdentityProvider")]
        public int ivnumIdentityProvider { get; set; }
        [Column("strIdentityProviderId")]
        public string ivstrIdentityProviderId { get; set; }
        [Column("dtmState")]
        public DateTime ivdtmState { get; set; }
        [Column("strchallenge")]
        public string ivstrCodeVerifier { get; set; }
    }
}