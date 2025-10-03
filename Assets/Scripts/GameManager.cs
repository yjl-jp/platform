using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPonit;
    [SerializeField] private float respawnDelay;
    public Player player;

    [Header("Fruits Management")]
    public bool fruitsHaveRandomLook;
    public int fruitsCollected;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); 
        }
            
    }
    public void UpdateRespawnPosition(Transform newPosition)=>respawnPonit = newPosition;

    public void RespawnPlayer()=>StartCoroutine(RespawnCourutine());
    private IEnumerator RespawnCourutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        GameObject newPlayer = Instantiate(playerPrefab, respawnPonit.position, Quaternion.identity);
        player = newPlayer.GetComponent<Player>();

    }
    public void AddFruit() => fruitsCollected++;
    public bool FruitsHaveRandomLook() => fruitsHaveRandomLook;


}
