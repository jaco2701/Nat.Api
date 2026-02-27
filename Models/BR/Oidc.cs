
namespace Applet.Nat.Api.Br
{
    public class OidcRequest
    {
        public short ivnroType { get; set; } 
        public int ivnumIdentityProvider { get; set; } = 0;
        public string? ivstrState { get; set; }
        public string? ivstrCode { get; set; }
        public string? ivstrToken { get; set; }
    }
    public class OidcResponse
    {
        public string? ivstrUrlRedirect { get; set; }
        public string? ivstrClientId { get; set; }
        public string? ivstrState { get; set; }
    }
    public class Oauth2Response
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}