using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMDefault : ControlMehtod
{
    PlayerController playerController;
    CameraTransformer cameraTransformer;
    Inventory inventory;
    ObjectPicker objectPicker;

    public CMDefault(PlayerController playerController, CameraTransformer cameraTransformer, Inventory inventory, ObjectPicker objectPicker)
    {
        this.playerController = playerController;
        this.cameraTransformer = cameraTransformer;
        this.inventory = inventory;
        this.objectPicker = objectPicker;
    }

    public override bool Controlable()
    {
        return true;
    }

    public override bool OnBeginControl()
    {
        Cursor.lockState = CursorLockMode.Locked;

        playerController.mainUI.SetCrosshair(true);

        return true;
    }

    public override void OnEndControl()
    {
    }

    public override UpdateResult Update()
    {
        // ��Ŀ�� ���� ������Ʈ�� �������� ���� �� �ִ� ������Ʈ�̸�
        if(objectPicker.focused && objectPicker.focused.isItemSocket)
        {
            // ��� �ִ� �������� �ش� ������Ʈ�� ������ŵ�ϴ�.
            inventory.FitToItemSocket(objectPicker.focused as ItemSocket);
        }
        if (Input.GetMouseButtonDown(0))
        {
            // ��� �ִ� �������� ���� �� �ִ� ������Ʈ�� ������ ���
            if (inventory.itemSocket)
            {
                // �������� �ش� ������Ʈ�� �����մϴ�.
                inventory.FitCurrentItemToSocket();
            }
            // ��Ŀ�� ���� ������Ʈ�� �������� ���
            else if (objectPicker.focused && objectPicker.focused.isItem)
            {
                // �ش� �������� �����մϴ�.
                ItemObject itemObject = objectPicker.focused as ItemObject;
                if (itemObject)
                {
                    objectPicker.ClearFocus();
                    inventory.PickUpItem(itemObject);
                }
            }
        }
        // ���� ���콺�� Ŭ���ϸ�, ��� �ִ� �������� �����ϴ�.
        if (Input.GetMouseButtonDown(1))
        {
            float distance = 1;
            if (objectPicker.lastHit.collider)
                distance = Mathf.Min(1, objectPicker.rayDistance);
            Vector3 dropPoisition = cameraTransformer.transform.position + cameraTransformer.transform.forward * distance;
            inventory.DropCurrentItem(dropPoisition);
        }
        // ���� ���� �ٸ� �������� �����մϴ�.
        if (Input.mouseScrollDelta.y != 0)
        {
            inventory.SetNextCurrentItem(Mathf.RoundToInt(Mathf.Sign(Input.mouseScrollDelta.y)));
        }

        return UpdateResult.Continue;
    }
}
