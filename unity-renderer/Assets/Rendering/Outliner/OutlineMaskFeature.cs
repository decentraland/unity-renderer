using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineMaskFeature : ScriptableRendererFeature
{
    private class OutlinerRenderPass : ScriptableRenderPass
    {
        private const int DEPTH_BUFFER_BITS = 0;
        private const string PROFILER_TAG = "Outliner Mask Pass";
        private const bool USE_BASE_MATERIAL = false; // Use the material in the renderer and look for an "Outliner" pass

        private static readonly int BONE_MATRICES = Shader.PropertyToID("_Matrices");
        private static readonly int BIND_POSES = Shader.PropertyToID("_BindPoses");
        private static readonly int RENDERER_WORLD_INVERSE = Shader.PropertyToID("_WorldInverse");
        private static readonly int AVATAR_MAP1 = Shader.PropertyToID("_AvatarMap1");
        private static readonly int AVATAR_MAP2 = Shader.PropertyToID("_AvatarMap2");
        private static readonly int AVATAR_MAP3 = Shader.PropertyToID("_AvatarMap3");
        private static readonly int AVATAR_MAP4 = Shader.PropertyToID("_AvatarMap4");
        private static readonly int AVATAR_MAP5 = Shader.PropertyToID("_AvatarMap5");
        private static readonly int AVATAR_MAP6 = Shader.PropertyToID("_AvatarMap6");
        private static readonly int AVATAR_MAP7 = Shader.PropertyToID("_AvatarMap7");
        private static readonly int AVATAR_MAP8 = Shader.PropertyToID("_AvatarMap8");
        private static readonly int AVATAR_MAP9 = Shader.PropertyToID("_AvatarMap9");
        private static readonly int AVATAR_MAP10 = Shader.PropertyToID("_AvatarMap10");
        private static readonly int AVATAR_MAP11 = Shader.PropertyToID("_AvatarMap11");
        private static readonly int AVATAR_MAP12 = Shader.PropertyToID("_AvatarMap12");

        private readonly OutlineRenderersSO outlineRenderersSo;
        private readonly Material material;
        private readonly Material gpuSkinningMaterial;

        private RenderTargetHandle outlineTextureHandle;
        private RenderTextureDescriptor descriptor;

        private readonly List<Material> toDispose = new List<Material>();

        public OutlinerRenderPass(OutlineRenderersSO outlineRenderersSo)
        {
            material = CoreUtils.CreateEngineMaterial("Hidden/DCL/OutlineMaskPass");
            gpuSkinningMaterial = CoreUtils.CreateEngineMaterial("Hidden/DCL/OutlineGPUSkinningMaskPass");
            this.outlineRenderersSo = outlineRenderersSo;
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

            //using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                if (outlineRenderersSo != null)
                {
                    //By now only outline avatars
                    //DrawRenderers(outlineRenderersSo?.renderers, renderingData.cameraData.camera.cullingMask, cmd);
                    DrawAvatar(outlineRenderersSo.avatar, renderingData.cameraData.camera.cullingMask, cmd);
                }

                cmd.SetGlobalTexture("_OutlineTexture", outlineTextureHandle.id);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void DrawRenderers(List<(Renderer renderer, int meshCount)> renderers, int cameraCulling, CommandBuffer cmd)
        {
            if (renderers == null)
                return;

            foreach ((Renderer renderer, int meshCount) in renderers)
            {
                //Ignore disabled renderers
                if (!renderer.gameObject.activeSelf || (cameraCulling & (1 << renderer.gameObject.layer)) == 0)
                    continue;

                // We have to manually render all the submeshes of the selected objects.
                for (var i = 0; i < meshCount; i++) { cmd.DrawRenderer(renderer, material, i); }
            }
        }

        private void DrawAvatar((Renderer renderer, int meshCount, float avatarHeight) avatar, int cameraCulling, CommandBuffer cmd)
        {
            if (avatar.renderer == null)
                return;

            //Ignore disabled or culled by camera avatars
            if (!avatar.renderer.gameObject.activeSelf || (cameraCulling & (1 << avatar.renderer.gameObject.layer)) == 0)
                return;

            for (var i = 0; i < avatar.meshCount; i++)
            {
                Material materialToUse = null;

                // We use a GPU Skinning based material
                if (avatar.renderer.materials[i] != null)
                {
                    //Enable it when we are capable of adding the pass into the Toon Shader
                    if (USE_BASE_MATERIAL)
                    {
                        int originalMaterialOutlinerPass = avatar.renderer.materials[i].FindPass("Outliner");

                        if (originalMaterialOutlinerPass != -1)
                        {
                            //The material has a built in pass we can use
                            cmd.DrawRenderer(avatar.renderer, avatar.renderer.materials[i], i, originalMaterialOutlinerPass);
                            continue;
                        }
                    }

                    // We use the original material to copy the GPUSkinning values.
                    // We cannot use materialToUse.CopyPropertiesFromMaterial because there are non serialized uniforms to set
                    materialToUse = new Material(gpuSkinningMaterial);
                    toDispose.Add(materialToUse);
                    CopyAvatarProperties(avatar.renderer.materials[i], materialToUse);
                }
                else // Fallback to the normal outliner without GPUSkinning
                {
                    materialToUse = material;
                }

                // We have to manually render all the submeshes of the selected objects.
                cmd.DrawRenderer(avatar.renderer, materialToUse, i);
            }
        }

        private void CopyAvatarProperties(Material source, Material target)
        {
            target.SetMatrixArray(BIND_POSES, source.GetMatrixArray(BIND_POSES));
            target.SetMatrix(RENDERER_WORLD_INVERSE, source.GetMatrix(RENDERER_WORLD_INVERSE));
            target.SetMatrixArray(BONE_MATRICES, source.GetMatrixArray(BONE_MATRICES));

            if (source.HasTexture(AVATAR_MAP1))
                target.SetTexture(AVATAR_MAP1, source.GetTexture(AVATAR_MAP1));

            if (source.HasTexture(AVATAR_MAP2))
                target.SetTexture(AVATAR_MAP2, source.GetTexture(AVATAR_MAP2));

            if (source.HasTexture(AVATAR_MAP3))
                target.SetTexture(AVATAR_MAP3, source.GetTexture(AVATAR_MAP3));

            if (source.HasTexture(AVATAR_MAP4))
                target.SetTexture(AVATAR_MAP4, source.GetTexture(AVATAR_MAP4));

            if (source.HasTexture(AVATAR_MAP5))
                target.SetTexture(AVATAR_MAP5, source.GetTexture(AVATAR_MAP5));

            if (source.HasTexture(AVATAR_MAP6))
                target.SetTexture(AVATAR_MAP6, source.GetTexture(AVATAR_MAP6));

            if (source.HasTexture(AVATAR_MAP7))
                target.SetTexture(AVATAR_MAP7, source.GetTexture(AVATAR_MAP7));

            if (source.HasTexture(AVATAR_MAP8))
                target.SetTexture(AVATAR_MAP8, source.GetTexture(AVATAR_MAP8));

            if (source.HasTexture(AVATAR_MAP9))
                target.SetTexture(AVATAR_MAP9, source.GetTexture(AVATAR_MAP9));

            if (source.HasTexture(AVATAR_MAP10))
                target.SetTexture(AVATAR_MAP10, source.GetTexture(AVATAR_MAP10));

            if (source.HasTexture(AVATAR_MAP11))
                target.SetTexture(AVATAR_MAP11, source.GetTexture(AVATAR_MAP11));

            if (source.HasTexture(AVATAR_MAP12))
                target.SetTexture(AVATAR_MAP12, source.GetTexture(AVATAR_MAP12));
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(outlineTextureHandle.id);

            for (var index = 0; index < toDispose.Count; index++) { Object.Destroy(toDispose[index]); }

            toDispose.Clear();
        }
    }

    public OutlineRenderersSO renderers;
    private OutlinerRenderPass scriptablePass;

    private RenderTargetHandle outlineTexture;

    public override void Create()
    {
        scriptablePass = new OutlinerRenderPass(renderers)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
        };

        outlineTexture.Init("_OutlineTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        scriptablePass.Setup(renderingData.cameraData.cameraTargetDescriptor, outlineTexture);
        renderer.EnqueuePass(scriptablePass);
    }
}
