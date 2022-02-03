using System.Collections.Generic;
using System.Linq;
using DCL.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public static class DataStore_WorldObjects_Extensions
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public static Rendereable GetRendereableByRenderer(this DataStore_WorldObjects self, string sceneId, string entityId, Renderer renderer)
        {
            if (string.IsNullOrEmpty(sceneId))
            {
                logger.LogWarning($"GetRendereableByRenderer", $"invalid sceneId!");
                return null;
            }

            if (string.IsNullOrEmpty(entityId))
            {
                logger.LogWarning($"GetRendereableByRenderer", $"invalid entityId!");
                return null;
            }

            var sceneData = self.sceneData[sceneId];

            if (!sceneData.filteredByOwner.ContainsKey(entityId))
                return null;

            var rendereables = sceneData.filteredByOwner[entityId].rendereables.Get();

            foreach (var r in rendereables)
            {
                if ( r.renderers.Contains(renderer))
                    return r;
            }

            return null;
        }

        public static void AddRendereable( this DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
        {
            if (rendereable == null)
            {
                logger.Log( $"Trying to add null rendereable! (id: {sceneId})");
                return;
            }

            if (string.IsNullOrEmpty(sceneId))
            {
                logger.LogWarning($"AddRendereable", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            if (string.IsNullOrEmpty(rendereable.ownerId))
            {
                logger.LogError($"AddRendereable", $"invalid ownerId! Make sure to assign ownerId to the given rendereable (hint: it's the entityId)");
                return;
            }

            if (!self.sceneData.ContainsKey(sceneId))
                self.sceneData.Add(sceneId, new DataStore_WorldObjects.SceneData());

            var sceneData = self.sceneData[sceneId];

            if ( !sceneData.renderedObjects.Contains(rendereable) )
                sceneData.renderedObjects.Add(rendereable);

            if ( !sceneData.filteredByOwner.ContainsKey(rendereable.ownerId) )
                sceneData.filteredByOwner.Add(rendereable.ownerId, new DataStore_WorldObjects.OwnerData());

            DataStore_WorldObjects.OwnerData ownerData = sceneData.filteredByOwner[rendereable.ownerId];
            ownerData.rendereables.Add(rendereable);
        }

        public static void RemoveRendereable( this DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
        {
            if ( rendereable == null )
            {
                logger.Log( $"Trying to remove null rendereable! (id: {sceneId})");
                return;
            }

            if ( string.IsNullOrEmpty(sceneId) || !self.sceneData.ContainsKey(sceneId) )
            {
                logger.LogWarning($"RemoveRendereable", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            if (string.IsNullOrEmpty(rendereable.ownerId))
            {
                logger.LogError($"AddRendereable", $"invalid ownerId! Make sure to assign ownerId to the given rendereable (hint: it's the entityId)");
                return;
            }

            var sceneData = self.sceneData[sceneId];

            if ( sceneData.renderedObjects.Contains(rendereable) )
                sceneData.renderedObjects.Remove(rendereable);

            if (self.sceneData[sceneId].IsEmpty())
                self.sceneData.Remove(sceneId);


            if ( sceneData.filteredByOwner.ContainsKey(rendereable.ownerId) )
            {
                DataStore_WorldObjects.OwnerData ownerData = sceneData.filteredByOwner[rendereable.ownerId];
                ownerData.rendereables.Remove(rendereable);

                if ( ownerData.rendereables.Count() == 0 )
                    sceneData.filteredByOwner.Remove(rendereable.ownerId);
            }
        }

        private static bool IsEmpty( this DataStore_WorldObjects.SceneData self)
        {
            return self.renderedObjects.Count() == 0;
        }
    }
}