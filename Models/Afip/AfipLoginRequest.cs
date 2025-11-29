using Microsoft.SqlServer.Server;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Applet.Nat.Api.AFIP.Model
{
    public class AfipLoginRequest
    {
        #region Ctor
        public AfipLoginRequest() { }
        #endregion
        #region Metodos
        public async Task<AfipLoginResponse> Send(string vivstrPathToken, string vivstrService, string vivstrUrlLogin, string vivstrPathCertificate, System.Security.SecureString viostrPassCertificate, bool vioblnverbose=false)
        {
            if (!Directory.Exists(vivstrPathToken))
                Directory.CreateDirectory(vivstrPathToken);
            DirectoryInfo lioDirinfo = new DirectoryInfo(vivstrPathToken);
            Regex lioRegex = new Regex($"{vivstrService.ToUpper()}[1-9]");
            FileInfo[] lcoFiles = lioDirinfo.GetFiles()
                                       .Where(file => lioRegex.IsMatch(file.Name))
                                       .ToArray();
            Array.Sort(lcoFiles, delegate (FileInfo liof1le1, FileInfo liofile2)
            {
                return liofile2.CreationTime.CompareTo(liof1le1.CreationTime);
            });

            System.Xml.XmlDocument lioxlmToken = new System.Xml.XmlDocument();
            if (lcoFiles.Length > 0)
            {
                lioxlmToken.Load(lcoFiles[0].FullName);
                DateTime livdtmExpirationTime = DateTimeOffset.Parse(lioxlmToken.SelectSingleNode("/loginTicketResponse/header/expirationTime").InnerXml).UtcDateTime;
                if (livdtmExpirationTime > DateTime.Now.ToUniversalTime())
                    return new AfipLoginResponse(lioxlmToken);
            }
            AfipLoginTicket lioLoginTicket = new AfipLoginTicket();
            string livstrAfipLoginTicketResponse= await lioLoginTicket.ObtenerLoginTicketResponse(vivstrService, vivstrUrlLogin, vivstrPathCertificate, viostrPassCertificate,vioblnverbose);     
            foreach (FileInfo lioFile in lcoFiles) //borrado de tokens anteriores
            {
                try
                {
                    lioFile.Delete();
                }
                catch (Exception)
                {
                }
            }
            lioxlmToken.LoadXml(livstrAfipLoginTicketResponse);
            lioxlmToken.Save($"{vivstrPathToken}/{vivstrService.ToUpper()}{DateTime.Now.ToString("yyyyMMddhhmmssfff")}.xml");
            return new AfipLoginResponse(lioxlmToken);
        }

        #endregion
    }
}

