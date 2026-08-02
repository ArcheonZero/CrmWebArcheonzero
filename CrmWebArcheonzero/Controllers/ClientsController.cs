using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly ILogger<ClientsController> _logger;
        private readonly EmailService _emailService;

        public ClientsController(
            IClientRepository clientRepository,
            IHistoryRepository historyRepository,
            ILogger<ClientsController> logger,
            EmailService emailService)
        {
            _clientRepository = clientRepository;
            _historyRepository = historyRepository;
            _logger = logger;
            _emailService = emailService;
        }

        // ============================================================
        // СПИСОК + ПОИСК + ФИЛЬТР
        // ============================================================
        public async Task<IActionResult> Index(string search = null, string status = null)
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

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            return View(clients);
        }

        // ============================================================
        // ДЕТАЛИ
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();

            return View(client);
        }

        // ============================================================
        // СОЗДАНИЕ
        // ============================================================
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                client.CreatedAt = DateTime.UtcNow;
                await _clientRepository.AddAsync(client, GetCurrentUserId());

                // 📧 Email уведомление
                await _emailService.SendClientCreatedEmail(client.Email, client.Name);

                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // ============================================================
        // РЕДАКТИРОВАНИЕ
        // ============================================================
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                await _clientRepository.UpdateAsync(client, GetCurrentUserId());

                // 📧 Email уведомление
                await _emailService.SendClientUpdatedEmail(client.Email, client.Name);

                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // ============================================================
        // УДАЛЕНИЕ И КОРЗИНА
        // ============================================================
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            await _clientRepository.SoftDeleteAsync(id, userId);

            // 📧 Email уведомление
            var client = await _clientRepository.GetByIdAsync(id);
            if (client != null)
                await _emailService.SendClientDeletedEmail(client.Email, client.Name);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,SuperManager")]
        public async Task<IActionResult> Restore(int id)
        {
            var userId = GetCurrentUserId();
            await _clientRepository.RestoreAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,SuperManager")]
        public async Task<IActionResult> Deleted()
        {
            var clients = await _clientRepository.GetDeletedAsync();
            return View(clients);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            await _clientRepository.PermanentDeleteAsync(id);
            TempData["Success"] = "Клиент окончательно удалён.";
            return RedirectToAction(nameof(Deleted));
        }
        // ============================================================
        // ДАШБОРД
        // ============================================================
        public async Task<IActionResult> Dashboard()
        {
            var stats = await _clientRepository.GetStatisticsAsync();
            return View(stats);
        }

        // ============================================================
        // ИСТОРИЯ ИЗМЕНЕНИЙ
        // ============================================================
        public async Task<IActionResult> History(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();

            var history = await _historyRepository.GetByClientAsync(id);
            ViewBag.ClientName = client.Name;
            ViewBag.ClientId = id;
            return View(history);
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