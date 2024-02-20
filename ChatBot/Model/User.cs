using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot;
using Aufgabe_GSOChatBot.Daten;
using Microsoft.EntityFrameworkCore;
using Aufgabe_GSOChatBot.Model;

namespace Aufgabe_GSOChatBot.Model
{
    internal class GSO_ChatBot_User
    {
        private GSOChatBotContext dbContext = new GSOChatBotContext();

        public async void UserStart()
        {
            TokenSpeichern();
        }
        public async void TokenSpeichern()
        {
            Console.Clear();
            Console.WriteLine("Token Speichern\n");
            Console.Write("Bitte geben Sie ihre ID ein: ");
            int ID = Convert.ToInt32(Console.ReadLine());
            var userTokenUpdate = await dbContext.Users.FindAsync(ID);

            if (userTokenUpdate != null)
            {
                Console.Write("Bitte geben Sie ihren API-Token ein: ");
                string API_Token = Console.ReadLine();

                userTokenUpdate.Token = API_Token;

                await dbContext.SaveChangesAsync();

                Console.WriteLine("\nAPI-Token erfolgreich hinzugefügt.");
                Console.ReadKey();
                NeuerChat();
            }
            else
            {
                Console.WriteLine($"Benutzer mit ID {ID} nicht gefunden.");
                Console.ReadKey();
                TokenSpeichern();
            }
        }

        public async void NeuerChat()
        {
            Console.Clear();
            Console.WriteLine("Neuer Chat\n");
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("1");
            Console.ResetColor();
            Console.Write("] Neuen Chat erstellen\n[");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("2");
            Console.ResetColor();
            Console.Write("] Vorhandenen Chat öffnen");
            Console.Write("\n\nBitte wählen Sie eine Option: ");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    GSO_ChatBot_Chat app = new GSO_ChatBot_Chat();

                    app.ChatStart();
                    break;
                case "2":
                    
                    break;
            }
        }
    }
}
