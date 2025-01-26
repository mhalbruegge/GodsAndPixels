using UnityEngine;
using UnityEngine.SceneManagement;

public class GodManager : MonoBehaviour
{
    public static GodManager instance;
    public int highscore = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
