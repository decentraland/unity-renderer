using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public static class DataStore_WorldObjects_Extensions
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public static void AddMesh( this DataStore.DataStore_WorldObjects self, string sceneId, Mesh mesh )
        {
            if ( mesh == null )
            {
                logger.Log( $"Trying to add null mesh! (id: {sceneId})");
                return;
            }

            if ( string.IsNullOrEmpty(sceneId))
            {
                logger.LogWarning($"AddMesh", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            if (!self.sceneData.ContainsKey(sceneId))
                self.sceneData.Add(sceneId, new DataStore.DataStore_WorldObjects.SceneData());

            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if ( sceneMeshes.ContainsKey(mesh))
                sceneMeshes[mesh]++;
            else
            {
                sceneMeshes.Add(mesh, 1);
                logger.Log($"Adding mesh {mesh.GetInstanceID()} to {sceneId} (refCount = {sceneMeshes[mesh]})");
            }
        }

        public static void RemoveMesh( this DataStore.DataStore_WorldObjects self, string sceneId, Mesh mesh )
        {
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

        public static void AddMesh( this DataStore.DataStore_WorldObjects self, IDCLEntity entity, Mesh mesh )
        {
            string sceneId = entity.scene.sceneData.id;
            self.AddMesh( sceneId, mesh );
        }

        public static void RemoveMesh( this DataStore.DataStore_WorldObjects self, IDCLEntity entity, Mesh mesh )
        {
            string sceneId = entity.scene.sceneData.id;
            self.RemoveMesh( sceneId, mesh );
        }

        public static void AddRendereable( this DataStore.DataStore_WorldObjects self, IDCLEntity entity, Rendereable rendereable )
        {
            string sceneId = entity.scene.sceneData.id;
            self.AddRendereable( sceneId, rendereable );
        }

        public static void RemoveRendereable( this DataStore.DataStore_WorldObjects self, IDCLEntity entity, Rendereable rendereable )
        {
            string sceneId = entity.scene.sceneData.id;
            self.RemoveRendereable( sceneId, rendereable );
        }

        public static void AddRendereable( this DataStore.DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
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

        public static void RemoveRendereable( this DataStore.DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
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

        public static bool IsEmpty( this DataStore.DataStore_WorldObjects.SceneData self)
        {
            return self.refCountedMeshes.Count() == 0 && self.renderedObjects.Count() == 0;
        }

        public static DataStore.DataStore_WorldObjects.SceneData GetSceneData(this DataStore.DataStore_WorldObjects self, string sceneId)
        {
            var sceneRenderingData = DataStore.i.sceneWorldObjects.sceneData;

            if ( !sceneRenderingData.ContainsKey(sceneId) )
                sceneRenderingData.Add(sceneId, new DataStore.DataStore_WorldObjects.SceneData());

            return sceneRenderingData[sceneId];
        }
    }
}