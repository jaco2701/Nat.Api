using Applet.Nat.Api.Br;
using Applet.Nat.Api.Br.Models;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Models;
using Applet.Nat.Api.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.SqlServer.Server;
using Nat.API.Properties;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE);
            }
        }

        [HttpPost("OidcState")]
        public async Task<Response> OidcState([FromBody] OidcRequest vioOidcRequest)
        {
            try
            {
                Oauth2Response lioOauth2Response = null;
                IdentityProviderModel lioIdentityProviderModel = null;
                switch ((eOidcRequestType)vioOidcRequest.ivnroType)
                {
                    case eOidcRequestType.GetAuthUrl:
                        if (vioOidcRequest.ivnumIdentityProvider == 0)
                            throw new Exception($"{Resources.lioE_NoCreds} IdentityProvider");
                        lioIdentityProviderModel = mioContext.IdentityProviders.Find(vioOidcRequest.ivnumIdentityProvider);
                        if (lioIdentityProviderModel == null)
                            throw new Exception($"{Resources.lioE_NoCreds} IdentityProviderModel");
                        string livstrState = Guid.NewGuid().ToString();
                        const int livnumVerifierLength = 50;
                        const string livstrChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
                        byte[] lcoRandom = new byte[livnumVerifierLength];
                        using (RandomNumberGenerator lioRandomNumberGenerator = RandomNumberGenerator.Create())
                        {
                            lioRandomNumberGenerator.GetBytes(lcoRandom);
                        }
                        StringBuilder lioSB = new StringBuilder(livnumVerifierLength);
                        foreach (byte lioByte in lcoRandom)
                        {
                            lioSB.Append(livstrChars[lioByte % livstrChars.Length]);
                        }
                        string livstrCodeVerifier = PkceHelper.GenerateCodeVerifier();
                        mioContext.OidcOidcStates.Add(
                            new OidcStateModel
                            {
                                ivstrState = livstrState,
                                ivstrIdentityProviderId = lioIdentityProviderModel.ivstrIdentityProviderId,
                                ivnumIdentityProvider = lioIdentityProviderModel.ivnumIdentityProvider,
                                ivdtmState = DateTime.UtcNow,
                                ivstrCodeVerifier = livstrCodeVerifier
                            });
                        mioContext.SaveChanges();
                        string livstrRedirectUrl = lioIdentityProviderModel.ivstrIdentityProviderUrlLogin
                            .Replace("{CI}", lioIdentityProviderModel.ivstrIdentityProviderId)
                            .Replace("{RU}", Uri.EscapeDataString(ListHelper.GetValue("FORMAT", "OidcUrl", mioContext)))
                            .Replace("{ST}", livstrState)
                            .Replace("{CH}", PkceHelper.GenerateCodeChallenge(livstrCodeVerifier));
                        LogHelper.writeinfo($"RedirectUrl: {livstrRedirectUrl}", ListHelper.Verbose(mioContext));

                        return ResponseHelper.Get(livstrRedirectUrl);

                    case eOidcRequestType.ExchangeCode:
                        {
                            if (string.IsNullOrEmpty(vioOidcRequest.ivstrState))
                                throw new Exception($"{Resources.lioE_NoCreds} STATE");
                            if (string.IsNullOrEmpty(vioOidcRequest.ivstrCode))
                                throw new Exception($"{Resources.lioE_NoCreds} CODE");
                            OidcStateModel lioO = mioContext.OidcOidcStates.FirstOrDefault(x => x.ivstrState == vioOidcRequest.ivstrState);
                            if (lioO == null)
                                throw new Exception($"{Resources.lioE_NoCreds} STATE");
                            if (lioO?.ivnumIdentityProvider == null || lioO.ivnumIdentityProvider == 0)
                                throw new Exception($"{Resources.lioE_NoCreds} IdentityProvider");
                            lioIdentityProviderModel = mioContext.IdentityProviders.Find(lioO.ivnumIdentityProvider);
                            if (lioIdentityProviderModel == null)
                                throw new Exception($"{Resources.lioE_NoCreds} IdentityProviderModel");
                            Dictionary<string, string> lcoParams = new Dictionary<string, string>
                            {
                                { "client_id", lioIdentityProviderModel.ivstrIdentityProviderId },
                                { "grant_type", "authorization_code" },
                                { "code", vioOidcRequest.ivstrCode },
                                { "redirect_uri",  ListHelper.GetValue("FORMAT", "OidcUrl", mioContext) },
                                { "code_verifier",  lioO.ivstrCodeVerifier }
                            };
                            using HttpClient lioHttpClient = new HttpClient();
                            {
                                using HttpResponseMessage lioResponse = await lioHttpClient.PostAsync(lioIdentityProviderModel.ivstrIdentityProviderUrlToken, new FormUrlEncodedContent(lcoParams));
                                {
                                    if (!lioResponse.IsSuccessStatusCode)
                                    {
                                        string lioError = await lioResponse.Content.ReadAsStringAsync();
                                        LogHelper.writeinfo("TOKEN EXCHANGE ERROR**************", ListHelper.Verbose(mioContext));
                                        LogHelper.writeinfo(JsonConvert.SerializeObject(lcoParams), ListHelper.Verbose(mioContext));
                                        LogHelper.writeinfo(lioError, ListHelper.Verbose(mioContext));
                                        throw new Exception($"{Resources.lioE_NoCreds} Token Exchange");
                                    }
                                    lioOauth2Response = JsonConvert.DeserializeObject<Oauth2Response>(await lioResponse.Content.ReadAsStringAsync());
                                }
                            }
                        }
                        break;
                    case eOidcRequestType.RefreshToken:
                        {
                            if (vioOidcRequest.ivnumIdentityProvider == 0)
                                throw new Exception($"{Resources.lioE_NoCreds} IdentityProvider");
                            if (string.IsNullOrEmpty(vioOidcRequest.ivstrRefreshToken))
                                throw new Exception($"{Resources.lioE_NoCreds} RefreshToken");
                            lioIdentityProviderModel = mioContext.IdentityProviders.Find(vioOidcRequest.ivnumIdentityProvider);
                            if (lioIdentityProviderModel == null)
                                throw new Exception($"{Resources.lioE_NoCreds} IdentityProviderModel");
                            Dictionary<string, string> lcoParams = new Dictionary<string, string>
                            {
                                { "client_id", lioIdentityProviderModel.ivstrIdentityProviderId },
                                { "grant_type", "refresh_token" },
                                { "refresh_token", vioOidcRequest.ivstrRefreshToken },
                            };
                            using HttpClient lioHttpClient = new HttpClient();
                            {
                                using HttpResponseMessage lioResponse = await lioHttpClient.PostAsync(lioIdentityProviderModel.ivstrIdentityProviderUrlToken, new FormUrlEncodedContent(lcoParams));
                                {
                                    if (!lioResponse.IsSuccessStatusCode)
                                    {
                                        string lioError = await lioResponse.Content.ReadAsStringAsync();
                                        LogHelper.writeinfo(lioError, true);
                                        throw new Exception($"{Resources.lioE_NoCreds} Token Exchange");
                                    }
                                    lioOauth2Response = JsonConvert.DeserializeObject<Oauth2Response>(await lioResponse.Content.ReadAsStringAsync());
                                }
                            }
                            break;
                        }
                    default:
                        throw new Exception(Resources.lioE_NoCreds);
                }
                if (lioOauth2Response == null || string.IsNullOrEmpty(lioOauth2Response.IdToken))
                    throw new Exception($"{Resources.lioE_NoCreds} Token Response");
                JwtSecurityTokenHandler lioJwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jsonToken = lioJwtSecurityTokenHandler.ReadToken(lioOauth2Response.AccessToken) as JwtSecurityToken;
                string livstrUserEmail = jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                UserModel lioUserModel = mioContext.Users.FirstOrDefault(u => u.ivstrUserEmail == livstrUserEmail);
                if (lioUserModel == null)
                    throw new Exception($"{Resources.lioE_NoCreds} User {livstrUserEmail}");
                LogHelper.writeinfo($"User {lioUserModel.ivstrUserName} authenticated successfully.", ListHelper.Verbose(mioContext));
                return ResponseHelper.Get(
                    new Login
                    {
                        ioUser = new User(lioUserModel, mioContext),
                        ivstrToken = lioOauth2Response.AccessToken,
                        ivstrRefreshToken = lioOauth2Response.RefreshToken,
                    });
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                return ResponseHelper.Get(-1, lioE.Message);
            }
        }
    }
}

