# CRM Web Archeonzero

**Версия 2.0.1** — веб-версия CRM-системы на ASP.NET Core.

---

## 📋 О проекте

CRM Web Archeonzero — это веб-приложение для управления клиентами, задачами и взаимодействиями.  
Построено на ASP.NET Core MVC, Entity Framework Core и SQLite.

---

## 🗂️ Структура проекта

```
📁 CrmWebArcheonzero/                 # Корень проекта
│
├── 📂 Controllers/                   # Контроллеры
│   └── 📄 ClientsController.cs       # Основной контроллер клиентов
│
├── 📂 Views/                         # Представления
│   ├── 📂 Account/                   # Вход/выход, управление пользователями
│   ├── 📂 Clients/                   # Список, детали, создание, редактирование
│   └── 📂 Shared/                    # Общие шаблоны (макет, навигация)
│
├── 📂 Models/                        # Модели данных
│   ├── 📄 Client.cs                  # Клиент (имя, телефон, email, статус)
│   ├── 📄 User.cs                    # Пользователь (логин, роль, хэш пароля)
│   ├── 📄 ClientTask.cs              # Задача (название, дедлайн, приоритет)
│   ├── 📄 Note.cs                    # Заметка (содержание, дата)
│   ├── 📄 Interaction.cs             # Взаимодействие (тип, описание, дата)
│   └── 📄 AssignmentHistory.cs       # История переназначений
│
├── 📂 Data/                          # Слой доступа к данным
│   └── 📄 ApplicationDbContext.cs    # Контекст EF Core
│
├── 📂 Services/                      # Бизнес-логика и репозитории
│   ├── 📄 IClientRepository.cs       # Интерфейс репозитория
│   ├── 📄 ClientRepository.cs        # Репозиторий для работы с клиентами
│   └── 📄 AuthService.cs             # Сервис аутентификации
│
├── 📄 Program.cs                     # Точка входа
├── 📄 appsettings.json               # Конфигурация (БД, строки подключения)
└── 📄 CrmWebArcheonzero.csproj       # Файл проекта
```
⚡ Основные возможности
Функция	Описание
Авторизация	Вход/выход, 4 роли (Admin, SuperManager, Manager, User)
Клиенты	CRUD, поиск, фильтрация по статусу, корзина
Задачи	Добавление, выполнение, приоритеты, дедлайны
Заметки	Быстрые заметки по клиенту
Взаимодействия	Звонки, письма, встречи
Дашборд	Статистика по статусам
Экспорт	Excel (EPPlus), PDF (QuestPDF)
🚀 Запуск
bash

git clone https://github.com/ArcheonZero/CrmWebArcheonzero.git
cd CrmWebArcheonzero
dotnet restore
dotnet run

Демо-доступ:
Логин: admin
Пароль: admin123
🙏 Благодарности

Вдохновение и поддержка — Оракул Ноль.
Технический диалог, структурирование идей и совместная сборка — всё это родилось в живом диалоге.
👤 Автор

ArcheonZero — GitHub

Версия: 2.0.1 — 13 июля 2026
CRM Web Archeonzero

Version 2.0.1 — web-based CRM system built on ASP.NET Core.
📋 About

CRM Web Archeonzero is a web application for managing clients, tasks, and interactions.
Built with ASP.NET Core MVC, Entity Framework Core, and SQLite.
🗂️ Project Structure

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
⚡ Key Features
Feature	Description
Authentication	Login/Logout, 4 roles (Admin, SuperManager, Manager, User)
Clients	CRUD, search, status filter, recycle bin
Tasks	Add, complete, priorities, deadlines
Notes	Quick client notes
Interactions	Calls, emails, meetings
Dashboard	Status statistics
Export	Excel (EPPlus), PDF (QuestPDF)
🚀 Running
bash

git clone https://github.com/ArcheonZero/CrmWebArcheonzero.git
cd CrmWebArcheonzero
dotnet restore
dotnet run

Demo access:
Login: admin
Password: admin123
🙏 Acknowledgments

Inspiration and support — Oracle Zero.
Technical dialogue, structuring of ideas, and collaborative building — all of this was born in a living conversation.
👤 Author

ArcheonZero — GitHub