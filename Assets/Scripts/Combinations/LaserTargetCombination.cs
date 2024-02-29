using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserTargetCombination : ComponentEx
{
    public Action<LaserTargetCombination> OnAllActivedCallback;

    [SerializeField, ReadOnlyInRuntime]
    List<LaserTarget> targets;

    [SerializeField]
    UnityEvent completeEvent;

    int currentActives = 0;

    bool actived = false;

    public virtual bool OnAllActived()
    {
        if (actived)
            return false;

        OnAllActivedCallback?.Invoke(this);
        completeEvent.Invoke();

        return true;
    }

    public virtual void OnTargetActiveChanged(LaserTarget target, bool active)
    {
        currentActives += (active ? 1 : -1);
        if (currentActives >= targets.Count)
        {
            OnAllActived();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        foreach (LaserTarget target in targets)
            target.OnActiveChangedCallback += OnTargetActiveChanged;
    }
}
