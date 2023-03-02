using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class NFTShapeVisualTests : VisualTestsBase
{
    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator NFTShapeVisualTests1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(NFTShapeVisualTests1()); }

    [UnityTest]
    [VisualTest]
    [Explicit("Works locally but breaks in CI")]
    [Category("Visual Tests")]
    public IEnumerator NFTShapeVisualTests1()
    {
        yield return SpawnNFTShapes(0, 6, 3);

        yield return VisualTestUtils.TakeSnapshot("NFTShapeVisualTest1", camera, new Vector3(2f, 1.5f, 6.25f));
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator NFTShapeVisualTests2_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(NFTShapeVisualTests2()); }

    [UnityTest]
    [VisualTest]
    [Explicit("Works locally but breaks in CI")]
    [Category("Visual Tests")]
    public IEnumerator NFTShapeVisualTests2()
    {
        yield return SpawnNFTShapes(6, 6, 3);

        yield return VisualTestUtils.TakeSnapshot("NFTShapeVisualTest2", camera, new Vector3(2f, 1.5f, 6.25f));
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator NFTShapeVisualTests3_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(NFTShapeVisualTests3()); }

    [UnityTest]
    [VisualTest]
    [Explicit("Works locally but breaks in CI")]
    [Category("Visual Tests")]
    public IEnumerator NFTShapeVisualTests3()
    {
        yield return SpawnNFTShapes(12, 6, 3);

        yield return VisualTestUtils.TakeSnapshot("NFTShapeVisualTest3", camera, new Vector3(2f, 1.5f, 6.25f));
    }

    [UnityTest]
    [VisualTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator NFTShapeVisualTests4_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(NFTShapeVisualTests4()); }

    [UnityTest]
    [VisualTest]
    [Explicit("Works locally but breaks in CI")]
    [Category("Visual Tests")]
    public IEnumerator NFTShapeVisualTests4()
    {
        yield return SpawnNFTShapes(18, 5, 3);

        yield return VisualTestUtils.TakeSnapshot("NFTShapeVisualTest4", camera, new Vector3(2f, 1.5f, 6.25f));
    }

    IEnumerator SpawnNFTShapes(int startingTypeIndex, int nftShapeTypesAmount, int maxColumns)
    {
        int currentCol = 0;
        int currentRow = 0;
        int prevRow = 0;
        float verticalOffset = 1f;

        for (int i = 0; i < nftShapeTypesAmount; i++)
        {
            currentRow = (i / maxColumns);
            if (prevRow != currentRow)
                currentCol = 0;

            currentCol++;

            yield return InstantiateNFTShape(new Vector3(currentCol, currentRow + verticalOffset, 8), startingTypeIndex + i);

            prevRow = currentRow;
        }
    }

    IEnumerator InstantiateNFTShape(Vector3 position, int styleIndex)
    {
        var entity = TestUtils.CreateSceneEntity(scene);

        TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = position });

        yield return null;

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536",
            color = Color.gray,
            style = styleIndex
        };

        NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        yield return component.routine;

        TestUtils.SharedComponentAttach(component, entity);

        // Override texture with a local test one
        var nftShape = Environment.i.world.state.GetLoaderForEntity(entity) as LoadWrapper_NFT;
        nftShape.loaderController.UpdateTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/TestTexture.png"));
        nftShape.loaderController.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    }
}
