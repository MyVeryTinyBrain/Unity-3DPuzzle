using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTap : OperableObject
{
    [SerializeField]
    ParticleSystem waveParticle;

    [SerializeField]
    WaterBubble waterBubblePrefab;

    [SerializeField]
    Transform waterBubbleDropStart;

    [SerializeField]
    Transform waterBubbleDropEnd;

    [SerializeField]
    List<int> dropCounts;

    [SerializeField]
    float loopDelay = 1.0f;

    [SerializeField]
    float groupDropDelay = 0.9f;

    [SerializeField]
    float singleDropDelay = 0.4f;

    private IEnumerator DropSingle(float duration)
    {
        WaterBubble watterBubble = ObjectPool.instance.Spawn(waterBubblePrefab) as WaterBubble;
        float accumulate = 0;
        while(accumulate < duration)
        {
            float ratio = accumulate / duration;
            Vector3 position = Vector3.Lerp(waterBubbleDropStart.position, waterBubbleDropEnd.position, ratio);
            watterBubble.transform.position = position;
            watterBubble.transform.SetParent(transform);

            accumulate += Time.deltaTime;
            yield return YieldRule.waitForEndOfFrame;
        }

        ObjectPool.instance.Return(watterBubble);
        waveParticle.Emit(1);
        waveParticle.Play();
    }

    private IEnumerator Drop()
    {
        const float dropDuration = 0.1f;
        while (true)
        {
            foreach (int dropCount in dropCounts)
            {
                for (int i = 0; i < dropCount; ++i)
                {
                    StartCoroutine(DropSingle(dropDuration));
                    yield return new WaitForSeconds(singleDropDelay);
                }
                yield return new WaitForSeconds(groupDropDelay);
            }
            yield return new WaitForSeconds(loopDelay);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(Drop());
    }
}
