using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DCL;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class CombineLayerUtilsCan
{
    private static CombineLayerComparer comparer = new CombineLayerComparer();

    [Test]
    public void Slice()
    {
        // Arrange
        List<SkinnedMeshRenderer> skrList = new List<SkinnedMeshRenderer>();

        const int RENDERER_COUNT = 10;

        for ( int i = 0; i < RENDERER_COUNT; i++ )
        {
            SkinnedMeshRenderer r = null;

            Texture2D texAlbedo = new Texture2D(1, 1);
            Texture2D texEmission = new Texture2D(1, 1);

            if ( i % 4 == 0 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(albedo: texAlbedo, emission: texEmission);
            else if ( i % 4 == 1 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(albedo: texAlbedo, emission: texEmission);
            else if ( i % 4 == 2 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Front, albedo: texAlbedo, emission: texEmission);
            else if ( i % 4 == 3 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(CullMode.Front, albedo: texAlbedo, emission: texEmission);

            skrList.Add(r);
        }

        // Act
        using var result = CombineLayersList.Rent();
        CombineLayerUtils.TrySlice(skrList.ToArray(), result);

        Assert.That(result.Count, Is.EqualTo(4));

        Assert.IsTrue(result.Layers.Any(x =>
            x.cullMode == CullMode.Back
            && x.isOpaque
            && x.Renderers.Count == 3
            && x.textureToId.Count == 6));

        Assert.IsTrue(result.Layers.Any(x =>
            x.cullMode == CullMode.Front
            && x.isOpaque
            && x.Renderers.Count == 2
            && x.textureToId.Count == 4));

        Assert.IsTrue(result.Layers.Any(x =>
            x.cullMode == CullMode.Back
            && !x.isOpaque
            && x.Renderers.Count == 3
            && x.textureToId.Count == 6));

        Assert.IsTrue(result.Layers.Any(x =>
            x.cullMode == CullMode.Front
            && !x.isOpaque
            && x.Renderers.Count == 2
            && x.textureToId.Count == 4));

        for ( int i = 0; i < skrList.Count; i++ )
        {
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(skrList[i]);
        }
    }

    [Test]
    public void SubsliceLayerByTexturesWithTooManyTextures()
    {
        // Arrange
        List<SkinnedMeshRenderer> skrList = new List<SkinnedMeshRenderer>();
        List<Texture2D> albedoList = new List<Texture2D>();
        List<Texture2D> emissionList = new List<Texture2D>();

        const int RENDERER_COUNT = 10;

        for ( int i = 0; i < RENDERER_COUNT; i++ )
        {
            Texture2D albedoTex = new Texture2D(1, 1);
            Texture2D emissionTex = new Texture2D(1, 1);
            var r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Back, albedoTex, emissionTex);
            skrList.Add(r);
            albedoList.Add(albedoTex);
            emissionList.Add(emissionTex);
        }

        using var result = CombineLayersList.Rent();
        CombineLayer layer = CombineLayer.Rent(CullMode.Back, true);
        result.AddRenderers(layer, skrList);

        // Act
        CombineLayerUtils.SubsliceLayerByTextures(layer, result);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].textureToId.Count, Is.EqualTo(12));
        Assert.That(result[1].textureToId.Count, Is.EqualTo(8));

        int idCounter = 0;

        for ( int i = 0; i < 6; i++)
        {
            Assert.That(result[0].textureToId[albedoList[i]], Is.EqualTo(idCounter), "id count failed " + i);
            idCounter++;
            Assert.That(result[0].textureToId[emissionList[i]], Is.EqualTo(idCounter), "id count failed " + i);
            idCounter++;
        }

        idCounter = 0;

        for ( int i = 6; i < 10; i++)
        {
            Assert.That(result[1].textureToId[albedoList[i]], Is.EqualTo(idCounter), "id count failed " + i);
            idCounter++;
            Assert.That(result[1].textureToId[emissionList[i]], Is.EqualTo(idCounter), "id count failed " + i);
            idCounter++;
        }

        for ( int i = 0; i < skrList.Count; i++ )
        {
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(skrList[i]);
        }
    }

    [Test]
    public void SubsliceLayerByTexturesWithEnoughTextures()
    {
        // Arrange
        List<SkinnedMeshRenderer> skrList = new List<SkinnedMeshRenderer>();
        List<Texture2D> albedoList = new List<Texture2D>();
        List<Texture2D> emissionList = new List<Texture2D>();

        const int RENDERER_COUNT = 3;

        for ( int i = 0; i < RENDERER_COUNT; i++ )
        {
            Texture2D albedoTex = new Texture2D(1, 1);
            Texture2D emissionTex = new Texture2D(1, 1);
            var r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Back, albedoTex, emissionTex);
            skrList.Add(r);
            albedoList.Add(albedoTex);
            emissionList.Add(emissionTex);
        }

        using var result = CombineLayersList.Rent();
        CombineLayer layer = CombineLayer.Rent(CullMode.Back, true, skrList);

        // Act
        CombineLayerUtils.SubsliceLayerByTextures(layer, result);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].textureToId.Count, Is.EqualTo(RENDERER_COUNT * 2));
        Assert.That(result[0].textureToId[albedoList[0]], Is.EqualTo(0));
        Assert.That(result[0].textureToId[emissionList[0]], Is.EqualTo(1));

        for ( int i = 0; i < skrList.Count; i++ )
        {
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(skrList[i]);
        }
    }

    [Test]
    public void SliceByRenderStateWithOpaquesAndTransparencies()
    {
        // Arrange
        List<SkinnedMeshRenderer> skrList = new List<SkinnedMeshRenderer>();

        const int RENDERER_COUNT = 10;

        for ( int i = 0; i < RENDERER_COUNT; i++ )
        {
            SkinnedMeshRenderer r = null;

            if ( i % 2 == 0 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Back, null, null);
            else
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(CullMode.Back, null, null);

            skrList.Add(r);
        }

        // Act
        var result = new List<CombineLayer>();
        SliceByRenderState.Execute(skrList, result, false);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[0].isOpaque, Is.True);
        Assert.That(result[1].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[1].isOpaque, Is.False);

        for ( int i = 0; i < skrList.Count; i++ )
        {
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(skrList[i]);
        }
    }

    [Test]
    public void SliceByRenderStateWithCullState()
    {
        // Arrange
        List<SkinnedMeshRenderer> skrList = new List<SkinnedMeshRenderer>();

        const int RENDERER_COUNT = 10;

        for ( int i = 0; i < RENDERER_COUNT; i++ )
        {
            SkinnedMeshRenderer r = null;

            if ( i % 2 == 0 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Back, null, null);
            else
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Front, null, null);

            skrList.Add(r);
        }

        // Act
        var result = new List<CombineLayer>();
        SliceByRenderState.Execute(skrList, result, false);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[0].cullMode, Is.EqualTo(CullMode.Back));
        Assert.That(result[0].isOpaque, Is.True);
        Assert.That(result[1].cullMode, Is.EqualTo(CullMode.Front));
        Assert.That(result[1].isOpaque, Is.True);

        for ( int i = 0; i < skrList.Count; i++ )
        {
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(skrList[i]);
        }
    }

    private class CombineLayerComparer : IEqualityComparer<CombineLayer>
    {
        public bool Equals(CombineLayer x, CombineLayer y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.cullMode == y.cullMode && x.isOpaque == y.isOpaque
                                            && x.Renderers.Count == y.Renderers.Count
                                            && x.textureToId.Count == y.textureToId.Count;
        }

        public int GetHashCode(CombineLayer obj)
        {
            return HashCode.Combine((int)obj.cullMode, obj.isOpaque, obj.Renderers.Count, obj.textureToId.Count);
        }
    }

    [Test]
    public void SliceByRenderStateWithEverythingMixed()
    {
        // Arrange
        List<SkinnedMeshRenderer> skrList = new List<SkinnedMeshRenderer>();

        const int RENDERER_COUNT = 10;

        for ( int i = 0; i < RENDERER_COUNT; i++ )
        {
            SkinnedMeshRenderer r = null;

            if ( i % 4 == 0 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Back, null, null);
            else if ( i % 4 == 1 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(CullMode.Back, null, null);
            else if ( i % 4 == 2 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Front, null, null);
            else if ( i % 4 == 3 )
                r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(CullMode.Front, null, null);

            skrList.Add(r);
        }

        // Act
        var result = new List<CombineLayer>();
        SliceByRenderState.Execute(skrList, result, false);

        var expected = new List<CombineLayer>
        {
            CombineLayer.Rent(CullMode.Back, true, new SkinnedMeshRenderer[3]),
            CombineLayer.Rent(CullMode.Back, false, new SkinnedMeshRenderer[3]),
            CombineLayer.Rent(CullMode.Front, true, new SkinnedMeshRenderer[2]),
            CombineLayer.Rent(CullMode.Front, false, new SkinnedMeshRenderer[2]),
        };

        Assert.That(result, Is.EquivalentTo(expected).Using(comparer));

        for ( int i = 0; i < skrList.Count; i++ )
        {
            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(skrList[i]);
        }
    }

    [Test]
    public void GetMapIdsWithUniqueTextures()
    {
        // Arrange
        Dictionary<Texture2D, int> textures = new Dictionary<Texture2D, int>();

        Material[] mats = new Material[]
        {
            DCL.Helpers.Material.Create(CullMode.Back, Texture2D.whiteTexture, Texture2D.blackTexture),
            DCL.Helpers.Material.Create(CullMode.Back, Texture2D.redTexture, Texture2D.grayTexture),
            DCL.Helpers.Material.Create(CullMode.Back, Texture2D.normalTexture, Texture2D.linearGrayTexture)
        };

        // Act
        var result = new Dictionary<Texture2D, int>();
        CombineLayerUtils.AddMapIds(textures, result, mats, 0);

        // Assert
        Assert.That(result.Count, Is.EqualTo(6));

        foreach ( Material mat in mats )
        {
            Object.Destroy(mat);
        }
    }

    [Test]
    public void GetMapIdsWithExistingTextures()
    {
        // Arrange
        Dictionary<Texture2D, int> textures = new Dictionary<Texture2D, int>();

        Material[] mats = new Material[]
        {
            DCL.Helpers.Material.CreateOpaque(CullMode.Back, Texture2D.whiteTexture, Texture2D.blackTexture),
            DCL.Helpers.Material.CreateOpaque(CullMode.Back, Texture2D.redTexture, Texture2D.grayTexture),
            DCL.Helpers.Material.CreateOpaque(CullMode.Back, Texture2D.normalTexture, Texture2D.linearGrayTexture)
        };

        textures.Add( Texture2D.whiteTexture, 0 );
        textures.Add( Texture2D.blackTexture, 1 );

        // Act
        var result = new Dictionary<Texture2D, int>();
        CombineLayerUtils.AddMapIds(textures, result, mats, 2);

        // Assert
        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(textures.Count, Is.EqualTo(2));
        Assert.That(result.ContainsValue(5), Is.True);
        Assert.That(result.ContainsValue(0), Is.False);
        Assert.That(result.ContainsValue(1), Is.False);

        foreach ( Material mat in mats )
        {
            Object.Destroy(mat);
        }
    }

    [Test]
    public void ReturnCorrectValuesForIsOpaque()
    {
        {
            // Arrange
            SkinnedMeshRenderer r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(CullMode.Back, null, null);

            // Act
            bool isOpaque = SliceByRenderState.IsOpaque(r.sharedMaterials[0]);

            // Assert
            Assert.That(isOpaque, Is.True);

            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(r);
        }

        {
            // Arrange
            SkinnedMeshRenderer r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(CullMode.Back, null, null);

            // Act
            bool isOpaque = SliceByRenderState.IsOpaque(r.sharedMaterials[0]);

            // Assert
            Assert.That(isOpaque, Is.False);

            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(r);
        }
    }

    [Test]
    [TestCase(CullMode.Back, CullMode.Back)]
    [TestCase(CullMode.Front, CullMode.Front)]
    [TestCase(CullMode.Off, CullMode.Off)]
    public void ReturnCorrectValuesForGetCullMode(CullMode cullMode, CullMode expectedResult)
    {
        // Arrange
        SkinnedMeshRenderer r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(cullMode, null, null);

        // Act
        CullMode resultMode = SliceByRenderState.GetCullMode(r.sharedMaterials[0]);

        // Assert
        Assert.That( resultMode, Is.EqualTo(expectedResult));

        DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(r);
    }

    [Test]
    [TestCase(CullMode.Back, CullMode.Back)]
    [TestCase(CullMode.Front, CullMode.Front)]
    [TestCase(CullMode.Off, CullMode.Back)]
    public void ReturnCorrectValuesForGetCullModeWithoutCullOff(CullMode cullMode, CullMode expectedResult)
    {
        // Arrange
        SkinnedMeshRenderer r = DCL.Helpers.SkinnedMeshRenderer.CreateWithOpaqueMat(cullMode, null, null);

        // Act
        CullMode resultMode = SliceByRenderState.GetCullModeWithoutCullOff(r);

        // Assert
        Assert.That( resultMode, Is.EqualTo(expectedResult));

        DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(r);
    }
}
