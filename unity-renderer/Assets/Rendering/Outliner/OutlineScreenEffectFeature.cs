// unset:none
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineScreenEffectFeature : ScriptableRendererFeature
{
    private class OutlinePass : ScriptableRenderPass
    {
        private static readonly int _DeltaX = Shader.PropertyToID("_DeltaX");
        private static readonly int _DeltaY = Shader.PropertyToID("_DeltaY");
        private static readonly int _OutlineColor = Shader.PropertyToID("_OutlineColor");

        private readonly OutlineSettings settings;
        private const string PROFILER_TAG = "Outline Screen Effect Pass";
        private readonly Material material = null;

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        private RenderTargetHandle outlineTexture { get; set; }
        private RenderTargetHandle finalOutput;

        public OutlinePass(OutlineSettings settings)
        {
            this.settings = settings;
            material = CoreUtils.CreateEngineMaterial("Hidden/DCL/SobelFilter");
        }

        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination, RenderTargetHandle outlineTexture)
        {
            this.source = source;
            this.destination = destination;
            this.outlineTexture = outlineTexture;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            material.SetColor(_OutlineColor, settings.color);
            material.SetFloat(_DeltaX, settings.deltaX);
            material.SetFloat(_DeltaY, settings.deltaY);
            CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);

            using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDescriptor.depthBufferBits = 0;
                cmd.GetTemporaryRT(finalOutput.id, opaqueDescriptor, FilterMode.Point);
                Blit(cmd, source, finalOutput.Identifier());
                cmd.SetGlobalTexture("_Source", finalOutput.Identifier());
                Blit(cmd, outlineTexture.Identifier(), source, material);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(finalOutput.id);
        }
    }

    [System.Serializable]
    public class OutlineSettings
    {
        [Range(0, 0.05f)] public float deltaX = 0.001f;
        [Range(0, 0.05f)] public float deltaY = 0.001f;
        public Color color = Color.cyan;
    }

    public OutlineSettings settings = new ();
    private OutlinePass outlinePass;
    private RenderTargetHandle outlineTexture;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRendering,
        };

        outlineTexture.Init("_OutlineTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        outlinePass.Setup(renderer.cameraColorTarget, RenderTargetHandle.CameraTarget, outlineTexture);
        renderer.EnqueuePass(outlinePass);
    }
}
