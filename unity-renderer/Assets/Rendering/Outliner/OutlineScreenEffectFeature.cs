using DCL;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineScreenEffectFeature : ScriptableRendererFeature
{
    private class OutlinePass : ScriptableRenderPass
    {
        private static readonly int INNER_COLOR = Shader.PropertyToID("_InnerColor");
        private static readonly int OUTLINE_COLOR = Shader.PropertyToID("_OutlineColor");
        private static readonly int OUTLINE_SIZE = Shader.PropertyToID("_OutlineSize");
        private static readonly int BLUR_COLOR = Shader.PropertyToID("_BlurColor");
        private static readonly int BLUR_SIZE = Shader.PropertyToID("_BlurSize");
        private static readonly int SIGMA = Shader.PropertyToID("_BlurSigma");
        private static readonly int FADE = Shader.PropertyToID("_Fade");
        private static readonly int OUTLINER_EFFECT = Shader.PropertyToID("_OutlinerEffect_Camera");
        private static readonly int OUTLINE_HORIZONTAL = Shader.PropertyToID("_OutlinerEffect_Outline1");
        private static readonly int OUTLINE_VERTICAL = Shader.PropertyToID("_OutlinerEffect_Outline2");

        private readonly OutlineSettings settings;
        private const string PROFILER_TAG = "Outline Screen Effect Pass";
        private readonly Material material = null;

        private ScriptableRenderer renderer { get; set; }
        private RTHandle destination { get; set; }
        private RTHandle outlineMask { get; set; }

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

        public void Setup(ScriptableRenderer renderer, RTHandle outlineTexture)
        {
            this.renderer = renderer;
            this.destination = k_CameraTarget;
            this.outlineMask = outlineTexture;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!IsOutlineAvailable()) return;

            material.SetColor(INNER_COLOR, settings.innerColor);
            material.SetColor(OUTLINE_COLOR, settings.outlineColor);
            material.SetFloat(OUTLINE_SIZE, settings.outlineThickness);
            material.SetColor(BLUR_COLOR, settings.blurColor);
            material.SetFloat(BLUR_SIZE, settings.blurSize);
            material.SetFloat(SIGMA, settings.blurSigma);
            material.SetFloat(FADE, settings.effectFade);
            CommandBuffer cmd = CommandBufferPool.Get(PROFILER_TAG);

            using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                RenderTextureDescriptor lowResDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                RenderTextureDescriptor mainDescriptor = renderingData.cameraData.cameraTargetDescriptor;

                // For high resolutions we dont need so much quality and the blur effect gets exponentially expensive so we lower the resolution to full-hd (1920x1080) but maintaining the aspect ratio
                float resolutionScale = 1;

                if (lowResDescriptor.width > 1920)
                    resolutionScale = (1920f * 100f / lowResDescriptor.width) / 100f;

                lowResDescriptor.width = Mathf.RoundToInt(lowResDescriptor.width * resolutionScale);
                lowResDescriptor.height = Mathf.RoundToInt(lowResDescriptor.height * resolutionScale);
                lowResDescriptor.depthBufferBits = 0;

                RTHandle camera = RTHandles.Alloc("_OutlinerEffect_Camera", "_OutlinerEffect_Camera");
                RTHandle outline1 = RTHandles.Alloc("_OutlinerEffect_Outline1", "_OutlinerEffect_Outline1");
                RTHandle outline2 = RTHandles.Alloc("_OutlinerEffect_Outline2", "_OutlinerEffect_Outline2");

                cmd.GetTemporaryRT(OUTLINER_EFFECT, mainDescriptor, FilterMode.Point);
                cmd.GetTemporaryRT(OUTLINE_HORIZONTAL, lowResDescriptor, (FilterMode)settings.filterMode);
                cmd.GetTemporaryRT(OUTLINE_VERTICAL, lowResDescriptor, (FilterMode)settings.filterMode);

                Blit(cmd, outlineMask, camera, material, (int)ShaderPasses.Outline); // Get the outline. Output in outline1
                Blit(cmd, outline1, outline2, material, (int)ShaderPasses.BlurHorizontal); // Apply Vertical blur. Output in outline2
                Blit(cmd, outline2, outline1, material, (int)ShaderPasses.BlurVertical); // Apply Horizontal blur. Output in outline1

                Blit(cmd, renderer.cameraColorTargetHandle, camera); // Get camera in a RT
                cmd.SetGlobalTexture("_Source", OUTLINER_EFFECT); // Apply RT as _Source for the material
                cmd.SetGlobalTexture("_ComposeMask", Shader.PropertyToID(outlineMask.name)); // Set the original outline mask

                Blit(cmd, outline1, renderer.cameraColorTargetHandle, material, (int)ShaderPasses.Compose);

                cmd.ReleaseTemporaryRT(OUTLINER_EFFECT);
                cmd.ReleaseTemporaryRT(OUTLINE_HORIZONTAL);
                cmd.ReleaseTemporaryRT(OUTLINE_VERTICAL);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private static bool IsOutlineAvailable() =>
            DataStore.i.outliner.avatarOutlined.Get().renderer != null;

        public override void FrameCleanup(CommandBuffer cmd) { }
    }

    [System.Serializable]
    public class OutlineSettings
    {
        public float outlineThickness;
        public float blurSize;
        public float blurSigma;
        public Color outlineColor = Color.cyan;
        public Color blurColor = Color.cyan;
        public Color innerColor = Color.cyan;
        [Range(0, 1)] public float effectFade = 1;

        public int filterMode;
    }

    public OutlineSettings settings = new ();
    private OutlinePass outlinePass;
    private RTHandle outlineTexture;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
        };

        outlineTexture = RTHandles.Alloc("_OutlineTexture", "_OutlineTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        outlinePass.Setup(renderer, outlineTexture);
        renderer.EnqueuePass(outlinePass);
    }
}
