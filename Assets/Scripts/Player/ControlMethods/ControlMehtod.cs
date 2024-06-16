using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlMehtod
{
    // Update �Լ����� Break�� ��ȯ�ϸ�, �⺻ ��Ʈ�� ������� ��ȯ�մϴ�.
    public enum UpdateResult { Continue, Break };

    // �÷��̾ �̵��� �� �ִ��� Ȯ���մϴ�.
    public abstract bool Controlable();
    // �� ��Ʈ�� ����� ����ϱ� ���� ȣ��˴ϴ�.
    // �ش� �Լ��� False�� ��ȯ�ϸ� �� ��Ʈ�� ����� ����� �� �����ϴ�.
    public abstract bool OnBeginControl();
    // �� ��Ʈ�� ����� ����� ����Ǹ� ȣ��˴ϴ�.
    public abstract void OnEndControl();
    // ���⼭ ��Ʈ�� ������ �����մϴ�.
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
