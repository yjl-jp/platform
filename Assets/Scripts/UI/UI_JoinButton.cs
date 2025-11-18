using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_JoinButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private float cycleDuration;
    [SerializeField] private TextMeshProUGUI text;

    private Color transperentColor = new Color(1, 1, 1, .2f);
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        float halfCycle = cycleDuration / 2;

        while (true)
        {
            ToggleColor(Color.white);
            yield return new WaitForSeconds(halfCycle);

            ToggleColor(transperentColor);
            yield return new WaitForSeconds(halfCycle);
        }
    }

    private void ToggleColor(Color color)
    {
        text.color = color;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerManager.instance.AddPlayer();
    }
}
