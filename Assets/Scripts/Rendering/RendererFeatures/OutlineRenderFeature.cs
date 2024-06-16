using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public partial class OutlineRenderFeature : ScriptableRendererFeature { }

partial class OutlineRenderFeature
{
    [SerializeField]
    RenderPassEvent inputRenderPassEvent = RenderPassEvent.AfterRenderingSkybox;

    [SerializeField]
    RenderPassEvent outputRenderPassEvent = RenderPassEvent.AfterRenderingSkybox;

    [SerializeField]
    LayerMask layerMask;

    NormalWritePass normalWritePass;
    OutlinePass outlinePass;
    NormalDebugPass normalDebugPass;

    public const string NormalWriteShaderName = "Hidden/OutlinePostProcessing_NormalWrite";
    public const string NormalOccludeShaderName = "Hidden/OutlinePostProcessing_NormalOcclude";
    public const string OutlineShaderName = "Hidden/OutlinePostProcessing";
    public const string NormalRTName = "_NormalRT";
    public static readonly int NormalRTID = Shader.PropertyToID(NormalRTName);
    public const string TemporaryRTName = "_TemporaryRT";
    public static readonly int TemporaryRTID = Shader.PropertyToID(TemporaryRTName);
}

partial class OutlineRenderFeature
{
    public override void Create()
    {
        normalWritePass = new NormalWritePass(inputRenderPassEvent, layerMask);
        outlinePass = new OutlinePass(outputRenderPassEvent);
        normalDebugPass = new NormalDebugPass(outputRenderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(normalWritePass);
        renderer.EnqueuePass(outlinePass);
        //renderer.EnqueuePass(normalDebugPass);
    }
}

partial class OutlineRenderFeature
{
    class NormalWritePass : ScriptableRenderPass
    {
        RenderTargetHandle normalRT;
        Material normalWriteMaterial;
        Material normalOccludeMaterial;
        FilteringSettings normalWriteFilteringSettings;
        FilteringSettings normalOccludeFilteringSettings;
        List<ShaderTagId> shaderTagIdList;

        public NormalWritePass(RenderPassEvent renderPassEvent, LayerMask layerMask)
        {
            this.renderPassEvent = renderPassEvent;
            this.normalRT.Init(NormalRTName);
            this.normalWriteMaterial = new Material(Shader.Find(NormalWriteShaderName));
            this.normalOccludeMaterial = new Material(Shader.Find(NormalOccludeShaderName));
            this.normalWriteFilteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            this.normalOccludeFilteringSettings = new FilteringSettings(RenderQueueRange.opaque, ~layerMask);

            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/urp-shaders/urp-shaderlab-pass-tags.html
            this.shaderTagIdList = new List<ShaderTagId> {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor doodleTextureDescriptor = cameraTextureDescriptor;
            doodleTextureDescriptor.colorFormat = cameraTextureDescriptor.colorFormat;
            doodleTextureDescriptor.depthBufferBits = cameraTextureDescriptor.depthBufferBits;
            FilterMode filterMode = FilterMode.Point;
            cmd.GetTemporaryRT(normalRT.id, doodleTextureDescriptor, filterMode);

            ConfigureTarget(normalRT.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            VolumeStack volumeStack = VolumeManager.instance.stack;
            OutlinePostProcess outlinePostProcess = volumeStack.GetComponent<OutlinePostProcess>();

            if (!outlinePostProcess)
                return;

            if (!outlinePostProcess.IsActive())
                return;

            if (!normalWriteMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("NormalWritePassSampler")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                DrawingSettings drawingSettings = CreateDrawingSettings(
                    shaderTagIdList, ref renderingData, 
                    renderingData.cameraData.defaultOpaqueSortFlags
                    );
                drawingSettings.overrideMaterial = normalWriteMaterial;
                DrawingSettings occludeSettings = drawingSettings;
                occludeSettings.overrideMaterial = normalOccludeMaterial;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref normalWriteFilteringSettings);
                context.DrawRenderers(renderingData.cullResults, ref occludeSettings, ref normalOccludeFilteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(normalRT.id);
        }
    }
}

partial class OutlineRenderFeature
{
    class OutlinePass : ScriptableRenderPass
    {
        RenderTargetIdentifier temporaryRT;
        RenderTargetIdentifier cameraColorRT;
        Material outlineMaterial;

        public OutlinePass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor temporaryTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            temporaryTargetDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(TemporaryRTID, temporaryTargetDescriptor, FilterMode.Bilinear);
            temporaryRT = new RenderTargetIdentifier(TemporaryRTID);

            cameraColorRT = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            VolumeStack volumeStack = VolumeManager.instance.stack;
            OutlinePostProcess outlinePostProcess = volumeStack.GetComponent<OutlinePostProcess>();

            if (!outlinePostProcess)
                return;

            if (!outlinePostProcess.IsActive())
                return;

            outlineMaterial = new Material(Shader.Find(OutlineShaderName));
            outlineMaterial.SetColor("_OutlineColor", outlinePostProcess.OutlineColor.value);
            outlineMaterial.SetFloat("_OutlineScale", outlinePostProcess.OutlineScale.value * Mathf.Min(Screen.width, Screen.height) * 0.001f);
            outlineMaterial.SetFloat("_RobertCrossMultiplier", outlinePostProcess.RobertCrossMultiplier.value);
            outlineMaterial.SetFloat("_DepthThreshold", outlinePostProcess.DepthThreshold.value);
            outlineMaterial.SetFloat("_NormalThreshold", outlinePostProcess.NormalThreshold.value);
            outlineMaterial.SetFloat("_SteepAngleThreshold", outlinePostProcess.SteepAngleThreshold.value);
            outlineMaterial.SetFloat("_SteepAngleMultiplier", outlinePostProcess.SteepAngleMultiplier.value);
            outlineMaterial.SetFloat("_RemoveCenter", outlinePostProcess.RemoveCenter.value ? 1.0f : 0.0f);

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("OutlinePassSampler")))
            {
                Blit(cmd, cameraColorRT, temporaryRT);
                Blit(cmd, temporaryRT, cameraColorRT, outlineMaterial);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(TemporaryRTID);
        }
    }
}

partial class OutlineRenderFeature
{
    class NormalDebugPass : ScriptableRenderPass
    {
        RenderTargetIdentifier normalRT;
        RenderTargetIdentifier cameraColorRT;

        public NormalDebugPass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            normalRT = new RenderTargetIdentifier(NormalRTID);
            cameraColorRT = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("UnnamedPassSampler")))
            {
                Blit(cmd, normalRT, cameraColorRT);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(NormalRTID);
        }
    }
}