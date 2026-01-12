using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public bool isOdd;
    private GameManager gameManager;
    private Image image;
    private Coroutine fadetile;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(GameManager manager, bool odd)
    {
        gameManager = manager;
        isOdd = odd;

        transform.rotation = Quaternion.identity;

        
        Color c = image.color;
        c.a = 1f;
        image.color = c;
    }

    public void StartFade(float fadeDuration)
    {
        if (isOdd) return; 

        if (fadetile != null)
            StopCoroutine(fadetile);

        fadetile = StartCoroutine(FadeOut(fadeDuration));
    }

    IEnumerator FadeOut(float duration)
    {
        float time = 0f;
        Color startColor = image.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            image.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    public void OnTileClicked()
    {
        gameManager.OnTileClicked(this);
    }
}
