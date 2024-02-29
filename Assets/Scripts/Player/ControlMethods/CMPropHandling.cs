using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMPropHandling : ControlMehtod
{
    PlayerController playerController;
    CameraTransformer cameraTransformer;
    ObjectPicker objectPicker;
    Transform cameraPivot;
    Inventory inventroy;

    OperableObject handlingObject;
    bool shouldBreak;

    public CMPropHandling(PlayerController playerController, CameraTransformer cameraTransformer, ObjectPicker objectPicker, Transform cameraPivot, Inventory inventroy)
    {
        this.playerController = playerController;
        this.cameraTransformer = cameraTransformer;
        this.objectPicker = objectPicker;
        this.cameraPivot = cameraPivot;
        this.inventroy = inventroy;
    }

    public override bool Controlable()
    {
        return false;
    }

    public override bool OnBeginControl()
    {
        if (!objectPicker.focused || !objectPicker.focused.cameraTransform || objectPicker.focused.isItem || objectPicker.focused.isItemSocket)
            return false;

        inventroy.SetHandlingItemVisible(false);
        shouldBreak = false;
        HandlingObject(objectPicker.focused);

        playerController.mainUI.SetCrosshair(false);

        return true;
    }

    public override void OnEndControl()
    {
        StopHandling();

        inventroy.SetHandlingItemVisible(true);

        objectPicker.enabled = true;

        playerController.mainUI.SetCrosshair(true);
    }

    public override UpdateResult Update()
    {
        if (cameraTransformer.translating)
        {
            return UpdateResult.Continue;
        }

        if (Input.GetMouseButtonDown(0) && objectPicker.focused && handlingObject)
        {
            objectPicker.enabled = false;
        }
        if (Input.GetMouseButtonUp(0) || (!handlingObject && !objectPicker.enabled))
        {
            objectPicker.enabled = true;
        }

        if (shouldBreak)
        {
            return UpdateResult.Break;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            return UpdateResult.Break;
        }

        return UpdateResult.Continue;
    }

    private void OnObjectCompleted(OperableObject obj)
    {
        if (obj == handlingObject)
        {
            shouldBreak = true;
        }
    }

    private void HandlingObject(OperableObject obj)
    {
        cameraTransformer.Translate(
            0.25f,
            cameraTransformer.transform.position,
            cameraTransformer.transform.rotation,
            obj.cameraTransform,
            Vector3.zero,
            Quaternion.identity);

        if (handlingObject != obj)
        {
            obj.OnCompletedCallback += OnObjectCompleted;
            obj.BeginHandling();
            if (handlingObject)
            {
                handlingObject.OnCompletedCallback -= OnObjectCompleted;
                handlingObject.EndHandling();
            }
        }
        handlingObject = obj;

        Cursor.lockState = CursorLockMode.None;

        objectPicker.mode = ObjectPicker.RayMode.Cursor;
        objectPicker.pickRootOnly = false;
        objectPicker.pickHandlingOnly = true;
    }

    private void StopHandling()
    {
        if (handlingObject)
        {
            cameraTransformer.Translate(
                0.25f,
                cameraTransformer.transform.position,
                cameraTransformer.transform.rotation,
                cameraPivot,
                Vector3.zero,
                Quaternion.identity);

            if (handlingObject)
            {
                handlingObject.OnCompletedCallback -= OnObjectCompleted;
                handlingObject.EndHandling();
            }
            handlingObject = null;

            Cursor.lockState = CursorLockMode.Locked;

            objectPicker.mode = ObjectPicker.RayMode.Look;
            objectPicker.pickRootOnly = true;
            objectPicker.pickHandlingOnly = false;
        }
    }
}
