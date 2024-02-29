using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponentEx : ComponentEx
{
    RectTransform _rectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if(_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }
}
