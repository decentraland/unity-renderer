using System.Collections.Generic;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public static class DataStore_Rendering_Extensions
    {
        public static void AddMesh( this DataStore.DataStore_Rendering self, string sceneId, Mesh mesh )
        {
            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if ( sceneMeshes.ContainsKey(mesh))
                sceneMeshes[mesh]++;
            else
                sceneMeshes.Add(mesh, 0);
        }

        public static void RemoveMesh( this DataStore.DataStore_Rendering self, string sceneId, Mesh mesh )
        {
            BaseDictionary<Mesh, int> sceneMeshes = self.sceneData[sceneId].refCountedMeshes;

            if ( !sceneMeshes.ContainsKey(mesh))
                return;

            sceneMeshes[mesh]--;

            if (sceneMeshes[mesh] == 0)
                sceneMeshes.Remove(mesh);
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
    }
}