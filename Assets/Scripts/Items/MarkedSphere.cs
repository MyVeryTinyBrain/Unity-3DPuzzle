using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkedSphere : ItemObject
{
    [SerializeField]
    MeshRenderer sphereMeshRenderer;

    public int socketIndex;

    Coroutine glowCoroutine;

    protected override void Awake()
    {
        base.Awake();

        breakFindParent = true;
    }

    private IEnumerator GlowCorutine(float duration, float maxEmission)
    {
        float accumulate = 0;
        while(accumulate < duration)
        {
            float ratio = accumulate / duration;
            sphereMeshRenderer.material.SetFloat("_EmissionWeight", ratio * maxEmission);
            sphereMeshRenderer.material.SetFloat("_BaseLerp", ratio);
            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }
        accumulate = 0;
        while (accumulate < duration)
        {
            float ratio = 1 - (accumulate / duration);
            sphereMeshRenderer.material.SetFloat("_EmissionWeight", ratio * maxEmission);
            sphereMeshRenderer.material.SetFloat("_BaseLerp", ratio);
            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }
        sphereMeshRenderer.material.SetFloat("_EmissionWeight", 0.0f);
        sphereMeshRenderer.material.SetFloat("_BaseLerp", 0.0f);

        glowCoroutine = null;
    }

    public void GlowOnce(float duration, float maxEmission)
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }
        glowCoroutine = StartCoroutine(GlowCorutine(duration * 0.5f, maxEmission));
    }

    public override bool Complete()
    {
        if (!base.Complete())
            return false;

        GlowOnce(2.0f, 2.0f);

        axisSpin.SetActive(true);
        manualDraggable = true;
        isItem = false;
        return true;
    }
}
