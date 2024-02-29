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
        if(objectPicker.focused && objectPicker.focused.isItemSocket)
        {
            inventory.FitToItemSocket(objectPicker.focused as ItemSocket);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (inventory.itemSocket)
            {
                inventory.FitCurrentItemToSocket();
            }
            else if (objectPicker.focused && objectPicker.focused.isItem)
            {
                ItemObject itemObject = objectPicker.focused as ItemObject;
                if (itemObject)
                {
                    objectPicker.ClearFocus();
                    inventory.PickUpItem(itemObject);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            float distance = 1;
            if (objectPicker.lastHit.collider)
                distance = Mathf.Min(1, objectPicker.rayDistance);
            Vector3 dropPoisition = cameraTransformer.transform.position + cameraTransformer.transform.forward * distance;
            inventory.DropCurrentItem(dropPoisition);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            inventory.SetNextCurrentItem(Mathf.RoundToInt(Mathf.Sign(Input.mouseScrollDelta.y)));
        }

        return UpdateResult.Continue;
    }
}
