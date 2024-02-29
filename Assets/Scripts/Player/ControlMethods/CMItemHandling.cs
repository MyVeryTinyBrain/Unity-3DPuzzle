using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMItemHandling : ControlMehtod
{
    PlayerController playerController;
    Inventory inventory;
    ObjectPicker objectPicker;
    bool canBreak;

    public CMItemHandling(PlayerController playerController, Inventory inventory, ObjectPicker objectPicker)
    {
        this.playerController = playerController;
        this.inventory = inventory;
        this.objectPicker = objectPicker;
    }

    public override bool Controlable()
    {
        return false;
    }

    public override bool OnBeginControl()
    {
        if (inventory.isHandling)
            return false;

        if (!inventory.OperateHandlingItem())
            return false;

        Cursor.lockState = CursorLockMode.None;

        inventory.handlingItem.BeginHandling();

        objectPicker.mode = ObjectPicker.RayMode.Cursor;
        objectPicker.pickRootOnly = false;
        objectPicker.pickHandlingOnly = true;

        playerController.mainUI.SetCrosshair(false);

        canBreak = false;

        return true;
    }

    public override void OnEndControl()
    {
        Cursor.lockState = CursorLockMode.Locked;

        inventory.handlingItem.EndHandling();

        objectPicker.ClearFocus();
        objectPicker.mode = ObjectPicker.RayMode.Look;
        objectPicker.pickRootOnly = true;
        objectPicker.pickHandlingOnly = false;

        playerController.mainUI.SetCrosshair(true);

        inventory.StopOperateHandlingItem();

        objectPicker.enabled = true;
    }

    public override UpdateResult Update()
    {
        if(Input.GetMouseButtonDown(0) && objectPicker.focused && objectPicker.focused != inventory.handlingItem)
        {
            ItemObject innerItemObject = objectPicker.focused as ItemObject;

            objectPicker.ClearFocus();
            inventory.PickUpInnerItem(innerItemObject);
        }

        if (Input.GetMouseButtonDown(0) && objectPicker.focused && inventory.handlingItem)
        {
            objectPicker.enabled = false;
        }
        if (Input.GetMouseButtonUp(0) || (!objectPicker.enabled && !inventory.handlingItem))
        {
            objectPicker.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && canBreak)
        {
            return UpdateResult.Break;
        }

        canBreak = true;
        return UpdateResult.Continue;
    }
}
