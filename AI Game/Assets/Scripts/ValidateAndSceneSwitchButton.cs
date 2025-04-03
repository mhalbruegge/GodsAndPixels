using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using UnityEngine;
using UnityEngine.UI;

public class ValidateAndSceneSwitchButton : MonoBehaviour
{
    public string sceneName;

    public GameObject invalidKey;
    public GameObject invalidKey2;

    private async Task<bool> ValidateOpenAIToken()
    {
        string token = GodManager.instance.GetComponent<APISettings>().OpenAIApiToken;

        try
        {
            var api = new OpenAIClient(token);
            var messages = new List<Message>
            {
                new Message(Role.System, "You are a helpful assistant."),
                new Message(Role.User, "Are you there?"),
            };
            var chatRequest = new ChatRequest(messages, Model.GPT4oMini);
            var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
            var choice = response.FirstChoice;

            string answer = choice.Message;

            return !string.IsNullOrEmpty(answer);
        }
        catch (System.Exception)
        {

            return false;
        }
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(async () =>
        {
            invalidKey.SetActive(false);
            invalidKey2.SetActive(false);
            await Task.Yield();

            if (await ValidateOpenAIToken())
            {
                GodManager.instance.LoadScene(sceneName);
            }
            else
            {
                invalidKey.SetActive(true);
                invalidKey2.SetActive(true);
            }
        });
    }
}
