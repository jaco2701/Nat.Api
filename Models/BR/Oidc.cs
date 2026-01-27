using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Applet.Nat.Api.Br
{
    [Table("OIDCLOGIN")]
    public class OidcRequest
    {
        public string ivstrState { get; set; }
    }
}