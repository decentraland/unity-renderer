using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public static class MaterialUtils
    {
        public static void UpdateMaterialFromRendereable(string sceneId, string entityId, Renderer renderer, Material oldMaterial, Material material)
        {
            var dataStore = DataStore.i.sceneWorldObjects;
            Rendereable r = dataStore.GetRendereableByRenderer(sceneId, entityId, renderer);

            if ( r == null )
                return;

            dataStore.RemoveRendereable(sceneId, r);

            if ( oldMaterial != null && r.materials.Contains(oldMaterial))
                r.materials.Remove(oldMaterial);

            if ( !r.materials.Contains(material))
                r.materials.Add(material);

            r.textures = MeshesInfoUtils.ExtractUniqueTextures(r.materials);
            dataStore.AddRendereable(sceneId, r);
        }

        public static void RemoveMaterialFromRendereable(string sceneId, string entityId, Renderer renderer, Material material)
        {
            var dataStore = DataStore.i.sceneWorldObjects;
            Rendereable r = dataStore.GetRendereableByRenderer(sceneId, entityId, renderer);

            if (r == null)
                return;

            dataStore.RemoveRendereable(sceneId, r);

            if ( r.materials.Contains(material))
                r.materials.Remove(material);

            r.textures = MeshesInfoUtils.ExtractUniqueTextures(r.materials);
            dataStore.AddRendereable(sceneId, r);
        }
    }
}