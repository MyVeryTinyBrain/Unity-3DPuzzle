using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlMehtod
{
    // Update 함수에서 Break를 반환하면, 기본 컨트롤 방법으로 전환합니다.
    public enum UpdateResult { Continue, Break };

    // 플레이어가 이동할 수 있는지 확인합니다.
    public abstract bool Controlable();
    // 이 컨트롤 방법을 사용하기 전에 호출됩니다.
    // 해당 함수가 False를 반환하면 이 컨트롤 방법을 사용할 수 없습니다.
    public abstract bool OnBeginControl();
    // 이 컨트롤 방법의 사용이 종료되면 호출됩니다.
    public abstract void OnEndControl();
    // 여기서 컨트롤 동작을 구현합니다.
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
