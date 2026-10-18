using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Updates the existing mana and health bars, plus the possession prompts.
/// </summary>
public class GameHud : MonoBehaviour
{
    [SerializeField] Mana mana;
    [SerializeField] WitchLives lives;
    [SerializeField] PossessionController possession;
    [SerializeField] Slider manaBar;
    [SerializeField] Slider livesBar;
    [SerializeField] TMP_Text manaText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text controlText;
    [SerializeField] TMP_Text promptText;
    [SerializeField] TMP_Text depletedText;
    [SerializeField] GameObject losePanel;
    [SerializeField] Button retryButton;
    [SerializeField] Button exitButton;
    [SerializeField] float depletedMessageSeconds = 2.5f;

    float depletedTimer;

    void Awake()
    {
        if (manaBar != null)
            manaBar.interactable = false;
        if (livesBar != null)
            livesBar.interactable = false;

        if (losePanel != null)
            losePanel.SetActive(false);
        if (depletedText != null)
            depletedText.gameObject.SetActive(false);
        if (manaText != null)
            manaText.gameObject.SetActive(false);
        if (livesText != null)
            livesText.gameObject.SetActive(false);
        if (controlText != null)
            controlText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (mana != null)
        {
            mana.Changed += OnManaChanged;
            mana.Depleted += OnManaDepleted;
        }

        if (lives != null)
        {
            lives.Changed += OnLivesChanged;
            lives.GameOver += OnGameOver;
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    void OnDisable()
    {
        if (mana != null)
        {
            mana.Changed -= OnManaChanged;
            mana.Depleted -= OnManaDepleted;
        }

        if (lives != null)
        {
            lives.Changed -= OnLivesChanged;
            lives.GameOver -= OnGameOver;
        }

        if (retryButton != null)
            retryButton.onClick.RemoveListener(Retry);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);
    }

    void Start()
    {
        if (mana != null)
            OnManaChanged(mana.Current, mana.Max);
        if (lives != null)
            OnLivesChanged(lives.Current, lives.Max);
        else
            Debug.LogWarning("GameHud has no lives reference.", this);

        if (mana == null)
            Debug.LogWarning("GameHud has no mana reference.", this);
        if (possession == null)
            Debug.LogWarning("GameHud has no possession reference.", this);
    }

    void Update()
    {
        if (possession == null)
            return;

        if (promptText != null)
        {
            bool show = !possession.IsGameOver && !string.IsNullOrEmpty(possession.PromptText);
            promptText.gameObject.SetActive(show);
            if (show)
                promptText.text = possession.PromptText;
        }

        if (depletedTimer > 0f)
            depletedTimer -= Time.deltaTime;

        if (depletedText != null)
        {
            bool show = depletedTimer > 0f && (possession == null || !possession.IsGameOver);
            depletedText.gameObject.SetActive(show);
        }
    }

    void OnManaChanged(float current, float max)
    {
        if (manaBar != null)
            manaBar.value = max <= 0f ? 0f : current / max;
    }

    void OnLivesChanged(int current, int max)
    {
        if (livesBar != null)
            livesBar.value = max <= 0 ? 0f : (float)current / max;
    }

    void OnManaDepleted()
    {
        depletedTimer = depletedMessageSeconds;
        if (depletedText != null)
            depletedText.text = "Mana depleted";
    }

    void OnGameOver()
    {
        if (losePanel != null)
            losePanel.SetActive(true);
        if (promptText != null)
            promptText.gameObject.SetActive(false);
        if (depletedText != null)
            depletedText.gameObject.SetActive(false);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
