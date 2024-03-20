using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Aufgabe_GSOChatBot.Daten;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure;
using Aufgabe_GSOChatBot.Model;

namespace Aufgabe_GSOChatBot
{
    internal class GSO_ChatBot_App
    {
        internal static User aktueller_user = new User();

        private GSOChatBotContext dbContext = new GSOChatBotContext();
        public void AppStart()
        {
            bool Exit = false;

            do
            {
                (int, int) cPosBM = Console.GetCursorPosition();
                Console.Clear();
                Console.WriteLine("CHAT BOT\n");
                Console.WriteLine("Eingabe: exit  ->  beendet das Programm\n");
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("1");
                Console.ResetColor();
                Console.Write("] Anmelden\n[");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("2");
                Console.ResetColor();
                Console.Write("] Registrieren");
                Console.Write("\n\nBitte wählen Sie eine Option: ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        UserEinloggen();
                        break;
                    case "2":
                        UserRegistrieren();
                        break;
                    case "exit":
                        Exit = true;
                        break;
                    default:
                        Console.WriteLine("Ungültige Eingabe");

                        (int, int) cPosAM = Console.GetCursorPosition();

                        ClearCurrentConsoleLine(cPosBM.Item2, cPosAM.Item2);
                        Console.ReadKey();
                        break;
                }
            } while (!Exit);
        }

        public async void UserEinloggen()
        {
            Console.Clear();
            Console.WriteLine("Anmeldung\n");

            string username;
            string passwort;

            do
            {
                (int, int) cPosBM = Console.GetCursorPosition();
                Console.Write("Username: ");
                username = Console.ReadLine();
                aktueller_user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (aktueller_user == null)
                {
                    Console.WriteLine("Ungültiger Benutzername. Bitte versuche es erneut.");
                    Console.ReadKey();
                    (int, int) cPosAM = Console.GetCursorPosition();

                    ClearCurrentConsoleLine(cPosBM.Item2, cPosAM.Item2);
                }
            } while (aktueller_user == null);

            do
            {
                (int, int) cPosBM = Console.GetCursorPosition();
                Console.Write("Passwort: ");
                passwort = Console.ReadLine();
                if (aktueller_user.Passwort != passwort)
                {
                    Console.WriteLine("Ungültiges Passwort. Bitte versuche es erneut.");
                    Console.ReadKey();
                    (int, int) cPosAM = Console.GetCursorPosition();

                    ClearCurrentConsoleLine(cPosBM.Item2, cPosAM.Item2);
                }
                NeuerChat();
            } while (aktueller_user.Passwort != passwort);
        }

        public async void UserRegistrieren()
        {
            Console.Clear();
            Console.WriteLine("Registrierung\n");
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Passwort: ");
            string passwort = Console.ReadLine();
            Console.Write("Token: ");
            string token = Console.ReadLine();

            User neuerUser = new User
            {
                Username = username,
                Passwort = passwort,
                Token = token
            };

            dbContext.Users.Add(neuerUser);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("\nSie wurden Erfolgreich Registriert!");
            Console.ReadKey();
            AppStart();
        }

        public async void NeuerChat()
        {
            Console.Clear();
            Console.WriteLine("Neuer Chat\n");
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("1");
            Console.ResetColor();
            Console.Write("] Chat erstellen\n[");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("2");
            Console.ResetColor();
            Console.Write("] Chat öffnen");
            Console.Write("\n\nBitte wählen Sie eine Option: ");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    GSO_ChatBot_Chat app = new GSO_ChatBot_Chat(null);
                    app.ChatStart();
                    break;
                case "2":
                    AlleNachrichtenAnzeigen();
                    break;
            }
        }

        public void AlleNachrichtenAnzeigen()
        {
            Console.Write("Geben Sie die Chat-ID ein: ");
            if (int.TryParse(Console.ReadLine(), out int chatId))
            {
                using (var db = new GSOChatBotContext())
                {
                    Chat aktiver_chat =  db.Chats.FirstOrDefault(k => k.Id ==  chatId);
                    var nachrichten = db.Nachrichten.Where(n => n.ChatId == chatId).ToList();

                    if (nachrichten.Any())
                    {
                        Console.Clear();
                        Console.WriteLine($"Alle Nachrichten im Chat {chatId}:");

                        foreach (var nachricht in nachrichten)
                        {
                            Console.WriteLine($"\n{nachricht.Sender}\n{nachricht.Content}");
                        }
                        Console.ReadKey();
                        GSO_ChatBot_Chat chatBot = new GSO_ChatBot_Chat(aktiver_chat);
                        chatBot.ChatStart();
                    }
                    else
                    {
                        Console.WriteLine($"Keine Nachrichten im Chat {chatId} vorhanden.");
                        Console.ReadKey();
                        NeuerChat();
                    }
                }
            }
            else
            {
                Console.WriteLine("Ungültige Eingabe. Bitte geben Sie eine gültige Chat-ID ein.");
                Console.ReadKey();
                NeuerChat();
            }
        }

        public void ClearCurrentConsoleLine(int from, int to)
        {

            for (int i = to; i >= from; i--)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(0, from);
        }

    }
}
