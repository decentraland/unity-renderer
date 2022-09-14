using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianBlurHandler : ScriptableRendererFeature
{
    [System.Serializable]
    public class GaussianBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial = null;

        [Range(1,25)]
        public int blurPasses = 1;

        [Range(1, 10)]
        public int downsample = 1;

        public string targetName = "_blurTexture";
    }
    
    [SerializeField]
    public GaussianBlurSettings settings = new GaussianBlurSettings();
    public class GaussianBlurPass : ScriptableRenderPass
    {
        public Material blurMat;
        public int passes;
        public int downsample;
        public string targetName;
        string profilerTag;

        int tmpID1;
        int tmpID2;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;

        private RenderTargetIdentifier source {get; set;}

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public GaussianBlurPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var width = cameraTextureDescriptor.width / downsample;
            var height = cameraTextureDescriptor.height / downsample;

            tmpID1 = Shader.PropertyToID("tmpBlurRT1");
            tmpID2 = Shader.PropertyToID("tmpBlurRT2");

            cmd.GetTemporaryRT(tmpID1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tmpID2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            tmpRT1 = new RenderTargetIdentifier(tmpID1);
            tmpRT2 = new RenderTargetIdentifier(tmpID2);

            ConfigureTarget(tmpRT1);
            ConfigureTarget(tmpRT2);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;


            //first pass
            cmd.SetGlobalFloat("_offset", 1.5f);
            cmd.Blit(source, tmpRT1, blurMat);

            for (int i = 1; i < passes - 1; i++)
            {
                cmd.SetGlobalFloat("_offset", 0.5f + i);
                cmd.Blit(tmpRT1, tmpRT2, blurMat);

                var RTtmp = tmpRT1;
                tmpRT1 = tmpRT2;
                tmpRT2 = RTtmp;
            }

            //final pass
            cmd.SetGlobalFloat("_offset", 0.5f + passes - 1);
            cmd.Blit(tmpRT1, tmpRT2, blurMat);
            cmd.SetGlobalTexture(targetName, tmpRT2);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    GaussianBlurPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new GaussianBlurPass("GaussianBlur");
        scriptablePass.blurMat = settings.blurMaterial;
        scriptablePass.passes = settings.blurPasses;
        scriptablePass.downsample = settings.downsample;
        scriptablePass.targetName = settings.targetName;
        scriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        scriptablePass.Setup(src);
        renderer.EnqueuePass(scriptablePass);
    }
}
