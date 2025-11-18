using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Spawning")]
    public GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;   // 在 Inspector 里拖 StartPoint 进来（推荐）

    [Header("UI")]
    public UI_JoinButton joingButton;
    public List<GameObject> objectsToDisable;

    // 事件（给摄像机等系统用）
    public static event Action OnPlayerRespawn;
    public static event Action OnPlayerDeath;

    // 单例
    public static PlayerManager instance;

    // 玩家 & 生命
    private List<Player> playerList = new List<Player>();
    public int lifePoints;
    public int maxPlayerCount = 1;
    public int playerCountWinCondition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // 关卡开始时调用一次（UI_InGame 那边已经在用）
    public void EnableJoinAndUpdateLifePoints()
    {
        playerCountWinCondition = maxPlayerCount;
        lifePoints = maxPlayerCount;

        if (joingButton == null)
            joingButton = FindFirstObjectByType<UI_JoinButton>();

        UI_InGame.instance.UpdateLifePointsUI(lifePoints, maxPlayerCount);
    }

    // UI_JoinButton 调用
    public void AddPlayer()
    {
        // 如果已经满员就不再生成
        if (playerList.Count >= maxPlayerCount)
            return;

        // 生成玩家
        GameObject newPlayerObj = Instantiate(playerPrefab);
        Player newPlayer = newPlayerObj.GetComponent<Player>();

        // 先放到出生点
        PlaceNewPlayerAtRespawnPoint(newPlayer.transform);

        // 加入列表（⚠️ 一定要在事件前做）
        playerList.Add(newPlayer);

        // 皮肤编号：0,1,2... 这样给
        int newPlayerSkinId = SkinManager.instance.GetSkinId(playerList.Count - 1);
        newPlayer.UpdateSkin(newPlayerSkinId);

        // 首次加入时，把「按任意键加入」之类的文字隐藏
        foreach (GameObject go in objectsToDisable)
        {
            if (go != null)
                go.SetActive(false);
        }

        // 如果人数满了，就把加入按钮关掉
        if (playerList.Count >= maxPlayerCount && joingButton != null)
            joingButton.gameObject.SetActive(false);

        // 通知其他系统（LevelCamera 会在这里刷新 Follow）
        OnPlayerRespawn?.Invoke();
    }

    // 玩家死亡时由 Player 调用
    public void RemovePlayer(Player player)
    {
        // 从列表中移除
        if (playerList.Contains(player))
            playerList.Remove(player);

        // 生命扣减逻辑（如果你暂时不想做多命，可以简化）
        if (CanRemoveLifePoints() && lifePoints > 0)
            lifePoints--;

        UI_InGame.instance.UpdateLifePointsUI(lifePoints, maxPlayerCount);

        // 允许再次加入
        if (joingButton != null)
            joingButton.gameObject.SetActive(true);

        // 相机、别的系统更新
        OnPlayerDeath?.Invoke();

        // 没命了就重开关卡
        if (lifePoints <= 0)
            GameManager.instance.RestartLevel();
    }

    private bool CanRemoveLifePoints()
    {
        if (DifficultyManager.instance.difficulty == DifficultyType.Hard)
            return true;

        if (GameManager.instance.fruitsCollected <= 0 &&
            DifficultyManager.instance.difficulty == DifficultyType.Normal)
            return true;

        return false;
    }

    public List<Player> GetPlayerList()
    {
        // 把已经 Destroy 的 Player 清掉
        playerList.RemoveAll(p => p == null);
        return playerList;
    }

    public void UpdateRespawnPosition(Transform newRespawnPoint) => respawnPoint = newRespawnPoint;

    private void PlaceNewPlayerAtRespawnPoint(Transform newPlayer)
    {
        if (respawnPoint == null)
        {
            // 如果 Inspector 没拖，就自动在场景找 StartPoint
            StartPoint startPoint = FindFirstObjectByType<StartPoint>();
            if (startPoint != null)
                respawnPoint = startPoint.transform;
        }

        if (respawnPoint != null)
            newPlayer.position = respawnPoint.position;
        else
            Debug.LogWarning("PlayerManager: 没找到 respawnPoint，玩家会在 (0,0) 生成。");
    }
}
