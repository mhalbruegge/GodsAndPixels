using UnityEngine;
using UnityEngine.UI;

public class SceneSwitchButton : MonoBehaviour
{
    public string sceneName;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GodManager.instance.LoadScene(sceneName);
        });
    }
}
