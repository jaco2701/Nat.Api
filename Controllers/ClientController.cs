using Applet.Misc.EncDec;
using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Ifaces;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nat.API.Models.BR;
using Nat.API.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Applet.Nat.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly NatContext mioContext;
        private readonly IConfiguration mioConfiguration;
        private readonly Token mioToken;
        public ClientController(NatContext vioContext, IConfiguration vioConfiguration, IHttpContextAccessor vioHttpContextAccessor)
        {
            mioContext = vioContext;
            mioConfiguration = vioConfiguration;
            mioToken = Auth.DeserializeToken(vioHttpContextAccessor.HttpContext.Request.Headers.Authorization, vioContext);
        }

        [HttpPost("load")]
        public async Task<ActionResult> Load([FromBody] Object vioPayload)
        {
            try
            {
                LogHelper.writeinfo(JsonConvert.SerializeObject(vioPayload), ListHelper.GetValue("FORMAT", "VERBOSE", mioContext) == "1");
                JObject lioJObject = JObject.Parse(JsonConvert.SerializeObject(vioPayload));
                JToken? lioToken = lioJObject.SelectToken("DocumentInfo.PropertyTaxNumber");
                if (lioToken == null || lioToken.Type == JTokenType.Null || string.IsNullOrWhiteSpace(lioToken.ToString()))
                    throw new Exception($"Cuit [DocumentInfo.PropertyTaxNumber] {Resources.lioE_ObjectNoM}");
                if (!long.TryParse(lioToken.ToString().Replace("-", ""), out long livlngCuitEmisor))
                    throw new Exception("DocumentInfo.PropertyTaxNumber " + Resources.lioE_ObjectNoM);
                DocumentUploadRequest lioDocumentUploadRequest = new DocumentUploadRequest
                {
                    ivblnComp = false,
                    ivlngCuit = livlngCuitEmisor,
                    ivstrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vioPayload))),
                    ivstrName = $"Load_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json"
                };
                List<DocumentUploadResponse> lcoResponses = DocHelper.UploadDocument(lioDocumentUploadRequest, mioConfiguration, mioToken.ivnumUser);
                Document lioDocument;
                string livstrRawResponse = "[";
                foreach (DocumentUploadResponse lioResponse in lcoResponses)
                {
                    if (lioResponse == null) continue;
                    if (lioResponse.ivnroStatus == 1)
                    {
                        if (lioResponse.ioDocumentUser == null || (lioResponse.ioDocumentUser.ivblnSaveOnLoad ?? true))
                            lioDocument = new Document(lioResponse.ivlngDoc ?? 0, mioContext, mioConfiguration);
                        else
                            lioDocument = new Document(lioResponse.ioDocumentUser, mioContext, mioConfiguration);
                        await lioDocument.Auth();
                        livstrRawResponse += await lioDocument.SendResponse();
                    }
                    else
                        livstrRawResponse += DocHelper.BuildDocumentResponse(livlngCuitEmisor, mioConfiguration, lioResponse.ivstrDescStatus??string.Empty);
                }
                livstrRawResponse += "]";
                LogHelper.writeinfo(livstrRawResponse, true);
                return Ok(JsonConvert.DeserializeObject(livstrRawResponse));
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return BadRequest(lioE.Message);
            }
        }
    }
}

