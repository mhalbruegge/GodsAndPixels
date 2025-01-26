using UnityEngine;

public class StartMenu : MonoBehaviour
{
    private static bool isInitialized = false;

    void Start()
    {
        if (isInitialized) return;

        if (PlayerPrefs.HasKey("OpenAIApiToken"))
        {
            string openAIApiToken = PlayerPrefs.GetString("OpenAIApiToken");
            if (string.Empty == openAIApiToken)
            {
                GodManager.instance.LoadScene("SettingsMenu");
            }
            else
            {
                GodManager.instance.GetComponent<APISettings>().OpenAIApiToken = openAIApiToken;
            }
        }
        else
        {
            GodManager.instance.LoadScene("SettingsMenu");
        }

        isInitialized = true;
    }
}
