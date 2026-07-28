using Magicodes.ExporterAndImporter.Core;
using System.ComponentModel.DataAnnotations;

namespace CrmWebArcheonzero.DTO
{
    public class ClientImportDto
    {
        [ImporterHeader(Name = "Имя")]
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        public string? Name { get; set; }

        [ImporterHeader(Name = "Телефон")]
        public string? Phone { get; set; }

        public string? CleanPhone => CleanPhoneNumber(Phone);

        private string? CleanPhoneNumber(string? phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            // Убираем ВСЁ, кроме цифр и плюса в начале
            var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // Если номер начинается с 8 и длиной 11 цифр — заменяем на +7
            if (cleaned.StartsWith("8") && cleaned.Length == 11)
            {
                cleaned = "+7" + cleaned.Substring(1);
            }

            return cleaned;
        }

        [ImporterHeader(Name = "Email")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        public string? Email { get; set; }

        [ImporterHeader(Name = "Компания")]
        public string? Company { get; set; }

        [ImporterHeader(Name = "Должность")]
        public string? Position { get; set; }

        [ImporterHeader(Name = "Статус")]
        public string? Status { get; set; }

        [ImporterHeader(Name = "Источник")]
        public string? Source { get; set; }

        [ImporterHeader(Name = "Дата рождения")]
        public DateTime? Birthday { get; set; }

        [ImporterHeader(Name = "Теги")]
        public string? Tags { get; set; }
    }
}