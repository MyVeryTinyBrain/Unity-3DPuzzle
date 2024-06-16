using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterControllerRigidbody
{
    [SerializeField]
    public float speed = 10.0f;

    [SerializeField, ReadOnlyInRuntime]
    Transform cameraPivot;

    [SerializeField, ReadOnlyInRuntime]
    new Camera camera;

    [SerializeField]
    public float sensitivity = 3.0f;

    [SerializeField]
    public float jumpHeight = 1.0f;

    [SerializeField, ReadOnlyInRuntime]
    public CameraTransformer cameraTransformer;

    [SerializeField, ReadOnlyInRuntime]
    ObjectPicker objectPicker;

    [SerializeField, ReadOnlyInRuntime]
    Inventory inventory;

    [SerializeField, ReadOnlyInRuntime]
    public MainUI mainUI;

    float cachedBodyYAngle;
    float cachedCameraXAngle;

    CMDefault cmDefault;
    CMPropHandling cmPropHandling;
    CMManualDrag cmManualDrag;
    CMItemHandling cmItemHandling;

    // 현재 사용중인 컨트롤 방법
    ControlMehtod controlMehtod;

    public void SetControlMethod(ControlMehtod cm)
    {
        if (cm == controlMehtod)
            return;

        // 이전의 컨트롤 방법 사용종료 콜백 호출
        controlMehtod?.OnEndControl();
        // 새로운 컨트 롤방법 사용시작 콜백 호출
        if (cm.OnBeginControl())
        {
            controlMehtod = cm;
        }
        else
        {
            // 새로운 컨트롤 방법 사용시작 콜백이 false 반환하면,
            // 새로운 컨트롤 방법을 사용하지 않습니다.
            controlMehtod?.OnBeginControl();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        cmDefault = new CMDefault(this, cameraTransformer, inventory, objectPicker);
        cmPropHandling = new CMPropHandling(this, cameraTransformer, objectPicker, cameraPivot, inventory);
        cmManualDrag = new CMManualDrag(this, cameraTransformer, objectPicker);
        cmItemHandling = new CMItemHandling(this, inventory, objectPicker);
        SetControlMethod(cmDefault);
    }

    protected override void Start()
    {
        base.Start();

        cachedBodyYAngle = transform.localEulerAngles.y;
        cachedCameraXAngle = cameraPivot.localEulerAngles.x;
        StartCoroutine(CameraControlCoroutine());
    }

    protected override void Update()
    {
        base.Update();

        if(controlMehtod == cmDefault)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (objectPicker.focused && objectPicker.focused.manualDraggable)
                {
                    SetControlMethod(cmManualDrag);
                }
                else if(objectPicker.focused && objectPicker.focused.cameraTransform)
                {
                    SetControlMethod(cmPropHandling);
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                SetControlMethod(cmItemHandling);
            }
        }
        ControlMehtod.UpdateResult result = controlMehtod != null ? controlMehtod.Update() : ControlMehtod.UpdateResult.Break;
        if(result == ControlMehtod.UpdateResult.Break)
        {
            SetControlMethod(cmDefault);
        }
    }

    private IEnumerator CameraControlCoroutine()
    {
        yield return YieldRule.waitForEndOfFrame;
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            // 조작 불가능한 방법이면 수행하지 않습니다.
            if (controlMehtod.Controlable())
            {
                // 캐릭터 카메라를 회전합니다.

                Vector2 axis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                Vector3 delta = axis * sensitivity;

                cachedCameraXAngle = Mathf.Clamp(cachedCameraXAngle - delta.y, -89.9f, 89.9f);
                cachedBodyYAngle = Mathf.Repeat(cachedBodyYAngle + delta.x, 360.0f);

                Vector3 bodyAngle = transform.localEulerAngles;
                Vector3 cameraAngle = cameraPivot.localEulerAngles;
                bodyAngle.y = cachedBodyYAngle;
                cameraAngle.x = cachedCameraXAngle;
                transform.localEulerAngles = bodyAngle;
                cameraPivot.localEulerAngles = cameraAngle;
            }
            yield return YieldRule.waitForEndOfFrame;
        }
    }

    public override void CustomMove()
    {
        base.CustomMove();

        // 조작 불가능한 컨트롤 방법이면 수행하지 않습니다.
        if (!controlMehtod.Controlable())
            return;

        // 캐릭터를 이동합니다.
        Vector3 axis = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 direction = transform.localToWorldMatrix.MultiplyVector(axis);
        Move(direction, speed);

        //if (Input.GetButtonDown("Jump"))
        //    Jump(Mathf.Sqrt(jumpHeight));
    }

    public override void OnBeginContact(Collider collider)
    {
        base.OnBeginContact(collider);

        if (collider.gameObject.layer == Layers.instance.PropLayerIndex || collider.gameObject.layer == Layers.instance.OutlinePropLayerIndex)
        {
            OperableObject operableObject = collider.GetComponentInParent<OperableObject>();
            if (operableObject)
            {
                operableObject.OnBeginPlayerControllerContact(this);
            }
        }
    }

    public override void OnEndContact(Collider collider)
    {
        base.OnEndContact(collider);

        if (collider.gameObject.layer == Layers.instance.PropLayerIndex || collider.gameObject.layer == Layers.instance.OutlinePropLayerIndex)
        {
            OperableObject operableObject = collider.GetComponentInParent<OperableObject>();
            if (operableObject)
            {
                operableObject.OnEndPlayerControllerContact(this);
            }
        }
    }
}
