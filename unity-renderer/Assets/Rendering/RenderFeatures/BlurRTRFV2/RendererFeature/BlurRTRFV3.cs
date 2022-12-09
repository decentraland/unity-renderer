using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRTRFV3 : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial = null;

        [Range(1,10)] public int blurPasses = 1;

        [Range(1, 6)] public int downsample = 1;

        [HideInInspector] public string targetName = "_BlurRTTex";

        public bool copyToFramebuffer;
    }

    [SerializeField]
    public Settings settings = new Settings();
    public class BlurtRTPass : ScriptableRenderPass // SRP Pass
    {
        public Material blurMaterial; // Material to use for blurring
        public int passes; // Number of blur passes to perform
        public int downsample; // Downsample factor for blurring

        public bool copyToFramebuffer; // Whether to copy the blurred image to the framebuffer for full screen effect

        public string targetName; // Name of the texture to use as the target for blurring
        string profilerTag; // Tag for the profiler



        int passID1; // Shader pass ID for the first blur pass
        int passID2; // Shader pass ID for the second blur pass

        RenderTargetIdentifier passRT1; // Render target for the first blur pass
        RenderTargetIdentifier passRT2; // Render target for the second blur pass

        private RenderTargetIdentifier source {get; set;} // Source render target

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;

            // here we could pass the identifier of the render texture we want to use as the target for blurring manually
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

        public BlurtRTPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Pass,Create,Set,Configure,Release

            int width = cameraTextureDescriptor.width / downsample; // Calculate the width of the render target
            int height = cameraTextureDescriptor.height / downsample; // Calculate the height of the render target

            passID1 = Shader.PropertyToID("tmpBlurRT1");
            passID2 = Shader.PropertyToID("tmpBlurRT2");

            // create render textures
            cmd.GetTemporaryRT(passID1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(passID2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            passRT1 = new RenderTargetIdentifier(passID1); // new RenderTargetIdentifier(tmpID1, 0, CubemapFace.Unknown, -1);
            passRT2 = new RenderTargetIdentifier(passID2); // new RenderTargetIdentifier(tmpID2, 0, CubemapFace.Unknown, -1);

            ConfigureTarget(passRT1); // set the target to the temporary render texture
            ConfigureTarget(passRT2); // set the target to the temporary render texture
        }

        // Execute the pass
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag); // Get a command buffer from the pool

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor; // Get the camera descriptor
            opaqueDesc.depthBufferBits = 0; // Disable depth buffer


            //blit passes
            cmd.SetGlobalFloat("_offset", 1.5f); // Set the offset for the blur OR ELSE WE GET PIXELS
            cmd.Blit(source, passRT1, blurMaterial); // Blit the source to the temporary render texture

            for (int i = 1; i < passes - 1; i++)
            {
                cmd.SetGlobalFloat("_offset", 0.5f + i);
                cmd.Blit(passRT1, passRT2, blurMaterial);

                // Swap the render targets to give more definition on the blur through the shader pass
                RenderTargetIdentifier RTtmp = passRT1;
                passRT1 = passRT2;
                passRT2 = RTtmp;
            }

            // send to FrameBuffer
            if (copyToFramebuffer == true)
            {

                cmd.SetGlobalFloat("_offset", 0.5f + passes - 1); // global float for shader

                cmd.Blit(passRT1, source, blurMaterial); // blit to framebuffer

                cmd.SetGlobalTexture(targetName, passRT2); // set the global texture

                ExecuteClearRelease(context , cmd); // Clear and release the render targets
            }
            // send to shader
            else if (copyToFramebuffer == false)
            {
                // shader pass
                cmd.SetGlobalFloat("_offset", 0.5f + passes - 1); // global float for shader

                cmd.Blit(passRT1, passRT2, blurMaterial); // last pass for shader

                cmd.SetGlobalTexture(targetName, passRT2); // set the global texture

                ExecuteClearRelease(context , cmd); // Clear and release the render targets
            }

        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // Release the render targets
            cmd.ReleaseTemporaryRT(passID1); // Release the temporary render target
            cmd.ReleaseTemporaryRT(passID2); // Release the temporary render target
        }
    }

    BlurtRTPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new BlurtRTPass("BlurRTRFV3"); // Create the pass

        scriptablePass.blurMaterial = settings.blurMaterial; // Set the material
        scriptablePass.passes = settings.blurPasses; // Set the number of blur passes

        scriptablePass.downsample = settings.downsample; // Set the downsample factor
        scriptablePass.targetName = settings.targetName; // Set the target name

        scriptablePass.renderPassEvent = settings.renderPassEvent; // Set the full screen effect
        scriptablePass.copyToFramebuffer = settings.copyToFramebuffer; // Set the render pass event
    }

    static void ExecuteClearRelease(ScriptableRenderContext context , CommandBuffer cmd)
    {
        context.ExecuteCommandBuffer(cmd);  // Execute the command buffer

        cmd.Clear(); // Clear the command buffer

        CommandBufferPool.Release(cmd); // Release the command buffer
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget; // Get the camera color target

        scriptablePass.Setup(src); // Setup the pass
        renderer.EnqueuePass(scriptablePass); // Enqueue the pass
    }
}
