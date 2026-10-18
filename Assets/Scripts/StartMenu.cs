using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Start screen: play, open the controls list, and close it again.
/// </summary>
public class StartMenu : MonoBehaviour
{
    [SerializeField] GameObject controlsMenu;
    [SerializeField] Button startButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button closeControlsButton;
    [SerializeField] Button exitButton;
    [SerializeField] string gameSceneName = "GameScene";

    void Awake()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (controlsMenu != null)
            controlsMenu.SetActive(false);
    }

    void OnEnable()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);
        if (closeControlsButton != null)
            closeControlsButton.onClick.AddListener(CloseControls);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    void OnDisable()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);
        if (controlsButton != null)
            controlsButton.onClick.RemoveListener(OpenControls);
        if (closeControlsButton != null)
            closeControlsButton.onClick.RemoveListener(CloseControls);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenControls()
    {
        if (controlsMenu != null)
            controlsMenu.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsMenu != null)
            controlsMenu.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
