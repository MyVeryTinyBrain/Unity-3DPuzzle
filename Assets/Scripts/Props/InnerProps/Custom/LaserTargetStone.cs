using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTargetStone : LaserTarget
{
    [SerializeField]
    MeshRenderer stone;

    public float onStateEmissionWeight = 50;
    float offStateEmissionWeight;

    protected override void Awake()
    {
        base.Awake();

        offStateEmissionWeight = stone.material.GetFloat("_EmissionWeight");
    }

    public override void OnActived()
    {
        base.OnActived();

        stone.material.SetFloat("_EmissionWeight", onStateEmissionWeight);

        Debug.Log("active");
    }

    public override void OnDeactived()
    {
        base.OnDeactived();

        stone.material.SetFloat("_EmissionWeight", offStateEmissionWeight);

        Debug.Log("deactive");
    }
}
