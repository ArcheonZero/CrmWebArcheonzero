using Microsoft.Extensions.Options;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Xunit;

namespace CrmWebArcheonzero.Tests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public void SendEmail_ShouldSkip_WhenSettingsEmpty()
        {
            var settings = Options.Create(new EmailSettings
            {
                SenderEmail = "",
                SenderPassword = ""
            });
            var service = new EmailService(settings);

            var exception = Record.Exception(() =>
                service.SendEmailAsync("test@test.com", "Тест", "Тело").Wait());

            Assert.Null(exception);
        }

        [Fact]
        public void SendEmail_ShouldSkip_WhenRecipientInvalid()
        {
            var settings = Options.Create(new EmailSettings
            {
                SenderEmail = "sender@test.com",
                SenderPassword = "pass"
            });
            var service = new EmailService(settings);

            var exception = Record.Exception(() =>
                service.SendEmailAsync("invalid", "Тест", "Тело").Wait());

            Assert.Null(exception);
        }

        [Fact]
        public void SendEmail_ShouldSkip_WhenSenderInvalid()
        {
            var settings = Options.Create(new EmailSettings
            {
                SenderEmail = "not-an-email",
                SenderPassword = "pass"
            });
            var service = new EmailService(settings);

            var exception = Record.Exception(() =>
                service.SendEmailAsync("test@test.com", "Тест", "Тело").Wait());

            Assert.Null(exception);
        }
    }
}