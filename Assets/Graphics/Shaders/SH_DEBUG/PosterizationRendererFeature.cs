using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PosterizationRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        [Range(2, 16)] public float steps = 4f;
        [Range(0.5f, 2f)] public float saturation = 1f;
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new Settings();
    private PosterizationPass pass;

    public override void Create()
    {
        pass = new PosterizationPass(settings);
        pass.renderPassEvent = settings.passEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // No aplicar en Scene view ni en previews
        if (renderingData.cameraData.cameraType == CameraType.Preview) return;
        if (settings.material == null) return;

        renderer.EnqueuePass(pass);
    }
}

public class PosterizationPass : ScriptableRenderPass
{
    private PosterizationRendererFeature.Settings settings;
    private RTHandle tempRT;

    public PosterizationPass(PosterizationRendererFeature.Settings s) => settings = s;

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_PosterizationTemp");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings.material == null) return;

        // No aplicar en cámaras de reflejo o preview
        if (renderingData.cameraData.cameraType == CameraType.Preview ||
            renderingData.cameraData.cameraType == CameraType.Reflection) return;

        settings.material.SetFloat("_Steps", settings.steps);
        settings.material.SetFloat("_Saturation", settings.saturation);

        CommandBuffer cmd = CommandBufferPool.Get("Posterization");
        RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

        Blitter.BlitCameraTexture(cmd, source, tempRT, settings.material, 0);
        Blitter.BlitCameraTexture(cmd, tempRT, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }
}