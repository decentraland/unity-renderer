using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class AvatarMeshCombinerUtils
    {
        private const int MAX_TEXTURE_ID_COUNT = 10;

        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        internal static List<CombineLayer> Slice(SkinnedMeshRenderer[] renderers)
        {
            // Define helper methods
            bool CanAddToMap(CombineLayer layer, Texture2D tex)
            {
                return tex != null && !layer.idMap.ContainsKey(tex);
            }

            void AddToMap(CombineLayer layer, Texture2D tex, ref int texId)
            {
                if ( !CanAddToMap(layer, tex) )
                    return;

                layer.idMap.Add(tex, texId);
                texId++;
            }

            // Implementation start
            logger.Log("Slice Start!");

            renderers = renderers.Where( (x) => x != null && x.enabled && x.sharedMesh != null ).ToArray();

            Dictionary<int, CullMode> groupToCullMode = new Dictionary<int, CullMode>();
            Dictionary<int, bool> groupToOpaqueMode = new Dictionary<int, bool>();

            List<List<SkinnedMeshRenderer>> rendererGroups = new List<List<SkinnedMeshRenderer>>();

            // Group renderers on opaque and transparent materials
            var rendererByOpaqueMode = renderers.GroupBy( IsOpaque );

            // Then, make subgroups to divide them between culling modes
            foreach ( var byOpaqueMode in rendererByOpaqueMode )
            {
                var rendererByCullingMode = byOpaqueMode.GroupBy( GetCullMode );

                foreach ( var byCulling in rendererByCullingMode )
                {
                    var byCullingRenderers = byCulling.ToList();
                    rendererGroups.Add(byCullingRenderers);
                    groupToCullMode.Add(rendererGroups.Count - 1, byCulling.Key);
                    groupToOpaqueMode.Add(rendererGroups.Count - 1, byOpaqueMode.Key);
                }
            }

            logger.Log($"Preparing slice. Found {rendererGroups.Count} groups.");

            /*
             * The grouping outcome ends up like this:
             *
             *                 Opaque           Transparent
             *             /     |     \        /    |    \
             *          Back - Front - Off - Back - Front - Off -> rendererGroups
             */

            List<CombineLayer> result = new List<CombineLayer>();

            for (int groupIndex = 0; groupIndex < rendererGroups.Count; groupIndex++)
            {
                var group = rendererGroups[groupIndex];
                int textureId = 0;

                CombineLayer currentLayer = new CombineLayer
                {
                    cullMode = groupToCullMode[ groupIndex ],
                    isOpaque = groupToOpaqueMode[ groupIndex ]
                };

                result.Add(currentLayer);

                foreach ( var r in @group )
                {
                    currentLayer.renderers.Add(r);

                    var mats = r.sharedMaterials;

                    for ( int i = 0; i < mats.Length; i++ )
                    {
                        var mat = mats[i];

                        if (mat == null)
                            continue;

                        var baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                        var emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);

                        AddToMap(currentLayer, baseMap, ref textureId);
                        AddToMap(currentLayer, emissionMap, ref textureId);

                        if ( textureId >= MAX_TEXTURE_ID_COUNT )
                            break;
                    }

                    if ( textureId >= MAX_TEXTURE_ID_COUNT )
                        break;
                }
            }

            // No valid materials were found
            if ( result.Count == 1 && result[0].idMap.Count == 0 )
            {
                logger.Log("Slice End Fail!");
                return null;
            }

            int layInd = 0;

            result = result.Where( x => x.renderers != null && x.renderers.Count > 0 ).ToList();

            foreach ( var layer in result )
            {
                string rendererNames = layer.renderers
                    .Select( (x) => $"{x.transform.parent.name}" )
                    .Aggregate( (i, j) => i + "\n" + j);

                logger.Log($"Layer index: {layInd} ... renderer count: {layer.renderers.Count} ... textures found: {layer.idMap.Count}\nrenderers: {rendererNames}");
                layInd++;
            }

            logger.Log("Slice End Success!");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        internal static void ResetBones(SkinnedMeshRenderer renderer)
        {
            var bindPoses = renderer.sharedMesh.bindposes;
            var bones = renderer.bones;

            for ( int i = 0 ; i < bones.Length; i++ )
            {
                Transform bone = bones[i];
                Matrix4x4 bindPose = bindPoses[i].inverse;
                bone.position = bindPose.MultiplyPoint3x4(Vector3.zero);
                bone.rotation = bindPose.rotation;

                Vector3 bindPoseScale = bindPose.lossyScale;
                Vector3 boneScale = bone.lossyScale;

                bone.localScale = new Vector3(bindPoseScale.x / boneScale.x,
                    bindPoseScale.y / boneScale.y,
                    bindPoseScale.z / boneScale.z);
            }
        }

        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Renderer renderer)
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return true;

            if (firstMat.HasProperty(ShaderUtils.ZWrite) &&
                (int) firstMat.GetFloat(ShaderUtils.ZWrite) == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        internal static CullMode GetCullMode( Renderer renderer )
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return CullMode.Back;

            return (CullMode)firstMat.GetInt( ShaderUtils.Cull );
        }
    }
}