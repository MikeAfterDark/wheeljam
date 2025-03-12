using UnityEngine;
using UnityEngine.SceneManagement;

public class InitProject : MonoBehaviour
{
    public string[] initScenes = { "Settings", "Menu" };

    void Start()
    {
        foreach (string scene in initScenes)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        }
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
