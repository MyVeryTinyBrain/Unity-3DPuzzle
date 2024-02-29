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

    private IEnumerator EventCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        playerController.cameraTransformer.SimpleTranslate(cameraTranslateDuration, cameraTarget);

        yield return new WaitForSeconds(Mathf.Max(0, cameraTranslateDuration + 3));
        playerController.mainUI.ShowHeartAndMent(3, 2);
    }

    public void StartEvent()
    {
        playerController.SetControlMethod(new NothingMethod());
        StartCoroutine(EventCoroutine(1.0f));
    }
}
