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

            string passwort;

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

            } while (aktueller_user.Passwort != passwort);

            if (string.IsNullOrEmpty(aktueller_user.Token))
            {
                Console.WriteLine("\nBevor Sie einen neuen Chat erstellen können, müssen Sie zuerst ihren API-Token zu Ihrem Account hinzufügen");
                Console.WriteLine("Den API-Token bekommen Sie auf der Seite: https://platform.openai.com/api-keys");
                Console.WriteLine("\nDrücken Sie Enter um fortzufahren.");
                Console.ReadKey();
                GSO_ChatBot_User app = new GSO_ChatBot_User();

                app.TokenSpeichern();
                return;
            }
            else
            {
                Console.WriteLine("\nLogin erfolgreich!");
                GSO_ChatBot_User app = new GSO_ChatBot_User();

                app.NeuerChat();
            }
        }

        public async void UserRegistrieren()
        {
            Console.Clear();
            Console.WriteLine("Registrierung\n");
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Passwort: ");
            string passwort = Console.ReadLine();

            User neuerUser = new User
            {
                Username = username,
                Passwort = passwort
            };

            dbContext.Users.Add(neuerUser);
            await dbContext.SaveChangesAsync();



            AppStart();
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
