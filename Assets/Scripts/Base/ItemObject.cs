using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemObject : OperableObject
{
    [SerializeField]
    new Rigidbody rigidbody;

    [SerializeField, ReadOnly]
    ItemSocket itemSocket;

    public bool isOwnedItem { get; private set; } = false;
    public bool isInnerItem { get; private set; } = false;
    public bool isHandlingItem { get; protected set; } = false;

    public ItemObject parentItemObject { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        isItem = true;

        ItemObject[] parnetItems = gameObject.GetComponentsInParent<ItemObject>();
        if (parnetItems.Length >= 2)
        {
            parentItemObject = parnetItems[1];
            isInnerItem = true;
        }

        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void NotifyPickup()
    {
        if (isInnerItem)
            OnBeginPickUpAsInnerItem();
        else
            OnBeginPickUp();
    }

    public virtual void OnBeginPickUp()
    {
        if (itemSocket)
        {
            itemSocket.DetachItem(this);
            itemSocket = null;
        }

        isOwnedItem = true;
        rigidbody.isKinematic = true;
        SetChildLayers(Layers.instance.OwningPropLayerIndex);
    }

    public virtual void OnBeginPickUpAsInnerItem()
    {
        parentItemObject?.SearchChildComponents();

        isInnerItem = false;
        parentItemObject = null;

        if (itemSocket)
        {
            itemSocket.DetachItem(this);
            itemSocket = null;
        }

        isOwnedItem = true;
        rigidbody.isKinematic = true;
        SetChildLayers(Layers.instance.StoredOwningPropLayerIndex);
    }

    public virtual void OnBeginDrop()
    {
        isOwnedItem = false;
        rigidbody.isKinematic = false;
        SetChildLayers(Layers.instance.PropLayerIndex);
    }

    public virtual void OnFitToItemSocket(ItemSocket socket)
    {
        itemSocket = socket;

        isOwnedItem = false;
        rigidbody.isKinematic = true;
        SetChildLayers(Layers.instance.PropLayerIndex);
    }

    public virtual void OnBeginFittingToItemSocket(ItemSocket socket)
    {
        SetChildLayers(Layers.instance.FittingOwningPropLayerIndex);
    }

    public virtual void OnEndFittingToItemSocket(ItemSocket socket)
    {
        SetChildLayers(Layers.instance.OwningPropLayerIndex);
    }

    public virtual void OnBeginHandlingItem()
    {
        SetChildLayers(Layers.instance.PropLayerIndex);
        isHandlingItem = true;
    }

    public virtual void OnEndHandlingItem()
    {
        SetChildLayers(Layers.instance.OwningPropLayerIndex);
        isHandlingItem = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        if (!isHandlingItem)
            return;

        bool repeated;
        Rect window;
        Win32Unity.RepeatCursor(out repeated);
        Win32Unity.GetWindowRect(out window);

        if (Mathf.Abs(eventData.delta.x) < window.width * 0.5f && Mathf.Abs(eventData.delta.y) < window.height * 0.5f)
        {
            const float sensitivity = 0.25f;
            Vector2 delta = eventData.delta * sensitivity;
            Quaternion rotateYAxis = Quaternion.AngleAxis(-delta.x, Camera.main.transform.up);
            Quaternion rotateXAxis = Quaternion.AngleAxis(delta.y, Camera.main.transform.right);
            transform.rotation = rotateXAxis * rotateYAxis * transform.rotation;
        }
    }
}
