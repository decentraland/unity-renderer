using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class CombineLayerUtils
    {
        // This heuristic forces double-sided opaque objects to have backface culling.
        // As many wearables are incorrectly modeled as double-sided, this greatly increases
        // the cases of avatars rendered with one draw call. Temporarily disabled until some wearables are fixed.
        public static bool ENABLE_CULL_OPAQUE_HEURISTIC = false;

        private static bool VERBOSE = false;
        private const int MAX_TEXTURE_ID_COUNT = 12;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };
        private static readonly int[] textureIds = new int[] { ShaderUtils.BaseMap, ShaderUtils.EmissionMap };

        /// <summary>
        /// This method takes a skinned mesh renderer list and turns it into a series of CombineLayer elements.<br/>
        /// 
        /// Each CombineLayer element represents a combining group, and the renderers are grouped using a set of criteria.<br/>
        ///
        /// <ul>
        /// <li>Each layer must correspond to renderers that share the same cull mode and blend state.</li>
        /// <li>Each layer can't contain renderers that sum up over MAX_TEXTURE_ID_COUNT textures.</li>
        /// </ul>
        /// Those layers can later be used to combine meshes as efficiently as possible by encoding their map
        /// samplers to UV attributes. This allows the usage of a special shader that can branch samplers according to
        /// the UV data. By encoding the samplers this way, materials that use different textures and uniform values
        /// can be grouped together, and thus, meshes can be further combined.
        /// </summary>
        /// <param name="renderers">List of renderers to slice.</param>
        /// <returns>List of CombineLayer objects that can be used to produce a highly optimized combined mesh.</returns>
        internal static List<CombineLayer> Slice(SkinnedMeshRenderer[] renderers)
        {
            logger.Log("Slice Start!");

            var rawLayers = SliceByRenderState(renderers);

            logger.Log($"Preparing slice. Found {rawLayers.Count} groups.");

            //
            // Now, we sub-slice the rawLayers.
            // A single rawLayer will be sub-sliced if the textures exceed the sampler limit (12 in this case).
            // Also, in this step the textureToId map is populated.
            //
            List<CombineLayer> result = new List<CombineLayer>();

            for (int i = 0; i < rawLayers.Count; i++)
            {
                var rawLayer = rawLayers[i];
                logger.Log($"Processing group {i}. Renderer count: {rawLayer.renderers.Count}. cullMode: {rawLayer.cullMode} - isOpaque: {rawLayer.isOpaque}");
                result.AddRange(SubsliceLayerByTextures(rawLayer));
            }

            // No valid materials were found
            if ( result.Count == 1 && result[0].textureToId.Count == 0 && result[0].renderers.Count == 0)
            {
                logger.Log("Slice End Fail!");
                return null;
            }

            result = result.Where( x => x.renderers != null && x.renderers.Count > 0 ).ToList();

            if ( VERBOSE )
            {
                int layInd = 0;
                foreach ( var layer in result )
                {
                    string rendererNames = layer.renderers
                        .Select( (x) => $"{x.transform.parent.name}" )
                        .Aggregate( (i, j) => i + "\n" + j);

                    logger.Log($"Layer index: {layInd} ... renderer count: {layer.renderers.Count} ... textures found: {layer.textureToId.Count}\nrenderers: {rendererNames}");
                    layInd++;
                }
            }

            logger.Log("Slice End Success!");
            return result;
        }

        /// <summary>
        /// <p>
        /// This method takes a single CombineLayer and sub-slices it according to the texture count of the
        /// contained renderers of the given layer.
        /// </p>
        /// <p>
        /// The resulting layers will have their <i>textureToId</i> field populated with the found textures.
        /// The <i>textureToId</i> int value is what will have to be passed over the uv attributes of the combined meshes.
        /// </p>
        /// </summary>
        /// <param name="layer">The CombineLayer layer to subdivide and populate by the ids.</param>
        /// <returns>A list that at least is guaranteed to contain the given layer.
        /// If the given layer exceeds the max texture count, more than a layer can be returned.
        /// </returns>
        internal static List<CombineLayer> SubsliceLayerByTextures( CombineLayer layer )
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
                    currentResultLayer.textureToId[ kvp.Key ] = kvp.Value;
                }

                currentResultLayer.renderers.Add(r);

                textureId += mapIdsToInsert.Count;

                if ( textureId >= MAX_TEXTURE_ID_COUNT )
                {
                    shouldAddLayerToResult = true;
                }
            }

            if ( VERBOSE )
            {
                for (int i = 0; i < result.Count; i++)
                {
                    var c = result[i];
                    Debug.Log($"layer {i} - {c}");
                }
            }

            return result;
        }

        /// <summary>
        /// <p>
        /// This method takes a skinned mesh renderer list and turns it into a series of CombineLayer elements.
        /// Each CombineLayer element represents a combining group, and the renderers are grouped using a set of criteria.
        /// </p>
        /// <p>
        /// For SliceByRenderState, the returned CombineLayer list will be grouped according to shared cull mode and
        /// blend state.
        /// </p>
        /// </summary>
        /// <param name="renderers">List of renderers to slice.</param>
        /// <returns>List of CombineLayer objects that can be used to produce a highly optimized combined mesh.</returns>
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
                Func<SkinnedMeshRenderer, CullMode> getCullModeFunc = null;

                if ( ENABLE_CULL_OPAQUE_HEURISTIC )
                {
                    getCullModeFunc = byOpaqueMode.Key ? new Func<SkinnedMeshRenderer, CullMode>(GetCullModeWithoutCullOff) : GetCullMode;
                }
                else
                {
                    getCullModeFunc = GetCullMode;
                }

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
        internal static Dictionary<Texture2D, int> GetMapIds(ReadOnlyDictionary<Texture2D, int> refDict, in Material[] mats, int startingId)
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
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="material">Material to be checked</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Material material)
        {
            if (material == null)
                return true;

            bool isTransparent = material.HasProperty(ShaderUtils.ZWrite) &&
                                 (int) material.GetFloat(ShaderUtils.ZWrite) == 0;

            return !isTransparent;
        }

        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Renderer renderer)
        {
            return IsOpaque(renderer.sharedMaterials[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        internal static CullMode GetCullMode(Material material)
        {
            CullMode result = (CullMode)material.GetInt( ShaderUtils.Cull );
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        internal static CullMode GetCullMode(Renderer renderer)
        {
            return GetCullMode(renderer.sharedMaterials[0]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        internal static CullMode GetCullModeWithoutCullOff(Renderer renderer)
        {
            CullMode result = GetCullMode(renderer.sharedMaterials[0]);

            if (result == CullMode.Off)
                result = CullMode.Back;

            return result;
        }
    }
}