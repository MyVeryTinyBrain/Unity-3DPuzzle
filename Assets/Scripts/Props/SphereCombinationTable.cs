using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class SphereCombinationTable : Prop
{
    [SerializeField, ReadOnlyInRuntime]
    List<MarkedSphereSocket> sockets;

    [SerializeField, ReadOnlyInRuntime]
    string combination;

    [SerializeField]
    float EventDelay = 0.3f;

    [SerializeField]
    UnityEvent MatchedEvent;

    int validAttaches = 0;
    bool correctFit = false;
    StringBuilder currentCombination;

    private void OnValidSphereAttachedCallback(MarkedSphereSocket socket)
    {
        validAttaches++;
        if(validAttaches == sockets.Count && !correctFit)
        {
            correctFit = true;
            CorrectFit();
        }
    }

    private void OnValidSphereDetachedCallback(MarkedSphereSocket socket)
    {
        validAttaches--;
    }

    private void OnSphereSpinned(AxisSpinnable spinnable)
    {
        MarkedSphere sphere = spinnable.root as MarkedSphere;
        currentCombination[sphere.socketIndex] = sphere.axisSpin.GetNumber().ToString()[0];
        if(currentCombination.ToString() == combination)
        {
            Complete();
        }
    }

    private void CorrectFit()
    {
        currentCombination = new StringBuilder();
        for (int i = 0; i < sockets.Count; ++i)
            currentCombination.Append('*');

        foreach (MarkedSphereSocket socket in sockets)
        {
            socket.Complete();
            MarkedSphere sphere = socket.itemObject as MarkedSphere;
            sphere.axisSpin.OnAxisSpinned += OnSphereSpinned;
            currentCombination[sphere.socketIndex] = sphere.axisSpin.GetNumber().ToString()[0];
        }
    }

    private IEnumerator ComplteCoroutine()
    {
        yield return new WaitForSeconds(EventDelay);

        MatchedEvent.Invoke();
    }

    public override bool Complete()
    {
        if (!base.Complete())
            return false;

        foreach (MarkedSphereSocket socket in sockets)
        {
            MarkedSphere sphere = socket.itemObject as MarkedSphere;
            sphere.axisSpin.SetActive(false);
            sphere.pickable = false;
            sphere.GlowOnce(2.0f, 2.0f);
        }

        StartCoroutine(ComplteCoroutine());
        return true;
    }

    protected override void Awake()
    {
        base.Awake();

        foreach (MarkedSphereSocket socket in sockets)
        {
            socket.OnValidSphereAttachedCallback += OnValidSphereAttachedCallback;
            socket.OnValidSphereDetachedCallback += OnValidSphereDetachedCallback;
        }
    }
}
