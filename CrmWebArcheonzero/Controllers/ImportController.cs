using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IClientRepository clientRepository, ILogger<ImportController> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
        }

        // ============================================================
        // СТРАНИЦА ИМПОРТА
        // ============================================================
        public IActionResult Index()
        {
            return View();
        }

        // ============================================================
        // ИМПОРТ ИЗ EXCEL
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Пожалуйста, выберите файл.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var importService = new ImportService();
                var importedClients = await importService.ImportClientsAsync(stream);

                int added = 0;
                int skipped = 0;

                foreach (var dto in importedClients)
                {
                    var existing = await _clientRepository.GetByPhoneOrEmailAsync(dto.CleanPhone, dto.Email);
                    if (existing != null)
                    {
                        _logger.LogWarning("Клиент с email {Email} или телефоном {Phone} уже существует (ID: {Id})",
                            dto.Email, dto.CleanPhone, existing.Id);
                        skipped++;
                        continue;
                    }

                    var client = new Client
                    {
                        Name = dto.Name,
                        Phone = dto.CleanPhone,
                        Email = dto.Email,
                        Company = dto.Company,
                        Position = dto.Position,
                        Status = dto.Status ?? "Lead",
                        Source = dto.Source,
                        Birthday = dto.Birthday,
                        Tags = dto.Tags,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _clientRepository.AddAsync(client, GetCurrentUserId());
                    added++;
                }

                TempData["Success"] = $"✅ Импортировано {added} клиентов. Пропущено дубликатов: {skipped}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при импорте клиентов");
                TempData["Error"] = $"❌ Ошибка при импорте: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ
        // ============================================================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }
    }
}