using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aufgabe_GSOChatBot;

namespace Aufgabe_GSOChatBot.Model
{
    internal class GSO_ChatBot_Chat
    {
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
                string userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                {
                    exitChat = true;
                    break;
                }

                Console.Clear();
                conversation.Add($"You\n{userInput}\n");

                try
                {
                    List<string> userMessages = new List<string> { userInput };

                    string gptResponse = await GenerateGPT3Response(userMessages);

                    conversation.Add($"ChatGPT\n{gptResponse}\n");
                    DisplayConversation(conversation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            } while (!exitChat);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
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
