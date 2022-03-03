using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public static class DataStore_WorldObjects_Extensions
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="entity"></param>
        /// <param name="rendereable"></param>
        public static void AddRendereable( this DataStore_WorldObjects self, IDCLEntity entity, Rendereable rendereable )
        {
            if ( entity == null )
            {
                logger.Log($"Null entity!");
                return;
            }

            if ( rendereable == null )
            {
                logger.Log( $"Trying to add null rendereable! (id: {entity.scene.sceneData.id})");
                return;
            }

            string sceneId = entity.scene.sceneData.id;
            rendereable.ownerId = entity.entityId;

            foreach (var mesh in rendereable.meshes)
            {
                self.AddMesh(entity, mesh);
            }

            self.AddRendereable( sceneId, rendereable );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="entity"></param>
        /// <param name="rendereable"></param>
        public static void RemoveRendereable( this DataStore_WorldObjects self, IDCLEntity entity, Rendereable rendereable )
        {
            if ( entity == null )
            {
                logger.Log($"Null entity!");
                return;
            }

            if ( rendereable == null )
            {
                logger.Log( $"Trying to remove null rendereable! (id: {entity.scene.sceneData.id})");
                return;
            }

            string sceneId = entity.scene.sceneData.id;

            foreach (var mesh in rendereable.meshes)
            {
                self.RemoveMesh(entity, mesh);
            }

            self.RemoveRendereable( sceneId, rendereable );
        }

        private static void AddMesh( this DataStore_WorldObjects self, string sceneId, Mesh mesh )
        {
            if (!self.sceneData.ContainsKey(sceneId))
                self.sceneData.Add(sceneId, new DataStore_WorldObjects.SceneData());

            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if ( sceneMeshes.ContainsKey(mesh))
                sceneMeshes[mesh]++;
            else
            {
                sceneMeshes.Add(mesh, 1);
                logger.Log($"Adding mesh {mesh.GetInstanceID()} to {sceneId} (refCount = {sceneMeshes[mesh]})");
            }
        }

        private static void RemoveMesh( this DataStore_WorldObjects self, string sceneId, Mesh mesh )
        {
            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if (!sceneMeshes.ContainsKey(mesh))
                return;

            sceneMeshes[mesh]--;

            if (sceneMeshes[mesh] == 0)
            {
                logger.Log($"Removing mesh {mesh.GetInstanceID()} from {sceneId} (refCount == 0)");
                sceneMeshes.Remove(mesh);
            }
            else
            {
                logger.Log($"Removing mesh {mesh.GetInstanceID()} from {sceneId} (refCount == {sceneMeshes[mesh]})");
            }

            if (self.sceneData[sceneId].IsEmpty())
                self.sceneData.Remove(sceneId);
        }

        private static void AddMesh( this DataStore_WorldObjects self, IDCLEntity entity, Mesh mesh )
        {
            if (entity == null)
            {
                logger.LogWarning($"AddMesh", $"invalid entity!");
                return;
            }

            string sceneId = entity.scene.sceneData.id;

            if (mesh == null)
            {
                logger.Log( $"Trying to add null mesh! (id: {sceneId})");
                return;
            }

            self.AddMesh( sceneId, mesh );
        }

        private static void RemoveMesh( this DataStore_WorldObjects self, IDCLEntity entity, Mesh mesh )
        {
            if (entity == null)
            {
                logger.LogWarning($"RemoveMesh", $"invalid entity!");
                return;
            }

            string sceneId = entity.scene.sceneData.id;

            if ( mesh == null )
            {
                logger.Log( $"Trying to remove null mesh! (id: {sceneId})");
                return;
            }

            if ( string.IsNullOrEmpty(sceneId) || !self.sceneData.ContainsKey(sceneId) )
            {
                logger.LogWarning($"RemoveMesh", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            self.RemoveMesh( sceneId, mesh );
        }

        private static void AddRendereable( this DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
        {
            if ( rendereable == null )
            {
                logger.Log( $"Trying to add null rendereable! (id: {sceneId})");
                return;
            }

            if ( string.IsNullOrEmpty(sceneId) || !self.sceneData.ContainsKey(sceneId) )
            {
                logger.LogWarning($"AddRendereable", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            var sceneData = self.sceneData[sceneId];

            if ( !sceneData.renderedObjects.Contains(rendereable) )
                sceneData.renderedObjects.Add(rendereable);
        }

        private static void RemoveRendereable( this DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
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

            var sceneData = self.sceneData[sceneId];

            if ( sceneData.renderedObjects.Contains(rendereable) )
                sceneData.renderedObjects.Remove(rendereable);

            if (self.sceneData[sceneId].IsEmpty())
                self.sceneData.Remove(sceneId);
        }

        private static bool IsEmpty( this DataStore_WorldObjects.SceneData self)
        {
            return self.refCountedMeshes.Count() == 0 && self.renderedObjects.Count() == 0;
        }
    }
}