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
    public class RoleActionController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly Token mioToken;
        public RoleActionController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor)
        {
            mioContext = vioContext;
            mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
        }

        [HttpPost("Get")]
        public Response Get([FromBody] Filter? vioFilter)
        {
            try
            {
                vioFilter.ivstrDtmFormat = ListHelper.GetValue("Format", "ApiDtm", mioContext);
                List<RoleAction> lcoRoleActions = new List<RoleAction>();
                if (vioFilter == null)
                    vioFilter = new Filter();
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                foreach (RoleActionModel lioRoleActionModel in mioContext.RoleActions.Where(vioFilter.Build<RoleActionModel>()))
                {
                    try
                    {
                        lcoRoleActions.Add(new RoleAction { ioDcModel = lioRoleActionModel });
                    }
                    catch (Exception lioE)
                    {
                        LogHelper.write(lioE);
                    }
                }
                return ResponseHelper.Get(lcoRoleActions);
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }

        [HttpPost("Task")]
        public async Task<Response> Task([FromBody] RoleAction[] vcoRoleActions)
        {
            try
            {
                mioContext.RoleActions.RemoveRange(mioContext.RoleActions.Where(x => x.ivnroRole == vcoRoleActions[0].ioDcModel.ivnroRole));
                mioContext.SaveChanges();
                foreach (RoleAction vioRoleAction in vcoRoleActions.Where(x => x.ieTask == eTask.Save))
                {
                    vioRoleAction.SetDC(mioContext);
                    vioRoleAction.Task();
                }
                return ResponseHelper.Get("OK");
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