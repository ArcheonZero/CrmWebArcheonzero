using CrmWebArcheonzero.Interfaces;
using CrmWebArcheonzero.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ClientNotesController : Controller
    {

        private readonly INoteRepository _noteRepository;

        public ClientNotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        // ============================================================
        // СПИСОК ЗАМЕТОК ДЛЯ КЛИЕНТА
        // ============================================================
        public async Task<IActionResult> Index(int clientId)
        {
            var client = await _noteRepository.GetByIdAsync(clientId);
            if (client == null)
                return NotFound();

            var notes = await _noteRepository.GetByClientAsync(clientId);
            ViewBag.ClientName = client.Client?.Name;
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

            await _noteRepository.AddAsync(note);
            return RedirectToAction(nameof(Index), new { clientId });
        }

        // ============================================================
        // РЕДАКТИРОВАНИЕ ЗАМЕТКИ
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
                return NotFound();

            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
                return NotFound();

            note.Content = content;
            await _noteRepository.UpdateAsync(note);

            return RedirectToAction(nameof(Index), new { clientId = note.ClientId });
        }

        // ============================================================
        // УДАЛЕНИЕ ЗАМЕТКИ
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id, int clientId)
        {
            await _noteRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index), new { clientId });
        }
    }
}