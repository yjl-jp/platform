using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3 用

public class PlayerManager : MonoBehaviour
{
    public static event Action OnPlayerRespawn;
    public static PlayerManager instance;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;   // プレイヤーのプレハブ
    [SerializeField] private Transform respawnPoint;    // 復活地点
    [SerializeField] private float respawnDelay = 1f;   // 復活までの待ち時間（秒）
    public Player player;                               // 現在操作中のプレイヤー

    [Header("Camera")]
    [SerializeField] private CinemachineCamera vcam;    // 追従用 Cinemachine カメラ
    private Transform mainCamTr;                        // Main Camera の Transform

    private void Awake()
    {
        // シングルトン初期化
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 復活地点が未設定なら StartPoint から取得
        if (respawnPoint == null)
            respawnPoint = FindFirstObjectByType<StartPoint>().transform;

        // シーン内のプレイヤーを取得（最初の1体）
        if (player == null)
            player = FindFirstObjectByType<Player>();

        // Main Camera Transform をキャッシュ
        if (Camera.main != null)
            mainCamTr = Camera.main.transform;

        // vcam が未設定ならシーン内から自動取得（念のため）
        if (vcam == null)
            vcam = FindFirstObjectByType<CinemachineCamera>();
    }

    /// <summary>
    /// プレイヤー復活要求
    /// </summary>
    public void RespawnPlayer()
    {
        DifficultyManager difficultyManager = DifficultyManager.instance;

        // Hard 難易度なら復活しない
        if (difficultyManager != null && difficultyManager.difficulty == DifficultyType.Hard)
            return;

        StartCoroutine(RespawnCourutine());
    }

    /// <summary>
    /// 復活処理本体
    /// 一定時間待った後、新しいプレイヤーを生成し、
    /// カメラの追従ターゲットと位置を復活地点に合わせる。
    /// </summary>
    private IEnumerator RespawnCourutine()
    {
        // 復活ディレイ
        yield return new WaitForSeconds(respawnDelay);

        // すでに残っている古い Player を全て削除（念のため）
        foreach (var p in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            Destroy(p.gameObject);
        }

        // 新しいプレイヤーを復活地点に生成
        GameObject newPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        player = newPlayer.GetComponent<Player>();

        // ---------- カメラ設定ここから ----------

        // 1) 仮想カメラの Follow を新プレイヤーに差し替え
        if (vcam != null)
        {
            vcam.Follow = player.transform;
        }

        // 2) Main Camera の位置を復活地点に強制移動
        if (mainCamTr != null)
        {
            Vector3 camPos = mainCamTr.position;
            camPos.x = respawnPoint.position.x;
            camPos.y = respawnPoint.position.y;
            mainCamTr.position = camPos;
        }

        // 3) vcam 自身の位置も一応復活地点付近に寄せておく
        if (vcam != null)
        {
            Vector3 vcamPos = vcam.transform.position;
            vcamPos.x = respawnPoint.position.x;
            vcamPos.y = respawnPoint.position.y;
            vcam.transform.position = vcamPos;

            // Center On Activate を確実に発動させたい場合はコメントアウト解除
            // vcam.enabled = false;
            // yield return null;          // 1フレーム待つ
            // vcam.enabled = true;       // 有効化し直すとプレイヤー中心に寄る
        }

        // ---------- カメラ設定ここまで ----------

        // UI やその他の処理に通知
        OnPlayerRespawn?.Invoke();
    }

    /// <summary>
    /// チェックポイント通過時に復活地点を更新する
    /// </summary>
    public void UpdateRespawnPosition(Transform newRespawnPoint)
        => respawnPoint = newRespawnPoint;
}
