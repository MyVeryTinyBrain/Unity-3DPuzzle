using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPicker : ComponentEx
{
    public enum RayMode { Look, Cursor };

    [SerializeField]
    public RayMode mode = RayMode.Look;

    [SerializeField]
    public bool pickRootOnly = true;

    [SerializeField]
    public bool pickHandlingOnly = false;

    [SerializeField]
    LayerMask raycastLayerMask;

    public float rayDistance = 3.0f;

    OperableObject _focused;
    public OperableObject focused => enabled ? _focused : null;

    public RaycastHit lastHit { get; private set; }
    public RaycastHit lastValidHit { get; private set; }

    Ray LookRay(out float length)
    {
        length = rayDistance;
        return new Ray(transform.position, transform.forward);
    }

    Ray CursorRay(out float length)
    {
        length = 10000.0f;
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }

    bool IsValidObject(OperableObject obj)
    {
        if (!obj)
            return false;
        if (pickHandlingOnly && !obj.isHandling)
            return false;
        return true;
    }

    public void ClearFocus()
    {
        _focused?.OnEndFocus();
        _focused = null;
    }

    protected override void Update()
    {
        base.Update();

        Ray ray;
        float length;
        switch (mode)
        {
            default:
            case RayMode.Look:
            ray = LookRay(out length);
            break;

            case RayMode.Cursor:
            ray = CursorRay(out length);
            break;
        }

        RaycastHit hit;
        OperableObject operableObject = null;
        if (Physics.Raycast(ray, out hit, length, raycastLayerMask))
        {
            operableObject = hit.collider.gameObject.GetComponentInParent<OperableObject>();
            if (!IsValidObject(operableObject))
                operableObject = null;

            if (pickRootOnly && operableObject && !operableObject.breakFindParent && !operableObject.isItem)
            {
                while (operableObject.transform.parent)
                {
                    OperableObject parentOperableObject = operableObject.transform.parent.GetComponentInParent<OperableObject>();
                    if (IsValidObject(parentOperableObject))
                        operableObject = parentOperableObject;
                    else
                        break;
                }
            }
            if (operableObject && !operableObject.pickable)
                operableObject = null;
        }
        lastHit = hit;
        if (operableObject != null)
        {
            lastValidHit = hit;
        }
        if (operableObject != _focused)
        {
            _focused?.OnEndFocus();
            operableObject?.OnBeginFocus();
            _focused = operableObject;
        }
    }
}
