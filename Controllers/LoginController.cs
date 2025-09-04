using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Static;
using Applet.Nat.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nat.Api.Properties;
using System.Net.Http.Headers;

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
                if (lcvstrCreds.Length != 3)
                    throw new Exception(Resources.lioE_NoCreds);
                if (string.IsNullOrEmpty(lcvstrCreds[1]))
                    throw new Exception(Resources.lioE_NoCreds);
                User lioUser = new User(lcvstrCreds[1], mioContext, mioToken);
                if (!lioUser.ioDcModel.ivblnEnable??false)
                    throw new Exception(Resources.lioE_UserDisabled);
                lioUser.ivstrPass = lcvstrCreds[2];
                lioUser.ieTask=eTask.Auth;
                lioUser.Task();
                lioUser.ivstrPass = null;
                return ResponseHelper.Get(
                    new Login
                    {
                        ioUser = lioUser,
                        ivstrToken = Auth.Get(lioUser.ioDcModel.ivnumUser, mioContext, lcvstrCreds[0]),
                    });
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }
    }
}

