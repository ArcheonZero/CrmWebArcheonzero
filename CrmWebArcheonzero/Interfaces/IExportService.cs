using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Interfaces
{
    public interface IExportService
    {
        byte[] ExportClientsList(List<Client> clients, string format);
        byte[] ExportClientToPdf(Client client);
        byte[] ExportClientToTxt(Client client);
        byte[] ExportClientToDocx(Client client);
    }
}