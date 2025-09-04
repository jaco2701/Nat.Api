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
    public class ListController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly Token mioToken;
        public ListController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor)
        {
            mioContext = vioContext;
            mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
        }

        [HttpPost("Get")]
        public Response Get([FromBody] Filter? vioFilter)
        {
            try
            {
                List<List> lcoLists = new List<List>();
                if (vioFilter == null)
                    vioFilter = new Filter();
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                foreach (ListModel lioListModel in mioContext.Lists.Where(vioFilter.Build<ListModel>()))
                {
                    try
                    {
                        lcoLists.Add(new List { ioDcModel = lioListModel });
                    }
                    catch (Exception lioE)
                    {
                        LogHelper.write(lioE);
                    }
                }
                return ResponseHelper.Get(lcoLists);
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
        public async Task<Response> Task([FromBody] List vioList)
        {
            try
            {
                vioList.SetDC(mioContext);
                vioList.Task();
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