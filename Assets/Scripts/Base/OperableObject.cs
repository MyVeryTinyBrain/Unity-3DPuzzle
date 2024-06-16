using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OperableObject : ClickableObject
{
    public event Action<OperableObject> OnCompletedCallback;
    public event Action<PlayerController> OnBeginPlayerControllerContactCallback;
    public event Action<PlayerController> OnEndPlayerControllerContactCallback;

    [SerializeField]
    string _objectName;

    public string objectName => _objectName;

    [SerializeField]
    public bool operable = true;

    [SerializeField]
    public bool pickable = true;

    [SerializeField]
    public bool manualDraggable = false;

    [SerializeField]
    public bool breakFindParent = false;

    [SerializeField, ReadOnlyInRuntime]
    Transform _cameraTransform;
    public Transform cameraTransform => _cameraTransform;

    [SerializeField]
    public AxisSpinnable axisSpin;

    [SerializeField]
    public FreeSpinnable freeSpin;

    public bool isHandling { get; set; } = false;

    public bool picked { get; private set; } = false;

    public bool completed { get; private set; } = false;

    public bool isItem { get; protected set; } = false;

    public bool isItemSocket { get; protected set; } = false;

    public void SearchChildOperableObjects()
    {
        _childOperableObjects = new List<OperableObject>();
        GetComponentsInChildren<OperableObject>(true, _childOperableObjects);
    }

    public void SearchChildMeshRenderers()
    {
        _meshRenderers = new List<MeshRenderer>();
        GetComponentsInChildren<MeshRenderer>(true, _meshRenderers);
    }

    public void SearchChildComponents()
    {
        SearchChildOperableObjects();
        SearchChildMeshRenderers();
    }

    List<OperableObject> _childOperableObjects;
    List<OperableObject> childOperableObjects
    {
        get
        {
            if (_childOperableObjects == null)
                SearchChildOperableObjects();
            return _childOperableObjects;
        }
    }

    List<MeshRenderer> _meshRenderers;
    List<MeshRenderer> meshRenderers
    {
        get
        {
            if (_meshRenderers == null)
                SearchChildMeshRenderers();
            return _meshRenderers;
        }
    }

    public PlayerController collidingPlayerController { get; private set; }

    public void SetOutline(bool active)
    {
        if (active)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.gameObject.layer = Layers.instance.OutlinePropLayerIndex;
        }
        else
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.gameObject.layer = Layers.instance.PropLayerIndex;
        }
    }

    public virtual void OnOperableChanged() { }

    public virtual void OnBeginFocus()
    {
        if (picked)
            return;

        SetOutline(true);
        picked = true;
    }

    public virtual void OnEndFocus()
    {
        if (!picked)
            return;

        SetOutline(false);
        picked = false;
    }

    protected virtual void OnBeginHandling() { }
    protected virtual void OnEndHandling() { }

    public virtual void BeginHandling()
    {
        if (isHandling)
            return;
        isHandling = true;

        foreach (OperableObject operableObject in childOperableObjects)
        {
            operableObject.isHandling = true;
            operableObject.OnBeginHandling();
        }
    }

    public virtual void EndHandling()
    {
        if (!isHandling)
            return;
        isHandling = false;

        foreach (OperableObject operableObject in childOperableObjects)
        {
            operableObject.isHandling = false;
            operableObject.OnEndHandling();
        }
    }

    public virtual void OnBeginPlayerControllerContact(PlayerController playerController) 
    {
        collidingPlayerController = playerController;
        OnBeginPlayerControllerContactCallback?.Invoke(playerController);
    }

    public virtual void OnEndPlayerControllerContact(PlayerController playerController) 
    {
        collidingPlayerController = null;
        OnEndPlayerControllerContactCallback?.Invoke(playerController);
    }

    public virtual bool Complete()
    {
        if (completed)
            return false;
        completed = true;
        OnCompletedCallback?.Invoke(this);
        return true;
    }

    protected override void Awake()
    {
        base.Awake();

        axisSpin.root = this;
        freeSpin.root = this;
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        axisSpin.OnDrag(eventData);
        freeSpin.OnDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        axisSpin.OnBeginDrag(eventData);
        freeSpin.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        axisSpin.OnEndDrag(eventData);
        freeSpin.OnEndDrag(eventData);
    }
}

