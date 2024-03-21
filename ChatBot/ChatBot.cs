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
            bool exit = false;

            do
            {
                string option;
                do
                {
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
                    option = Console.ReadLine();

                    if (!IsValidOption(option))
                    {
                        Console.WriteLine("Ungültige Eingabe. Bitte erneut eingeben.");
                        Console.ReadKey();
                    }
                    else
                    {
                        switch (option)
                        {
                            case "1":
                                UserEinloggen();
                                break;
                            case "2":
                                UserRegistrieren();
                                break;
                            case "exit":
                                exit = true;
                                break;
                        }
                    }
                } while (!IsValidOption(option));

            } while (!exit);
        }

        public async void UserEinloggen()
        {
            Console.Clear();
            Console.WriteLine("Anmeldung\n");

            string username;
            string passwort = "";

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
                ConsoleKeyInfo key;

                while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
                {
                    if (key.Key == ConsoleKey.Backspace && passwort.Length > 0)
                    {
                        passwort = passwort[0..^1];
                        Console.Write("\b \b");
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        passwort += key.KeyChar;
                        Console.Write("*");
                    }
                }

                if (aktueller_user.Passwort != passwort)
                {
                    Console.WriteLine("\nUngültiges Passwort. Bitte versuche es erneut.");
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
            Console.Write("API-Token: ");
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

        public void NeuerChat()
        {
            string option;
            do
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
                option = Console.ReadLine();

                if (!IsValidOption(option))
                {
                    Console.WriteLine("Ungültige Eingabe. Bitte erneut eingeben.");
                    Console.ReadKey();
                }
                else
                {
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
            } while (option != "2");
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

        private bool IsValidOption(string input)
        {
            return input == "1" || input == "2" || input.ToLower() == "exit";
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
