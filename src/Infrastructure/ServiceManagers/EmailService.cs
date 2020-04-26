using LetsWork.Domain.Interfaces.ServiceManagers;
using LetsWork.Domain.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LetsWork.Infrastructure.ServiceManagers
{
    public class EmailService : IEmailService
    {
        private readonly ConfigSettings _configSettings;
        public EmailService(IOptions<ConfigSettings> ConfigurationSettingsOptions)
        {
            _configSettings = ConfigurationSettingsOptions.Value;
        }

        public async Task SendEmailAsync(EmailModel EmailModelObject)
        {
            try
            {
                using (MailMessage mailMessage = new MailMessage(_configSettings.GoogleSMTPSettings.FromEmail, EmailModelObject.EmailTo)
                {
                    IsBodyHtml = true,
                    Body = EmailModelObject.Body,
                    BodyEncoding = Encoding.UTF8,
                    Subject = EmailModelObject.Subject
                })
                    await SendEmailViaSMTPAsync(mailMessage);
            }
            catch(Exception)
            {
                throw;
            }
        }

        #region SMTP Email Sending Helper
        public async Task SendEmailViaSMTPAsync(MailMessage MessageToBeSent)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient
                {
                    Host = _configSettings.GoogleSMTPSettings.Host,
                    Port = _configSettings.GoogleSMTPSettings.Port,
                    Timeout = _configSettings.GoogleSMTPSettings.Timeout,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_configSettings.GoogleSMTPSettings.FromEmail, _configSettings.GoogleSMTPSettings.FromPassword),
                    EnableSsl = true
                })
                    await smtpClient.SendMailAsync(MessageToBeSent);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
