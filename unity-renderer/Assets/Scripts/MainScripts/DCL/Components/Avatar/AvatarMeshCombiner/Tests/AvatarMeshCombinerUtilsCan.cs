using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;
using SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer;

public class AvatarMeshCombinerUtilsCan
{
    [Test]
    public void ResetBones()
    {
        // Arrange
        SkinnedMeshRenderer r = null;

        // Act
        //AvatarMeshCombinerUtils.ResetBones(r);

        // Assert
        Assert.That(true, Is.False);
    }

    [Test]
    public void ComputeBoneWeights()
    {
        // Arrange
        var layers = new List<CombineLayer>();
        int counter = 0;

        SkinnedMeshRenderer CreateRendererWithWeights()
        {
            Material mat = DCL.Helpers.Material.Create(CullMode.Back, null, null);
            SkinnedMeshRenderer renderer = DCL.Helpers.SkinnedMeshRenderer.Create(mat);

            int vertexCount = renderer.sharedMesh.vertexCount;
            var boneWeights = new BoneWeight[vertexCount];

            for (int i = 0; i < boneWeights.Length; i++)
            {
                var b = new BoneWeight();
                b.weight0 = counter;
                b.boneIndex1 = counter;
                boneWeights[i] = b;
                counter++;
            }

            renderer.sharedMesh.boneWeights = boneWeights;
            return renderer;
        }

        for ( int i = 0; i < 2; i++ )
        {
            layers.Add(
                new CombineLayer()
                {
                    renderers = new List<SkinnedMeshRenderer>()
                    {
                        CreateRendererWithWeights(),
                        CreateRendererWithWeights(),
                    }
                }
            );
        }

        // Act
        var result = AvatarMeshCombinerUtils.ComputeBoneWeights(layers);

        // Assert
        Assert.That(result.Count, Is.EqualTo(96));

        int assertCounter = 0;

        foreach ( var b in result )
        {
            Assert.That(b.weight0, Is.EqualTo(assertCounter));
            Assert.That(b.boneIndex1, Is.EqualTo(assertCounter));
            assertCounter++;
        }

        Assert.That(assertCounter, Is.EqualTo(counter));

        layers.SelectMany( (x) => x.renderers ).ToList().ForEach(
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload
        );
    }

