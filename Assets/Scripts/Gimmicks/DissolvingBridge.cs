using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingBridge : ComponentEx
{
    [SerializeField]
    MeshRenderer bridgeMeshRenderer;

    [SerializeField]
    Collider wallCollider;

    private void Awake()
    {
        bridgeMeshRenderer.material.SetFloat("_Cutoff", 1);
        wallCollider.enabled = true;
    }

    private IEnumerator ShowCoroutine()
    {
        wallCollider.enabled = false;

        const float duration = 2;
        float accumulate = 0;
        while(accumulate < duration)
        {
            float ratio = accumulate / duration;
            bridgeMeshRenderer.material.SetFloat("_Cutoff", 1 - ratio);

            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }

        bridgeMeshRenderer.material.SetFloat("_Cutoff", 0);
    }

    public void ShowBridge()
    {
        StartCoroutine(ShowCoroutine());
    }
}
