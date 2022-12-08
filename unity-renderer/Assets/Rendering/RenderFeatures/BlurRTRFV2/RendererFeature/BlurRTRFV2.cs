using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class BlurRTRFV2 : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox; // Render pass event to inject this pass
        public Material blurMaterial = null; // Material to use for blurring

        [Range(2,15)] public int blurPasses = 6; // Number of blur passes to perform

        [Range(1,4)] public int downsample = 2; // Downsample factor for blurring

        [FormerlySerializedAs("copyToFramebuffer")] public bool fullScreenEffect; // Whether to copy the blurred image to the framebuffer

        [HideInInspector]public string targetName = "_BlurRTTex"; // Name of the texture to use as the target for blurring
    }

    public Settings settings = new Settings();

    class CustomRenderPass : ScriptableRenderPass // SRP Pass
    {
        public Material blurMaterial; // Material to use for blurring
        public int passes; // Number of blur passes to perform
        public int downsample; // Downsample factor for blurring

        public bool copyToFramebuffer; // Whether to copy the blurred image to the framebuffer for full screen effect

        public string targetName; // Name of the texture to use as the target for blurring
        string profilerTag; // Tag for the profiler

        int PassId1; // Shader pass ID for the first blur pass
        int PassId2; // Shader pass ID for the second blur pass

        RenderTargetIdentifier passRT1; // Render target for the first blur pass
        RenderTargetIdentifier passRT2; // Render target for the second blur pass

        private RenderTargetIdentifier source { get; set; } // Source render target

        // Constructor
        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
            //this.source = new RenderTargetIdentifier("_CameraColorTexture");
            //this.source = new RenderTargetIdentifier("_CameraDepthTexture");
            //this.source = new RenderTargetIdentifier("_CameraMotionVectorsTexture");
            //this.source = new RenderTargetIdentifier("_CameraOpaqueTexture");
            //this.source = new RenderTargetIdentifier("_CameraTransparentTexture");
            //this.source = new RenderTargetIdentifier("_CameraGBufferTexture0");
            //this.source = new RenderTargetIdentifier("_CameraGBufferTexture1");
            //this.source = new RenderTargetIdentifier("_CameraGBufferTexture2");
            //this.source = new RenderTargetIdentifier("_CameraGBufferTexture3");
            //this.source = new RenderTargetIdentifier("_CameraDepthNormalsTexture");
            //this.source = new RenderTargetIdentifier("_CameraMotionVectorsTexture");



        }

        // Configure the pass
        public CustomRenderPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        // Configure the pass override
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Pass,Create,Set,Configure,Release

            var width = cameraTextureDescriptor.width / downsample; // Calculate the width of the render target
            var height = cameraTextureDescriptor.height / downsample; // Calculate the height of the render target

            // Pass to shader
            PassId1 = Shader.PropertyToID("tmpBlurRT1");
            PassId2 = Shader.PropertyToID("tmpBlurRT2");

            cmd.GetTemporaryRT(PassId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32); // Create the first render target
            cmd.GetTemporaryRT(PassId2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32); // Create the second render target

            passRT1 = new RenderTargetIdentifier(PassId1); // Set the first render target
            passRT2 = new RenderTargetIdentifier(PassId2); // Set the second render target

            ConfigureTarget(passRT1);  // Configure the first render target
            ConfigureTarget(passRT2); // Configure the second render target
        }

        // Execute the pass
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag); // Get the command buffer

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor; // Get the camera target descriptor
            opaqueDesc.depthBufferBits = 0; // Set the depth buffer bits to 0

            // pass A
            //cmd.GetTemporaryRT(PassId1, opaqueDesc, FilterMode.Bilinear);

            cmd.SetGlobalFloat("_offset", 1.5f); // Global shader offset

            cmd.Blit(source, passRT1, blurMaterial); // Blit the source to the first render target

            // blit passes
            for (var i=1; i<passes-1; i++)
            {
                cmd.SetGlobalFloat("_offset", 0.5f + i);
                cmd.Blit(passRT1, passRT2, blurMaterial);

                // Swap the render targets to give more definition on the blur
                RenderTargetIdentifier rtTemp = passRT1;
                passRT1 = passRT2;
                passRT2 = rtTemp;
            }

            // final pass
            cmd.SetGlobalFloat("_offset", 0.5f + passes - 1f);

            // full screen effect
            if (copyToFramebuffer)
            {
                cmd.Blit(passRT1, source, blurMaterial);
            }
            else
            // blit sends to material
            {
                cmd.Blit(passRT1, passRT2, blurMaterial);
                cmd.SetGlobalTexture(targetName, passRT2);
            }

            context.ExecuteCommandBuffer(cmd);  // Execute the command buffer

            cmd.Clear(); // Clear the command buffer

            CommandBufferPool.Release(cmd); // Release the command buffer
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // Release the render targets
            cmd.ReleaseTemporaryRT(PassId1);
            cmd.ReleaseTemporaryRT(PassId2);
        }
    }

    // Create the pass instanc
    CustomRenderPass scriptablePass;

    // On create of the render feature
    public override void Create()
    {
        scriptablePass = new CustomRenderPass("BlurRTRFV2"); // Create the pass

        scriptablePass.blurMaterial = settings.blurMaterial; // Set the material

        scriptablePass.passes = settings.blurPasses; // Set the number of blur passes
        scriptablePass.downsample = settings.downsample; // Set the downsample factor

        scriptablePass.copyToFramebuffer = settings.fullScreenEffect; // Set the full screen effect
        scriptablePass.targetName = settings.targetName; // Set the target name

        scriptablePass.renderPassEvent = settings.renderPassEvent; // Set the render pass event
    }

    // On add of the render feature
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        RenderTargetIdentifier src = renderer.cameraColorTarget; // Get the camera color target

        scriptablePass.Setup(src); // Setup the pass
        renderer.EnqueuePass(scriptablePass); // Enqueue the pass
    }
}


