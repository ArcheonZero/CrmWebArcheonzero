using CrmWebArcheonzero.DTO;
using CrmWebArcheonzero.Models;
using Magicodes.ExporterAndImporter.Csv;
using Magicodes.ExporterAndImporter.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using Xceed.Words.NET;

namespace CrmWebArcheonzero.Services
{
    public class ExportService
    {
        private readonly ILogger<ExportService> _logger;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ============================================================
        // МАССОВЫЙ ЭКСПОРТ (список клиентов)
        // ============================================================
        public byte[] ExportClientsList(List<Client> clients, string format)
        {
            var dto = clients.Select(c => new ClientExportDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneFormatted = c.Phone,
                Email = c.Email,
                Company = c.Company,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList();

            return format.ToLower() switch
            {
                "xlsx" => new ExcelExporter().ExportAsByteArray(dto).GetAwaiter().GetResult(),
                "csv" => new CsvExporter().ExportAsByteArray(dto).GetAwaiter().GetResult(),
                "html" => ExportToHtml(dto),
                _ => throw new NotSupportedException($"Формат {format} не поддерживается")
            };
        }

        private byte[] ExportToHtml(List<ClientExportDto> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
            sb.AppendLine("<h1>Клиенты</h1>");
            sb.AppendLine("<table border='1' cellpadding='5'>");
            sb.AppendLine("<tr><th>ID</th><th>Имя</th><th>Телефон</th><th>Email</th><th>Компания</th><th>Статус</th></tr>");

            foreach (var client in data)
            {
                sb.AppendLine($"<tr><td>{client.Id}</td><td>{client.Name}</td><td>{client.PhoneFormatted}</td><td>{client.Email}</td><td>{client.Company}</td><td>{client.Status}</td></tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // ============================================================
        // ЭКСПОРТ КАРТОЧКИ КЛИЕНТА
        // ============================================================

        // TXT
        public byte[] ExportClientToTxt(Client client)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Карточка клиента: {client.Name}");
            sb.AppendLine($"ID: {client.Id}");
            sb.AppendLine($"Имя: {client.Name}");
            sb.AppendLine($"Телефон: {client.Phone}");
            sb.AppendLine($"Email: {client.Email}");
            sb.AppendLine($"Компания: {client.Company}");
            sb.AppendLine($"Должность: {client.Position}");
            sb.AppendLine($"Статус: {client.Status}");
            sb.AppendLine($"Источник: {client.Source}");
            sb.AppendLine($"Теги: {client.Tags}");
            sb.AppendLine($"Дата создания: {client.CreatedAt:dd.MM.yyyy}");
            sb.AppendLine($"Дата рождения: {client.Birthday?.ToString("dd.MM.yyyy")}");
            sb.AppendLine($"Последний контакт: {client.LastContact?.ToString("dd.MM.yyyy")}");
            sb.AppendLine($"Ответственный: {client.AssignedUser?.FullName}");
            sb.AppendLine($"Адрес: {client.Address}");
            sb.AppendLine($"Примечания: {client.Notes}");

            if (client.Interactions != null && client.Interactions.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Взаимодействия:");
                foreach (var i in client.Interactions.OrderByDescending(i => i.Date))
                {
                    sb.AppendLine($"  {i.Date:dd.MM.yyyy HH:mm} — {i.Type}: {i.Description}");
                }
            }

            if (client.Tasks != null && client.Tasks.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Задачи:");
                foreach (var t in client.Tasks.OrderBy(t => t.DueDate))
                {
                    var status = t.IsCompleted ? "[Выполнена]" : "[В работе]";
                    sb.AppendLine($"  {t.Title} (до {t.DueDate:dd.MM.yyyy}) — {status}");
                }
            }

            if (client.ClientNotes != null && client.ClientNotes.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Заметки:");
                foreach (var n in client.ClientNotes.OrderByDescending(n => n.CreatedAt))
                {
                    sb.AppendLine($"  {n.CreatedAt:dd.MM.yyyy HH:mm} — {n.Content}");
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // DOCX
        public byte[] ExportClientToDocx(Client client)
        {
            using var stream = new MemoryStream();
            using var doc = DocX.Create(stream);

            var title = doc.InsertParagraph($"Карточка клиента: {client.Name}")
                .FontSize(18)
                .Bold()
                .Color(System.Drawing.Color.FromArgb(42, 59, 140));

            doc.InsertParagraph($"ID: {client.Id}");
            doc.InsertParagraph($"Имя: {client.Name}");
            doc.InsertParagraph($"Телефон: {client.Phone}");
            doc.InsertParagraph($"Email: {client.Email}");
            doc.InsertParagraph($"Компания: {client.Company}");
            doc.InsertParagraph($"Должность: {client.Position}");
            doc.InsertParagraph($"Статус: {client.Status}");
            doc.InsertParagraph($"Источник: {client.Source}");
            doc.InsertParagraph($"Теги: {client.Tags}");
            doc.InsertParagraph($"Дата создания: {client.CreatedAt:dd.MM.yyyy}");
            doc.InsertParagraph($"Дата рождения: {client.Birthday?.ToString("dd.MM.yyyy")}");
            doc.InsertParagraph($"Последний контакт: {client.LastContact?.ToString("dd.MM.yyyy")}");
            doc.InsertParagraph($"Ответственный: {client.AssignedUser?.FullName}");
            doc.InsertParagraph($"Адрес: {client.Address}");
            doc.InsertParagraph($"Примечания: {client.Notes}");

            if (client.Interactions != null && client.Interactions.Any())
            {
                doc.InsertParagraph("Взаимодействия").Bold().FontSize(14);
                foreach (var i in client.Interactions.OrderByDescending(i => i.Date))
                {
                    doc.InsertParagraph($"- {i.Date:dd.MM.yyyy HH:mm} — {i.Type}: {i.Description}");
                }
            }

            if (client.Tasks != null && client.Tasks.Any())
            {
                doc.InsertParagraph("Задачи").Bold().FontSize(14);
                foreach (var t in client.Tasks.OrderBy(t => t.DueDate))
                {
                    var status = t.IsCompleted ? "✅ Выполнена" : "⏳ В работе";
                    doc.InsertParagraph($"- {t.Title} (до {t.DueDate:dd.MM.yyyy}) — {status}");
                }
            }

            if (client.ClientNotes != null && client.ClientNotes.Any())
            {
                doc.InsertParagraph("Заметки").Bold().FontSize(14);
                foreach (var n in client.ClientNotes.OrderByDescending(n => n.CreatedAt))
                {
                    doc.InsertParagraph($"- {n.CreatedAt:dd.MM.yyyy HH:mm} — {n.Content}");
                }
            }

            doc.Save();
            return stream.ToArray();
        }

        // PDF
        public byte[] ExportClientToPdf(Client client)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x
                        .FontFamily("Times New Roman")
                        .Fallback(fallback => fallback
                            .FontFamily("Segoe UI Emoji")
                            .FontFamily("Segoe UI Symbol")
                        )
                        .FontSize(12)
                    );

                    page.Header()
                        .Text($"Карточка клиента: {client.Name}")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(10);

                            // Основная информация
                            x.Item().Text("Основная информация").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                });

                                AddRow(table, "Имя:", client.Name);
                                AddRow(table, "Телефон:", client.Phone);
                                AddRow(table, "Email:", client.Email);
                                AddRow(table, "Компания:", client.Company);
                                AddRow(table, "Статус:", client.Status);
                                AddRow(table, "Источник:", client.Source);
                                AddRow(table, "Теги:", client.Tags);
                                AddRow(table, "Дата рождения:", client.Birthday?.ToString("dd.MM.yyyy"));
                                AddRow(table, "Заметки:", client.Notes);
                                AddRow(table, "Ответственный:", client.AssignedUser?.FullName);
                            });

                            // Задачи
                            if (client.Tasks?.Any() == true)
                            {
                                x.Item().Text("Задачи").FontSize(16).Bold();
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Название").Bold();
                                        header.Cell().Text("Срок").Bold();
                                        header.Cell().Text("Статус").Bold();
                                    });

