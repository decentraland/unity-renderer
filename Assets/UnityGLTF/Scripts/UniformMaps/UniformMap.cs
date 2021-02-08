using UnityEngine;

namespace UnityGLTF
{
    interface IUniformMap
    {
        Material Material { get; }

        Texture NormalTexture { get; set; }
        int NormalTexCoord { get; set; }
        double NormalTexScale { get; set; }

        Texture OcclusionTexture { get; set; }
        int OcclusionTexCoord { get; set; }
        double OcclusionTexStrength { get; set; }

        Texture EmissiveTexture { get; set; }
        int EmissiveTexCoord { get; set; }
        Color EmissiveFactor { get; set; }

        GLTF.Schema.AlphaMode AlphaMode { get; set; }
        double AlphaCutoff { get; set; }
        bool DoubleSided { get; set; }
        bool VertexColorsEnabled { get; set; }

        IUniformMap Clone();

        Material GetMaterialCopy();

    }

    interface IMetalRoughUniformMap : IUniformMap
    {
        Texture BaseColorTexture { get; set; }
        int BaseColorTexCoord { get; set; }
        Vector2 BaseColorXOffset { get; set; }
        Vector2 BaseColorXScale { get; set; }
        Color BaseColorFactor { get; set; }

        Texture MetallicRoughnessTexture { get; set; }
        int MetallicRoughnessTexCoord { get; set; }
        double MetallicFactor { get; set; }
        double RoughnessFactor { get; set; }
    }

    interface ISpecGlossUniformMap : IUniformMap
    {
        Texture DiffuseTexture { get; set; }
        int DiffuseTexCoord { get; set; }
        Vector2 DiffuseXOffset { get; set; }
        Vector2 DiffuseXScale { get; set; }
        Color DiffuseFactor { get; set; }
        Texture SpecularGlossinessTexture { get; set; }
        int SpecularGlossinessTexCoord { get; set; }
        Vector3 SpecularFactor { get; set; }
        double GlossinessFactor { get; set; }
    }

    interface IUnlitUniformMap : IUniformMap
    {
        Texture BaseColorTexture { get; set; }
        int BaseColorTexCoord { get; set; }
        Color BaseColorFactor { get; set; }
    }
}
