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
        private readonly ILogger<ClientsController> _logger;
        public ClientsController(IClientRepository clientRepository, ILogger<ClientsController> logger)
        {
            _clientRepository = clientRepository;
            _logger = logger;
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

            ViewBag.Tasks = await _clientRepository.GetTasksByClientAsync(id);
            ViewBag.Notes = await _clientRepository.GetNotesByClientAsync(id);
            ViewBag.Interactions = await _clientRepository.GetInteractionsByClientAsync(id);

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
                await _clientRepository.AddAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        // ============================================================
        // ИМПОРТ ИЗ EXCEL
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
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
                int updated = 0;

                foreach (var dto in importedClients)
                {
                    // Ищем клиента, у которого совпадают И телефон, И email
                    var existing = await _clientRepository.GetByPhoneAndEmailAsync(dto.CleanPhone, dto.Email);

                    if (existing != null)
                    {
                        // Полное совпадение — обновляем
                        existing.Name = dto.Name ?? existing.Name;
                        existing.Company = dto.Company ?? existing.Company;
                        existing.Position = dto.Position ?? existing.Position;
                        existing.Status = dto.Status ?? existing.Status;
                        existing.Source = dto.Source ?? existing.Source;
                        existing.Birthday = dto.Birthday ?? existing.Birthday;
                        existing.Tags = dto.Tags ?? existing.Tags;


                        await _clientRepository.UpdateAsync(existing, GetCurrentUserId());
                        updated++;
                        _logger.LogInformation("Обновлён клиент: ID={Id}, Name={Name}", existing.Id, existing.Name);
                        continue;
                    }

                    // Проверяем, нет ли клиента с таким телефоном (но другим email)
                    var existingByPhone = await _clientRepository.GetByPhoneAsync(dto.CleanPhone);
                    if (existingByPhone != null)
                    {
                        _logger.LogWarning("Найден клиент с таким же телефоном, но другим email: ID={Id}, Name={Name}, Phone={Phone}, Email={Email}",
                            existingByPhone.Id, existingByPhone.Name, existingByPhone.Phone, existingByPhone.Email);
                        // Можно добавить в список ошибок или пропустить
                        continue;
                    }

                    // Проверяем, нет ли клиента с таким email (но другим телефоном)
                    var existingByEmail = await _clientRepository.GetByEmailAsync(dto.Email);
                    if (existingByEmail != null)
                    {
                        _logger.LogWarning("Найден клиент с таким же email, но другим телефоном: ID={Id}, Name={Name}, Phone={Phone}, Email={Email}",
                            existingByEmail.Id, existingByEmail.Name, existingByEmail.Phone, existingByEmail.Email);
                        // Можно добавить в список ошибок или пропустить
                        continue;
                    }

                    // Новый клиент
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

                    await _clientRepository.AddAsync(client);
                    added++;
                }

                TempData["Success"] = $"✅ Импортировано: {added} новых, обновлено: {updated}.";
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
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,SuperManager")]
        public async Task<IActionResult> Restore(int id)
        {
            await _clientRepository.RestoreAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,SuperManager")]
        public async Task<IActionResult> Deleted()
        {
            var clients = await _clientRepository.GetDeletedAsync();
            return View(clients);
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
        // ВСПОМОГАТЕЛЬНЫЕ
        // ============================================================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }

        // ============================================================
        // ЗАДАЧИ, ЗАМЕТКИ, ВЗАИМОДЕЙСТВИЯ (будут вынесены позже)
        // ============================================================
        // Пока оставляем здесь, но в следующем шаге вынесем в отдельные контроллеры
        [HttpPost]
        public async Task<IActionResult> AddTask(int clientId, string title, DateTime dueDate, string priority)
        {
            var task = new ClientTask
            {
                ClientId = clientId,
                Title = title,
                Description = " ",
                DueDate = dueDate,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            await _clientRepository.AddTaskAsync(task);
            return RedirectToAction(nameof(Details), new { id = clientId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTask(int taskId, int clientId)
        {
            await _clientRepository.ToggleTaskCompletionAsync(taskId);
            return RedirectToAction(nameof(Details), new { id = clientId });
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(int clientId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction(nameof(Details), new { id = clientId });

            var note = new Note
            {
                ClientId = clientId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _clientRepository.AddNoteAsync(note);
            return RedirectToAction(nameof(Details), new { id = clientId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNote(int noteId, int clientId)
        {
            await _clientRepository.DeleteNoteAsync(noteId);
            return RedirectToAction(nameof(Details), new { id = clientId });
        }

        [HttpPost]
        public async Task<IActionResult> AddInteraction(int clientId, string type, string description, DateTime date)
        {
            var interaction = new Interaction
            {
                ClientId = clientId,
                Type = type,
                Description = description,
                Date = date
            };

            await _clientRepository.AddInteractionAsync(interaction);
            return RedirectToAction(nameof(Details), new { id = clientId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInteraction(int interactionId, int clientId)
        {
            await _clientRepository.DeleteInteractionAsync(interactionId);
            return RedirectToAction(nameof(Details), new { id = clientId });
        }
        // GET: Clients/EditNote/5
        public async Task<IActionResult> EditNote(int id)
        {
            var note = await _clientRepository.GetNoteByIdAsync(id);
            if (note == null) return NotFound();
            return PartialView("_EditNote", note);
        }

        [HttpPost]
        public async Task<IActionResult> EditNote(int id, string content)
        {
            var note = await _clientRepository.GetNoteByIdAsync(id);
            if (note == null) return NotFound();

            note.Content = content;
            await _clientRepository.UpdateNoteAsync(note);
            return RedirectToAction(nameof(Details), new { id = note.ClientId });
        }

        // GET: Clients/EditInteraction/5
        public async Task<IActionResult> EditInteraction(int id)
        {
            var interaction = await _clientRepository.GetInteractionByIdAsync(id);
            if (interaction == null) return NotFound();
            return PartialView("_EditInteraction", interaction);
        }

        [HttpPost]
        public async Task<IActionResult> EditInteraction(int id, string type, string description, DateTime date)
        {
            var interaction = await _clientRepository.GetInteractionByIdAsync(id);
            if (interaction == null) return NotFound();

            interaction.Type = type;
            interaction.Description = description;
            interaction.Date = date;
            await _clientRepository.UpdateInteractionAsync(interaction);
            return RedirectToAction(nameof(Details), new { id = interaction.ClientId });
        }

    }
}