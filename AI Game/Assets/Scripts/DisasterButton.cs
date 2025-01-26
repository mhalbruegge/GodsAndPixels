using UnityEngine;
using UnityEngine.UI;

public class DisasterButton : MonoBehaviour
{
    [SerializeField]
    GameManager.DisasterType type;
    [SerializeField]
    string prompt;

    [SerializeField]
    Button button;
    [SerializeField]
    PlayButton playButton;

    private void Awake()
    {
        button.onClick.AddListener(
            () =>
            {
                playButton.Disaster(type, prompt);
                playButton.StartSimulation();
            }
        );
    }
}
