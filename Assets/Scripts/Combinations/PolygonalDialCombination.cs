using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PolygonalDialCombination : ComponentEx
{
    public event Action<PolygonalDialCombination> OnMatchedCallback;

    [SerializeField, ReadOnlyInRuntime]
    List<PolygonalDial> dials;

    Dictionary<PolygonalDial, int> _dialIndices = null;
    Dictionary<PolygonalDial, int> dialIndices
    {
        get
        {
            if (_dialIndices == null)
            {
                _dialIndices = new Dictionary<PolygonalDial, int>();
                for (int i = 0; i < dials.Count; i++)
                    _dialIndices.TryAdd(dials[i], i);
            }
            return _dialIndices;
        }
    }

    [SerializeField, NumberOnlyString]
    string combination;

    private bool CombinationMatch()
    {
        int end = Mathf.Min(dials.Count, combination.Length);
        if (end == 0)
            return false;

        for (int i = 0; i < end; ++i)
        {
            int digit = combination[i] - '0';
            if (digit != dials[i].axisSpin.GetNumber())
                return false;
        }
        return true;
    }

    public virtual void OnMatched()
    {
        OnMatchedCallback?.Invoke(this);
    }

    private void OnChanged(AxisSpinnable dial)
    {
        int dialIndex;
        if (!dialIndices.TryGetValue(dial.root as PolygonalDial, out dialIndex))
            return;

        if (CombinationMatch())
        {
            OnMatched();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        foreach (PolygonalDial dial in dials)
            if (dial)
                dial.axisSpin.OnAxisSpinned += OnChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        foreach (PolygonalDial dial in dials)
            if (dial)
                dial.axisSpin.OnAxisSpinned -= OnChanged;
    }
}