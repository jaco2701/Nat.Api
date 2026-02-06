using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;

namespace Applet.Nat.Api.Models
{
    public class Login
    {
        public User? ioUser { get; set; }
        public string? ivstrToken { get; set; }
        public string? ivstrRefreshToken { get; set; }
    }
}