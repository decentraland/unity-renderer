using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BlurRT
{
    // blur settings for the renderer // class for the renderer
    public class BlurRTRF : ScriptableRendererFeature
    {
        [SerializeField] private BlurSettings settings = new BlurSettings(); // settings for the renderer

        // instance parameters
        [System.Serializable]
        public class BlurSettings
        {
            public Material blurMaterial; // material for the blur
            [Range(0, 5)] public int blurPasses = 5; // number of blur passes // max to 5 for compatibility
            [Range(1, 4)] public int downSample = 2; // downsample amount // max to 4 for compatibility

            //public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox; // render pass event
            public BlurType blurType = BlurType.UI; // blur type

            public enum BlurType // blur type enum
            {
                Scene3D ,
                UI
            }
        }

        // get data to renderer
        private class CustomRenderPass : ScriptableRenderPass
        {
            public Material BlurMaterial; // material for the blur
            public string Name, ProfilerTag; // name and profiler tag
            public int Passes, Downsample; // number of blur passes and downsample amount

            private RenderTargetIdentifier[] renderTargetIdentifiers = new RenderTargetIdentifier[2]; // render target identifiers

            private RenderTargetIdentifier source; // source render target identifier

            // source
            public void Setup(RenderTargetIdentifier source)
            {
                this.source = source; // set source
            }

            // Blur area
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                for (int i = 0; i < 2; i++)
                {
                    int id = Shader.PropertyToID("BlurURP" + Random.Range(int.MinValue, int.MaxValue)); // get random id

                    //cmd.GetTemporaryRT(id, cameraTextureDescriptor.width / Downsample, cameraTextureDescriptor.height / Downsample, 0, FilterMode.Bilinear, RenderTextureFormat.Default); //ARGB32 // get temporary render texture
                    cmd.GetTemporaryRT(id, cameraTextureDescriptor.width / Downsample, cameraTextureDescriptor.height / Downsample); //ARGB32 // get temporary render texture

                    renderTargetIdentifiers[i] = new RenderTargetIdentifier(id); // set render target identifier

                    ConfigureTarget(renderTargetIdentifiers[i]); // configure target
                }
            }

            // Blurring
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag); // get command buffer
                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor; // get camera target descriptor
                opaqueDesc.depthBufferBits = -1; // set depth buffer bits to 0

                cmd.SetGlobalFloat("_offset", 1.5f); // set offset
                cmd.Blit(source, renderTargetIdentifiers[0], BlurMaterial); // blit source to render target identifier 0

                // blur passes
                for (int i = 0; i < Passes; i++)
                {
                    cmd.SetGlobalFloat("_offset", 1.5f + i); // set offset
                    cmd.Blit(renderTargetIdentifiers[0], renderTargetIdentifiers[1], BlurMaterial); // blit render target identifier 0 to render target identifier 1

                    RenderTargetIdentifier rttmp = renderTargetIdentifiers[0]; // get render target identifier 0

                    renderTargetIdentifiers[0] = renderTargetIdentifiers[1]; // set render target identifier 0 to render target identifier 1
                    renderTargetIdentifiers[1] = rttmp; // set render target identifier 1 to render target identifier 0 for the pass
                }

                cmd.SetGlobalFloat("_offset", Passes - 0.5f); // set offset

                cmd.Blit(renderTargetIdentifiers[0], renderTargetIdentifiers[1], BlurMaterial); // blit render target identifier 0 to render target identifier 1

                cmd.SetGlobalTexture(Name, renderTargetIdentifiers[1]); // set global texture

                context.ExecuteCommandBuffer(cmd); // execute command buffer

                cmd.Clear(); // clear command buffer

                CommandBufferPool.Release(cmd); // release command buffer
            }
        }

        // Instance of the render pass
        private CustomRenderPass scriptablePass;

        // URP render pass
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            scriptablePass.Setup(renderer.cameraColorTarget); // source texture

            renderer.EnqueuePass(scriptablePass); // enqueue pass

        }

        // instance parameters
        public override void Create()
        {
            scriptablePass = new CustomRenderPass();

            if (settings.blurType == BlurSettings.BlurType.Scene3D)
            {
                scriptablePass.Name = "_LitBlurTexture";
            }

            else if (settings.blurType == BlurSettings.BlurType.UI)
            {
                scriptablePass.Name = "_UnlitBlurTexture";
                scriptablePass.ProfilerTag = "BlurRTRF";

                scriptablePass.BlurMaterial = settings.blurMaterial;
                scriptablePass.Passes = settings.blurPasses;

                scriptablePass.Downsample = settings.downSample;
                scriptablePass.renderPassEvent = settings.renderPassEvent;
            }

        }
    }
}
