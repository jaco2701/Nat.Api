using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Microsoft.AspNetCore.Mvc;
using Applet.Nat.Api.Static;
using Applet.Nat.Api.Ifaces;
using Nat.API.Properties;
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
                User lioFromUser = new User(mioToken.ivnumUser, mioContext, mioToken);
                if (lioFromUser.ieRol == eRol.User)
                    throw new Exception(Resources.lioE_NoAuth);
                List<int> cvnumUsersThatAdmins = new List<int>();
                if (lioFromUser.ieRol == eRol.CuitAdmin)
                {
                    List<long> cvlngCuitsThatAdmins = mioContext.UserCuits.Where(x => x.ivnumUser == lioFromUser.ioDcModel.ivnumUser).Select(x => x.ivlngCuit).ToList();
                    cvnumUsersThatAdmins = mioContext.UserCuits.Where(x => cvlngCuitsThatAdmins.Contains(x.ivlngCuit)).Select(x => x.ivnumUser).ToList();
                }
                List<User> lcoUsers = new List<User>();
                if (vioFilter == null || vioFilter.coFilterItems == null || vioFilter.coFilterItems.Count == 0)
                    throw (new Exception(Resources.lioE_NoFilter));
                if (vioFilter.coFilterItems == null || vioFilter.coFilterItems.Count() == 0)
                    vioFilter.coFilterItems = [];
                UserModel[] lcoUserModels = null;
                if (vioFilter.coFilterItems[0].ivstrPropValue == "*")
                    lcoUserModels = mioContext.Users.ToArray();
                else
                    lcoUserModels = mioContext.Users.Where(x => x.ivstrUserName.Contains(vioFilter.coFilterItems[0].ivstrPropValue) || x.ivstrUserEmail.Contains(vioFilter.coFilterItems[0].ivstrPropValue) || x.ivstrUserId.Contains(vioFilter.coFilterItems[0].ivstrPropValue)).ToArray();
                foreach (UserModel lioUserModel in lcoUserModels.Where(x=> ! (new string[] {"admin","services" }.Contains(x.ivstrUserId))))
                {
                    try
                    {
                        if (cvnumUsersThatAdmins.Count > 0 && !cvnumUsersThatAdmins.Contains(lioUserModel.ivnumUser))
                            continue;
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