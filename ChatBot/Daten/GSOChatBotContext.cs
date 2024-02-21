using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe_GSOChatBot.Daten
{
    internal class GSOChatBotContext: DbContext
    {
        // DbSet-Eigenschaften repräsentieren Tabellen in der Datenbank
        // Jede Eigenschaft ist stark typisiert mit einer Modellklasse
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Nachricht> Nachrichten { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;

        // OnModelCreating-Methode wird verwendet, um das Modell und die Beziehungen mittels Fluent API zu konfigurieren
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definieren von Primärschlüsseln für jede Entität
            modelBuilder.Entity<User>().HasKey(k => k.Id);
            modelBuilder.Entity<Nachricht>().HasKey(b => b.Id);
            modelBuilder.Entity<Chat>().HasKey(p => p.Id);

            // Immer die Basis-Methode aufrufen, um das Basisverhalten einzuschließen
            base.OnModelCreating(modelBuilder);
        }

        // OnConfiguring-Methode wird verwendet, um die Datenbankverbindung und andere Konfigurationen einzustellen
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Überprüfen, ob der optionsBuilder bereits konfiguriert ist, wenn nicht, konfigurieren
            if (!optionsBuilder.IsConfigured)
            {
                // Konfigurieren der Verwendung der SQLite-Datenbank mit dem angegebenen Verbindungsstring
                optionsBuilder.UseSqlite(@"Data Source=C:\Users\lukas\source\repos\GSO-ChatBot\ChatBot\gso_chatbot.db");
            }

            // Immer die Basis-Methode aufrufen, um das Basisverhalten einzuschließen
            base.OnConfiguring(optionsBuilder);
        }
    }
}
