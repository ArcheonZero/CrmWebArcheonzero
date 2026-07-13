using CrmWebArcheonzero.Models;
using CrmWebArcheonzero.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Net.NetworkInformation;

namespace CrmWebArcheonzero.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        // GET: Clients
        // GET: Clients
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

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();

            // Загружаем задачи для этого клиента
            var tasks = await _clientRepository.GetTasksByClientAsync(id);
            ViewBag.Tasks = tasks;
            var notes = await _clientRepository.GetNotesByClientAsync(id);
            ViewBag.Notes = notes;
            var interactions = await _clientRepository.GetInteractionsByClientAsync(id);
            ViewBag.Interactions = interactions;

            return View(client);
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
        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                client.CreatedAt = DateTime.Now;
                await _clientRepository.AddAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _clientRepository.UpdateAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("User ID not found");
            return userId;
        }
        // GET: Clients/Delete/5
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();
            return View(client);
        }
        [Authorize(Roles = "Admin,SuperManager")]
        public async Task<IActionResult> Deleted()
        {
            var clients = await _clientRepository.GetDeletedAsync();
            return View(clients);
        }
        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperManager,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            await _clientRepository.SoftDeleteAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Restore/5
        [Authorize(Roles = "Admin,SuperManager")]
        public async Task<IActionResult> Restore(int id)
        {
            await _clientRepository.RestoreAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Dashboard()
        {
            var stats = await _clientRepository.GetStatisticsAsync();
            return View(stats);
        }
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction(nameof(Index));

            var clients = await _clientRepository.SearchAsync(query);
            return View("Index", clients);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask(int clientId, string title, DateTime dueDate, string priority)
        {
            var task = new ClientTask
            {
                ClientId = clientId,
                Title = title,
                Description = " ", // <-- Добавили пустое описание, чтобы не было NULL
                DueDate = dueDate,
                Priority = priority,
                CreatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now
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
        public async Task<IActionResult> ExportExcel(string search = null, string status = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Клиенты");

            // Заголовки
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Имя";
            worksheet.Cells[1, 3].Value = "Телефон";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "Компания";
            worksheet.Cells[1, 6].Value = "Статус";
            worksheet.Cells[1, 7].Value = "Дата создания";

            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                worksheet.Cells[i + 2, 1].Value = client.Id;
                worksheet.Cells[i + 2, 2].Value = client.Name;
                worksheet.Cells[i + 2, 3].Value = client.Phone;
                worksheet.Cells[i + 2, 4].Value = client.Email;
                worksheet.Cells[i + 2, 5].Value = client.Company;
                worksheet.Cells[i + 2, 6].Value = client.Status;
                worksheet.Cells[i + 2, 7].Value = client.CreatedAt.ToString("dd.MM.yyyy");
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Clients_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        public async Task<IActionResult> ExportPdf(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();

            var pdfBytes = PdfGenerator.GenerateClientCard(client);
            var fileName = $"Клиент_{client.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}