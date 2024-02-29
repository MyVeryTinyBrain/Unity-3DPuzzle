using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTarget : Prop
{
    public event Action<LaserTarget, bool> OnActiveChangedCallback;

    public bool hit => prevHit;
    bool prevHit = false;
    bool currentHit = false;
    float hitAccumulate = 0;

    public void NowHit()
    {
        currentHit = true;
    }

    public virtual void OnActived()
    {
        OnActiveChangedCallback?.Invoke(this, true);
    }

    public virtual void OnDeactived()
    {
        OnActiveChangedCallback?.Invoke(this, false);
    }

    protected override void Update()
    {
        base.Update();

        if (prevHit != currentHit)
        {
            if (currentHit)
                OnActived();
            else
                OnDeactived();
        }
        prevHit = currentHit;
        currentHit = false;
    }
}
