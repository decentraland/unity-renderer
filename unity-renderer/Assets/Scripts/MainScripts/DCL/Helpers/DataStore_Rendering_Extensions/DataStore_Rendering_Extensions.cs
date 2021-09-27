using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public static class DataStore_Rendering_Extensions
    {
        public static void AddMesh( this DataStore.DataStore_Rendering self, string sceneId, Mesh mesh )
        {
            if ( string.IsNullOrEmpty(sceneId))
            {
                Debug.LogWarning($"AddMesh: invalid sceneId! (id: {sceneId})");
                return;
            }

            if (!self.sceneData.ContainsKey(sceneId))
                self.sceneData.Add(sceneId, new DataStore.DataStore_Rendering.SceneData());

            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if ( sceneMeshes.ContainsKey(mesh))
                sceneMeshes[mesh]++;
            else
            {
                sceneMeshes.Add(mesh, 1);
                Debug.Log($"Adding mesh {mesh.GetInstanceID()} to {sceneId} (refCount = {sceneMeshes[mesh]})");
            }
        }

        public static void RemoveMesh( this DataStore.DataStore_Rendering self, string sceneId, Mesh mesh )
        {
            if ( string.IsNullOrEmpty(sceneId) || !self.sceneData.ContainsKey(sceneId) )
            {
                Debug.LogWarning($"RemoveMesh: invalid sceneId! (id: {sceneId})");
                return;
            }

            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if (!sceneMeshes.ContainsKey(mesh))
                return;

            sceneMeshes[mesh]--;

            if (sceneMeshes[mesh] == 0)
            {
                Debug.Log($"Removing mesh {mesh.GetInstanceID()} from {sceneId} (refCount == 0)");
                sceneMeshes.Remove(mesh);
            }
            else
            {
                Debug.Log($"Removing mesh {mesh.GetInstanceID()} from {sceneId} (refCount == {sceneMeshes[mesh]})");
            }
        }

        public static void AddMesh( this DataStore.DataStore_Rendering self, IDCLEntity entity, Mesh mesh )
        {
            string sceneId = entity.scene.sceneData.id;
            self.AddMesh( sceneId, mesh );
        }

        public static void RemoveMesh( this DataStore.DataStore_Rendering self, IDCLEntity entity, Mesh mesh )
        {
            string sceneId = entity.scene.sceneData.id;
            self.RemoveMesh( sceneId, mesh );
        }

        public static bool IsEmpty( this DataStore.DataStore_Rendering.SceneData self)
        {
            return self.refCountedMeshes.Count() == 0;
        }
    }
}