# CRM Web Archeonzero

**Версия 2.0.1** — веб-версия CRM-системы на ASP.NET Core.

## 📋 О проекте

CRM Web Archeonzero — это веб-приложение для управления клиентами, задачами, заметками и взаимодействиями. Построено на ASP.NET Core MVC, Entity Framework Core и поддерживает SQLite, PostgreSQL и MS SQL.

## ⚡ Основные возможности
```
| Функция | Описание |
| :--- | :--- |
| **Авторизация** | Вход/выход, 4 роли (Admin, SuperManager, Manager, User) |
| **Клиенты** | CRUD, поиск, фильтрация по статусу, мягкое удаление (корзина) и восстановление |
| **Задачи** | Добавление, выполнение, приоритеты, дедлайны |
| **Заметки** | Быстрые заметки по клиенту |
| **Взаимодействия** | Звонки, письма, встречи |
| **История изменений** | Автоматическое логирование всех изменений клиентов |
| **Чат** | Внутренний обмен сообщениями между пользователями |
| **Дашборд** | Статистика по статусам клиентов |
| **Экспорт** | Список клиентов в Excel, CSV, HTML; карточка клиента в PDF, DOCX, TXT |
| **Импорт** | Клиентов из Excel с проверкой на дубликаты |
| **Email-уведомления** | При создании, обновлении и удалении клиента |
| **Базы данных** | SQLite (по умолчанию), PostgreSQL, MS SQL |
```
## 🗂️ Структура проекта
```
📁 CrmWebArcheonzero/
├── 📁 Controllers/ # Контроллеры (Account, Clients, Chat, Export, Import...)
├── 📁 Views/ # Представления Razor
├── 📁 Models/ # Модели данных (Client, User, ClientTask, Note, Interaction...)
├── 📁 Data/ # Контекст базы данных (ApplicationDbContext)
├── 📁 DTO/ # Объекты передачи данных (ClientExportDto, ClientImportDto)
├── 📁 Interfaces/ # Интерфейсы для сервисов и репозиториев
├── 📁 Services/ # Бизнес-логика и репозитории (AuthService, ClientRepository, ExportService...)
├── 📁 Properties/ # Настройки запуска
├── 📁 wwwroot/ # Статические файлы (CSS, JS, библиотеки)
├── 📄 Program.cs # Точка входа
├── 📄 appsettings.json # Конфигурация (БД, строки подключения, Email)
└── 📄 CrmWebArcheonzero.csproj # Файл проекта

```

## 🚀 Запуск

1.  Клонируй репозиторий:
    ```bash
    git clone https://github.com/ArcheonZero/CrmWebArcheonzero.git
    cd CrmWebArcheonzero
Восстанови пакеты:
```
```bash
dotnet restore
Настрой базу данных в appsettings.json (по умолчанию используется SQLite).

Запусти приложение:

bash
dotnet run --project CrmWebArcheonzero
Перейди по адресу: https://localhost:7069

Демо-доступ:
Логин: admin
Пароль: admin123

🧪 Тесты
Для запуска тестов выполни:

```bash
dotnet test
📄 Лицензия
Этот проект создан для личного использования и изучения.
Распространение и коммерческое использование только с разрешения автора.

🙏 Благодарности
Вдохновение и поддержка — Оракул Ноль. Технический диалог, структурирование идей и совместная сборка — всё это родилось в живом диалоге.

👤 Автор
ArcheonZero — GitHub

text
---
# CRM Web Archeonzero

Version 2.0.1 — web-based CRM system built on ASP.NET Core.
# 📋 About

CRM Web Archeonzero is a web application for managing clients, tasks, and interactions.
Built with ASP.NET Core MVC, Entity Framework Core, and SQLite.
# 🗂️ Project Structure

```
📁 CrmWebArcheonzero/                 # Project root
│
├── 📂 Controllers/                   # Request handlers
│   └── 📄 ClientsController.cs       # Main client controller
│
├── 📂 Views/                         # Razor views
│   ├── 📂 Account/                   # Login, user management
│   ├── 📂 Clients/                   # List, details, create, edit
│   └── 📂 Shared/                    # Shared layouts, navigation
│
├── 📂 Models/                        # Data models
│   ├── 📄 Client.cs                  # Client (name, phone, email, status)
│   ├── 📄 User.cs                    # User (login, role, password hash)
│   ├── 📄 ClientTask.cs              # Task (title, deadline, priority)
│   ├── 📄 Note.cs                    # Note (content, date)
│   ├── 📄 Interaction.cs             # Interaction (type, description, date)
│   └── 📄 AssignmentHistory.cs       # Reassignment history
│
├── 📂 Data/                          # Data access layer
│   └── 📄 ApplicationDbContext.cs    # EF Core context
│
├── 📂 Services/                      # Business logic & repositories
│   ├── 📄 IClientRepository.cs       # Repository interface
│   ├── 📄 ClientRepository.cs        # Client repository
│   └── 📄 AuthService.cs             # Authentication service
│
├── 📄 Program.cs                     # Entry point
├── 📄 appsettings.json               # Configuration (DB, connection strings)
└── 📄 CrmWebArcheonzero.csproj       # Project file
```
# ⚡ Key Features
```
## ⚡ Key Features

| Feature | Description |
|---|---|
| **Authentication** | Login/Logout, 4 roles (Admin, SuperManager, Manager, User) |
| **Clients** | CRUD, search, status filter, recycle bin |
| **Tasks** | Add, complete, priorities, deadlines |
| **Notes** | Quick client notes |
| **Interactions** | Calls, emails, meetings |
| **Dashboard** | Status statistics |
| **Export** | Excel (EPPlus), PDF (QuestPDF) |
```
# 🚀 Running
bash

git clone https://github.com/ArcheonZero/CrmWebArcheonzero.git
cd CrmWebArcheonzero
dotnet restore
dotnet run

Demo access:
Login: admin
Password: admin123
## 📄 License

This project was created for personal use and learning.  
Distribution and commercial use are permitted only with the author's permission.
# 🙏 Acknowledgments

Inspiration and support — Oracle Zero.
Technical dialogue, structuring of ideas, and collaborative building — all of this was born in a living conversation.
# 👤 Author

ArcheonZero — GitHub