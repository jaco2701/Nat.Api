using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.Net.Http.Headers;
using System.Text;

namespace Applet.Nat.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly NatContext mioContext;

        private readonly Token mioToken;
        public LoginController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor)
        {
            mioContext = vioContext;
            if (vioHttpContextAccessor.HttpContext.Request.Headers.Authorization.ToString().StartsWith("NatToken"))
                mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
        }
        // GET: api/Login/5
        [HttpGet]
        public Response GetLogin()
        {
            try
            {
                string[] lcvstrCreds = HttpsHeaderHelper.GetCredencials(AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]));
                if (lcvstrCreds == null || lcvstrCreds.Length == 0)
                    throw new Exception(Resources.lioE_NoCreds);
                if (lcvstrCreds[0] == "NatAuth")
                {
                    if (lcvstrCreds.Length != 3)
                        throw new Exception(Resources.lioE_NoCreds);
                    if (string.IsNullOrEmpty(lcvstrCreds[1]))
                        throw new Exception(Resources.lioE_NoCreds);
                    User lioUser = new User(lcvstrCreds[1], mioContext, mioToken);
                    if (!lioUser.ioDcModel.ivblnEnable ?? false)
                        throw new Exception(Resources.lioE_UserDisabled);
                    lioUser.ivstrPass = lcvstrCreds[2];
                    lioUser.ieTask = eTask.Auth;
                    lioUser.Task();
                    lioUser.ivstrPass = null;
                    var lioO = ResponseHelper.Get(
                        new Login
                        {
                            ioUser = lioUser,
                            ivstrToken = Auth.Get(lioUser.ioDcModel.ivnumUser, mioContext, lcvstrCreds[0]),
                        });
                    LogHelper.writeinfo($"User {lioUser.ioDcModel.ivstrUserName} authenticated successfully. Response: {JsonConvert.SerializeObject(lioO)}", ListHelper.Verbose(mioContext));
                    return (lioO);
                }
                //OIDC:
                if (lcvstrCreds.Length != 3)
                    throw new Exception(Resources.lioE_NoCreds);
                mioContext.OidcOidcStates.Add(
                    new OidcStateModel
                    {
                        ivstrState = lcvstrCreds[2],
                        ivstrIdentityProviderId = lcvstrCreds[1],
                        ivnumIdentityProvider = int.Parse(lcvstrCreds[0]),
                        ivdtmState = DateTime.UtcNow,
                    });
                mioContext.SaveChanges();
                return ResponseHelper.Get(
                    new Login
                    {
                        ioUser = null,
                        ivstrToken = null
                    });
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }

        [HttpGet("OidcState")]
        public async Task<Response> Task()
        {
            try
            {
                // borra los estados viejos
                mioContext.OidcOidcStates.RemoveRange(mioContext.OidcOidcStates.Where(s => s.ivdtmState < DateTime.UtcNow.AddMinutes(-30)));
                mioContext.SaveChanges();
                string[] lcvstrCreds = HttpsHeaderHelper.GetCredencials(AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]));
                if (lcvstrCreds == null || lcvstrCreds.Length != 1)
                    throw new Exception(Resources.lioE_NoCreds);
                OidcStateModel lioO = mioContext.OidcOidcStates.Find(lcvstrCreds[0]);
                if (lioO == null)
                    throw new Exception(Resources.lioE_NoCreds);
                return ResponseHelper.Get(lioO);
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE.Message);
            }
        }
    }
}

