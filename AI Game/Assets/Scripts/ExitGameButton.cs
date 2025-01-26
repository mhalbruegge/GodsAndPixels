using UnityEngine;
using UnityEngine.UI;

public class ExitGameButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
