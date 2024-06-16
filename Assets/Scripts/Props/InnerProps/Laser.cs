using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Laser : Prop
{
    [SerializeField, ReadOnlyInRuntime]
    LineRenderer lineRenderer;

    [SerializeField]
    float MaxSegmentDistance = 100.0f;

    [SerializeField]
    int MaxVertices = 100;

    [SerializeField]
    float SegmentSliceDistance = 10.0f;

    [SerializeField]
    LayerMask layerMask;

    [SerializeField]
    List<Tag> reflectTags;

    [SerializeField]
    Tag targetTag;

    [SerializeField]
    LaserHitEffect hitEffectPrefab;

    Vector3[] vertices = new Vector3[0];

    int usingHitEffectsCount = 0;
    List<LaserHitEffect> usingHitEffects = new List<LaserHitEffect>();

    void SliceSegment(ref int i, Vector3 start, Vector3 direction, float distance)
    {
        // �ִ� ���� ���̷� ������ ���� ������ ���մϴ�.
        int slice = Mathf.CeilToInt(distance / SegmentSliceDistance);
        for (int j = 1; j <= slice; ++j)
        {
            vertices[i++] = start + direction * distance * (float)(j / slice);
        }
    }

    void ClearHitEffects()
    {
        if (usingHitEffectsCount != usingHitEffects.Count)
        {
            for (int i = usingHitEffectsCount; i < usingHitEffects.Count; ++i)
                ObjectPool.instance.Return(usingHitEffects[i]);
            usingHitEffects.RemoveRange(usingHitEffectsCount, usingHitEffects.Count - usingHitEffectsCount);
        }
        usingHitEffectsCount = 0;
    }

    void SpawnHitEffect(Vector3 worldPosition, Vector3 forward)
    {
        LaserHitEffect effect;
        if (usingHitEffectsCount < usingHitEffects.Count)
        {
            effect = usingHitEffects[usingHitEffectsCount++];
        }
        else
        {
            effect = ObjectPool.instance.Spawn(hitEffectPrefab) as LaserHitEffect;
            if (effect)
            {
                usingHitEffects.Add(effect);
                usingHitEffectsCount++;
            }
        }
        effect.transform.position = worldPosition;
        effect.transform.forward = forward;
    }

    public void SetupVertices()
    {
        if (!operable)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.useWorldSpace = true;

        if (vertices.Length != MaxVertices)
            vertices = new Vector3[MaxVertices];

        // �������� ���� ���� ��ġ ����
        vertices[0] = lineRenderer.transform.position;
        // �������� ���� ����
        Vector3 direction = lineRenderer.transform.forward;
        int i = 1;
        bool canActiveTarget = true;
        while(i < MaxVertices)
        {
            // ������ ���� �������� ����ĳ��Ʈ
            RaycastHit hit;
            bool isHit = Physics.Raycast(vertices[i - 1], direction, out hit, MaxSegmentDistance, layerMask);

            if (isHit)
            {
                // �������� �浹�� �������� ���� ������ ������ �����մϴ�.
                SliceSegment(ref i, vertices[i - 1], direction, hit.distance);
                // �������� �浹�� ������ ������ Ÿ�� ����Ʈ�� �����մϴ�. (������Ʈ Ǯ ���)
                SpawnHitEffect(hit.point, hit.normal);
                // �ݻ� ������ ������Ʈ�� �浹�� ���
                if (reflectTags.Contains(hit.collider.gameObject.tag))
                {
                    LaserReflector reflector = hit.collider.gameObject.GetComponentInParent<LaserReflector>();
                    if (reflector && reflector.isDragging)
                        canActiveTarget = false;
                    if (reflector && reflector.axisSpin.isSpinning) 
                        canActiveTarget = false;
                    // �ݻ� ���͸� ����ϰ�, �� ���� ���͸� ������ ���� �������� ����մϴ�.
                    direction = Vector3.Reflect(direction, hit.normal);
                }
                else // �ݻ� ������ ������Ʈ�� �浹���� ���� ���
                {
                    // Ÿ�ٿ� �浹�� ��� �ش� Ÿ���� Ȱ��ȭ�մϴ�.
                    if (targetTag == hit.collider.gameObject.tag && canActiveTarget)
                    {
                        LaserTarget laserTarget = hit.collider.gameObject.GetComponentInParent<LaserTarget>();
                        laserTarget?.NowHit();
                    }
                    // �� �̻� �ݻ� ������ ������Ʈ�� �����Ƿ� �����մϴ�.
                    break;
                }
            }
            else // �ƹ� ������Ʈ�͵� �浹���� ���� ���
            {
                // ���� �Ÿ����� ���� ������ ������ �����մϴ�.
                SliceSegment(ref i, vertices[i - 1], direction, MaxSegmentDistance);
                // �� �̻� �ݻ� ������ ������Ʈ�� �����Ƿ� �����մϴ�.
                break;
            }
        }
        i = Mathf.Clamp(i, 0, MaxVertices);

        lineRenderer.positionCount = i;
        lineRenderer.SetPositions(vertices);

        ClearHitEffects();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        SetupVertices();
    }
}
