using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Services
{
    public static class PdfGenerator
    {
        public static byte[] GenerateClientCard(Client client)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);

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
                                AddRow(table, "Дата создания:", client.CreatedAt.ToString("dd.MM.yyyy"));
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm} | CRM Arkheonzero");
                });
            });

            return document.GeneratePdf();
        }

        private static void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Text(label).Bold();
            table.Cell().Text(value ?? "-");
        }
    }
}