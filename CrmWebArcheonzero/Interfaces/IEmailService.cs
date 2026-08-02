namespace CrmWebArcheonzero.Interfaces
{
    public interface IEmailService
    {
        Task SendClientCreatedEmail(string to, string clientName);
        Task SendClientUpdatedEmail(string to, string clientName);
        Task SendClientDeletedEmail(string to, string clientName);
        Task SendEmailAsync(string to, string subject, string body);
    }
}