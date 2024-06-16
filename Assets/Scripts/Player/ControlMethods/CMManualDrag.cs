using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 월드에 존재하는 오브젝트를 회전합니다.
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
        // 오브젝트 회전 중에는 오브젝트 내부의 다른 오브젝트를 습득할 수 없습니다.
        if (Input.GetMouseButtonDown(0) && objectPicker.focused)
        {
            objectPicker.enabled = false;
        }
        if (Input.GetMouseButtonUp(0) || (!manualDraggingObject && !objectPicker.enabled))
        {
            objectPicker.enabled = true;
        }

        // 오브젝트가 카메라와 너무 멀어지면 이 컨트를 방법을 중단합니다.
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
