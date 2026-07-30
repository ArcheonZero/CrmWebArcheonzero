using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ClientNotesController : Controller
    {
        private readonly IClientRepository _clientRepository;

        public ClientNotesController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        // ============================================================
        // СПИСОК ЗАМЕТОК ДЛЯ КЛИЕНТА
        // ============================================================
        public async Task<IActionResult> Index(int clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return NotFound();

            var notes = await _clientRepository.GetNotesByClientAsync(clientId);
            ViewBag.ClientName = client.Name;
            ViewBag.ClientId = clientId;
            return View(notes);
        }

        // ============================================================
        // СОЗДАНИЕ ЗАМЕТКИ
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create(int clientId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Текст заметки не может быть пустым.";
                return RedirectToAction(nameof(Index), new { clientId });
            }

            var note = new Note
            {
                ClientId = clientId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _clientRepository.AddNoteAsync(note);
            return RedirectToAction(nameof(Index), new { clientId });
        }

        // ============================================================
        // РЕДАКТИРОВАНИЕ ЗАМЕТКИ
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var note = await _clientRepository.GetNoteByIdAsync(id);
            if (note == null)
                return NotFound();

            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var note = await _clientRepository.GetNoteByIdAsync(id);
            if (note == null)
                return NotFound();

            note.Content = content;
            await _clientRepository.UpdateNoteAsync(note);

            return RedirectToAction(nameof(Index), new { clientId = note.ClientId });
        }

        // ============================================================
        // УДАЛЕНИЕ ЗАМЕТКИ
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id, int clientId)
        {
            await _clientRepository.DeleteNoteAsync(id);
            return RedirectToAction(nameof(Index), new { clientId });
        }
    }
}