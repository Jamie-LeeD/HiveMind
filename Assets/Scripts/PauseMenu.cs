using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Escape pauses the game. Resume, controls, retry, and exit live on the pause panel.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject controlsMenu;
    [SerializeField] Button resumeButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button closeControlsButton;
    [SerializeField] Button retryButton;
    [SerializeField] Button exitButton;
    [SerializeField] PossessionController possession;
    [SerializeField] GameplayInput input;

    bool paused;

    void Awake()
    {
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (controlsMenu != null)
            controlsMenu.SetActive(false);
    }

    void OnEnable()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);
        if (closeControlsButton != null)
            closeControlsButton.onClick.AddListener(CloseControls);
        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    void OnDisable()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);
        if (controlsButton != null)
            controlsButton.onClick.RemoveListener(OpenControls);
        if (closeControlsButton != null)
            closeControlsButton.onClick.RemoveListener(CloseControls);
        if (retryButton != null)
            retryButton.onClick.RemoveListener(Retry);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;
        if (possession != null && possession.IsGameOver)
            return;

        if (controlsMenu != null && controlsMenu.activeSelf)
        {
            CloseControls();
            return;
        }

        if (paused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
        if (controlsMenu != null)
            controlsMenu.SetActive(false);
        if (input != null)
            input.SetGameplayEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (controlsMenu != null)
            controlsMenu.SetActive(false);
        if (input != null)
            input.SetGameplayEnabled(true);

        if (possession == null || !possession.IsGameOver)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OpenControls()
    {
        if (controlsMenu != null)
            controlsMenu.SetActive(true);
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void CloseControls()
    {
        if (controlsMenu != null)
            controlsMenu.SetActive(false);
        if (paused && pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
