using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 오브젝트에 줌 인 하여, 해당 오브젝트를 조작합니다.
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
        // 카메라 이벤트 중에는 동작하지 않습니다.
        if (cameraTransformer.translating)
        {
            return UpdateResult.Continue;
        }
        /* 오브젝트를 핸들링 할 때, 그 오브젝트에 장착되어 있는 다른 오브젝트를 습득 가능한 로직 */
        // 현재 핸들링 중인 오브젝트를 다시 습득할 수 없습니다.
        if (Input.GetMouseButtonDown(0) && objectPicker.focused && handlingObject)
        {
            objectPicker.enabled = false;
        }
        // 현재 핸들링 중인 오브젝트가 아니면 습득할 수 있습니다.
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
