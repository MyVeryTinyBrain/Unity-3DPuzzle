using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : ComponentEx
{
    [SerializeField]
    public ItemCanvas itemCanvas;

    [SerializeField]
    public Image crosshair;

    [SerializeField]
    public Image heart;

    [SerializeField]
    public Image ment;

    public void SetCrosshair(bool active)
    {
        crosshair.gameObject.SetActive(active);
    }

    private IEnumerator ShowHeartAndMentCoroutine(float heartDuration, float mentDuration)
    {
        heart.gameObject.SetActive(true);
        ment.gameObject.SetActive(true);

        Color heartColor = heart.color;
        heartColor.a = 0;
        Color mentColor = ment.color;
        mentColor.a = 0;
        heart.color = heartColor;
        ment.color = mentColor;

        heart.color = heartColor;
        ment.color = mentColor;

        float accumulate = 0;
        while(accumulate < heartDuration)
        {
            float ratio = accumulate / heartDuration;
            heartColor.a = ratio;
            heart.color = heartColor;

            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }
        heartColor.a = 1;
        heart.color = heartColor;

        accumulate = 0;
        while (accumulate < mentDuration)
        {
            float ratio = accumulate / mentDuration;
            mentColor.a = ratio;
            ment.color = mentColor;

            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }
        mentColor.a = 1;
        ment.color = mentColor;
    }

    public void ShowHeartAndMent(float heartDuration, float mentDuration)
    {
        itemCanvas.gameObject.SetActive(false);
        crosshair.gameObject.SetActive(false);
        StartCoroutine(ShowHeartAndMentCoroutine(heartDuration, mentDuration));
    }
}
