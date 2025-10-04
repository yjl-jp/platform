using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FruitType { Apple,Banana,Cherry,Kiwi,Melon,Orange,Pineapple,Strawberry}

public class Fruit : MonoBehaviour
{
    [SerializeField] private FruitType fruitType;
    [SerializeField] private GameObject pickupVfx;

    private GameManager gameManager;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        SetRandomLookIfNeeded();
    }

    private void SetRandomLookIfNeeded()
    {
        if (gameManager.FruitsHaveRandomLook() == false)
        {
            UpdateFruitVisuals();
            return;
        }


        int randomIndex = Random.Range(0, 8); // max value is exclusive, so it will give number from 0 to 7.
        anim.SetFloat("fruitIndex", randomIndex);
    }
    private void UpdateFruitVisuals() => anim.SetFloat("fruitIndex", (int)fruitType);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            gameManager.AddFruit();
            Destroy(gameObject);

            GameObject newFx = Instantiate(pickupVfx, transform.position, Quaternion.identity);
        }
    }
}
