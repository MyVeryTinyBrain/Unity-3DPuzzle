using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingBridge : ComponentEx
{
    [SerializeField]
    MeshRenderer bridgeMeshRenderer;

    [SerializeField]
    Collider wallCollider;

    [SerializeField, Range(0f, 1f)]
    float cutoff = 1f;

    float cachedCutoff = 1f;

    protected override void Awake()
    {
        bridgeMeshRenderer.material.SetFloat("_Cutoff", cutoff);
        cachedCutoff = cutoff;
        wallCollider.enabled = true;
    }

    protected override void Update()
    {
        if (cachedCutoff != cutoff)
        {
            cachedCutoff = cutoff;
            bridgeMeshRenderer.material.SetFloat("_Cutoff", cutoff);
        }
    }

    private IEnumerator ShowCoroutine()
    {
        wallCollider.enabled = false;

        const float duration = 2;
        float accumulate = 0;
        while (accumulate < duration)
        {
            float ratio = accumulate / duration;
            cutoff = 1f - ratio;

            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }

        cutoff = 0f;
    }

    public void ShowBridge()
    {
        StartCoroutine(ShowCoroutine());
    }
}
