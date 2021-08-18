using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DCL;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CombineLayerUtilsCan
{
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
        List<CombineLayer> result = DCL.CombineLayerUtils.Slice(skrList.ToArray());

        // Assert
        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result[0].textureToId.Count, Is.EqualTo(6));
        Assert.That(result[0].renderers.Count, Is.EqualTo(3));
        Assert.That(result[0].isOpaque, Is.True);
        Assert.That(result[0].cullMode, Is.EqualTo(CullMode.Back));

        Assert.That(result[1].textureToId.Count, Is.EqualTo(4));
        Assert.That(result[1].renderers.Count, Is.EqualTo(2));
        Assert.That(result[1].isOpaque, Is.True);
        Assert.That(result[1].cullMode, Is.EqualTo(CullMode.Front));

        Assert.That(result[2].textureToId.Count, Is.EqualTo(6));
        Assert.That(result[2].renderers.Count, Is.EqualTo(3));
        Assert.That(result[2].isOpaque, Is.False);
        Assert.That(result[2].cullMode, Is.EqualTo(CullMode.Back));

        Assert.That(result[3].textureToId.Count, Is.EqualTo(4));
        Assert.That(result[3].renderers.Count, Is.EqualTo(2));
        Assert.That(result[3].isOpaque, Is.False);
        Assert.That(result[3].cullMode, Is.EqualTo(CullMode.Front));

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

        CombineLayer layer = new CombineLayer();
        layer.cullMode = CullMode.Back;
        layer.isOpaque = true;
        layer.renderers = skrList;

        // Act
        var result = DCL.CombineLayerUtils.SubsliceLayerByTextures(layer);

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

        CombineLayer layer = new CombineLayer();
        layer.cullMode = CullMode.Back;
        layer.isOpaque = true;
        layer.renderers = skrList;

        // Act
        var result = DCL.CombineLayerUtils.SubsliceLayerByTextures(layer);

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
        var result = DCL.CombineLayerUtils.SliceByRenderState(skrList.ToArray());

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
        var result = DCL.CombineLayerUtils.SliceByRenderState(skrList.ToArray());

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
        var result = DCL.CombineLayerUtils.SliceByRenderState(skrList.ToArray());

        // Assert
        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result[0].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[0].cullMode, Is.EqualTo(CullMode.Back));
        Assert.That(result[0].isOpaque, Is.True);

        Assert.That(result[1].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[1].cullMode, Is.EqualTo(CullMode.Front));
        Assert.That(result[1].isOpaque, Is.True);

        Assert.That(result[2].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[2].cullMode, Is.EqualTo(CullMode.Back));
        Assert.That(result[2].isOpaque, Is.False);

        Assert.That(result[3].textureToId.Count, Is.EqualTo(0));
        Assert.That(result[3].cullMode, Is.EqualTo(CullMode.Front));
        Assert.That(result[3].isOpaque, Is.False);

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
        var result = DCL.CombineLayerUtils.GetMapIds(new ReadOnlyDictionary<Texture2D, int>(textures), mats, 0);

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
        var result = DCL.CombineLayerUtils.GetMapIds(new ReadOnlyDictionary<Texture2D, int>(textures), mats, 2);

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
            bool isOpaque = DCL.CombineLayerUtils.IsOpaque(r.sharedMaterials[0]);

            // Assert
            Assert.That(isOpaque, Is.True);

            DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(r);
        }

        {
            // Arrange
            SkinnedMeshRenderer r = DCL.Helpers.SkinnedMeshRenderer.CreateWithTransparentMat(CullMode.Back, null, null);

            // Act
            bool isOpaque = DCL.CombineLayerUtils.IsOpaque(r.sharedMaterials[0]);

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
        CullMode resultMode = DCL.CombineLayerUtils.GetCullMode(r.sharedMaterials[0]);

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
        CullMode resultMode = DCL.CombineLayerUtils.GetCullModeWithoutCullOff(r);

        // Assert
        Assert.That( resultMode, Is.EqualTo(expectedResult));

        DCL.Helpers.SkinnedMeshRenderer.DestroyAndUnload(r);
    }
}