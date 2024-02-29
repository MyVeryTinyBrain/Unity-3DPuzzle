using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlMehtod
{
    public enum UpdateResult { Continue, Break };

    public abstract bool Controlable();
    public abstract bool OnBeginControl();
    public abstract void OnEndControl();
    public abstract UpdateResult Update();
}

public class NothingMethod : ControlMehtod
{
    public override bool Controlable()
    {
        return false;
    }

    public override bool OnBeginControl()
    {
        return true;
    }

    public override void OnEndControl() { }

    public override UpdateResult Update()
    {
        return UpdateResult.Continue;
    }
}
