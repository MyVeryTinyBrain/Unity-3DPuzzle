using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YieldRule
{
    static WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
    static WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    public static WaitForEndOfFrame waitForEndOfFrame => _waitForEndOfFrame;
    public static WaitForFixedUpdate waitForFixedUpdate => _waitForFixedUpdate;
}
