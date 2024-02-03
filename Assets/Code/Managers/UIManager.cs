using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Dan.Demo;

public class UIManager : MonoBehaviour
{
    #region Cached Components
    [Header("Cached Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] PlayerController playerController;
    [SerializeField] AudioSource _audio;
    #endregion
    #region UI GameObjects
    [Header("UI GameObjects")]
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private GameObject ContinueUI;
    [SerializeField] private GameObject PauseUI;
    [SerializeField] private GameObject HealthUI;
    [SerializeField] private GameObject PersonalBestUI;
    [SerializeField] private GameObject LeaderBoardUI;
    [SerializeField] private GameObject ScoreUI;
    [SerializeField] private GameObject PerfectUI;
    #endregion
    #region TMP Text
    [Header("TMP Text")]
    [SerializeField] private TMP_Text POPUPTextScoreUI;
    [SerializeField] private TMPro.TMP_Text TextLifeUI;
    [SerializeField] private TMPro.TMP_Text TextContinueUI;
    [SerializeField] private TMPro.TMP_Text TextScoreUI;
    #endregion
    [SerializeField] private List<GameObject> LifeImages;
    [SerializeField] private TMP_InputField InputName;
    [SerializeField] private float scoreDeltaTime;
    [SerializeField] private List<AudioClip> PerfectTimingVariations;

    #region Unity Messages
    void OnEnable()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventsManager.Instance.OnScoreAdded += AddScoreToUI;
            GameEventsManager.Instance.OnPlayerDeath += UpdateLifeUI;
            GameEventsManager.Instance.OnGameOver += DisplayGameOverUI;
            GameEventsManager.Instance.OnPlayerRespawned += Respawn;
        }
    }
    void OnDisable()
    {
        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventsManager.Instance.OnScoreAdded -= AddScoreToUI;
            GameEventsManager.Instance.OnPlayerDeath -= UpdateLifeUI;
            GameEventsManager.Instance.OnGameOver -= DisplayGameOverUI;
            GameEventsManager.Instance.OnPlayerRespawned -= Respawn;
        }
    }

    private void Start()
    {
        GameOverUI.SetActive(false);

        if (GameEventsManager.Instance)
        {
            GameEventsManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventsManager.Instance.OnScoreAdded += AddScoreToUI;
            GameEventsManager.Instance.OnPlayerDeath += UpdateLifeUI;
            GameEventsManager.Instance.OnGameOver += DisplayGameOverUI;
            GameEventsManager.Instance.OnPlayerRespawned += Respawn;
        }
    }
    #endregion
    #region Player UI Functions
    private void AddScoreToUI(int currentScore, int AdditionalScore, bool isObstical)
    {
        if (isObstical)
        {
            _animator.SetTrigger("Score");
            POPUPTextScoreUI.gameObject.SetActive(true);
            POPUPTextScoreUI.text = AdditionalScore.ToString();
        }
        bool perfectScore = false;
        if (AdditionalScore == 100)
        {
            perfectScore = true;
        }

        StartCoroutine(LerpScore(currentScore, AdditionalScore, perfectScore));
    }

    private IEnumerator LerpScore(int currentScore, int AdditionalScore, bool perfectScore)
    {
        if (perfectScore)
        {
            PerfectUI.SetActive(perfectScore);
            if (PerfectTimingVariations.Count > 0)
            {
                int index = Random.Range(0, PerfectTimingVariations.Count);

                if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlaySFX(PerfectTimingVariations[index]);
                }
            }
        }
        
        for (int i = 0; i < AdditionalScore; i++)
        {
            // TODO: add any logic we want here
            currentScore++;
            int n = (int)Mathf.Floor(Mathf.Log10(currentScore) + 1);
            int numberOfZeros = 6 - n;
            TextScoreUI.text = "";
            for (int x = 0; x < numberOfZeros; x++)
            {
                TextScoreUI.text += "0";
            }
            TextScoreUI.text += currentScore.ToString();

            yield return new WaitForSeconds(scoreDeltaTime);
        }
        _animator.ResetTrigger("Score");
        PerfectUI.SetActive(false);
    }

    private void UpdateLifeUI(int CurrentLiveCount)
    {
        _animator.ResetTrigger("Transition");

        if (CurrentLiveCount >= 0)
        {
            LifeImages[CurrentLiveCount].SetActive(false);
            TextLifeUI.text = "Lives : " + CurrentLiveCount;

            if (CurrentLiveCount != 0)
            {
                ContinueUI.SetActive(true);
                _animator.SetTrigger("Continue");
            }
        }
    }

    private void Respawn()
    {
        ContinueUI.SetActive(false);
        _animator.ResetTrigger("Continue");
        _animator.SetTrigger("Transition");
        TextContinueUI.rectTransform.localScale = Vector3.zero;
    }

    private void DisplayGameOverUI()
    {
        GameOverUI.SetActive(true);
        LeaderBoardUI.SetActive(true);
        ScoreUI.SetActive(false);

        LeaderboardShowcase showcase = LeaderBoardUI.GetComponentInChildren<LeaderboardShowcase>();
        if (showcase != null)
        {
            showcase.SetPlayerScore(ScoreManager.Instance.CurrentScore);
        }

        PersonalBestUI.SetActive(false);

        if (PlayerPrefs.HasKey("Score"))
        {
            int playerScore = PlayerPrefs.GetInt("Score");
            if (playerScore < ScoreManager.Instance.CurrentScore)
            {
                PersonalBestUI.SetActive(true);
                PlayerPrefs.SetInt("Score", ScoreManager.Instance.CurrentScore);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Score", ScoreManager.Instance.CurrentScore);
        }
    }
    #endregion
    #region UI Button Event Functions

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }

    public void ResetRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion
    private void OnGameStateChanged(GameState _newState)
    {
        switch (_newState)
        {
            case GameState.GAMEPLAY:
                HealthUI.SetActive(true);
                PauseUI.SetActive(false);
                break;
            case GameState.PAUSE:
                HealthUI.SetActive(false);
                PauseUI.SetActive(true);
                break;
            case GameState.GAMEOVER:
                HealthUI.SetActive(false);
                DisplayGameOverUI();
                break;
            case GameState.DEATH:
                PauseUI.SetActive(false);
                break;
            case GameState.RESPAWN:
                PauseUI.SetActive(false);
                Respawn();
                break;
            default:
                break;
        }
    }
}
