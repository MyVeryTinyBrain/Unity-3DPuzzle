using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
[VolumeComponentMenuForRenderPipeline("Custom/OutlinePostProcess", typeof(UniversalRenderPipeline))]
public class OutlinePostProcess : VolumeComponent, IPostProcessComponent
{
    public ColorParameter OutlineColor = new ColorParameter(Color.white);
    public FloatParameter OutlineScale = new FloatParameter(1.5f);
    public FloatParameter RobertCrossMultiplier = new FloatParameter(100.0f);
    public FloatParameter DepthThreshold = new FloatParameter(15.0f);
    public FloatParameter NormalThreshold = new FloatParameter(0.4f);
    public FloatParameter SteepAngleThreshold = new FloatParameter(0.2f);
    public FloatParameter SteepAngleMultiplier = new FloatParameter(25.0f);
    public BoolParameter RemoveCenter = new BoolParameter(false);

    public bool IsActive() => true;

    public bool IsTileCompatible() => true;
}
