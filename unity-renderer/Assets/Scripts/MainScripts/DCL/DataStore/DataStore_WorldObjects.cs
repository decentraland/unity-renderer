using UnityEngine;

namespace DCL
{
    public class DataStore_WorldObjects
    {
        public class SceneData
        {
            //public readonly BaseDictionary<Mesh, int> refCountedMeshes = new BaseDictionary<Mesh, int>();
            public readonly BaseHashSet<Rendereable> renderedObjects = new BaseHashSet<Rendereable>();
            public readonly BaseDictionary<string, OwnerData> filteredByOwner = new BaseDictionary<string, OwnerData>();
        }

        public class OwnerData
        {
            public readonly BaseHashSet<Rendereable> rendereables = new BaseHashSet<Rendereable>();
        }

        public readonly BaseDictionary<string, SceneData> sceneData = new BaseDictionary<string, SceneData>();
    }
}