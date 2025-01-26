using TMPro;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public TMP_InputField openAIKey;

    public void Start()
    {
        //Adds a listener to the main input field and invokes a method when the value changes.
        openAIKey.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        string token = GodManager.instance.GetComponent<APISettings>().OpenAIApiToken;
        if (string.Empty != token)
        {
            openAIKey.SetTextWithoutNotify(token);
        }
    }

    // Invoked when the value of the text field changes.
    public void ValueChangeCheck()
    {
        string token = openAIKey.GetComponent<TMP_InputField>().text;
        GodManager.instance.GetComponent<APISettings>().OpenAIApiToken = token;
        PlayerPrefs.SetString("OpenAIApiToken", token);
    }
}