    [Test]
    public void FlattenMaterialsWithOpaques()
    {
        // Arrange
        // TODO(Brian): Construct layers manually
        Material material = DCL.Helpers.Material.CreateOpaque();
        var layers = GetMockedLayers(isOpaque: true);

        // Act
        FlattenedMaterialsData result = AvatarMeshCombinerUtils.FlattenMaterials(layers, material);

        // Assert
        FlattenedMaterialsData expected = new FlattenedMaterialsData();
        expected.colors = new List<Color>();
        expected.texturePointers = new List<Vector3>();
        expected.emissionColors = new List<Vector4>();

        expected.colors.AddRange(Enumerable.Repeat(Color.blue, 24));
        expected.emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.yellow, 24));
        expected.texturePointers.AddRange(Enumerable.Repeat(new Vector3(6, 11, 0.5f), 24));

        expected.colors.AddRange(Enumerable.Repeat(Color.red, 24));
        expected.emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.white, 24));
        expected.texturePointers.AddRange(Enumerable.Repeat(new Vector3(4, 10, 0.5f), 24));

        CollectionAssert.AreEquivalent(expected.colors, result.colors);
        CollectionAssert.AreEquivalent(expected.texturePointers, result.texturePointers);
        CollectionAssert.AreEquivalent(expected.emissionColors, result.emissionColors);

        // Materials count is 2 because FlattenMaterials doesn't combine layers.
        // Layer combining is CombineLayerUtils.Slice responsibility.
        Assert.That( result.materials.Count, Is.EqualTo(2) );
        Assert.That( CombineLayerUtils.IsOpaque(result.materials[0]), Is.True );
        Assert.That( CombineLayerUtils.IsOpaque(result.materials[1]), Is.True );
    }

    [Test]
    public void FlattenMaterialsWithTransparencies()
    {
        // Arrange
        // TODO(Brian): Construct layers manually
        Material material = DCL.Helpers.Material.CreateTransparent();
        var layers = GetMockedLayers(isOpaque: false);

        // Act
        FlattenedMaterialsData result = AvatarMeshCombinerUtils.FlattenMaterials(layers, material);

        // Assert
        FlattenedMaterialsData expected = new FlattenedMaterialsData();
        expected.colors = new List<Color>();
        expected.texturePointers = new List<Vector3>();
        expected.emissionColors = new List<Vector4>();

        expected.colors.AddRange(Enumerable.Repeat(Color.blue, 24));
        expected.emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.yellow, 24));
        expected.texturePointers.AddRange(Enumerable.Repeat(new Vector3(6, 11, 0.5f), 24));

        expected.colors.AddRange(Enumerable.Repeat(Color.red, 24));
        expected.emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.white, 24));
        expected.texturePointers.AddRange(Enumerable.Repeat(new Vector3(4, 10, 0.5f), 24));

        CollectionAssert.AreEquivalent(expected.colors, result.colors);
        CollectionAssert.AreEquivalent(expected.texturePointers, result.texturePointers);
        CollectionAssert.AreEquivalent(expected.emissionColors, result.emissionColors);

        // Materials count is 2 because FlattenMaterials doesn't combine layers.
        // Layer combining is CombineLayerUtils.Slice responsibility.
        Assert.That( result.materials.Count, Is.EqualTo(2) );
        Assert.That( CombineLayerUtils.IsOpaque(result.materials[0]), Is.False );
        Assert.That( CombineLayerUtils.IsOpaque(result.materials[1]), Is.False );
    }

    [Test]
    public void ComputeSubMeshes()
    {
        // Arrange
        var layers = new List<CombineLayer>();
        layers.Add( new CombineLayer() );
        layers.Add( new CombineLayer() );

        Material tmpMat = DCL.Helpers.Material.Create();

        layers[0].renderers.Add(DCL.Helpers.SkinnedMeshRenderer.Create(tmpMat));
        layers[0].renderers.Add(DCL.Helpers.SkinnedMeshRenderer.Create(tmpMat));

        layers[1].renderers.Add(DCL.Helpers.SkinnedMeshRenderer.Create(tmpMat));
        layers[1].renderers.Add(DCL.Helpers.SkinnedMeshRenderer.Create(tmpMat));

        // Act
        var result = AvatarMeshCombinerUtils.ComputeSubMeshes(layers);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].indexStart, Is.EqualTo(0));
        Assert.That(result[0].indexCount, Is.EqualTo(72));
        Assert.That(result[0].vertexCount, Is.EqualTo(48));

        Assert.That(result[1].indexStart, Is.EqualTo(72));
        Assert.That(result[1].indexCount, Is.EqualTo(72));
        Assert.That(result[1].vertexCount, Is.EqualTo(48));
    }

    public void ComputeCombineInstancesData()
    {
        // Arrange
        var layers = new List<CombineLayer>();

        // Act
        AvatarMeshCombinerUtils.ComputeCombineInstancesData(layers);

        // Assert
        Assert.That(true, Is.False);
    }

    private List<CombineLayer> GetMockedLayers(bool isOpaque = true)
    {
        var layers = new List<CombineLayer>();
        layers.Add(new CombineLayer());
        layers.Add(new CombineLayer());


        // Material 1 setup
        //
        Material material1 = null;
        var albedo1 = Texture2D.grayTexture;
        var emission1 = Texture2D.redTexture;

        if ( isOpaque )
            material1 = DCL.Helpers.Material.CreateOpaque(CullMode.Back, albedo1, emission1);
        else
            material1 = DCL.Helpers.Material.CreateTransparent(CullMode.Back, albedo1, emission1);

        material1.SetColor( ShaderUtils.BaseColor, Color.blue);
        material1.SetColor( ShaderUtils.EmissionColor, Color.yellow);

        // Material 2 setup
        //
        Material material2 = null;
        var albedo2 = Texture2D.grayTexture;
        var emission2 = Texture2D.redTexture;

        if ( isOpaque )
            material2 = DCL.Helpers.Material.CreateOpaque(CullMode.Back, albedo2, emission2);
        else
            material2 = DCL.Helpers.Material.CreateTransparent(CullMode.Back, albedo2, emission2);

        material2.SetColor( ShaderUtils.BaseColor, Color.red);
        material2.SetColor( ShaderUtils.EmissionColor, Color.white);

        // Layer setup
        //
        layers[0].renderers = new List<SkinnedMeshRenderer>() { DCL.Helpers.SkinnedMeshRenderer.Create(material1) };
        layers[0].textureToId = new Dictionary<Texture2D, int>()
        {
            { albedo1, 6 },
            { emission1, 11 }
        };
        layers[0].cullMode = CullMode.Back;
        layers[0].isOpaque = isOpaque;

        layers[1].renderers = new List<SkinnedMeshRenderer>() { DCL.Helpers.SkinnedMeshRenderer.Create(material2) };
        layers[1].textureToId = new Dictionary<Texture2D, int>()
        {
            { albedo2, 4 },
            { emission2, 10 }
        };
        layers[1].cullMode = CullMode.Back;
        layers[1].isOpaque = isOpaque;

        return layers;
    }
}