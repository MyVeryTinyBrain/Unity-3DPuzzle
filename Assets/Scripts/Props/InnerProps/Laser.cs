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
        // 최대 선분 길이로 나누어 분할 개수를 구합니다.
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

        // 레이저의 최초 시작 위치 설정
        vertices[0] = lineRenderer.transform.position;
        // 레이저의 진행 방향
        Vector3 direction = lineRenderer.transform.forward;
        int i = 1;
        bool canActiveTarget = true;
        while(i < MaxVertices)
        {
            // 레이저 진행 방향으로 레이캐스트
            RaycastHit hit;
            bool isHit = Physics.Raycast(vertices[i - 1], direction, out hit, MaxSegmentDistance, layerMask);

            if (isHit)
            {
                // 레이저가 충돌한 지점까지 라인 렌더러 정점을 생성합니다.
                SliceSegment(ref i, vertices[i - 1], direction, hit.distance);
                // 레이저가 충돌한 지점에 레이저 타격 이펙트를 생성합니다. (오브젝트 풀 사용)
                SpawnHitEffect(hit.point, hit.normal);
                // 반사 가능한 오브젝트에 충돌한 경우
                if (reflectTags.Contains(hit.collider.gameObject.tag))
                {
                    LaserReflector reflector = hit.collider.gameObject.GetComponentInParent<LaserReflector>();
                    if (reflector && reflector.isDragging)
                        canActiveTarget = false;
                    if (reflector && reflector.axisSpin.isSpinning) 
                        canActiveTarget = false;
                    // 반사 벡터를 계산하고, 이 방향 벡터를 레이저 진행 방향으로 사용합니다.
                    direction = Vector3.Reflect(direction, hit.normal);
                }
                else // 반사 가능한 오브젝트에 충돌하지 않은 경우
                {
                    // 타겟에 충돌한 경우 해당 타겟을 활성화합니다.
                    if (targetTag == hit.collider.gameObject.tag && canActiveTarget)
                    {
                        LaserTarget laserTarget = hit.collider.gameObject.GetComponentInParent<LaserTarget>();
                        laserTarget?.NowHit();
                    }
                    // 더 이상 반사 가능한 오브젝트가 없으므로 종료합니다.
                    break;
                }
            }
            else // 아무 오브젝트와도 충돌하지 않은 경우
            {
                // 일정 거리까지 라인 렌더러 정점을 생성합니다.
                SliceSegment(ref i, vertices[i - 1], direction, MaxSegmentDistance);
                // 더 이상 반사 가능한 오브젝트가 없으므로 종료합니다.
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