                                    foreach (var task in client.Tasks)
                                    {
                                        table.Cell().Text(task.Title);
                                        table.Cell().Text(task.DueDate.ToString("dd.MM.yyyy"));
                                        table.Cell().Text(task.IsCompleted ? "✅ Выполнена" : "⏳ Активна");
                                    }
                                });
                            }

                            // Взаимодействия
                            if (client.Interactions?.Any() == true)
                            {
                                x.Item().Text("Взаимодействия").FontSize(16).Bold();
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Дата").Bold();
                                        header.Cell().Text("Тип").Bold();
                                        header.Cell().Text("Описание").Bold();
                                    });

                                    foreach (var interaction in client.Interactions)
                                    {
                                        table.Cell().Text(interaction.Date.ToString("dd.MM.yyyy HH:mm"));
                                        table.Cell().Text(interaction.Type);
                                        table.Cell().Text(interaction.Description);
                                    }
                                });
                            }

                            // Заметки
                            if (client.ClientNotes?.Any() == true)
                            {
                                x.Item().Text("Заметки").FontSize(16).Bold();
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Заметка").Bold();
                                        header.Cell().Text("Дата").Bold();
                                    });

                                    foreach (var note in client.ClientNotes)
                                    {
                                        table.Cell().Text(note.Content);
                                        table.Cell().Text(note.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Сгенерировано: {DateTime.UtcNow:dd.MM.yyyy HH:mm} | CRM Archeonzero");
                });
            });

            return document.GeneratePdf();
        }

        private void AddRow(TableDescriptor table, string label, string? value)
        {
            table.Cell().Text(label).Bold();
            table.Cell().Text(value ?? "-");
        }
    }
}