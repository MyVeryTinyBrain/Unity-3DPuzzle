using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CombinationLock : Prop
{
    [SerializeField, ReadOnlyInRuntime]
    PolygonalDialCombination combination;

    [SerializeField, ReadOnlyInRuntime]
    Animator animator;
    
    [SerializeField, ReadOnlyInRuntime]
    new Rigidbody rigidbody;

    [SerializeField]
    float unlockDelay = 0.2f;

    [SerializeField]
    float activeRigidbodyDelay = 0.5f;

    [SerializeField]
    float unlockAngularVelocity = 45;

    [SerializeField, ReadOnlyInRuntime]
    string UnlockTrigger = "Unlock";

    [SerializeField]
    UnityEvent unlockedEvent;

    protected void OnMatched(PolygonalDialCombination combination)
    {
        Complete();
    }

    public override bool Complete()
    {
        if (!base.Complete())
            return false;

        this.pickable = false;
        foreach (Prop innerProp in innerProps)
        {
            innerProp.pickable = false;
        }
        
        StartCoroutine(UnlockCoroutine());
        return true;
    }

    private IEnumerator UnlockCoroutine()
    {
        yield return new WaitForSeconds(unlockDelay);
        animator.SetTrigger(UnlockTrigger);

        yield return new WaitForSeconds(activeRigidbodyDelay);
        rigidbody.isKinematic = false;
        rigidbody.AddTorque(transform.right * unlockAngularVelocity * Random.Range(1.0f, 2.0f), ForceMode.Acceleration);
        unlockedEvent.Invoke();
    }

    protected override void Awake()
    {
        base.Awake();

        if (combination)
            combination.OnMatchedCallback += OnMatched;
    }

    public override void BeginHandling()
    {
        base.BeginHandling();

        foreach (Prop innerProp in innerProps)
        {
            PolygonalDial dial = innerProp as PolygonalDial;
            if (dial)
            {
                dial.axisSpin.SetActive(true);
            }
        }
    }

    public override void EndHandling()
    {
        base.EndHandling();

        foreach (Prop innerProp in innerProps)
        {
            PolygonalDial dial = innerProp as PolygonalDial;
            if (dial)
            {
                dial.axisSpin.SetActive(false);
            }
        }
    }
}
