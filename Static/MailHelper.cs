using Microsoft.Extensions.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using Applet.Nat.Api.DC;

namespace Applet.Nat.Api.Static
{
    public static class MailHelper
    {
        public const string mivstrAddressPattern =
           @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				        [0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
           + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				        [0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
           + @"([a-zA-Z0-9]+[\w-]+\.)+[a-zA-Z]{1}[a-zA-Z0-9-]{1,23})$";

        public static bool IsValidEmail(string vivstreAddress)
        {
            return !string.IsNullOrEmpty(vivstreAddress) && Regex.IsMatch(vivstreAddress, mivstrAddressPattern);
        }

        public static void Send(string vivstrSubject, string vivstrBody, string vivstrAddress, IReadOnlyCollection<AlternateView> vcoAlternateViews, IReadOnlyCollection<Attachment> vcoAttachments, NatContext vioContext)
        {
            EnviarEmail(vivstrSubject, vivstrBody, new List<string> { vivstrAddress }, vcoAlternateViews, vcoAttachments, vioContext);
        }


        public static void EnviarEmail(string vivsterSubject, string vivstrBody, List<string> vcvstrAddress,IReadOnlyCollection<AlternateView> vcoAlternateViews, IReadOnlyCollection<Attachment> vcoAttachments, NatContext vioContext)
        {
            try
            {
                ListModel[] lcoSmtpConfig = ListHelper.GetAll("Smtp", vioContext);
                var message = new MailMessage
                {
                    From = new MailAddress(lcoSmtpConfig.First(x=>x.ivcodId == "From").ivstrDesc,lcoSmtpConfig.First(x => x.ivcodId == "FromDisplay").ivstrDesc),
                    Subject = vivsterSubject,
                    IsBodyHtml = true,
                    Body = vivstrBody
                };
                foreach (var livstrAddress in vcvstrAddress) 
                    message.Bcc.Add(new MailAddress(livstrAddress));
                if (vcoAlternateViews != null)
                    foreach (var lioO in vcoAlternateViews)
                        message.AlternateViews.Add(lioO);
                if (vcoAttachments != null)
                    foreach (var lioO in vcoAttachments)
                        message.Attachments.Add(lioO);

                using var client = new SmtpClient
                {
                    Host = lcoSmtpConfig.First(x => x.ivcodId == "Host").ivstrDesc,
                    Port = int.Parse(lcoSmtpConfig.First(x => x.ivcodId == "Port").ivstrDesc),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(lcoSmtpConfig.First(x => x.ivcodId == "Usr").ivstrDesc, lcoSmtpConfig.First(x => x.ivcodId == "Pass").ivstrDesc)
                };
                client.Send(message);
                client.Dispose();
            }
            catch (Exception lioE)
            {
                throw new Exception("Error en Envio de Correo:" + lioE.ToString()); ;
            }
        }
    }
}



