namespace CrmWebArcheonzero.Services
{
    public interface IDatabaseService
    {
        string GetProvider();
        string GetConnectionString();
        void SetProvider(string provider);
        void SetConnectionString(string connectionString);
    }
}