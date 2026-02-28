using System.Net.Http.Headers;
using Applet.Nat.Api.Static;
using Nat.API.Properties;

namespace Applet.Nat.Api.Middleware
{
    public class HeaderValidation
    {
        private readonly RequestDelegate mioNextRequest;
        public IConfiguration mioConfiguration { get; }

        public HeaderValidation(RequestDelegate vioNextRequest, IConfiguration vioConfiguration)
        {
            mioNextRequest = vioNextRequest;
            mioConfiguration = vioConfiguration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (context.Request.Headers.Where(x => x.Key == "Authorization").Count() == 0)
                    throw new Exception(Resources.lioE_AuthHeaderNo);
                AuthenticationHeaderValue lioAuthHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
                if (lioAuthHeader == null)
                    throw new Exception(Resources.lioE_TokenNo);
                string[] lcvstrNotoken = mioConfiguration.GetSection("Noto")?.Value?.Split(',');
                if (!lcvstrNotoken.Contains(context.Request.Path.Value.ToLower()))
                    Auth.Validate(HttpsHeaderHelper.GetCredencials(lioAuthHeader)[0]);
                await mioNextRequest(context);
            }
            catch (Exception lioE)
            {
                LogHelper.write(lioE);
                if (lioE is Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException)
                    context.Response.StatusCode = 527;
                else
                    throw lioE;
            }
        }



    }
    public static class HeaderValidationExtensions
    {
        public static IApplicationBuilder UseHeaderValidation(
            this IApplicationBuilder builder, IConfiguration vioConfiguration)
        {
            return builder.UseMiddleware<HeaderValidation>(vioConfiguration);
        }
    }


}
