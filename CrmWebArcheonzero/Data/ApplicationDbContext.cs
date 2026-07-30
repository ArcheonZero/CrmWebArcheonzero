using Microsoft.EntityFrameworkCore;
using CrmWebArcheonzero.Models;

namespace CrmWebArcheonzero.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<ClientTask> Tasks { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AssignmentHistory> AssignmentHistories { get; set; }
        public DbSet<ClientTask> ClientTasks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === НАСТРОЙКА ДЛЯ USER ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue("User");
                entity.HasMany(u => u.AssignedClients)
                    .WithOne(c => c.AssignedUser)
                    .HasForeignKey(c => c.AssignedUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // === НАСТРОЙКА ДЛЯ CLIENT ===
            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(c => c.Status).HasDefaultValue("Lead");
                entity.HasIndex(c => c.Name).HasDatabaseName("IX_Clients_Name");
                entity.HasIndex(c => c.Email).HasDatabaseName("IX_Clients_Email");
                entity.HasIndex(c => c.Phone).HasDatabaseName("IX_Clients_Phone");
                entity.HasIndex(c => c.Status).HasDatabaseName("IX_Clients_Status");

                entity.HasMany(c => c.Interactions)
                    .WithOne(i => i.Client)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(c => c.Tasks)
                    .WithOne(t => t.Client)
                    .HasForeignKey(t => t.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(c => c.ClientNotes)
                    .WithOne(n => n.Client)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // === НАСТРОЙКА ДЛЯ CLIENT TASK ===
            modelBuilder.Entity<ClientTask>(entity =>
            {
                entity.HasIndex(t => t.DueDate).HasDatabaseName("IX_Tasks_DueDate");
                entity.HasIndex(t => t.IsCompleted).HasDatabaseName("IX_Tasks_IsCompleted");
            });

            // === НАСТРОЙКА ДЛЯ ASSIGNMENT HISTORY ===
            modelBuilder.Entity<AssignmentHistory>(entity =>
            {
                entity.HasOne(ah => ah.Client)
                    .WithMany()
                    .HasForeignKey(ah => ah.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ah => ah.FromUser)
                    .WithMany()
                    .HasForeignKey(ah => ah.FromUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ah => ah.ToUser)
                    .WithMany()
                    .HasForeignKey(ah => ah.ToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ah => ah.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(ah => ah.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // === НАСТРОЙКА ДЛЯ CHAT MESSAGE ===
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Message).IsRequired();
                entity.HasOne(m => m.User)
                    .WithMany()
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }


public void EnsureSeedData()
        {
            // === ПОЛЬЗОВАТЕЛИ ===
            if (!Users.Any(u => u.Username == "admin"))
            {
                Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Email = "admin@crm.com",
                    FullName = "Администратор",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (!Users.Any(u => u.Username == "admin5"))
            {
                Users.Add(new User
                {
                    Username = "admin5",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin1235"),
                    Email = "admin5@crm.com",
                    FullName = "Администратор",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (!Users.Any(u => u.Username == "manager"))
            {
                Users.Add(new User
                {
                    Username = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    Email = "manager@crm.com",
                    FullName = "Менеджер",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!Users.Any(u => u.Username == "super"))
            {
                Users.Add(new User
                {
                    Username = "super",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"),
                    Email = "super@crm.com",
                    FullName = "Super менеджер",
                    Role = "SuperManager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!Users.Any(u => u.Username == "user"))
            {
                Users.Add(new User
                {
                    Username = "user",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                    Email = "user@crm.com",
                    FullName = "Пользователь",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            SaveChanges();

            // === КЛИЕНТЫ (если нет ни одного) ===
            if (Clients.Any()) return;

            var clients = new List<Client>
    {
        new Client
        {
            Name = "Иван Петров",
            Phone = "+7 (912) 345-67-89",
            Email = "ivan@mail.ru",
            Status = "Active",
            Company = "ООО ТехноСервис",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Birthday = new DateTime(1985, 5, 15)
        },
        new Client
        {
            Name = "Мария Сидорова",
            Phone = "+7 (903) 222-33-44",
            Email = "maria@yandex.ru",
            Status = "Lead",
            Company = "ИП Сидорова",
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        },
        new Client
        {
            Name = "Алексей Иванов",
            Phone = "+7 (911) 555-66-77",
            Email = "alex@google.com",
            Status = "Inactive",
            Company = "ООО Альфа",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        }
    };

            Clients.AddRange(clients);
            SaveChanges();
        }
    }
    }

