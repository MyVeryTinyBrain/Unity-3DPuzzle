using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSocket : OperableObject
{
    [SerializeField]
    string _targetItemName;

    [SerializeField]
    Transform _itemTransform;

    public string targetItemName => _targetItemName;
    public Transform itemTransform => _itemTransform;
    public ItemObject itemObject { get; private set; }

    public bool AttachItem(ItemObject item)
    {
        if (itemObject)
            return false;

        itemObject = item;
        OnItemAttached(item);
        return true;
    }

    public bool DetachItem(ItemObject item)
    {
        if (itemObject != item)
            return false;

        OnItemDetach(item);
        itemObject = null;
        return true;
    }

    public virtual void OnItemAttached(ItemObject item)
    {

    }

    public virtual void OnItemDetach(ItemObject item)
    {

    }

    protected override void Awake()
    {
        base.Awake();

        isItemSocket = true;
    }
}
