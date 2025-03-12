using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ReplaceCurrentSceneWith(string sceneName)
    {
        Debug.Log("Unloading Scene " + gameObject.scene);
        SceneManager.UnloadSceneAsync(gameObject.scene);

        Debug.Log("Loading Scene " + sceneName);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}