[System.Serializable]
public abstract class Spinnable
{
    [HideInInspector]
    public OperableObject root;

    public bool active { get; private set; }

    public Transform transform => root.transform;

    bool started = false;

    public void SetActive(bool active)
    {
        if (this.active == active)
            return;

        this.active = active;
        OnActiveChanged();

        if (this.active && !started)
        {
            started = true;
            Start();
        }
    }

    public abstract void OnActiveChanged();
    public abstract void Start();
    public abstract void OnDrag(PointerEventData eventData);
    public abstract void OnBeginDrag(PointerEventData eventData);
    public abstract void OnEndDrag(PointerEventData eventData);
}

[System.Serializable]
public class AxisSpinnable : Spinnable
{
    public enum Axis { X, Y, Z };

    public event Action<AxisSpinnable> OnAxisSpinned;

    [SerializeField, ReadOnlyInRuntime]
    Axis axis = Axis.X;

    [SerializeField, ReadOnlyInRuntime]
    int faces = 10;

    [SerializeField, ReadOnlyInRuntime]
    int minNumber = 0;

    [SerializeField, ReadOnlyInRuntime]
    int number = 0;

    [SerializeField]
    public float spinWeight = 0.5f;

    [SerializeField]
    public float fixSpinAngularSpeed = 45.0f;

    [SerializeField, ReadOnlyInRuntime]
    Transform pivot;

    [SerializeField, ReadOnly]
    float _cachedAngle = 0;
    float angle
    {
        get => _cachedAngle;
        set
        {
            _cachedAngle = Mathf.Repeat(value, 360);
            switch (axis)
            {
                case Axis.X:
                pivot.localRotation = Quaternion.Euler(_cachedAngle, 0, 0);
                break;

                case Axis.Y:
                pivot.localRotation = Quaternion.Euler(0, _cachedAngle, 0);
                break;

                case Axis.Z:
                pivot.localRotation = Quaternion.Euler(0, 0, _cachedAngle);
                break;
            }
        }
    }

    [SerializeField, ReadOnly]
    int targetNumber = 0;

    Coroutine fixAngleCoroutine = null;

    int maxNumber => minNumber + faces - 1;
    float angleStep => 360.0f / faces;

    public bool isSpinning => number != targetNumber;

    public int GetNumber()
    {
        return number;
    }

    private int RepeatNumber(int inNumber)
    {
        if (inNumber < minNumber)
            inNumber = maxNumber;
        else if (inNumber > maxNumber)
            inNumber = minNumber;
        return inNumber;
    }

    private float CalculateAngle(int inNumber)
    {
        // 입력된 번호를 최소~최대 사이에서 반복되게 합니다.
        inNumber = RepeatNumber(inNumber);
        // 번호에 해당하는 각도를 계산합니다.
        float angleByNumber = angleStep * ((maxNumber + 1) - inNumber);
        return Mathf.Repeat(angleByNumber, 360);
    }

    private int CalculateNumber(float inAngle)
    {
        int targetNumber = (maxNumber + 1) - Mathf.RoundToInt(inAngle / angleStep);
        return RepeatNumber(targetNumber);
    }

    public void SetNumberAndAngle(int inNumber)
    {
        inNumber = RepeatNumber(inNumber);
        angle = CalculateAngle(inNumber);
        number = inNumber;
        targetNumber = inNumber;
        OnAxisSpinned?.Invoke(this);
    }

    public override void OnActiveChanged() { }

    public override void Start()
    {
        switch (axis)
        {
            case Axis.X:
            _cachedAngle = pivot.localEulerAngles.x;
            break;

            case Axis.Y:
            _cachedAngle = pivot.localEulerAngles.y;
            break;

            case Axis.Z:
            _cachedAngle = pivot.localEulerAngles.z;
            break;
        }

        SetNumberAndAngle(CalculateNumber(_cachedAngle));
    }

    void StopFixAngle()
    {
        if (fixAngleCoroutine != null)
        {
            root.StopCoroutine(fixAngleCoroutine);
            fixAngleCoroutine = null;
        }
    }

