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
    public class CuitCuitController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly Token mioToken;
        public CuitCuitController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor)
        {
            mioContext = vioContext;
            mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
        }

        [HttpPost("Get")]
        public Response Get([FromBody] Filter? vioFilter)
        {
            try
            {
                List<CuitCuit> lcoCuitCuits = new List<CuitCuit>();
                if (vioFilter == null)
                    vioFilter = new Filter();
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                foreach (CuitCuitModel lioCuitCuitModel in mioContext.CuitCuits.Where(vioFilter.Build<CuitCuitModel>()))
                {
                    try
                    {
                        lcoCuitCuits.Add(new CuitCuit { ioDcModel = lioCuitCuitModel });
                    }
                    catch (Exception lioE)
                    {
                        LogHelper.write(lioE);
                    }
                }
                return ResponseHelper.Get(lcoCuitCuits);
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }

        [HttpPost("Task")]
        public async Task<Response> Task([FromBody] CuitCuit[] vcoCuitCuits)
        {
            try
            {
                foreach (CuitCuit lioCuitCuit in vcoCuitCuits)
                {
                    lioCuitCuit.SetDC(mioContext);
                    lioCuitCuit.Task();
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