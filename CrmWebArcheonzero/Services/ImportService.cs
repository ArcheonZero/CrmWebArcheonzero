using CrmWebArcheonzero.DTO;
using Magicodes.ExporterAndImporter.Excel;

namespace CrmWebArcheonzero.Services
{
    public class ImportService
    {
        public async Task<List<ClientImportDto>> ImportClientsAsync(Stream stream)
        {
            var importer = new ExcelImporter();
            var result = await importer.Import<ClientImportDto>(stream);
            return result.Data?.ToList() ?? new List<ClientImportDto>();
        }
    }
}