    private IEnumerator FixAngleCoroutine()
    {
        // 현재 각도와 가장 근접한 면의 각도를 찾습니다.
        float targetAngle = CalculateAngle(targetNumber);
        // 해당 각도로 회전합니다.
        while (!Mathf.Approximately(angle, targetAngle))
        {
            yield return YieldRule.waitForEndOfFrame;
            angle = Mathf.MoveTowardsAngle(
                angle, 
                targetAngle, 
                fixSpinAngularSpeed * Time.deltaTime
                );
        }
        // 해당하는 번호로 재설정합니다.
        SetNumberAndAngle(targetNumber);
    }

    void BeginDrag()
    {
        StopFixAngle();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!active)
            return;

        BeginDrag();
    }

    void EndDrag()
    {
        StopFixAngle();
        fixAngleCoroutine = root.StartCoroutine(FixAngleCoroutine());
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (!active)
            return;

        EndDrag();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!active)
            return;

        bool repeated;
        Rect window;
        // 마우스가 화면 경계에 도달하면 반대쪽으로 이동시킵니다.
        Win32Unity.RepeatCursor(out repeated);
        Win32Unity.GetWindowRect(out window);

        // 스크린 공간에서 드래그하기 위해,
        // 내적에 사용할 월드 공간의 방향 벡터를 계산합니다.
        Vector3 projVec = Vector3.zero;
        switch (axis)
        {
            case Axis.X:
            projVec = Vector3.Cross(transform.forward, Camera.main.transform.right);
            break;

            case Axis.Y:
            projVec = Vector3.Cross(transform.up, Camera.main.transform.forward);
            break;

            case Axis.Z:
            projVec = Vector3.Cross(transform.right, Camera.main.transform.up);
            break;
        }
        projVec.Normalize();
        // 방향 벡터를 스크린 공간으로 변환합니다.
        Vector3 v = Camera.main.worldToCameraMatrix.MultiplyVector(projVec);
        Vector4 p = Camera.main.projectionMatrix.MultiplyVector(v).normalized;
        p.z = 0;
        p.Normalize();
        p = ((Vector3)p).Abs();

        float deltaMultiplier = 1;
        if (axis == Axis.Y)
        {
            deltaMultiplier = -1;
        }
        // 스크린 공간에서 마우스의 드래그 방향과 축 벡터와 내적한 결과를 회전크기로 사용합니다.
        float delta = Vector3.Dot(eventData.delta, p) * spinWeight;

        // 해당 프레임에서의 마우스 이동 거리가 화면의 절반을 넘지 않을 때만 회전합니다.
        // 게임의 프리징 또는 마우스가 반대쪽으로 이동했을 때,
        // 크게 회전하는 현상을 방지하기 위함입니다.
        if (Mathf.Abs(eventData.delta.x) < window.width * 0.5f && 
            Mathf.Abs(eventData.delta.y) < window.height * 0.5f)
        {
            angle += delta * deltaMultiplier;
        }
        // 현재 각도에 해당하는 번호로 설정합니다.
        targetNumber = CalculateNumber(angle);
    }
}

[System.Serializable]
public class FreeSpinnable : Spinnable
{
    public override void OnActiveChanged() { }
    public override void Start() { }
    public override void OnEndDrag(PointerEventData eventData) { }
    public override void OnBeginDrag(PointerEventData eventData) { }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!active)
            return;

        bool repeated;
        Rect window;
        // 마우스가 화면 경계에 도달하면 반대쪽으로 이동시킵니다.
        Win32Unity.RepeatCursor(out repeated);
        Win32Unity.GetWindowRect(out window);

        // 해당 프레임에서의 마우스 이동 거리가 화면의 절반을 넘지 않을 때만 회전합니다.
        // 게임의 프리징 또는 마우스가 반대쪽으로 이동했을 때, 크게 회전하는 현상을 방지하기 위함입니다.
        if (Mathf.Abs(eventData.delta.x) < window.width * 0.5f && Mathf.Abs(eventData.delta.y) < window.height * 0.5f)
        {
            const float sensitivity = 0.25f;
            Vector2 delta = eventData.delta * sensitivity;
            Quaternion rotateYAxis = Quaternion.AngleAxis(-delta.x, Camera.main.transform.up);
            Quaternion rotateXAxis = Quaternion.AngleAxis(delta.y, Camera.main.transform.right);
            transform.rotation = rotateXAxis * rotateYAxis * transform.rotation;
        }
    }
}