using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : ComponentEx, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public bool isHovering { get; private set; } = false;
    public bool isClicking { get; private set; } = false;
    public bool isDragging { get; private set; } = false;
    public bool isManualDragging { get; private set; } = false;

    PointerEventData manualDragData = null;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isClicking = true;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isClicking = false;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
    }

    public virtual void OnBeginManualDrag(PointerEventData eventData) { }
    public virtual void OnEndManualDrag(PointerEventData eventData) { }

    public void StartManualDrag()
    {
        if (isDragging || isManualDragging)
            return;
        isManualDragging = true;
        isDragging = true; // append

        manualDragData = new PointerEventData(EventSystem.current);
        manualDragData.position = Input.mousePosition;
        manualDragData.delta = Vector2.zero;
        OnBeginDrag(manualDragData);
        OnBeginManualDrag(manualDragData);
    }

    public void ImmediateManualDrag()
    {
        if (isManualDragging)
        {
            isManualDragging = false;
            OnEndDrag(manualDragData);
            OnEndManualDrag(manualDragData);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isManualDragging)
        {
            manualDragData.delta = (Vector2)Input.mousePosition - manualDragData.position;
            manualDragData.position = Input.mousePosition;

            if (Input.GetMouseButton(0))
            {
                OnDrag(manualDragData);
            }
            else
            {
                ImmediateManualDrag();
            }
        }
    }
}
