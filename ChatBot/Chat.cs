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
    internal class GSO_ChatBot_Chat:GSO_ChatBot_App
    {
        private GSOChatBotContext dbContext = new GSOChatBotContext();
        private bool exitChat;
        
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
            string chatName = GenerateUniqueChatName();

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

                    NachrichtSpeichern("You", userInput, chatName);
                    NachrichtSpeichern("ChatGPT", gptResponse, chatName);

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

        private void NachrichtSpeichern(string sender, string message, string chatName)
        {
            var existingChat = dbContext.Chats
                .FirstOrDefault(c => c.Name == chatName && c.UserId == aktueller_user.Id);

            int chatId;

            if (existingChat == null)
            {
                Console.WriteLine($"Creating a new chat with name: {chatName}");

                var newChat = new Chat
                {
                    Name = chatName,
                    UserId = aktueller_user.Id,
                    Charakter = "UserCharacter"
                };

                dbContext.Chats.Add(newChat);
                dbContext.SaveChanges();

                chatId = newChat.Id;

                Console.WriteLine($"New chat created with name: {chatName}");
            }
            else
            {
                chatId = existingChat.Id;
            }
        }

        private string GenerateUniqueChatName()
        {
            return "Chat_" + Guid.NewGuid().ToString("N");
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
            // Check if the input string is not null
            if (!string.IsNullOrEmpty(input))
            {
                // Take the first n characters if available, otherwise take the entire string
                return input.Length >= n ? input.Substring(0, n) : input;
            }
            else
            {
                // Handle the case where the input string is null
                // You may choose to throw an exception or handle it differently based on your requirements
                return input;
            }
        }
    }
}
