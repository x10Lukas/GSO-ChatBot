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

namespace Aufgabe_GSOChatBot
{
    internal class GSO_ChatBot_App
    {
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
                        break;
                        Console.ReadKey();
                }
            } while (!Exit);
        }

        public async void UserEinloggen()
        {
            Console.Clear();
            Console.WriteLine("Anmeldung\n");

            string username;
            User user;

            do
            {
                (int, int) cPosBM = Console.GetCursorPosition();
                Console.Write("Username: ");
                username = Console.ReadLine();
                user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    Console.WriteLine("Ungültiger Benutzername. Bitte versuche es erneut.");
                    Console.ReadKey();
                    (int, int) cPosAM = Console.GetCursorPosition();

                    ClearCurrentConsoleLine(cPosBM.Item2, cPosAM.Item2);
                }

            } while (user == null);

            string passwort;

            do
            {
                (int, int) cPosBM = Console.GetCursorPosition();
                Console.Write("Passwort: ");
                passwort = Console.ReadLine();
                if (user.Passwort != passwort)
                {
                    Console.WriteLine("Ungültiges Passwort. Bitte versuche es erneut.");
                    Console.ReadKey();
                    (int, int) cPosAM = Console.GetCursorPosition();

                    ClearCurrentConsoleLine(cPosBM.Item2, cPosAM.Item2);
                }

            } while (user.Passwort != passwort);

            Console.WriteLine("\nLogin successful!");
            Console.ReadKey();
            NachrichtSchreiben();
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

        public async void NachrichtSchreiben()
        {
            Console.Clear();
            Console.WriteLine("Nachricht schreiben\n");

            Console.Write("Ihre Nachricht: ");
            string userInput = Console.ReadLine();

            if (userInput != null)
            {
                try
                {
                    List<string> userMessages = new List<string> { userInput };

                    string gptResponse = await GenerateGPT3Response(userMessages);

                    Console.WriteLine("ChatBot: " + gptResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Error: User input cannot be null.");
            }

            Console.ReadKey();
        }

        private async Task<string> GenerateGPT3Response(List<string> userMessages)
        {
            string openaiApiKey = "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";
            string openaiEndpoint = "https://api.openai.com/v1/chat/completions";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openaiApiKey}");

                // Baue die Liste der Nachrichten auf
                var messages = new List<object>
                {
                    new { role = "system", content = "Du bist Coding-Assistent" }
                };

                foreach (var userMessage in userMessages)
                {
                    messages.Add(new { role = "user", content = userMessage });
                }

                var requestData = new
                {
                    model = "gpt-3.5-turbo",
                    messages = messages,
                    max_tokens = 150
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(openaiEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var json = System.Text.Json.JsonDocument.Parse(responseBody);

                        if (json.RootElement.TryGetProperty("choices", out var choices))
                        {
                            if (choices.ValueKind == JsonValueKind.Array && choices.EnumerateArray().Any())
                            {

                                if (choices[0].TryGetProperty("message", out var message))
                                {
                                    if (message.TryGetProperty("content", out var contentProperty))
                                    {
                                        return contentProperty.GetString();
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error: 'content' key not found in the message.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error: 'message' key not found in the first choice.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error: 'choices' array is empty or not an array.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: 'choices' key not found in the JSON response.");
                        }

                        return "Error generating response";
                    }
                    else
                    {
                        Console.WriteLine($"Error calling OpenAI API: {response.StatusCode} - {response.ReasonPhrase}");
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                        return "Error generating response";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return "Error generating response";
                }
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
