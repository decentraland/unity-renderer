using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public static class AvatarMeshCombinerUtils
    {
        private const int MAX_TEXTURE_ID_COUNT = 12;

        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);

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
            List<CombineLayer> result = new List<CombineLayer>();

            // Group renderers on opaque and transparent materials
            var surfaceGroups = renderers.GroupBy( IsOpaque );
            int textureId = 0;

            foreach ( var group in surfaceGroups )
            {
                CombineLayer currentLayer = new CombineLayer();

                result.Add(currentLayer);
                textureId = 0;

                var groupRenderers = group.ToArray();

                foreach ( var r in groupRenderers )
                {
                    if ( !r.enabled || r.sharedMesh == null )
                    {
                        //logger.Log($"Filtering out renderer: {r.transform.parent.name}");
                        continue;
                    }

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
                        {
                            textureId = 0;
                            currentLayer = new CombineLayer();
                            result.Add(currentLayer);
                        }
                    }
                }
            }

            // No valid materials were found
            if ( result.Count == 1 && textureId == 0 )
            {
                logger.Log("Slice End Fail!");
                return null;
            }

            int layInd = 0;

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
    }
}