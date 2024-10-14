using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URP_Feature : ScriptableRendererFeature
{
    class ViewNormalsTexturePass : ScriptableRenderPass
    {
        private readonly List<ShaderTagId> shaderIdTagList;
        private readonly RenderTargetHandle normals;
        private readonly Material normalsMaterial;

        public ViewNormalsTexturePass(RenderPassEvent renderPassEvent)
        {
            normalsMaterial = new Material(Shader.Find("Shader Graphs/ViewSpaceNormalsShader"));
            shaderIdTagList = new List<ShaderTagId>()
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };
            this.renderPassEvent = renderPassEvent;
            normals.Init("_SceneViewSpaceNormals");
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);

            cmd.GetTemporaryRT(normals.id, cameraTextureDescriptor, FilterMode.Point);
            ConfigureTarget(normals.Identifier());
            ConfigureClear(ClearFlag.All, Color.white);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!normalsMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderIdTagList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = normalsMaterial;
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(normals.id);
        }
    }
    class ScreenSpaceOutlinesPass : ScriptableRenderPass
    {
        private readonly Material screenSpaceOutlineMaterial;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetIdentifier temporaryBuffer;
        private int temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");
        public ScreenSpaceOutlinesPass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
            screenSpaceOutlineMaterial = new Material(Shader.Find("Shader Graphs/OutlineShader"));
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!screenSpaceOutlineMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                Blit(cmd, cameraColorTarget, temporaryBuffer);
                Blit(cmd, temporaryBuffer, cameraColorTarget, screenSpaceOutlineMaterial);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
    class FogPass : ScriptableRenderPass
    {
        private readonly Material fogMaterial;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetIdentifier temporaryBuffer2;
        private int temporaryBufferID2 = Shader.PropertyToID("_TemporaryBuffer2");
        public FogPass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
            fogMaterial = new Material(Shader.Find("Shader Graphs/FogShader"));
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!fogMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Fog")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                Blit(cmd, cameraColorTarget, temporaryBuffer2);
                Blit(cmd, temporaryBuffer2, cameraColorTarget, fogMaterial);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    [SerializeField]
    private RenderPassEvent renderPassEvent;
    ViewNormalsTexturePass viewNormalsTexturePass;
    ScreenSpaceOutlinesPass screenSpaceOutlinesPass;
    FogPass fogPass;

    /// <inheritdoc/>
    public override void Create()
    {
        viewNormalsTexturePass = new ViewNormalsTexturePass(renderPassEvent);
        viewNormalsTexturePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        screenSpaceOutlinesPass = new ScreenSpaceOutlinesPass(renderPassEvent);
        screenSpaceOutlinesPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        fogPass = new FogPass(renderPassEvent);
        fogPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewNormalsTexturePass);
        renderer.EnqueuePass(screenSpaceOutlinesPass);
        renderer.EnqueuePass(fogPass);
    }
}


