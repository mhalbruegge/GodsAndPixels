using TMPro;
using UnityEngine;

public class HighScore : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TextMeshProUGUI>().text = GodManager.instance.highscore.ToString();
    }
}
