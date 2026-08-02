using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Проверка на пустые настройки
            if (string.IsNullOrEmpty(_settings.SenderEmail) || string.IsNullOrEmpty(_settings.SenderPassword))
            {
                Console.WriteLine("Email settings not configured. Skipping email send.");
                return;
            }

            // Проверка адреса получателя
            if (string.IsNullOrEmpty(to) || !to.Contains('@'))
            {
                Console.WriteLine($"Invalid recipient email: {to}. Skipping email send.");
                return;
            }

            // Проверка адреса отправителя
            if (!_settings.SenderEmail.Contains('@'))
            {
                Console.WriteLine($"Invalid sender email: {_settings.SenderEmail}. Skipping email send.");
                return;
            }

            try
            {
                using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
                {
                    EnableSsl = _settings.UseSsl,
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error (full): {ex}");
                throw;
            }
        }

        // Вспомогательные методы
        public async Task SendClientCreatedEmail(string to, string clientName)
        {
            var subject = $"Новый клиент создан: {clientName}";
            var body = $@"
                <h2>Клиент создан</h2>
                <p>Клиент <strong>{clientName}</strong> был успешно добавлен в систему.</p>
                <p>С уважением,<br/>CRM Archeonzero</p>";
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendClientUpdatedEmail(string to, string clientName)
        {
            var subject = $"Клиент обновлён: {clientName}";
            var body = $@"
                <h2>Клиент обновлён</h2>
                <p>Данные клиента <strong>{clientName}</strong> были изменены.</p>
                <p>С уважением,<br/>CRM Archeonzero</p>";
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendClientDeletedEmail(string to, string clientName)
        {
            var subject = $"Клиент удалён: {clientName}";
            var body = $@"
                <h2>Клиент удалён</h2>
                <p>Клиент <strong>{clientName}</strong> был удалён из системы.</p>
                <p>С уважением,<br/>CRM Archeonzero</p>";
            await SendEmailAsync(to, subject, body);
        }
    }
}