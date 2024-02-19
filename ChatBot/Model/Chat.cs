using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot;
using Aufgabe_GSOChatBot.Daten;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe_GSOChatBot.Model
{
    internal class GSO_ChatBot_Chat
    {
        private GSOChatBotContext dbContext = new GSOChatBotContext();
        private bool exitChat;

        public void ChatStart()
        {
            exitChat = false;

            while (!exitChat)
            {
                NachrichtSchreiben();
            }
        }

        public async void NachrichtSchreiben()
        {
            Console.Clear();
            Console.WriteLine("Nachricht schreiben\n");

            List<string> conversation = new List<string>();

            do
            {
                string userInput = GetUserInput();

                if (userInput?.ToLower() == "exit")
                {
                    exitChat = true;
                    break;
                }

                Console.Clear();
                conversation.Add($"You\n{userInput}\n");

                try
                {
                    Console.WriteLine("ChatGPT: Generating response...");

                    List<string> userMessages = new List<string> { userInput };
                    string gptResponse = await GenerateGPT3Response(userMessages);

                    conversation.Add($"ChatGPT\n{gptResponse}\n");

                    SaveMessageToDatabase("You", userInput);
                    SaveMessageToDatabase("ChatGPT", gptResponse);

                    DisplayConversation(conversation);
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
            Console.ReadLine();
        }
        private void SaveMessageToDatabase(string sender, string message)
        {
            int chatId = dbContext.Chats
                .Where(c => c.Name == "Test")
                .Select(c => c.Id)
                .FirstOrDefault();

            if (chatId == 0)
            {
                var newUser = new User
                {
                    Username = "GSO-Test",
                    Passwort = "1234",
                    Token = "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"
                };

                var newChat = new Chat
                {
                    Name = GenerateUniqueName(),
                    User = newUser,
                    Charakter = "UserCharacter"
                };

                dbContext.Chats.Add(newChat);
                dbContext.SaveChanges();

                chatId = newChat.Id;
            }

            var newMessage = new Nachricht
            {
                Content = message,
                Sender = sender,
                Gesendet = DateTime.Now,
                ChatId = chatId,
                ParenId = 0
            };

            dbContext.Nachrichten.Add(newMessage);
            dbContext.SaveChanges();

            int messageId = newMessage.Id;

            if (messageId != 0)
            {
                var responseMessage = dbContext.Nachrichten
                    .Where(m => m.Content == message && m.ChatId == chatId && m.ParenId == 0)
                    .FirstOrDefault();

                if (responseMessage != null)
                {
                    responseMessage.ParenId = messageId;
                    dbContext.SaveChanges();
                }
            }
        }
        private string GenerateUniqueName()
        {
            // Hier kannst du eine Logik implementieren, um einen eindeutigen Namen zu generieren
            // Zum Beispiel könntest du einen Basisnamen und eine eindeutige Nummer kombinieren
            return "GeneratedName" + Guid.NewGuid().ToString("N");
        }
        private string GetUserInput()
        {
            Console.Write("Your message: ");
            return Console.ReadLine();
        }

        private void DisplayConversation(List<string> conversation)
        {
            foreach (var message in conversation)
            {
                Console.WriteLine(message);
            }
        }

        private async Task<string> GenerateGPT3Response(List<string> userMessages)
        {
            string openaiApiKey = "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";
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
