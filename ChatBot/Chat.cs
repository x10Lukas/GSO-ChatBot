using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot;
using Aufgabe_GSOChatBot.Daten;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe_GSOChatBot.Model
{
    internal class GSO_ChatBot_Chat:GSO_ChatBot_App
    {
        private GSOChatBotContext dbContext = new GSOChatBotContext();
        private bool exitChat;
        private Chat chat_active = new Chat();

        internal GSO_ChatBot_Chat()
        {
            chat_active = null;
        }

        public void ChatStart()
        {
            exitChat = false;

            while (!exitChat)
            {
                Console.Clear();
                NachrichtSchreiben();
            }
        }

        public async void NachrichtSchreiben()
        {
            Console.WriteLine("Nachricht schreiben\n");

            List<string> conversation = new List<string>();

            if (chat_active == null)
            {
                ChatErstellen();
            }

            do
            {
                Console.Write("Your message: ");
                string userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                {
                    exitChat = true;
                    break;
                }
                conversation.Add($"\n{userInput}\n");

                try
                {
                    Console.WriteLine("ChatGPT: Generating response...");

                    List<string> userMessages = new List<string> { userInput };
                    string gptResponse = await GenerateGPT3Response(userMessages);

                    conversation.Add($"ChatGPT\n{gptResponse}\n");

                    await NachrichtSpeichern("You", userInput);
                    await NachrichtSpeichern("ChatGPT", gptResponse);

                    foreach (var message in conversation)
                    {
                        Console.WriteLine(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                }

            } while (!exitChat);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadKey();
            AppStart();
        }

        private string GenerateRandomString(Random random, string chars, int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task ChatErstellen()
        {
            var newChat = new Chat
            {
                UserId = aktueller_user.Id,
                Name = GenerateRandomString(new Random(), "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", 8),
                Charakter = "UserCharacter"
            };

            try
            {
                dbContext.Chats.Add(newChat);

                await dbContext.SaveChangesAsync();
                Console.WriteLine($"chat_active nach Speichern: {newChat}");
                chat_active = newChat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private async Task NachrichtSpeichern(string sender, string message)
        {
            try
            {
                var newMessage = new Nachricht
                {
                    Content = message,
                    Gesendet = DateTime.Now,
                    Sender = sender,
                    Chat = chat_active
                };

                Console.WriteLine($"chat_active vor Speichern der Nachricht: {chat_active}");
                chat_active.Nachricht.Add(newMessage);
                dbContext.Nachrichten.Add(newMessage);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Nachricht: {ex.Message}");
            }
        }



        private async Task<string> GenerateGPT3Response(List<string> userMessages)
        {
            //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
            string openaiApiKey = aktueller_user.Token;
            string openaiEndpoint = "https://api.openai.com/v1/chat/completions";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openaiApiKey}");

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
                    messages
                };

                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(openaiEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var json = JsonDocument.Parse(responseBody);

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

        static string GetFirstNChars(string input, int n)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return input.Length >= n ? input.Substring(0, n) : input;
            }
            else
            {
                return input;
            }
        }
    }
}
