// unset:none
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineScreenEffectFeature : ScriptableRendererFeature
{
    private class OutlinePass : ScriptableRenderPass
    {
        private static readonly int _InnerColor = Shader.PropertyToID("_InnerColor");
        private static readonly int _OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int _OutlineSize = Shader.PropertyToID("_OutlineSize");
        private static readonly int _BlurColor = Shader.PropertyToID("_BlurColor");
        private static readonly int _BlurSize = Shader.PropertyToID("_BlurSize");
        private static readonly int _Sigma = Shader.PropertyToID("_BlurSigma");
        private static readonly int _Fade = Shader.PropertyToID("_Fade");

        private readonly OutlineSettings settings;
        private const string PROFILER_TAG = "Outline Screen Effect Pass";
        private readonly Material material = null;

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        private RenderTargetHandle outlineMask { get; set; }

        private enum ShaderPasses
        {
            Outline = 0,
            BlurHorizontal = 1,
            BlurVertical = 2,
            Compose = 3
        }

        public OutlinePass(OutlineSettings settings)
        {
            this.settings = settings;
            material = CoreUtils.CreateEngineMaterial("DCL/OutlinerEffect");
        }

        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination, RenderTargetHandle outlineTexture)
        {
            this.source = source;
            this.destination = destination;
            this.outlineMask = outlineTexture;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            material.SetColor(_InnerColor, settings.innerColor);
            material.SetColor(_OutlineColor, settings.outlineColor);
            material.SetFloat(_OutlineSize, settings.outlineSize);
            material.SetColor(_BlurColor, settings.blurColor);
            material.SetFloat(_BlurSize, settings.blurSize);
            material.SetFloat(_Sigma, settings.blurSigma);
            material.SetFloat(_Fade, settings.effectFade);
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
        }
    }

    [System.Serializable]
    public class OutlineSettings
    {
        [Range(1, 6)] public float outlineSize = 0.001f;
        [Range(1, 6)] public float blurSize = 0.001f;
        public float blurSigma = 1;
        public Color outlineColor = Color.cyan;
        public Color blurColor = Color.cyan;
        public Color innerColor = Color.cyan;
        [Range(0, 1)] public float effectFade = 1;
    }

    public OutlineSettings settings = new ();
    private OutlinePass outlinePass;
    private RenderTargetHandle outlineTexture;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
        };

        outlineTexture.Init("_OutlineTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        outlinePass.Setup(renderer.cameraColorTarget, RenderTargetHandle.CameraTarget, outlineTexture);
        renderer.EnqueuePass(outlinePass);
    }
}
