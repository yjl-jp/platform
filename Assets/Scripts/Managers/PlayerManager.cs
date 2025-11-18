using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static event Action OnPlayerRespawn;
    public static event Action OnPlayerDeath;

    private LevelSplitscreenSetup splitscreenSetup;

    public PlayerInputManager playerInputManager { get; private set; }
    public static PlayerManager instance;

    public List<GameObject> objectsToDisable;

    public int lifePoints;
    public int maxPlayerCount = 1;
    public int playerCountWinCondition;
    [Header("Player")]
    [SerializeField] private List<Player> playerList = new List<Player>();
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private string[] playerDevice;

    private void Awake()
    {
        
        playerInputManager = GetComponent<PlayerInputManager>();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
            return; 
        }
    }

    private void OnEnable()
    {
        if (playerInputManager == null)
            playerInputManager = GetComponent<PlayerInputManager>();

        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += AddPlayer;
            playerInputManager.onPlayerLeft += RemovePlayer;
        }
    }



    private void OnDisable()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= AddPlayer;
            playerInputManager.onPlayerLeft -= RemovePlayer;
        }
    }

    public void SetupMaxPlayersCount(int newPlayersCount)
    {
        maxPlayerCount = newPlayersCount;
        //playerInputManager.SetMaximumPlayerCount(maxPlayerCount);
        if (playerList.Count >= maxPlayerCount)
            playerInputManager.DisableJoining();
        else
            playerInputManager.EnableJoining();
    }

    public void EnableJoinAndUpdateLifePoints()
    {
        splitscreenSetup = FindFirstObjectByType<LevelSplitscreenSetup>();

        playerInputManager.EnableJoining();
        playerCountWinCondition = maxPlayerCount;
        lifePoints = maxPlayerCount;
        UI_InGame.instance.UpdateLifePointsUI(lifePoints, maxPlayerCount);
    }

    private void AddPlayer(PlayerInput newPlayer)
    {
        
        if (playerList.Count >= maxPlayerCount)
        {
            playerInputManager.DisableJoining();
            Destroy(newPlayer.gameObject);
            return;
        }

        Player playerScript = newPlayer.GetComponent<Player>();

        playerList.Add(playerScript);

        OnPlayerRespawn?.Invoke();
        PlaceNewPlayerAtRespawnPoint(newPlayer.transform);

        int newPlayerNumber = GetPlayerNumber(newPlayer);
        int newPlayerSkinId = SkinManager.instance.GetSkinId(newPlayerNumber);

        playerScript.UpdateSkin(newPlayerSkinId);

        foreach (GameObject gameObject in objectsToDisable)
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }

        if (playerInputManager.splitScreen == true)
        {
            newPlayer.camera = splitscreenSetup.mainCamera[newPlayerNumber];
            splitscreenSetup.cinemachineCamera[newPlayerNumber].Follow = newPlayer.transform;
        }
    }

    private void RemovePlayer(PlayerInput player)
    {
        Player playerScript = player.GetComponent<Player>();
        playerList.Remove(playerScript);


        if (CanRemoveLifePoints() && lifePoints > 0)
            lifePoints--;

        if (lifePoints <= 0)
        {
            playerCountWinCondition--;
            playerInputManager.DisableJoining();

            if (playerList.Count <= 0)
                GameManager.instance.RestartLevel();
        }

        //UI_InGame.instance.UpdateLifePointsUI(lifePoints, maxPlayerCount);
        if (UI_InGame.instance != null)
        {
            UI_InGame.instance.UpdateLifePointsUI(lifePoints, maxPlayerCount);
        }

        OnPlayerDeath?.Invoke();
    }

    private bool CanRemoveLifePoints()
    {
        if (DifficultyManager.instance.difficulty == DifficultyType.Hard)
        {
            return true;
        }


        if (GameManager.instance.fruitsCollected <= 0 && DifficultyManager.instance.difficulty == DifficultyType.Normal)
        {
            return true;
        }

        return false;
    }

    private int GetPlayerNumber(PlayerInput newPlayer)
    {
        int newPlayerNumber = 0;

        foreach (var device in newPlayer.devices)
        {
            for (int i = 0; i < playerDevice.Length; i++)
            {
                if (playerDevice[i] == "Empty")
                {
                    newPlayerNumber = i;
                    playerDevice[i] = device.name;
                    break;
                }
                else if (playerDevice[i] == device.name)
                {
                    newPlayerNumber = i;
                    break;
                }

            }
        }

        return newPlayerNumber;
    }

    public List<Player> GetPlayerList() => playerList;

    public void UpdateRespawnPosition(Transform newRespawnPoint) => respawnPoint = newRespawnPoint;
    private void PlaceNewPlayerAtRespawnPoint(Transform newPlayer)
    {
        if (newPlayer == null)
        {
            Debug.LogError("PlaceNewPlayerAtRespawnPoint: newPlayer is null");
            return;
        }

        if (respawnPoint == null)
        {
            var start = FindFirstObjectByType<StartPoint>();
            if (start != null)
            {
                respawnPoint = start.transform;
            }
            else
            {
                Debug.LogError("PlaceNewPlayerAtRespawnPoint: StartPoint not found in scene.");
                // 至少放在原点，避免继续 NRE
                newPlayer.position = Vector3.zero;
                return;
            }
        }

        newPlayer.position = respawnPoint.position;
    }
}
