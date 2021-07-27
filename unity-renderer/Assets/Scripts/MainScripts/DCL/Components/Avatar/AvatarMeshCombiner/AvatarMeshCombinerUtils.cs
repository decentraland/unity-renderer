﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class AvatarMeshCombinerUtils
    {
        private const int MAX_TEXTURE_ID_COUNT = 12;

        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = LogType.Warning };

        private static readonly int[] textureIds = new int[] { ShaderUtils.BaseMap, ShaderUtils.EmissionMap };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        internal static List<CombineLayer> Slice(SkinnedMeshRenderer[] renderers)
        {
            logger.Log("Slice Start!");

            renderers = renderers.Where( (x) => x != null && x.enabled && x.sharedMesh != null ).ToArray();

            var rawLayers = SliceByRenderState(renderers);

            logger.Log($"Preparing slice. Found {rawLayers.Count} groups.");

            //
            // Now, we subdivide the rawLayers.
            // A single rawLayer will be subdivided if the textures exceed the sampler limit (12 in this case).
            // Also, in this step the textureToId map is populated.
            //
            List<CombineLayer> result = new List<CombineLayer>();

            for (int i = 0; i < rawLayers.Count; i++)
            {
                var rawLayer = rawLayers[i];
                logger.Log($"Processing group {i}. Renderer count: {rawLayer.renderers.Count}. cullMode: {rawLayer.cullMode} - isOpaque: {rawLayer.isOpaque}");
                result.AddRange(SubdivideLayerByTextures(rawLayer));
            }

            // No valid materials were found
            if ( result.Count == 1 && result[0].textureToId.Count == 0 )
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

                logger.Log($"Layer index: {layInd} ... renderer count: {layer.renderers.Count} ... textures found: {layer.textureToId.Count}\nrenderers: {rendererNames}");
                layInd++;
            }

            logger.Log("Slice End Success!");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        internal static List<CombineLayer> SubdivideLayerByTextures( CombineLayer layer )
        {
            var result = new List<CombineLayer>();
            int textureId = 0;

            bool shouldAddLayerToResult = true;
            CombineLayer currentResultLayer = null;

            for (int rendererIndex = 0; rendererIndex < layer.renderers.Count; rendererIndex++)
            {
                var r = layer.renderers[rendererIndex];

                if ( shouldAddLayerToResult )
                {
                    shouldAddLayerToResult = false;
                    textureId = 0;

                    currentResultLayer = new CombineLayer
                    {
                        cullMode = layer.cullMode,
                        isOpaque = layer.isOpaque
                    };

                    result.Add(currentResultLayer);
                }

                var mats = r.sharedMaterials;

                var mapIdsToInsert = GetMapIds(
                    new ReadOnlyDictionary<Texture2D, int>(currentResultLayer.textureToId),
                    mats,
                    textureId );

                // The renderer is too big to fit in a single layer? (This should never happen).
                if (mapIdsToInsert.Count > MAX_TEXTURE_ID_COUNT)
                {
                    logger.Log(LogType.Warning, "The renderer is too big to fit in a single layer? (This should never happen).");
                    shouldAddLayerToResult = true;
                    continue;
                }

                // The renderer can fit in a single layer.
                // But can't fit in this one, as previous renderers filled this layer out.
                if ( textureId + mapIdsToInsert.Count > MAX_TEXTURE_ID_COUNT )
                {
                    rendererIndex--;
                    shouldAddLayerToResult = true;
                    continue;
                }

                // put GetMapIds result into currentLayer id map.
                foreach ( var kvp in mapIdsToInsert )
                {
                    //logger.Log("Adding textureId " + kvp.Value);
                    currentResultLayer.textureToId[ kvp.Key ] = kvp.Value;
                }

                currentResultLayer.renderers.Add(r);

                textureId += mapIdsToInsert.Count;

                if ( textureId >= MAX_TEXTURE_ID_COUNT )
                {
                    shouldAddLayerToResult = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        internal static List<CombineLayer> SliceByRenderState(SkinnedMeshRenderer[] renderers)
        {
            List<CombineLayer> result = new List<CombineLayer>();

            // Group renderers on opaque and transparent materials
            var rendererByOpaqueMode = renderers.GroupBy( IsOpaque );

            // Then, make subgroups to divide them between culling modes
            foreach ( var byOpaqueMode in rendererByOpaqueMode )
            {
                // For opaque renderers, we replace the CullOff value by CullBack to reduce group count,
                // This workarounds many opaque wearables that use Culling Off by mistake. 
                var getCullModeFunc = byOpaqueMode.Key ? new Func<SkinnedMeshRenderer, CullMode>(GetCullModeWithoutCullOff) : new Func<SkinnedMeshRenderer, CullMode>(GetCullMode);

                var rendererByCullingMode = byOpaqueMode.GroupBy( getCullModeFunc );

                foreach ( var byCulling in rendererByCullingMode )
                {
                    var byCullingRenderers = byCulling.ToList();

                    CombineLayer layer = new CombineLayer();
                    result.Add(layer);
                    layer.cullMode = byCulling.Key;
                    layer.isOpaque = byOpaqueMode.Key;
                    layer.renderers = byCullingRenderers;
                }
            }

            /*
            * The grouping outcome ends up like this:
            *
            *                 Opaque           Transparent
            *             /     |     \        /    |    \
            *          Back - Front - Off - Back - Front - Off -> rendererGroups
            */

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refDict"></param>
        /// <param name="mats"></param>
        /// <param name="startingId"></param>
        /// <returns></returns>
        private static Dictionary<Texture2D, int> GetMapIds(ReadOnlyDictionary<Texture2D, int> refDict, in Material[] mats, int startingId)
        {
            var result = new Dictionary<Texture2D, int>();

            for ( int i = 0; i < mats.Length; i++ )
            {
                var mat = mats[i];

                if (mat == null)
                    continue;

                for ( int texIdIndex = 0; texIdIndex < textureIds.Length; texIdIndex++ )
                {
                    var texture = (Texture2D)mat.GetTexture(textureIds[texIdIndex]);

                    if ( texture == null )
                        continue;

                    if ( refDict.ContainsKey(texture) || result.ContainsKey(texture) )
                        continue;

                    result.Add(texture, startingId);
                    startingId++;
                }
            }

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

#if UNITY_EDITOR
            DrawDebugSkeleton(renderer);
#endif
        }

        internal static void DrawDebugSkeleton(SkinnedMeshRenderer renderer)
        {
            var bones = renderer.bones;

            for ( int i = 0 ; i < bones.Length; i++ )
            {
                Transform bone = bones[i];
                Debug.DrawLine(bone.position, bone.position + bone.forward, Color.cyan, 60);

                foreach ( Transform child in bone )
                {
                    Debug.DrawLine(bone.position, child.position, Color.green, 60);
                }
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
            CullMode result = (CullMode)firstMat.GetInt( ShaderUtils.Cull );
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        internal static CullMode GetCullModeWithoutCullOff(Renderer renderer)
        {
            CullMode result = GetCullMode(renderer);

            if (result == CullMode.Off)
                result = CullMode.Back;

            return result;
        }
    }
}