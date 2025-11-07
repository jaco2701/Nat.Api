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
    public class CuitController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly Token mioToken;
        public CuitController(NatContext vioContext, IHttpContextAccessor vioHttpContextAccessor)
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
                List<Cuit> lcoCuits = new List<Cuit>();
                if (vioFilter == null)
                    vioFilter = new Filter();
                if (vioFilter.coFilterItems == null)
                    vioFilter.coFilterItems = [];
                foreach (CuitModel lioCuitModel in mioContext.Cuits.Where(vioFilter.Build<CuitModel>()))
                {
                    try
                    {
                        lcoCuits.Add(new Cuit { ioDcModel = lioCuitModel });
                    }
                    catch (Exception lioE)
                    {
                        LogHelper.write(lioE);
                    }
                }
                return ResponseHelper.Get(lcoCuits);
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }

        [HttpPost("Task")]
        public async Task<Response> Task([FromBody] Cuit vioCuit)
        {
            try
            {
                vioCuit.SetDC(mioContext);
                vioCuit.Task();
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