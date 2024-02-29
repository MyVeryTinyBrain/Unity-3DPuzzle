using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkedSphereSocket : ItemSocket
{
    public event Action<MarkedSphereSocket> OnValidSphereAttachedCallback;
    public event Action<MarkedSphereSocket> OnValidSphereDetachedCallback;

    public int socketIndex;

    bool IsValidSphere(MarkedSphere sphere)
    {
        if (sphere)
        {
            return sphere.socketIndex == socketIndex;
        }
        return false;
    }

    public override void OnItemAttached(ItemObject item)
    {
        base.OnItemAttached(item);

        MarkedSphere sphere = item as MarkedSphere;
        if (IsValidSphere(sphere))
        {
            OnValidSphereAttachedCallback?.Invoke(this);
        }
    }

    public override void OnItemDetach(ItemObject item)
    {
        base.OnItemDetach(item);

        MarkedSphere sphere = item as MarkedSphere;
        if (IsValidSphere(sphere))
        {
            OnValidSphereDetachedCallback?.Invoke(this);
        }
    }

    public override bool Complete()
    {
        if (!base.Complete())
            return false;

        pickable = false;

        itemObject.Complete();

        return true;
    }
}
