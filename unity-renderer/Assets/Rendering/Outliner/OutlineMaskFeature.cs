using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineMaskFeature : ScriptableRendererFeature
{
    private class OutlinerRenderPass : ScriptableRenderPass
    {
        private const int DEPTH_BUFFER_BITS = 32;
        private const string PROFILER_TAG = "Outliner Mask Pass";

        private readonly OutlineRenderers renderers;
        private readonly Material material;

        private RenderTargetHandle outlineTextureHandle;
        private RenderTextureDescriptor descriptor;

        public OutlinerRenderPass(OutlineRenderers renderers)
        {
            material = CoreUtils.CreateEngineMaterial("Hidden/DCL/OutlineMaskPass");
            this.renderers = renderers;
        }

        public void Setup(RenderTextureDescriptor descriptor, RenderTargetHandle outlineTextureHandle)
        {
            this.outlineTextureHandle = outlineTextureHandle;
            descriptor.colorFormat = RenderTextureFormat.ARGB32;
            descriptor.depthBufferBits = DEPTH_BUFFER_BITS;
            this.descriptor = descriptor;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //Configure CommandBuffer to output the mask result in the provided texture
            cmd.GetTemporaryRT(outlineTextureHandle.id, descriptor, FilterMode.Point);
            ConfigureTarget(outlineTextureHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);

            using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                if (renderers?.renderers != null)
                {
                    foreach ((Renderer renderer, int meshCount) in renderers.renderers)
                    {
                        //Ignore disabled renderers
                        if (!renderer.gameObject.activeSelf)
                            continue;

                        // We have to manually render all the submeshes of the selected objects.
                        for (var i = 0; i < meshCount; i++) { cmd.DrawRenderer(renderer, material, i); }
                    }
                }

                cmd.SetGlobalTexture("_OutlineTexture", outlineTextureHandle.id);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(outlineTextureHandle.id);
        }
    }

    public OutlineRenderers renderers;
    private OutlinerRenderPass scriptablePass;
    private RenderTargetHandle outlineTexture;

    public override void Create()
    {
        scriptablePass = new OutlinerRenderPass(renderers)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPrePasses,
        };

        outlineTexture.Init("_OutlineTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        scriptablePass.Setup(renderingData.cameraData.cameraTargetDescriptor, outlineTexture);
        renderer.EnqueuePass(scriptablePass);
    }
}
