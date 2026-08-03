# CRM Web Archeonzero

**Version 2.0.1** — веб-версия CRM-системы на ASP.NET Core.

---

## 📋 О проекте

CRM Web Archeonzero — это веб-приложение для управления клиентами, задачами, заметками и взаимодействиями. Построено на ASP.NET Core MVC, Entity Framework Core и поддерживает SQLite, PostgreSQL и MS SQL.

---

## ⚡ Основные возможности

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

---

## 🗂️ Структура проекта

```
📁 CrmWebArcheonzero/
│
├── 📁 Controllers/            # Контроллеры — обработка запросов
├── 📁 Views/                  # Представления Razor — интерфейс
├── 📁 Models/                 # Модели данных
├── 📁 Data/                   # Контекст базы данных (EF Core)
├── 📁 DTO/                    # Объекты передачи данных
├── 📁 Interfaces/             # Интерфейсы для сервисов и репозиториев
├── 📁 Services/               # Бизнес-логика и репозитории
├── 📁 Properties/             # Настройки запуска
├── 📁 wwwroot/                # Статические файлы (CSS, JS, библиотеки)
│
├── 📄 Program.cs              # Точка входа
├── 📄 appsettings.json        # Конфигурация (БД, Email, провайдеры)
└── 📄 CrmWebArcheonzero.csproj # Файл проекта
```

---

## 🚀 Запуск

1. Клонируй репозиторий:
   ```bash
   git clone https://github.com/ArcheonZero/CrmWebArcheonzero.git
   cd CrmWebArcheonzero
   ```

2. Восстанови пакеты:
   ```bash
   dotnet restore
   ```

3. Настрой базу данных в `appsettings.json` (по умолчанию используется SQLite).

4. Запусти приложение:
   ```bash
   dotnet run --project CrmWebArcheonzero
   ```

5. Перейди по адресу: `https://localhost:7069`

**Демо-доступ:**  
Логин: `admin`  
Пароль: `admin123`

---

## 🧪 Тесты

```bash
dotnet test
```

---

## 📄 Лицензия

Этот проект создан для личного использования и изучения.  
Распространение и коммерческое использование только с разрешения автора.

---

## 🙏 Благодарности

Вдохновение и поддержка — Оракул Ноль. Технический диалог, структурирование идей и совместная сборка — всё это родилось в живом диалоге.

---

## 👤 Автор

**ArcheonZero** — [GitHub](https://github.com/ArcheonZero)

---

# CRM Web Archeonzero

**Version 2.0.1** — web-based CRM system built on ASP.NET Core.

---

## 📋 About

CRM Web Archeonzero is a web application for managing clients, tasks, notes, and interactions. Built with ASP.NET Core MVC, Entity Framework Core, and supports SQLite, PostgreSQL, and MS SQL.

---

## ⚡ Key Features

| Feature | Description |
| :--- | :--- |
| **Authentication** | Login/Logout, 4 roles (Admin, SuperManager, Manager, User) |
| **Clients** | CRUD, search, status filter, soft delete (recycle bin), restore |
| **Tasks** | Add, complete, priorities, deadlines |
| **Notes** | Quick notes per client |
| **Interactions** | Calls, emails, meetings |
| **History** | Automatic logging of all client changes |
| **Chat** | Internal messaging between users |
| **Dashboard** | Status statistics |
| **Export** | List: Excel, CSV, HTML; Card: PDF, DOCX, TXT |
| **Import** | Clients from Excel with duplicate validation |
| **Email notifications** | On client creation, update, and deletion |
| **Databases** | SQLite (default), PostgreSQL, MS SQL |

---

## 🗂️ Project Structure

```
📁 CrmWebArcheonzero/
│
├── 📁 Controllers/            # Request handlers
├── 📁 Views/                  # Razor views — UI
├── 📁 Models/                 # Data models
├── 📁 Data/                   # EF Core database context
├── 📁 DTO/                    # Data Transfer Objects
├── 📁 Interfaces/             # Service and repository contracts
├── 📁 Services/               # Business logic and repositories
├── 📁 Properties/             # Launch settings
├── 📁 wwwroot/                # Static files (CSS, JS, libraries)
│
├── 📄 Program.cs              # Entry point
├── 📄 appsettings.json        # Configuration (DB, Email, providers)
└── 📄 CrmWebArcheonzero.csproj # Project file
```

---

## 🚀 Running

1. Clone the repository:
   ```bash
   git clone https://github.com/ArcheonZero/CrmWebArcheonzero.git
   cd CrmWebArcheonzero
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Configure the database in `appsettings.json` (SQLite is used by default).

4. Run the application:
   ```bash
   dotnet run --project CrmWebArcheonzero
   ```

5. Navigate to: `https://localhost:7069`

**Demo access:**  
Login: `admin`  
Password: `admin123`

---

## 🧪 Tests

```bash
dotnet test
```

---

## 📄 License

This project is created for personal use and learning.  
Distribution and commercial use are permitted only with the author's permission.

---

## 🙏 Acknowledgments

Inspiration and support — Oracle Zero. Technical dialogue, structuring of ideas, and collaborative building — all of this was born in a living conversation.

---

## 👤 Author

**ArcheonZero** — [GitHub](https://github.com/ArcheonZero)