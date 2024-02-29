using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMManualDrag : ControlMehtod
{
    PlayerController playerController;
    CameraTransformer cameraTransformer;
    ObjectPicker objectPicker;

    OperableObject manualDraggingObject;
    Vector3 manualDragStartPoint;

    public CMManualDrag(PlayerController playerController, CameraTransformer cameraTransformer, ObjectPicker objectPicker)
    {
        this.playerController = playerController;
        this.cameraTransformer = cameraTransformer;
        this.objectPicker = objectPicker;
    }

    public override bool Controlable()
    {
        return false;
    }

    public override bool OnBeginControl()
    {
        if (!objectPicker.focused)
            return false;

        manualDragStartPoint = objectPicker.lastValidHit.point;
        StartManualDrag(objectPicker.focused);

        playerController.mainUI.SetCrosshair(false);

        return true;
    }

    public override void OnEndControl()
    {
        StopManualDrag();

        objectPicker.enabled = true;

        playerController.mainUI.SetCrosshair(true);
    }

    public override UpdateResult Update()
    {
        if (Input.GetMouseButtonDown(0) && objectPicker.focused)
        {
            objectPicker.enabled = false;
        }
        if (Input.GetMouseButtonUp(0) || (!manualDraggingObject && !objectPicker.enabled))
        {
            objectPicker.enabled = true;
        }

        float distance = Vector3.Distance(cameraTransformer.transform.position, manualDragStartPoint);
        if (!Input.GetMouseButton(0) || distance > objectPicker.rayDistance)
        {
            return UpdateResult.Break;
        }

        return UpdateResult.Continue;
    }

    private void StartManualDrag(OperableObject obj)
    {
        Cursor.lockState = CursorLockMode.None;

        manualDraggingObject = obj;
        manualDraggingObject.StartManualDrag();

        objectPicker.enabled = false;
    }

    private void StopManualDrag()
    {
        if (manualDraggingObject)
        {
            Cursor.lockState = CursorLockMode.Locked;

            manualDraggingObject.ImmediateManualDrag();
            manualDraggingObject = null;

            objectPicker.enabled = true;
        }
    }
}
