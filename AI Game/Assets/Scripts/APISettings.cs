using UnityEngine;

public class APISettings : MonoBehaviour
{
    // This field will appear in the Inspector. 
    // Be cautious: The token is visible to anyone with project access or the built game.
    [SerializeField]
    private string openAIApiToken;

    // Provide a public property if you want to access it from other scripts
    public string OpenAIApiToken
    {
        get => openAIApiToken;
        set
        {
            openAIApiToken = value;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);

    }
}
