using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Shaders;
using MainScripts.DCL.Helpers.Utils;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using logger = DCL.MeshCombinerLogger;


namespace DCL
{
    public static class CombineLayerUtils
    {
        // This heuristic forces double-sided opaque objects to have backface culling.
        // As many wearables are incorrectly modeled as double-sided, this greatly increases
        // the cases of avatars rendered with one draw call. Temporarily disabled until some wearables are fixed.
        public static bool ENABLE_CULL_OPAQUE_HEURISTIC = false;

        private const int MAX_TEXTURE_ID_COUNT = 12;
        private static readonly int[] textureIds = { ShaderUtils.BaseMap, ShaderUtils.EmissionMap };

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
        internal static bool TrySlice(IReadOnlyList<SkinnedMeshRenderer> renderers, CombineLayersList result)
        {
            logger.Log("Slice Start!");

            using (var rental = PoolUtils.RentListOfDisposables<CombineLayer>())
            {
                var rawLayers = rental.GetList();
                SliceByRenderState.Execute(renderers, rawLayers, ENABLE_CULL_OPAQUE_HEURISTIC);

                logger.Log($"Preparing slice. Found {rawLayers.Count} groups.");

                // Now, we sub-slice the rawLayers.
                // A single rawLayer will be sub-sliced if the textures exceed the sampler limit (12 in this case).
                // Also, in this step the textureToId map is populated.

                for (int i = 0; i < rawLayers.Count; i++)
                {
                    var rawLayer = rawLayers[i];

                    logger.Log($"Processing group {i}. Renderer count: {rawLayer.Renderers.Count}. cullMode: {rawLayer.cullMode} - isOpaque: {rawLayer.isOpaque}");
                    SubsliceLayerByTextures(rawLayer, result);
                }
            }

            // No valid materials were found
            if (result.Count == 1 && result[0].textureToId.Count == 0 && result[0].Renderers.Count == 0)
            {
                logger.Log("Slice End Fail!");
                return false;
            }

            result.Sanitize();

            [Conditional(MeshCombinerLogger.COMPILATION_DEFINE)]
            static void LogLayers(CombineLayersList result)
            {
                int layInd = 0;

                foreach (var layer in result.Layers)
                {
                    string rendererNames = layer.Renderers
                                                .Select((x) => $"{x.transform.parent.name}")
                                                .Aggregate((i, j) => i + "\n" + j);

                    logger.Log($"Layer index: {layInd} ... renderer count: {layer.Renderers.Count} ... textures found: {layer.textureToId.Count}\nrenderers: {rendererNames}");
                    layInd++;
                }
            }

            LogLayers(result);

            logger.Log("Slice End Success!");
            return true;
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
        internal static void SubsliceLayerByTextures(CombineLayer layer, CombineLayersList results)
        {
            int textureId;

            CombineLayer currentResultLayer;

            void AddLayerToResult()
            {
                textureId = 0;
                currentResultLayer = CombineLayer.Rent(layer.cullMode, layer.isOpaque);
                results.Add(currentResultLayer);
            }

            AddLayerToResult();

            for (int rendererIndex = 0; rendererIndex < layer.Renderers.Count; rendererIndex++)
            {
                var r = layer.Renderers[rendererIndex];

                using var materialRent = PoolUtils.RentList<Material>();
                var mats = materialRent.GetList();

                r.GetSharedMaterials(mats);

                using var mapIdsToInsertRental = PoolUtils.RentDictionary<Texture2D, int>();
                var mapIdsToInsert = mapIdsToInsertRental.GetDictionary();

                AddMapIds(
                    currentResultLayer.textureToId,
                    mapIdsToInsert,
                    mats,
                    textureId);

                // The renderer is too big to fit in a single layer? (This should never happen).
                if (mapIdsToInsert.Count > MAX_TEXTURE_ID_COUNT)
                {
                    logger.LogWarning("The renderer is too big to fit in a single layer? (This should never happen).");
                    AddLayerToResult();
                    continue;
                }

                // The renderer can fit in a single layer.
                // But can't fit in this one, as previous renderers filled this layer out.
                if (textureId + mapIdsToInsert.Count > MAX_TEXTURE_ID_COUNT)
                {
                    rendererIndex--;
                    AddLayerToResult();
                    continue;
                }

                // put GetMapIds result into currentLayer id map.
                foreach (var kvp in mapIdsToInsert)
                    currentResultLayer.textureToId[kvp.Key] = kvp.Value;

                results.AddRenderer(currentResultLayer, r);

                textureId += mapIdsToInsert.Count;

                if (textureId >= MAX_TEXTURE_ID_COUNT)
                    AddLayerToResult();
            }

            [Conditional(MeshCombinerLogger.COMPILATION_DEFINE)]
            static void LogResults(CombineLayersList results)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    var c = results[i];
                    Debug.Log($"layer {i} - {c}");
                }
            }

            LogResults(results);
        }

        internal static void AddMapIds(IReadOnlyDictionary<Texture2D, int> refDict, IDictionary<Texture2D, int> candidates, IReadOnlyList<Material> mats, int startingId)
        {
            for (int i = 0; i < mats.Count; i++)
            {
                var mat = mats[i];

                if (mat == null)
                    continue;

                for (int texIdIndex = 0; texIdIndex < textureIds.Length; texIdIndex++)
                {
                    var texture = (Texture2D)mat.GetTexture(textureIds[texIdIndex]);

                    if (texture == null)
                        continue;

                    if (refDict.ContainsKey(texture) || candidates.ContainsKey(texture))
                        continue;

                    candidates.Add(texture, startingId);
                    startingId++;
                }
            }
        }
    }
}
