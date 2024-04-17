using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot;
using Microsoft.EntityFrameworkCore;
using Aufgabe_GSOChatBot.Daten;

namespace Aufgabe_GSOChatBot.Model
{
    internal class GSO_ChatBot_Chat:GSO_ChatBot_App
    {
        private GSOChatBotContext dbContext = new GSOChatBotContext();
        private bool exitChat;
        private Chat chat_active = new Chat();

        internal GSO_ChatBot_Chat(Chat aktiv)
        {
            if(aktiv != null)
            {
                chat_active = aktiv;
            }
            else
            {
                chat_active = null;
            }
        }

        public async void ChatStart()
        {
            exitChat = false;

            if (chat_active == null)
            {
                await ChatSpeichern();
            }

            while (!exitChat)
            {
                NachrichtSchreiben();
            }
        }

        public async void NachrichtSchreiben()
        {
            List<string> conversation = new List<string>();

            if (chat_active == null)
            {
                ChatSpeichern();
            }

            do
            {
                Console.WriteLine("");
                string userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                {
                    exitChat = true;
                    break;
                }

                try
                {
                    List<string> userMessages = new List<string> { userInput };
                    string gptResponse = await GenerateGPT3Response(userMessages);

                    conversation.Add($"You\n{userInput}\n");
                    conversation.Add($"ChatGPT\n{gptResponse}");

                    //await NachrichtSpeichern("You", userInput);
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

            exitChat = true;
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
                    ChatId = chat_active.Id,
                };

                dbContext.Nachrichten.Add(newMessage);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Nachricht: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Innere Ausnahme: {ex.InnerException.Message}");
                }
            }
        }

        private async Task ChatSpeichern()
        {
            var newChat = new Chat
            {
                UserId = aktueller_user.Id,
                Name = GenerateRandomString(new Random(), "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", 8),
                Nachricht = new List<Nachricht>(),
                Charakter = "UserCharacter"
            };

            try
            {
                dbContext.Chats.Add(newChat);

                await dbContext.SaveChangesAsync();
                chat_active = newChat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        private string GenerateRandomString(Random random, string chars, int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
    }
}
