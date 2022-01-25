﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Material = UnityEngine.Material;
using SkinnedMeshRenderer = UnityEngine.SkinnedMeshRenderer;

public class AvatarMeshCombinerUtilsCan
{
    [UnityTest]
    public IEnumerator ResetBones()
    {
        GameObject parentMock = new GameObject();
        parentMock.transform.position = new Vector3(1000, 100, 100);
        parentMock.transform.localScale = new Vector3(1, 2, 0.5f);

        GameObject parentMock2 = new GameObject();
        parentMock2.transform.position = new Vector3(1000, 100, 100);
        parentMock2.transform.localScale = new Vector3(0.5f, 2, 0.5f);
        parentMock2.transform.parent = parentMock.transform;

        var keeper = new AssetPromiseKeeper_GLTF();
        keeper.throttlingCounter.enabled = false;

        string url = TestAssetsUtils.GetPath() + "/Avatar/Assets/BaseMale.glb";
        WebRequestController webRequestController = WebRequestController.Create();
        AssetPromise_GLTF prom = new AssetPromise_GLTF(url, webRequestController);
        prom.settings.parent = parentMock2.transform;
        keeper.Keep(prom);
        yield return prom;

        // Arrange
        SkinnedMeshRenderer renderer = prom.asset.container.GetComponentInChildren<SkinnedMeshRenderer>();

        Randomizer randomizer = new Randomizer(0);

        for (int i = 0; i < renderer.bones.Length; i++)
        {
            Transform bone = renderer.bones[i];

            float x = randomizer.NextFloat(-10, 10);
            float y = randomizer.NextFloat(-10, 10);
            float z = randomizer.NextFloat(-10, 10);

            bone.position += new Vector3(x, y, z);
        }

        // Act
        AvatarMeshCombinerUtils.ResetBones(renderer.sharedMesh.bindposes, renderer.bones);


        // Assert
        var expectedBonePositions = new Vector3[]
        {
            new Vector3(0f, 0f, -99.99998f), new Vector3(-8.91f, 0f, -93.72998f),
            new Vector3(-8.91f, 1.007904f, -48.85135f), new Vector3(-8.91f, 0f, -8.15039f),
            new Vector3(-8.910008f, 12.95472f, -1.888074f), new Vector3(8.91f, 0f, -93.72998f),
            new Vector3(8.91f, 1.007904f, -48.85135f), new Vector3(8.91f, 0f, -8.15039f),
            new Vector3(8.910008f, 12.95472f, -1.888074f), new Vector3(0f, 0f, -107f), new Vector3(0f, 0f, -121.0785f),
            new Vector3(0f, 0f, -135.1571f), new Vector3(-6.999999f, 0f, -146.5885f),
            new Vector3(-17.70724f, 0f, -146.5885f), new Vector3(-45.01273f, -0.999176f, -146.5886f),
            new Vector3(-71.70988f, 0f, -146.8592f), new Vector3(-74.76115f, 5.100403f, -144.984f),
            new Vector3(-77.27374f, 5.807861f, -144.4479f), new Vector3(-80.09776f, 6.402435f, -144.4511f),
            new Vector3(-82.78875f, 6.938522f, -144.4545f), new Vector3(-80.53186f, 3.951172f, -147.0865f),
            new Vector3(-85.71362f, 4.098373f, -147.0865f), new Vector3(-89.12752f, 4.190826f, -147.0865f),
            new Vector3(-91.2538f, 4.259048f, -147.0864f), new Vector3(-80.51976f, 1.361099f, -147.3876f),
            new Vector3(-85.49585f, 1.361404f, -147.3876f), new Vector3(-89.36112f, 1.361572f, -147.3876f),
            new Vector3(-91.36678f, 1.361694f, -147.3877f), new Vector3(-80.80108f, -1.498306f, -147.2667f),
            new Vector3(-84.88754f, -1.498306f, -147.2667f), new Vector3(-88.15109f, -1.498306f, -147.2667f),
            new Vector3(-90.27187f, -1.498306f, -147.2667f), new Vector3(-80.67677f, -4.097702f, -146.7023f),
            new Vector3(-84.17112f, -4.180984f, -146.7023f), new Vector3(-86.59622f, -4.236267f, -146.7023f),
            new Vector3(-88.37508f, -4.280792f, -146.7023f), new Vector3(6.999999f, 0f, -146.5885f),
            new Vector3(17.70724f, 0f, -146.5885f), new Vector3(45.01271f, -0.999176f, -146.5886f),
            new Vector3(71.70986f, 0f, -146.8592f), new Vector3(74.76112f, 5.100418f, -144.984f),
            new Vector3(77.27373f, 5.807861f, -144.448f), new Vector3(80.09774f, 6.402435f, -144.4512f),
            new Vector3(82.78872f, 6.938522f, -144.4545f), new Vector3(80.53184f, 3.951172f, -147.0865f),
            new Vector3(85.71358f, 4.098373f, -147.0865f), new Vector3(89.12749f, 4.190826f, -147.0865f),
            new Vector3(91.25376f, 4.259048f, -147.0865f), new Vector3(80.51974f, 1.361099f, -147.3877f),
            new Vector3(85.49581f, 1.361404f, -147.3877f), new Vector3(89.36109f, 1.361572f, -147.3877f),
            new Vector3(91.36671f, 1.361694f, -147.3877f), new Vector3(80.80106f, -1.498306f, -147.2667f),
            new Vector3(84.88752f, -1.498306f, -147.2667f), new Vector3(88.15105f, -1.498306f, -147.2667f),
            new Vector3(90.27183f, -1.498306f, -147.2667f), new Vector3(80.67674f, -4.097702f, -146.7023f),
            new Vector3(84.1711f, -4.180984f, -146.7023f), new Vector3(86.5962f, -4.236267f, -146.7023f),
            new Vector3(88.37507f, -4.280792f, -146.7023f), new Vector3(0f, 0f, -149.2356f),
            new Vector3(0f, 0f, -173.4713f),
        };

        for (int i = 0; i < renderer.bones.Length; i++)
        {
            var bone = renderer.bones[i];
            var position = bone.position;
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(position.x, expectedBonePositions[i].x, 0.01f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(position.y, expectedBonePositions[i].y, 0.01f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(position.z, expectedBonePositions[i].z, 0.01f);
        }

        keeper.Forget(prom);
        webRequestController.Dispose();
        UnityEngine.Object.Destroy(parentMock);
    }

    [Test]
    public void ComputeBoneWeights()
    {
        // Arrange
        var layers = new List<CombineLayer>();
        int counter = 0;

        SkinnedMeshRenderer CreateRendererWithWeights()
        {
            Material mat = DCL.Helpers.Material.Create();
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

        var colors = new List<Vector4>();
        var texturePointers = new List<Vector3>();
        var emissionColors = new List<Vector4>();

        colors.AddRange(Enumerable.Repeat((Vector4)Color.blue, 24));
        emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.yellow, 24));
        texturePointers.AddRange(Enumerable.Repeat(new Vector3(6, 11, 0.5f), 24));

        colors.AddRange(Enumerable.Repeat((Vector4)Color.red, 24));
        emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.white, 24));
        texturePointers.AddRange(Enumerable.Repeat(new Vector3(4, 10, 0.5f), 24));

        expected.colors = colors.ToArray();
        expected.emissionColors = emissionColors.ToArray();
        expected.texturePointers = texturePointers.ToArray();

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

        var colors = new List<Vector4>();
        var texturePointers = new List<Vector3>();
        var emissionColors = new List<Vector4>();

        colors.AddRange(Enumerable.Repeat((Vector4)Color.blue, 24));
        emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.yellow, 24));
        texturePointers.AddRange(Enumerable.Repeat(new Vector3(6, 11, 0.5f), 24));

        colors.AddRange(Enumerable.Repeat((Vector4)Color.red, 24));
        emissionColors.AddRange(Enumerable.Repeat((Vector4)Color.white, 24));
        texturePointers.AddRange(Enumerable.Repeat(new Vector3(4, 10, 0.5f), 24));

        expected.colors = colors.ToArray();
        expected.emissionColors = emissionColors.ToArray();
        expected.texturePointers = texturePointers.ToArray();

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