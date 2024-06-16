using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerRigidbody : ComponentEx
{
    public event Action<Collider> OnBeginContactCallback;
    public event Action<Collider> OnEndContactCallback;

    [SerializeField, ReadOnlyInRuntime]
    protected CharacterController character;

    [SerializeField]
    public float gravity = -9.81f;

    [SerializeField]
    public float groundCheckEpsilon = 0.1f;

    [SerializeField]
    public LayerMask groundCheckLayerMask;

    [HideInInspector]
    public Vector3 velocity; 

    [SerializeField]
    public float friction = 2.0f; 

    RaycastHit _groundHit;
    public RaycastHit groundHit => _groundHit;

    bool _isGrounded;
    public bool isGrounded => _isGrounded;

    Dictionary<Collider, int> contacts = new Dictionary<Collider, int>();
    Collider[] contactsArray = new Collider[128];
    int beginContactArrayCount = 0;
    Collider[] beginContactArray = new Collider[128];
    int endContactArrayCount = 0;
    Collider[] endContactArray = new Collider[128];

    CapsuleCollider capsuleTrigger;

    public virtual void Move(Vector3 direction, float speed)
    {
        if (_isGrounded)
        {
            // 지면에 접지해 있을 때, 지면에 이동 벡터를 투영합니다.
            // 이를 통해 일정한 속도로 이동할 수 있습니다.
            direction = Vector3.ProjectOnPlane(direction, _groundHit.normal);
        }
        Vector3 delta = direction * speed * Time.deltaTime;
        character.Move(delta);
    }

    public virtual bool Jump(float height)
    {
        if (_isGrounded)
        {
            velocity.y = Mathf.Sqrt(height * -2.0f * gravity);
        }
        return _isGrounded;
    }

    public virtual void CustomMove() { }

    public virtual void OnBeginContact(Collider collider) { }
    public virtual void OnEndContact(Collider collider) { }

    protected override void Awake()
    {
        base.Awake();

        capsuleTrigger = gameObject.AddComponent<CapsuleCollider>();
        capsuleTrigger.isTrigger = true;
    }

    protected override void Update()
    {
        base.Update();

        capsuleTrigger.radius = character.radius;
        capsuleTrigger.height = character.height;

        // 캐릭터 컨트롤러 캡슐의 아래쪽부터 지면을 향한 레이입니다.
        Ray groundRay = new Ray(transform.position - transform.up * ((character.height * 0.5f) - character.radius), -transform.up);
        _isGrounded = Physics.SphereCast(groundRay, character.radius - 0.01f, out _groundHit, groundCheckEpsilon, groundCheckLayerMask.value);
        // 지면과 접촉중일 때, 지면과의 각도가 일정 이상이면 지면 접지로 인식하지 않도록 구현해, 미끄러지도록 합니다.
        if (_isGrounded)
        {
            float angle = Mathf.Acos(Vector3.Dot(Vector3.up, _groundHit.normal)) * Mathf.Rad2Deg;
            if (angle > character.slopeLimit)
            {
                _isGrounded = false;
            }
        }
        // 지면에 접지중일 때, 중력을 비활성화합니다.
        if (_isGrounded && velocity.y < 0)
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, friction * Time.deltaTime);
            // 지면에 완전히 닿기 전에 속도가 0이 되어, 떠 있지 않도록 하는 트릭입니다.
            velocity.y = -2.0f;
        }

        CustomMove();

        if (!_isGrounded)
        {
            // 접지된 콜라이더가 없으면 중력을 적용합니다.
            if (_groundHit.collider == null)
                velocity.y += gravity * Time.deltaTime;
            // 지면과의 각도가 일정 이상이며, 접지된 콜라이더가 있을 땐, 미끄러지도록 합니다.
            else
                velocity += Vector3.ProjectOnPlane(Vector3.up, _groundHit.normal) * gravity * Time.deltaTime;
        }
        // 속도 적용
        character.Move(velocity * Time.deltaTime);

        Vector3 up = transform.position + transform.up * (character.height * 0.5f - character.radius);
        Vector3 down = transform.position - transform.up * (character.height * 0.5f - character.radius);
        int colliderCount = Physics.OverlapCapsuleNonAlloc(up, down, character.radius + 0.05f, contactsArray, groundCheckLayerMask);
        int checkValue = Time.frameCount;
        beginContactArrayCount = 0;
        for(int i = 0; i < colliderCount; i++)
        {
            if(contacts.TryAdd(contactsArray[i], checkValue))
            {
                beginContactArray[beginContactArrayCount++] = contactsArray[i];
            }
            contacts[contactsArray[i]] = checkValue;
        }
        endContactArrayCount = 0;
        foreach(KeyValuePair<Collider, int> pair in contacts)
        {
            if(pair.Value != checkValue)
            {
                endContactArray[endContactArrayCount++] = pair.Key;
            }
        }
        for(int i = 0; i < beginContactArrayCount; ++i)
        {
            OnBeginContact(beginContactArray[i]);
            OnBeginContactCallback?.Invoke(beginContactArray[i]);
        }
        for(int i = 0; i < endContactArrayCount; ++i)
        {
            contacts.Remove(endContactArray[i]);
            OnEndContact(endContactArray[i]);
            OnEndContactCallback?.Invoke(endContactArray[i]);
        }
    }
}
