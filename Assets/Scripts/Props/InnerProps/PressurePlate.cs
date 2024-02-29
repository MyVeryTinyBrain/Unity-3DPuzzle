using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Prop
{
    public event Action<PressurePlate> OnActivedCallback;
    public event Action<PressurePlate> OnDeactivedCallback;

    [SerializeField, ReadOnlyInRuntime]
    new Collider collider;

    [SerializeField]
    float moveHeight = 0.4f;

    [SerializeField]
    float moveSpeed = 1.5f;

    public bool ManualDeactive = true;

    public bool isActived { get; private set; } = false;

    public bool isCollidingWithPlayerController { get; private set; } = false;

    public Bounds worldSpaceBounds => collider.bounds;

    public void MoveToActive(bool notify)
    {
        if (isActived)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveHeightCoroutine(moveHeight, moveSpeed));
        isActived = true;
        if(notify)
            OnActived();
    }

    public void MoveToDeactive(bool notify)
    {
        if (!isActived)
            return;

        StopAllCoroutines();
        StartCoroutine(MoveHeightCoroutine(0, moveSpeed));
        isActived = false;
        if (notify)
            OnDeactived();
    }

    public override void OnBeginPlayerControllerContact(PlayerController playerController)
    {
        base.OnBeginPlayerControllerContact(playerController);

        isCollidingWithPlayerController = true;

        if (!operable)
            return;

        MoveToActive(true);
    }

    public override void OnEndPlayerControllerContact(PlayerController playerController)
    {
        base.OnEndPlayerControllerContact(playerController);

        isCollidingWithPlayerController = false; 

        if (!operable)
            return;

        if (!ManualDeactive)
            MoveToDeactive(true);
    }

    public virtual void OnActived()
    {
        OnActivedCallback?.Invoke(this);
    }

    public virtual void OnDeactived()
    {
        OnDeactivedCallback?.Invoke(this);
    }

    private IEnumerator MoveHeightCoroutine(float height, float speed)
    {
        Func<float, bool> less = (y) => { return y < height; };
        Func<float, bool> greater = (y) => { return y > height; };
        Func<float, bool> compare = collider.transform.localPosition.y < height ? greater : less;
        float speedMultiplider = collider.transform.localPosition.y < height ? +1 : -1;

        Vector3 localPosition;
        while (!compare(collider.transform.localPosition.y))
        {
            localPosition = collider.transform.localPosition;
            localPosition.y += speed * Time.deltaTime * speedMultiplider;
            collider.transform.localPosition = localPosition;
            yield return YieldRule.waitForEndOfFrame;
        }

        localPosition = collider.transform.localPosition;
        localPosition.y = height;
        collider.transform.localPosition = localPosition;
    }
}
