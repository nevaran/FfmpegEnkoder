using FfmpegEnkoder.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;
using System.Text.Json;

namespace FfmpegEnkoder.Services
{
    public static class NotifyByEmail
    {
        /// <summary>
        /// Sends an email from one assigned email to another using MailKit
        /// </summary>
        public static void SendNotify()
        {
            try
            {
                EmailInfo eHost = FetchEmailInfo();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Nevaran FFMPEG Enkoder", eHost.Email));
                message.To.Add(new MailboxAddress(eHost.EmailClient, eHost.EmailClient));
                message.Subject = "Ffmpeg Enkoder";

                message.Body = new TextPart("plain")
                {
                    Text = $"Encoding done @{DateTime.Now}"
                };

                using var client = new SmtpClient();

                client.Connect("smtp.gmail.com", 587, false);

                client.Authenticate(eHost.Email, eHost.Password);

                client.Send(message);
                client.Disconnect(true);
            }
            catch
            {

            }
        }

        static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = false,
        };

        /// <summary>
        /// Create a json template used for email information
        /// </summary>
        public static void WriteEmailDefaultInfo()
        {
            string settingsPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\emailhost.json";

            if (!File.Exists(settingsPath))
            {
                var eHost = new EmailInfo();

                var options = jsonSerializerOptions;
                string jsonString = JsonSerializer.Serialize(eHost, options);
                File.WriteAllText(settingsPath, jsonString);
            }
        }

        /// <summary>
        /// Grabs the json file (if it exists) to be used in sending an email
        /// </summary>
        private static EmailInfo FetchEmailInfo()
        {
            string settingsPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\emailhost.json";
            EmailInfo eHost = null;

            if (File.Exists(settingsPath))
            {
                string jsonString = File.ReadAllText(settingsPath);
                eHost = JsonSerializer.Deserialize<EmailInfo>(jsonString);
            }

            return eHost;//no settings, no email sent
        }

        /// <summary>
        /// Sends an email from one assigned email to another using System.Net.Mail.MailMessage
        /// </summary>
        /*public static void SendNotify()
        {
            string settingsPath = @$"{AppDomain.CurrentDomain.BaseDirectory}\emailhost.json";
            EmailInfo eHost;

            if (File.Exists(settingsPath))
            {
                string jsonString = File.ReadAllText(settingsPath);
                eHost = JsonSerializer.Deserialize<EmailInfo>(jsonString);
            }
            else//no settings, no email sent
            {
                return;
            }

            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(eHost.Email, eHost.Password)
            };

            System.Net.Mail.MailMessage mm = new System.Net.Mail.MailMessage(eHost.Email, eHost.EmailClient, "Ffmpeg Enkoder", "Encoding done!")
            {
                BodyEncoding = UTF8Encoding.UTF8,
                DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.OnFailure,
            };
            client.Send(mm);
        }*/
    }
}
