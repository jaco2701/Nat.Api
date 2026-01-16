using Applet.Nat.Api.Br;
using Applet.Nat.Api.DC;
using Microsoft.IdentityModel.Tokens;
using Nat.API.Properties;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace Applet.Nat.Api.Static
{
    public static class Auth
    {
        public static void Validate(string vivstrToken)
        {
            TokenValidationParameters lioValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetKey()
            };
            new JwtSecurityTokenHandler().ValidateToken(vivstrToken, lioValidationParameters, out SecurityToken rioSecurityToken);
            if (rioSecurityToken == null)
                throw (new Exception(Resources.lioE_TokenNo));
        }
        public static string Get(int vivnumUser, NatContext vioContext, string vivstrAuthType)
        {
            Token lioTokenModel = new Token
            {
                ivnumUser = vivnumUser,
                ivstrAuthType = vivstrAuthType
            };
            return GetJwtSecurityTokenHandler(lioTokenModel, "Claim", int.Parse(ListHelper.GetValue("Expire", "Token", vioContext)));
        }
        public static Token DeserializeToken(string vivstrToken, NatContext vioContext)
        {
            vivstrToken = vivstrToken.Replace("NatToken ", "");
            if (!(new JwtSecurityTokenHandler().ReadToken(vivstrToken) is JwtSecurityToken lioJwtSecurityToken))
                throw new Exception(Resources.lioE_TokenNo);
            Claim lioClaim = lioJwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "Claim");
            if (lioClaim == null)
                throw new Exception(Resources.lioE_TokenNo);
            Token lioToken = JsonSerializer.Deserialize<Token>(lioClaim.Value);
            if (lioToken == null)
                throw new Exception(Resources.lioE_TokenNo);

            return lioToken;
        }
        private static string GetJwtSecurityTokenHandler(Token vioTokenModel, string vivstrClaimType, int numHoursTokenExpiration)
        {
            Claim[] lcoClaims = new[]
            {
                new Claim(vivstrClaimType, JsonSerializer.Serialize(vioTokenModel), JsonClaimValueTypes.Json),
            };
            SymmetricSecurityKey lioKey = GetKey();

            SigningCredentials lioSigningCredentials = new SigningCredentials(lioKey, SecurityAlgorithms.HmacSha256);
            //DateTime livdtm = DateTime.Now.AddMinutes((DateTime.Now - DateTime.UtcNow).TotalMinutes + numMinsTokenExpiration);
            DateTime livdtm = DateTime.Now.AddHours(numHoursTokenExpiration);
            JwtSecurityToken lioToken = new JwtSecurityToken(
                claims: lcoClaims,
                expires: livdtm.ToLocalTime(),
                signingCredentials: lioSigningCredentials);
            return new JwtSecurityTokenHandler().WriteToken(lioToken);
        }
        public static string Encrypt(string vivstr)
        {
            SymmetricSecurityKey lioKey = GetKey();
            Byte[] lcoEncryptor = lioKey.Key;

            using (var lioAes = Aes.Create())
            {
                lioAes.Key = lcoEncryptor;
                lioAes.GenerateIV();
                var lioIV = lioAes.IV;

                using (var lioCryptorTransform = lioAes.CreateEncryptor(lioAes.Key, lioIV))
                using (var lioMS = new MemoryStream())
                {
                    lioMS.Write(lioIV, 0, lioIV.Length);
                    using (var cs = new CryptoStream(lioMS, lioCryptorTransform, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(vivstr);
                    }
                    return Convert.ToBase64String(lioMS.ToArray());
                }
            }

        }
        public static string Decrypt(string vivstr)
        {
            var lioRegValue = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Applet\Nat", "DcKey", null);
            if (lioRegValue == null)
            {
                throw new InvalidOperationException("Key no encontrada");
            }
            SymmetricSecurityKey lioKey = GetKey();
            Byte[] lcoEncryptor = lioKey.Key; var cipherBytes = Convert.FromBase64String(vivstr);

            using (var lioAes = Aes.Create())
            {
                lioAes.Key = lcoEncryptor;

                // Extract IV from the encrypted data
                var iv = new byte[lioAes.BlockSize / 8];
                Array.Copy(cipherBytes, iv, iv.Length);

                using (var decryptorTransform = lioAes.CreateDecryptor(lioAes.Key, iv))
                using (var ms = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptorTransform, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        private static SymmetricSecurityKey GetKey()
        {
            var lioRegValue = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Applet\Nat", "DcKey", null);
            if (lioRegValue == null)
            {
                throw new InvalidOperationException("Key no encontrada");
            }
            return new SymmetricSecurityKey(lioRegValue as Byte[]);
        }
    }
    public static class HttpsHeaderHelper
    {
        public static string[] GetCredencials(AuthenticationHeaderValue vioAuthenticationHeaderValue)
        {
            if (vioAuthenticationHeaderValue == null)
                throw new Exception("Cabecera de Autorizacion invalida");
            string livstrCreds = vioAuthenticationHeaderValue.Parameter ?? string.Empty;
            if (string.IsNullOrEmpty(Format.SanitizeBase64String(livstrCreds)))
                throw new Exception("Cabecera de Autorizacion invalida");
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            if (vioAuthenticationHeaderValue.Scheme == "NatAuth" || vioAuthenticationHeaderValue.Scheme == "VwAuth" || vioAuthenticationHeaderValue.Scheme == "NatSvc")
            {
                List<string> lcvstrRet = new List<string>();
                lcvstrRet.Add(vioAuthenticationHeaderValue.Scheme);
                string[] lcvstr;
                lcvstr = encoding.GetString(Convert.FromBase64String(Format.SanitizeBase64String(livstrCreds))).Split(':');
                if (lcvstr.Length != 2)
                    throw new Exception("Cabecera de Autorizacion invalida");
                lcvstrRet.Add(lcvstr[0]);
                lcvstrRet.Add(lcvstr[1]);
                return lcvstrRet.ToArray();
            }
            if (vioAuthenticationHeaderValue.Scheme == "NatToken")
                return new string[] { livstrCreds };
            throw new Exception("Cabecera de Autorizacion invalida");
        }
    }
    //public static byte[] EncryptWith3Des(string DataToEncrypt)
    //{
    //    UTF8Encoding utF8Encoding = new UTF8Encoding();
    //    MemoryStream memoryStream = new MemoryStream();
    //    try
    //    {
    //        byte[] bytes = utF8Encoding.GetBytes(DataToEncrypt);
    //        ICryptoTransform encryptor = new TripleDESCryptoServiceProvider().CreateEncryptor(k.pKey, k.pIV);
    //        CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
    //        cryptoStream.Write(bytes, 0, bytes.Length);
    //        cryptoStream.FlushFinalBlock();
    //        cryptoStream.Close();
    //        return memoryStream.ToArray();
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception("SimetricEncrypt::EncryptWith3Des \n" + ex.Message, ex);
    //    }
    //    finally
    //    {
    //        memoryStream.Close();
    //    }
    //}

    //public static string DecryptFrom3Des(string vivstrData)
    //{
    //    byte[] coDataToDecrypt = new UTF8Encoding().GetBytes(vivstrData);
    //    ICryptoTransform decryptor = new TripleDESCryptoServiceProvider().CreateDecryptor(k.pKey, k.pIV);
    //    MemoryStream memoryStream = new MemoryStream(coDataToDecrypt);
    //    CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
    //    StreamReader streamReader = new StreamReader((Stream)cryptoStream, true);
    //    string end = streamReader.ReadToEnd();
    //    cryptoStream.Close();
    //    memoryStream.Close();
    //    streamReader.Close();
    //    return end;
    //}

    //public static string Encrypt(string clearData)
    //{
    //    if (!(clearData != "") || clearData == null)
    //        return "";
    //    MemoryStream memoryStream = new MemoryStream();
    //    TripleDES tripleDes = TripleDES.Create();
    //    tripleDes.Key = k.pKey;
    //    tripleDes.IV = k.pIV;
    //    byte[] bytes = Encoding.Unicode.GetBytes(clearData);
    //    CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, tripleDes.CreateEncryptor(), CryptoStreamMode.Write);
    //    try
    //    {
    //        cryptoStream.Write(bytes, 0, bytes.Length);
    //    }
    //    finally
    //    {
    //        cryptoStream.Close();
    //    }
    //    return Convert.ToBase64String(memoryStream.ToArray());
    //}
    //public static string Decrypt(string cipherData)
    //{
    //    if (!(cipherData != "") || cipherData == null)
    //        return "";
    //    MemoryStream memoryStream = new MemoryStream();
    //    TripleDES tripleDes = TripleDES.Create();
    //    tripleDes.Key = k.pKey;
    //    tripleDes.IV = k.pIV;
    //    byte[] buffer = Convert.FromBase64String(cipherData);
    //    CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, tripleDes.CreateDecryptor(), CryptoStreamMode.Write);
    //    try
    //    {
    //        cryptoStream.Write(buffer, 0, buffer.Length);
    //    }
    //    finally
    //    {
    //        cryptoStream.Close();
    //    }
    //    return Encoding.Unicode.GetString(memoryStream.ToArray());
    //}
    //internal class k
    //{
    //    public static byte[] pKey = new byte[24]
    //    {
    //      (byte) 22,
    //      (byte) 245,
    //      (byte) 162,
    //      (byte) 10,
    //      (byte) 65,
    //      (byte) 146,
    //      (byte) 151,
    //      (byte) 39,
    //      (byte) 217,
    //      (byte) 92,
    //      (byte) 199,
    //      (byte) 77,
    //      (byte) 240,
    //      (byte) 182,
    //      (byte) 152,
    //      (byte) 28,
    //      (byte) 214,
    //      (byte) 232,
    //      (byte) 11,
    //      (byte) 14,
    //      (byte) 121,
    //      (byte) 59,
    //      (byte) 139,
    //      (byte) 28
    //    };
    //    public static byte[] pIV = new byte[8]
    //    {
    //      (byte) 123,
    //      (byte) 180,
    //      (byte) 223,
    //      (byte) 208,
    //      (byte) 139,
    //      (byte) 236,
    //      (byte) 11,
    //      (byte) 209
    //    };
    //    public static byte[] pKey2 = new byte[32]
    //    {
    //      (byte) 25,
    //      (byte) 86,
    //      (byte) 250,
    //      (byte) 56,
    //      (byte) 28,
    //      (byte) 202,
    //      (byte) 25,
    //      (byte) 151,
    //      (byte) 94,
    //      (byte) 70,
    //      (byte) 132,
    //      (byte) 243,
    //      (byte) 171,
    //      (byte) 149,
    //      (byte) 233,
    //      (byte) 164,
    //      (byte) 17,
    //      (byte) 105,
    //      (byte) 159,
    //      (byte) 219,
    //      (byte) 231,
    //      (byte) 112,
    //      (byte) 42,
    //      (byte) 100,
    //      (byte) 155,
    //      (byte) 17,
    //      (byte) 163,
    //      (byte) 78,
    //      (byte) 105,
    //      (byte) 170,
    //      (byte) 26,
    //      (byte) 131
    //    };
    //    public static byte[] piv2 = new byte[16]
    //    {
    //      (byte) 196,
    //      (byte) 175,
    //      (byte) 189,
    //      (byte) 217,
    //      (byte) 243,
    //      (byte) 153,
    //      (byte) 113,
    //      (byte) 148,
    //      (byte) 32,
    //      (byte) 236,
    //      (byte) 8,
    //      (byte) 217,
    //      (byte) 88,
    //      (byte) 126,
    //      (byte) 220,
    //      (byte) 6
    //    };
    //}
}

