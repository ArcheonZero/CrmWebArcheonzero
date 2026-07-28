using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ClientInteractionsController : Controller
    {
        private readonly IClientRepository _clientRepository;

        public ClientInteractionsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        // ============================================================
        // СПИСОК ВЗАИМОДЕЙСТВИЙ ДЛЯ КЛИЕНТА
        // ============================================================
        public async Task<IActionResult> Index(int clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return NotFound();

            var interactions = await _clientRepository.GetInteractionsByClientAsync(clientId);
            ViewBag.ClientName = client.Name;
            ViewBag.ClientId = clientId;
            return View(interactions);
        }

        // ============================================================
        // СОЗДАНИЕ ВЗАИМОДЕЙСТВИЯ
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create(int clientId, string type, string description, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                TempData["Error"] = "Описание взаимодействия не может быть пустым.";
                return RedirectToAction(nameof(Index), new { clientId });
            }

            var interaction = new Interaction
            {
                ClientId = clientId,
                Type = type,
                Description = description,
                Date = date
            };

            await _clientRepository.AddInteractionAsync(interaction);
            return RedirectToAction(nameof(Index), new { clientId });
        }

        // ============================================================
        // РЕДАКТИРОВАНИЕ ВЗАИМОДЕЙСТВИЯ
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var interaction = await _clientRepository.GetInteractionByIdAsync(id);
            if (interaction == null)
                return NotFound();

            return View(interaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string type, string description, DateTime date)
        {
            var interaction = await _clientRepository.GetInteractionByIdAsync(id);
            if (interaction == null)
                return NotFound();

            interaction.Type = type;
            interaction.Description = description;
            interaction.Date = date;
            await _clientRepository.UpdateInteractionAsync(interaction);

            return RedirectToAction(nameof(Index), new { clientId = interaction.ClientId });
        }

        // ============================================================
        // УДАЛЕНИЕ ВЗАИМОДЕЙСТВИЯ
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id, int clientId)
        {
            await _clientRepository.DeleteInteractionAsync(id);
            return RedirectToAction(nameof(Index), new { clientId });
        }
    }
}