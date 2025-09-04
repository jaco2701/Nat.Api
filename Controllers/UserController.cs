using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Microsoft.AspNetCore.Mvc;
using Applet.Nat.Api.Static;
using Applet.Nat.Api.Ifaces;
using Nat.Api.Properties;
using Nat.Api.Models.BR;
using System.Data;
using Applet.Nat.Api.Models.BR;
using System.Net.Http.Headers;

namespace Applet.Nat.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly Token mioToken;
        public UserController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor)
        {
            mioContext = vioContext;
            mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
        }

        [HttpPost("Get")]
        public Response Get([FromBody] Filter? vioFilter)
        {
            try
            {
                List<User> lcoUsers = new List<User>();
                if (vioFilter == null || vioFilter.coFilterItems == null || vioFilter.coFilterItems.Count == 0)
                    throw (new Exception("Filtro Invalido"));
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                foreach (UserModel lioUserModel in mioContext.Users.Where(x => x.ivstrUserName.Contains(vioFilter.coFilterItems[0].ivstrPropValue) || x.ivstrUserEmail.Contains(vioFilter.coFilterItems[0].ivstrPropValue) || x.ivstrUserId.Contains(vioFilter.coFilterItems[0].ivstrPropValue)))
                {
                    try
                    {
                        lcoUsers.Add(new User(lioUserModel, mioContext, mioToken));
                    }
                    catch (Exception lioE)
                    {
                        LogHelper.write(lioE);
                    }
                }
                return ResponseHelper.Get(lcoUsers);
            }
            catch (Exception lioE)
            {
                {
                    LogHelper.write(lioE);
                    return ResponseHelper.Get(-1, lioE);
                }
            }
        }

        [HttpPost("Task")]
        public async Task<Response> Task([FromBody] User vioUser)
        {
            try
            {
                vioUser.SetDC(mioContext);
                return ResponseHelper.Get(vioUser.Task().ToString());
            }
            catch (Exception lioE)
            {
                {
                    LogHelper.write(lioE);
                    return ResponseHelper.Get(-1, lioE.Message);
                }
            }
        }
    }
}