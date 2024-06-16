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
        // 포커싱 중인 오브젝트가 아이템을 끼울 수 있는 오브젝트이면
        if(objectPicker.focused && objectPicker.focused.isItemSocket)
        {
            // 들고 있는 아이템을 해당 오브젝트에 근접시킵니다.
            inventory.FitToItemSocket(objectPicker.focused as ItemSocket);
        }
        if (Input.GetMouseButtonDown(0))
        {
            // 들고 있는 아이템이 끼울 수 있는 오브젝트에 근접한 경우
            if (inventory.itemSocket)
            {
                // 아이템을 해당 오브젝트에 장착합니다.
                inventory.FitCurrentItemToSocket();
            }
            // 포커싱 중인 오브젝트가 아이템인 경우
            else if (objectPicker.focused && objectPicker.focused.isItem)
            {
                // 해당 아이템을 습득합니다.
                ItemObject itemObject = objectPicker.focused as ItemObject;
                if (itemObject)
                {
                    objectPicker.ClearFocus();
                    inventory.PickUpItem(itemObject);
                }
            }
        }
        // 우측 마우스를 클릭하면, 들고 있는 아이템을 버립니다.
        if (Input.GetMouseButtonDown(1))
        {
            float distance = 1;
            if (objectPicker.lastHit.collider)
                distance = Mathf.Min(1, objectPicker.rayDistance);
            Vector3 dropPoisition = cameraTransformer.transform.position + cameraTransformer.transform.forward * distance;
            inventory.DropCurrentItem(dropPoisition);
        }
        // 휠을 굴려 다른 아이템을 선택합니다.
        if (Input.mouseScrollDelta.y != 0)
        {
            inventory.SetNextCurrentItem(Mathf.RoundToInt(Mathf.Sign(Input.mouseScrollDelta.y)));
        }

        return UpdateResult.Continue;
    }
}
