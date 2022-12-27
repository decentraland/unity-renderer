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
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                RenderTargetHandle camera = new RenderTargetHandle() { id = Shader.PropertyToID("_OutlinerEffect_Camera") };
                RenderTargetHandle outline1 = new RenderTargetHandle() { id = Shader.PropertyToID("_OutlinerEffect_Outline1") };
                RenderTargetHandle outline2 = new RenderTargetHandle() { id = Shader.PropertyToID("_OutlinerEffect_Outline2") };
                cmd.GetTemporaryRT(camera.id, descriptor, FilterMode.Point);
                cmd.GetTemporaryRT(outline1.id, descriptor, FilterMode.Point);
                cmd.GetTemporaryRT(outline2.id, descriptor, FilterMode.Point);

                Blit(cmd, outlineMask.id, outline1.id, material, (int)ShaderPasses.Outline); // Get the outline. Output in outline1
                Blit(cmd, outline1.id, outline2.id, material, (int)ShaderPasses.BlurHorizontal); // Apply Vertical blur. Output in outline2
                Blit(cmd, outline2.id, outline1.id, material, (int)ShaderPasses.BlurVertical); // Apply Horizontal blur. Output in outline1

                Blit(cmd, source, camera.id); // Get camera in a RT
                cmd.SetGlobalTexture("_Source", camera.id); // Apply RT as _Source for the material
                cmd.SetGlobalTexture("_ComposeMask", outlineMask.id); // Set the original outline mask
                Blit(cmd, outline1.id, source, material, (int)ShaderPasses.Compose);

                cmd.ReleaseTemporaryRT(camera.id);
                cmd.ReleaseTemporaryRT(outline1.id);
                cmd.ReleaseTemporaryRT(outline2.id);


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
        public float outlineSize = 0.001f;
        public float blurSize = 0.001f;
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
