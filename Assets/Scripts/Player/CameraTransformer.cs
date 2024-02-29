using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransformer : ComponentEx
{
    public event Action OnArrived;
    public bool translating { get; private set; }
    float duration;
    float accumulation;
    Vector3 startWorldPosition;
    Quaternion startWorldRotation;
    public Transform targetTransform { get; private set; }
    Vector3 targetLocalPosition;
    Quaternion targetLocalRotation;

    public void Translate(float duration, Vector3 startWorldPosition, Quaternion startWorldRotation, Transform targetTransform, Vector3 targetLocalPosition, Quaternion targetLocalRotation)
    {
        this.duration = duration;
        this.accumulation = 0;
        this.startWorldPosition = startWorldPosition;
        this.startWorldRotation = startWorldRotation;
        this.targetTransform = targetTransform;
        this.targetLocalPosition = targetLocalPosition;
        this.targetLocalRotation = targetLocalRotation;
        this.translating = true;
        this.transform.parent = targetTransform;
    }

    public void SimpleTranslate(float duration, Transform target)
    {
        Translate(duration, transform.position, transform.rotation, target, Vector3.zero, Quaternion.identity);
    }

    protected override void Update()
    {
        base.Update();

        if (!translating)
            return;

        accumulation += Time.deltaTime;
        float x = Mathf.Clamp(accumulation / duration, 0.0f, 1.0f);
        float p = (float)Math.E;
        float ratio = MathfUtility.Smooth(x, p);

        // -(1-x)^q+1

        Vector3 targetWorldPosition = targetTransform.TransformPoint(targetLocalPosition);
        Quaternion targetWorldRotation = targetTransform.rotation * targetLocalRotation;
        Vector3 worldPosition = Vector3.Lerp(startWorldPosition, targetWorldPosition, ratio);
        Quaternion worldRotation = Quaternion.Lerp(startWorldRotation, targetWorldRotation, ratio);
        transform.position = worldPosition;
        transform.rotation = worldRotation;

        if(x == 1.0f)
        {
            translating = false;
            OnArrived?.Invoke();
        }
    }
}
