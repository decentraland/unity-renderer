
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {

        private string profilingName; // name of the pass
        private Material material; // material to use
        private int materialPassIndex;  // pass index of the material to use
        
        private RenderTargetIdentifier sourceID; // source render target
        private RenderTargetHandle tempTextureHandle; // temporary render target

        public RenderPass(string profilingName, Material material, int passIndex) : base() // constructor
        {
            this.profilingName = profilingName;
            this.material = material;
            this.materialPassIndex = passIndex;
            this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents; // render after transparent objects
        }
        

        public void SetSource(RenderTargetIdentifier source) // set source render target
        {
            this.sourceID = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) // execute the pass
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilingName); // get command buffer

            // create temporary render target
            cmd.GetTemporaryRT(tempTextureHandle.id, renderingData.cameraData.cameraTargetDescriptor);
            Blit(cmd, sourceID, tempTextureHandle.Identifier(), material, materialPassIndex); // blit to temporary render target
            Blit(cmd, tempTextureHandle.Identifier(), sourceID); // blit back to source render target

            context.ExecuteCommandBuffer(cmd); // execute command buffer
            CommandBufferPool.Release(cmd); // release command buffer
        }

        public override void FrameCleanup(CommandBuffer cmd) // cleanup
        {
            if (tempTextureHandle != RenderTargetHandle.CameraTarget) // if temporary render target is not camera target
            {
                cmd.ReleaseTemporaryRT(tempTextureHandle.id); // release temporary render target
                
            }
        }
        
    }

    [System.Serializable]
    public class Settings // settings for the feature
    {
        public Material material = null; // material to use
        public int materialPassIndex = 0; // pass index of the material to use
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }
    

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public Material Material // material to use
    {
        get => settings.material;
        set => settings.material = value;
    }
    

    public override void Create() // create the feature
    
    {
        this.renderPass = new RenderPass(name, settings.material, settings.materialPassIndex);
        renderPass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) // add the pass to the renderer
    {
        if (settings.material == null)
        {
            Debug.LogErrorFormat("Missing material. {0} render pass will not be added. Check for missing reference in the renderer resources.", GetType().Name);
            return;
        }

        renderPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
    
}