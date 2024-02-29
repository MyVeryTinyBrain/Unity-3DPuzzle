using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : OperableObject
{
    List<Prop> _innerProps;
    public List<Prop> innerProps
    {
        get
        {
            if (_innerProps == null)
            {
                _innerProps = new List<Prop>();
                GetComponentsInChildren<Prop>(true, _innerProps);
                if (_innerProps.Count >= 1)
                    _innerProps.Remove(this);
            }
            return _innerProps;
        }
    }
}
