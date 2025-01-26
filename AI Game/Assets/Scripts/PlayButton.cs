using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    [SerializeField]
    Button[] disasterButtons;
    [SerializeField]
    Button playButton;
    [SerializeField]
    Color toggleColor = Color.blue;
    [SerializeField]
    TextMeshProUGUI maxRounds;
    [SerializeField]
    TextMeshProUGUI currentRound;

    Color defaultColor;
    private GameManager.DisasterType disasterType = GameManager.DisasterType.None;
    private string disasterPrompt = string.Empty;

    private void Start()
    {
        GameManager.instance.onSimulationStarted += SimStarted;
        GameManager.instance.onSimulationOver += SimOver;
        defaultColor = disasterButtons[0].colors.normalColor;
        Disaster(GameManager.DisasterType.None, "");

        foreach (var button in disasterButtons)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.selectedColor = toggleColor;
            button.colors = colorBlock;
        }

        maxRounds.text = GameManager.instance.maxGameDuration.ToString();
        currentRound.text = GameManager.instance.currentTimestep.ToString();
    }

    public void StartSimulation()
    {
        if (GameManager.instance.isSimulatingTimestep)
            return;

        if (disasterType != GameManager.DisasterType.None)
        {
            GameManager.instance.Disaster(disasterType, disasterPrompt);
        }

        _ = GameManager.instance.SimulateTimestep();

        currentRound.text = GameManager.instance.currentTimestep.ToString();
    }

    private void SimStarted()
    {
        playButton.interactable = false;
        foreach (var butt in disasterButtons)
        {
            butt.interactable = false;
        }
    }

    private void SimOver()
    {
        playButton.interactable = true;
        foreach (var butt in disasterButtons)
        {
            butt.interactable = true;
        }

        Disaster(GameManager.DisasterType.None, "");
    }

    public void Disaster(GameManager.DisasterType type, string prompt)
    {
        disasterType = type;
        disasterPrompt = prompt;

        foreach (Button button in disasterButtons)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = button.name.ToLower() == type.ToString().ToLower() ? toggleColor : defaultColor;
            button.colors = colorBlock;
        }
    }
}
