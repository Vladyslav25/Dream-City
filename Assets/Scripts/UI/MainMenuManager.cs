using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnClickStart() { SceneManager.LoadScene(1, LoadSceneMode.Single); }

    public void OnClickQuit() { Application.Quit(); }
}
