using Magicodes.ExporterAndImporter.Core;
using System;

namespace CrmWebArcheonzero.DTO
{
    public class ClientExportDto
    {
        private string? _phone;

        [ExporterHeader(DisplayName = "ID")]
        public int Id { get; set; }

        [ExporterHeader(DisplayName = "Имя")]
        public string? Name { get; set; }

        [ExporterHeader(DisplayName = "Телефон")]
        public string? PhoneFormatted
        {
            get => string.IsNullOrEmpty(_phone) ? string.Empty : $"'{_phone}";
            set => _phone = value;
        }

        [ExporterHeader(DisplayName = "Email")]
        public string? Email { get; set; }

        [ExporterHeader(DisplayName = "Компания")]
        public string? Company { get; set; }

        [ExporterHeader(DisplayName = "Должность")]
        public string? Position { get; set; }

        [ExporterHeader(DisplayName = "Статус")]
        public string? Status { get; set; }

        [ExporterHeader(DisplayName = "Источник")]
        public string? Source { get; set; }

        [ExporterHeader(DisplayName = "Дата рождения")]
        public DateTime? Birthday { get; set; }

        [ExporterHeader(DisplayName = "Дата создания")]
        public DateTime CreatedAt { get; set; }

        [ExporterHeader(DisplayName = "Последний контакт")]
        public DateTime? LastContact { get; set; }

        [ExporterHeader(DisplayName = "Теги")]
        public string? Tags { get; set; }
    }
}