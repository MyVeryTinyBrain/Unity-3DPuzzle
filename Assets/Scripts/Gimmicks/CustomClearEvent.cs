using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomClearEvent : MonoBehaviour
{
    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    Transform cameraTarget;

    [SerializeField]
    float cameraTranslateDuration = 5;

    private IEnumerator HearEventCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        playerController.cameraTransformer.SimpleTranslate(cameraTranslateDuration, cameraTarget);

        yield return new WaitForSeconds(Mathf.Max(0, cameraTranslateDuration + 3));
        playerController.mainUI.ShowHeartAndMent(3, 2);
    }

    private IEnumerator SimpleEventCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        playerController.mainUI.ShowSimpleClear(3);
    }

    public void StartHearEvent()
    {
        playerController.SetControlMethod(new NothingMethod());
        StartCoroutine(HearEventCoroutine(1.0f));
    }

    public void StartSimpleEvent()
    {
        playerController.SetControlMethod(new NothingMethod());
        StartCoroutine(SimpleEventCoroutine(1.0f));
    }
}
