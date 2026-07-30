using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly ExportService _exportService;

        public ExportController(IClientRepository clientRepository, ExportService exportService)
        {
            _clientRepository = clientRepository;
            _exportService = exportService;
        }

        // ============================================================
        // ЭКСПОРТ СПИСКА КЛИЕНТОВ
        // ============================================================
        public async Task<IActionResult> Excel(string search = null, string status = null)
        {
            var clients = await _clientRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                clients = clients.Where(c =>
                    c.Name.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Phone.Contains(search) ||
                    c.Company.Contains(search)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                clients = clients.Where(c => c.Status == status).ToList();
            }

            var bytes = _exportService.ExportClientsList(clients, "xlsx");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Clients_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }

        public async Task<IActionResult> Csv(string search = null, string status = null)
        {
            var clients = await _clientRepository.GetAllAsync();
            // аналогичная фильтрация
            var bytes = _exportService.ExportClientsList(clients, "csv");
            return File(bytes, "text/csv", 
                $"Clients_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        public async Task<IActionResult> Html(string search = null, string status = null)
        {
            var clients = await _clientRepository.GetAllAsync();
            var bytes = _exportService.ExportClientsList(clients, "html");
            return File(bytes, "text/html", 
                $"Clients_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html");
        }

        // ============================================================
        // ЭКСПОРТ КАРТОЧКИ КЛИЕНТА
        // ============================================================
        public async Task<IActionResult> Pdf(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return NotFound();

            var bytes = _exportService.ExportClientToPdf(client);
            return File(bytes, "application/pdf", 
                $"Клиент_{client.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
        }

        public async Task<IActionResult> Txt(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return NotFound();

            var bytes = _exportService.ExportClientToTxt(client);
            return File(bytes, "text/plain", 
                $"Клиент_{client.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt");
        }

        public async Task<IActionResult> Docx(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return NotFound();

            var bytes = _exportService.ExportClientToDocx(client);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 
                $"Клиент_{client.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.docx");
        }
    }
}