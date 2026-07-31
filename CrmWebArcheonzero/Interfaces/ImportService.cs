using CrmWebArcheonzero.DTO;
using Magicodes.ExporterAndImporter.Excel;

namespace CrmWebArcheonzero.Interfaces
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