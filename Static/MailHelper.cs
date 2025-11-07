using Applet.Nat.Api.DC;
using Applet.Nat.Api.Models.BR;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;

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

        public static void Send(string vivstrSubject, string vivstrBody, string[] vcvstrAddress, IReadOnlyCollection<AlternateView> vcoAlternateViews, IReadOnlyCollection<Attachment> vcoAttachments, NatContext vioContext)
        {
            SmtpClient lioSmtpClient = GetSmtpClient(vioContext);
            MailMessage lioMailMessage = PrepareMsg(vivstrSubject, vivstrBody, vcvstrAddress, vcoAlternateViews, vcoAttachments, vioContext);
            try
            {
                lioSmtpClient.Send(lioMailMessage);
            }
            catch 
            {
               throw;
            }
            finally
            {
                lioSmtpClient.Dispose();
                lioMailMessage.Dispose();
            }
        }

        public static SmtpClient GetSmtpClient(NatContext vioContext)
        {
            ListModel[] lcoSmtpConfig = ListHelper.GetAll("SMTP", vioContext);
            var lioSmtpClient = new SmtpClient
            {
                Host = lcoSmtpConfig.First(x => x.ivcodId == "HOST").ivstrDesc,
                Port = int.Parse(lcoSmtpConfig.First(x => x.ivcodId == "PORT").ivstrDesc),
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = int.Parse(lcoSmtpConfig.First(x => x.ivcodId == "TIMEOUT")?.ivstrDesc ?? "10000"),
                Credentials = new NetworkCredential(lcoSmtpConfig.First(x => x.ivcodId == "USER").ivstrDesc, lcoSmtpConfig.First(x => x.ivcodId == "PASS").ivstrDesc),
            };
            return lioSmtpClient;
        }
        public static MailMessage PrepareMsg(string vivstrSubject, string vivstrBody, string[] vcvstrAddress, IReadOnlyCollection<AlternateView> vcoAlternateViews, IReadOnlyCollection<Attachment> vcoAttachments, NatContext vioContext)
        {
            ListModel[] lcoSmtpConfig = ListHelper.GetAll("SMTP", vioContext);
            var lioMailMessage = new MailMessage
            {
                From = new MailAddress(lcoSmtpConfig.First(x => x.ivcodId == "USER").ivstrDesc, lcoSmtpConfig.First(x => x.ivcodId == "USERDISP").ivstrDesc),
                Subject = vivstrSubject,
                IsBodyHtml = false,
                Body = vivstrBody
            };
            foreach (var livstrAddress in vcvstrAddress)
                lioMailMessage.To.Add(new MailAddress(livstrAddress));
            if (vcoAlternateViews != null)
                foreach (var lioO in vcoAlternateViews)
                    lioMailMessage.AlternateViews.Add(lioO);
            if (vcoAttachments != null)
                foreach (var lioO in vcoAttachments)
                    lioMailMessage.Attachments.Add(lioO);
            return lioMailMessage;
        }
        public static void SendAsync(long vivlngDoc, string vivstrSubject, string vivstrBody, string[] vcvstrAddress, IReadOnlyCollection<AlternateView> vcoAlternateViews, IReadOnlyCollection<Attachment> vcoAttachments, IConfiguration viIConfiguration)
        {
            using NatContext lioContext = NatContext.GetContext(viIConfiguration);
            {
                SmtpClient lioSmtpClient = GetSmtpClient(lioContext);
                lioSmtpClient.EnableSsl = true;
                MailMessage lioMailMessage = PrepareMsg(vivstrSubject, vivstrBody, vcvstrAddress, vcoAlternateViews, vcoAttachments, lioContext);
                lioSmtpClient.SendCompleted += (sender, e) =>
                {
                    SendCompletedCallback(sender, e, vivlngDoc, viIConfiguration);
                };
                string livstrUsrToken = new Random(999).Next().ToString();
                lioSmtpClient.SendAsync(lioMailMessage, livstrUsrToken);
                //lioSmtpClient.Dispose();
                //lioMailMessage.Dispose();
            }
        }
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs vioEventArgs, long vivlngDoc, IConfiguration viIConfiguration)
        {
            using NatContext lioContext = NatContext.GetContext(viIConfiguration);
            {
                if (vioEventArgs.Error != null)
                {
                    LogHelper.write(vioEventArgs.Error);
                    new DocumentTracking(lioContext, vivlngDoc)
                        .addTrack(80, $"Error: {vioEventArgs.Error.Message}");
                }
                else if (vioEventArgs.Cancelled)
                {
                    LogHelper.writeinfo("Envio de Correo Cancelado.", false);
                    new DocumentTracking(lioContext, vivlngDoc)
                        .addTrack(80, "Envio de Correo Cancelado.");
                }
                else
                {
                    new DocumentTracking(lioContext, vivlngDoc)
                        .addTrack(70, "Email sent successfully.");
                }
            }
        }
    }
}



