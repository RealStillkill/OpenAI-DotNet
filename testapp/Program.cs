using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System;

namespace testapp;

internal class Program
{
    public static async Task Main(string[] args)
    {
        OpenAIAuthentication auth = OpenAIAuthentication.LoadFromEnv();
        OpenAIClientSettings settings = OpenAIClientSettings.Default;

        OpenAIClient client = new OpenAIClient(auth, settings)
        {
            EnableDebug = true
        };

        var messages = new List<Message>
            {
                new(Role.System, "You are the AI powering an operating system called Nyarch. It is an operating system with the personality of an anime catgirl."),
                new(Role.User, "Nyarch, should I put a bunch of catgirl stickers on your case?")
            };
        ChatRequest chatRequest = new ChatRequest(messages, Model.GPT4o);
        ChatResponse response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);
        Console.WriteLine(response.ResetRequestsTimespan);
        Console.WriteLine(response.ResetTokensTimespan);
        Console.WriteLine(response.FirstChoice.Message.Content);
    }
}
