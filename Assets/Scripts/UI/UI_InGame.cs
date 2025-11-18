using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_InGame : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;
    public static event Action OnJumpPressed;

    private PlayerInput playerInput;
    private List<Player> playerList;
    private Player player;
    public static UI_InGame instance;
    public UI_FadeEffect fadeEffect { get; private set; } // read-only

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI fruitText;
    [SerializeField] private TextMeshProUGUI lifePointsText;

    [SerializeField] private GameObject pauseUI;
    private bool isPaused;

    private void Awake()
    {
        instance = this;

        fadeEffect = GetComponentInChildren<UI_FadeEffect>();
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.UI.Pause.performed += ctx => PauseButton();
        playerInput.UI.Navigate.performed += ctx => UpdateSelected();
    }

    private void OnDisable()
    {
        playerInput.Disable();
        playerInput.UI.Pause.performed -= ctx => PauseButton();
        playerInput.UI.Navigate.performed -= ctx => UpdateSelected();
    }

    private void Start()
    {
        fadeEffect.ScreenFade(0, 1);
        GameObject pressJoinText = FindFirstObjectByType<UI_JoinButton>().gameObject;
        PlayerManager.instance.objectsToDisable.Add(pressJoinText);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            PauseButton();
    }

    private void UpdateSelected()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void PauseButton()
    {
        playerList = PlayerManager.instance.GetPlayerList();

        if (isPaused)
            UnpauseTheGame();
        else
            PauseTheGame();
    }

    public void JumpButton()
    {
        OnJumpPressed?.Invoke();
    }

    public void SetNewPlayer(Player newPlayer) => player = newPlayer;

    private void PauseTheGame()
    {
        //foreach (var player in playerList)
        //{
        //    player.playerInput.Disable();
        //}


        EventSystem.current.SetSelectedGameObject(firstSelected);
        isPaused = true;
        Time.timeScale = 0;
        pauseUI.SetActive(true);
    }

    private void UnpauseTheGame()
    {
        //foreach (var player in playerList)
        //{
        //    player.playerInput.Enable();
        //}

        isPaused = false;
        Time.timeScale = 1;
        pauseUI.SetActive(false);
    }

    public void GoToMainMenuButton()
    {
        SceneManager.LoadScene(0);
    }

    public void UpdateFruitUI(int collectedFruits, int totalFruits)
    {
        fruitText.text = collectedFruits + "/" + totalFruits;
    }

    public void UpdateTimerUI(float timer)
    {
        timerText.text = timer.ToString("00") + " s";
    }

    public void UpdateLifePointsUI(int lifePoints, int maxLifePoints)
    {
        if (DifficultyManager.instance.difficulty == DifficultyType.Easy)
        {
            lifePointsText.transform.parent.gameObject.SetActive(false);
            return;
        }

        lifePointsText.text = lifePoints + "/" + maxLifePoints;
    }
}
