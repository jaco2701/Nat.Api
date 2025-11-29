using Applet.Nat.Api.AFIP.Model;
using Applet.Nat.Api.DC;
using Applet.Nat.Api.Static;

namespace Applet.Nat.Api.Models.Afip
{
    public class AfipService
    {
        public string ivstrName { get; set; }
        public NatContext ioContext { get; set; }
        public string ivstrUrl
        {
            get
            {
                if (mivstrUrl == null)
                {
                    mivstrUrl = ListHelper.GetValue("PATH", ivstrName, ioContext);
                }
                return mivstrUrl;
            }
        }
        public string ivstrUrlLogin
        {
            get
            {
                if (mivstrUrlLogin == null)
                {
                    mivstrUrlLogin = ListHelper.GetValue("PATH", "login" , ioContext);
                }
                return mivstrUrlLogin;
            }
        }
        public string ivstrPathCertificate
        {
            get
            {
                if (mivstrPathCertificate == null)
                {
                    mivstrPathCertificate = ListHelper.GetValue("PATH", "cert" , ioContext);
                }
                return mivstrPathCertificate;
            }
        }
        public System.Security.SecureString ioPassCertificate
        {
            get
            {
                if (mioPassCertificate == null)
                {
                    mioPassCertificate = new System.Security.SecureString();
                    foreach (char c in ListHelper.GetValue("PASS", "cert" , ioContext))
                        mioPassCertificate.AppendChar(c);
                    mioPassCertificate.MakeReadOnly();
                }
                return mioPassCertificate;
            }
        }
        public string ivstrPathToken
        {
            get
            {
                if (mivstrPathToken == null)
                {
                    mivstrPathToken = ListHelper.GetValue("PATH", "token" , ioContext);
                }
                return mivstrPathToken;
            }
        }
        public string ivstrDateformat
        {
            get
            {
                if (mivstrDateformat == null || mivstrDateformat == string.Empty)
                    mivstrDateformat = ListHelper.GetValue("FORMAT", "dtm" + ivstrName, ioContext);
                return (mivstrDateformat);
            }
        }
        public async Task<AfipLoginResponse> GetAfipLogin()
        {
            if (mioAfipLogin != null)
            {
                DateTime expirationTime = DateTimeOffset.Parse(mioAfipLogin.ivstrExpirationTime).UtcDateTime;
                if (expirationTime > DateTime.Now.ToUniversalTime())
                return mioAfipLogin;
            }
            try
            {
                AfipLoginRequest lioAfipLoginRequest = new AfipLoginRequest();
                mioAfipLogin = await lioAfipLoginRequest.Send(ivstrPathToken, ivstrName, ivstrUrlLogin, ivstrPathCertificate, ioPassCertificate);
                return mioAfipLogin;
            }
            catch (Exception lioEx)
            {
                throw new Exception(string.Format("Error en Autentificacion Afip Detalle: {0}", lioEx.Message));
            }
        }
        private string mivstrUrl;
        private string mivstrUrlLogin;
        private string mivstrPathCertificate;
        private System.Security.SecureString mioPassCertificate;
        private string mivstrPathToken;
        private string mivstrDateformat;
        private AfipLoginResponse mioAfipLogin;
    }
}
