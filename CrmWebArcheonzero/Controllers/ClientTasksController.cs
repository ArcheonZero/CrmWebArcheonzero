using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ClientTasksController : Controller
    {
        private readonly IClientRepository _clientRepository;

        public ClientTasksController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        // ============================================================
        // СПИСОК ЗАДАЧ ДЛЯ КЛИЕНТА
        // ============================================================
        public async Task<IActionResult> Index(int clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
                return NotFound();

            var tasks = await _clientRepository.GetTasksByClientAsync(clientId);
            ViewBag.ClientName = client.Name;
            ViewBag.ClientId = clientId;
            return View(tasks);
        }

        // ============================================================
        // СОЗДАНИЕ ЗАДАЧИ
        // ============================================================
        public IActionResult Create(int clientId)
        {
            ViewBag.ClientId = clientId;
            return View(new ClientTask { ClientId = clientId, DueDate = DateTime.UtcNow.AddDays(7) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientTask task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedAt = DateTime.UtcNow;
                task.IsCompleted = false;
                await _clientRepository.AddTaskAsync(task);
                return RedirectToAction(nameof(Index), new { clientId = task.ClientId });
            }
            return View(task);
        }

        // ============================================================
        // РЕДАКТИРОВАНИЕ ЗАДАЧИ
        // ============================================================
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _clientRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientTask task)
        {
            if (id != task.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _clientRepository.UpdateTaskAsync(task);
                return RedirectToAction(nameof(Index), new { clientId = task.ClientId });
            }
            return View(task);
        }

        // ============================================================
        // ПЕРЕКЛЮЧЕНИЕ СТАТУСА (выполнена/не выполнена)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var task = await _clientRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            task.IsCompleted = !task.IsCompleted;
            await _clientRepository.UpdateTaskAsync(task);

            return RedirectToAction(nameof(Index), new { clientId = task.ClientId });
        }

        // ============================================================
        // УДАЛЕНИЕ ЗАДАЧИ
        // ============================================================
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _clientRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _clientRepository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            var clientId = task.ClientId;
            await _clientRepository.DeleteTaskAsync(id);
            return RedirectToAction(nameof(Index), new { clientId = clientId });
        }
    }
}