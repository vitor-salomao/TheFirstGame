using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Name of the scene that Play loads. It must be in the build's scene list.")]
    [SerializeField] private string gameScene = "Lobby";
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        playButton.onClick.AddListener(Play);
        quitButton.onClick.AddListener(Quit);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(Play);
        quitButton.onClick.RemoveListener(Quit);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Play()
    {
        SceneManager.LoadScene(gameScene);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
