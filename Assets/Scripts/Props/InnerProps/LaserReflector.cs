using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LaserReflector : Prop
{
    protected override void Awake()
    {
        base.Awake();

        axisSpin.SetActive(true);
    }
}
