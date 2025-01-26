using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using UnityEngine;

public class AICharacter : MonoBehaviour
{
    public string playerName;
    public string systemPrompt;

    private OpenAIClient api;
    private List<Message> messages;

    public async Task<string> Chat(string prompt)
    {
        messages.Add(new Message(Role.User, prompt));

        //var chatRequest = new ChatRequest(messages, model: Model.GPT3_5_Turbo, maxTokens: 50, temperature: 0.7f);
        var chatRequest = new ChatRequest(messages, model: Model.GPT4oMini, temperature: 0.7f);
        var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var choice = response.FirstChoice;
        string answer = choice.Message;

        messages.Add(new Message(Role.Assistant, answer));

        return answer;
    }

    private void Start()
    {
        string token = GodManager.instance.GetComponent<APISettings>().OpenAIApiToken;

        api = new OpenAIClient(token);

        messages = new List<Message>
        {
            new Message(Role.System, systemPrompt),
        };
    }
}
