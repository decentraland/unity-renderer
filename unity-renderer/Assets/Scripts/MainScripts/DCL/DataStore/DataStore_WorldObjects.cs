using UnityEngine;

namespace DCL
{
    public class DataStore_WorldObjects
    {
        public class SceneData
        {
            public readonly BaseHashSet<long> ignoredOwners = new BaseHashSet<long>();

            public readonly BaseRefCounter<Mesh> meshes = new BaseRefCounter<Mesh>();
            public readonly BaseRefCounter<Material> materials = new BaseRefCounter<Material>();
            public readonly BaseRefCounter<Texture> textures = new BaseRefCounter<Texture>();
            public readonly BaseHashSet<long> owners = new BaseHashSet<long>();
            public readonly BaseHashSet<Renderer> renderers = new BaseHashSet<Renderer>();
            public readonly BaseHashSet<AudioClip> audioClips = new BaseHashSet<AudioClip>();
            public readonly BaseRefCounter<AnimationClip> animationClips = new BaseRefCounter<AnimationClip>();

            public readonly BaseVariable<int> triangles = new BaseVariable<int>();
            public readonly BaseVariable<long> animationClipSize = new BaseVariable<long>();
            public readonly BaseVariable<long> meshDataSize = new BaseVariable<long>();
        }

        public readonly BaseDictionary<int, SceneData> sceneData = new BaseDictionary<int, SceneData>();
    }
}
