using UnityEngine;

namespace DCL
{
    public class DataStore_WorldObjects
    {
        public class SceneData
        {
            public readonly BaseHashSet<string> ignoredOwners = new BaseHashSet<string>();

            public readonly BaseRefCounter<Mesh> meshes = new BaseRefCounter<Mesh>();
            public readonly BaseRefCounter<Material> materials = new BaseRefCounter<Material>();
            public readonly BaseRefCounter<Texture> textures = new BaseRefCounter<Texture>();
            public readonly BaseHashSet<string> owners = new BaseHashSet<string>();
            public readonly BaseHashSet<Renderer> renderers = new BaseHashSet<Renderer>();
            public readonly BaseHashSet<AudioClip> audioClips = new BaseHashSet<AudioClip>();

            public readonly BaseVariable<int> triangles = new BaseVariable<int>();
            public readonly BaseVariable<long> animationClipSize = new BaseVariable<long>();
            public readonly BaseVariable<long> meshDataSize = new BaseVariable<long>();
        }

        public readonly BaseDictionary<string, SceneData> sceneData = new BaseDictionary<string, SceneData>();
    }
}