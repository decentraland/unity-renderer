using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvatarSystem;
using DCL;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = UnityEngine.WaitUntil;

public class WearablesVisualTests : VisualTestsBase
{
    private AvatarMeshCombinerHelper combiner;
    private Material avatarMaterial;
    private Color skinColor;
    private Color hairColor;
    private IWearablesCatalogService wearablesCatalogService;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        combiner = new AvatarMeshCombinerHelper();
        combiner.uploadMeshToGpu = false;
        combiner.prepareMeshForGpuSkinning = false;

        wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        avatarMaterial = Resources.Load<Material>("Avatar Material");
        Assert.IsTrue(ColorUtility.TryParseHtmlString("#F2C2A5", out skinColor));
        Assert.IsTrue(ColorUtility.TryParseHtmlString("#1C1C1C", out hairColor));
        Assert.NotNull(avatarMaterial);
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator EmissiveWearable_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(EmissiveWearable()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator EmissiveWearable()
    {
        const string EMISSIVE_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:dc_niftyblocksmith:blocksmith_feet";

        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(7.5f, 1.8f, 11), new Vector3(7.5f, 1.75f, 8));

        //Act
        GameObject holder = CreateTestGameObject("_Original", new Vector3(7.5f, 1, 10.2f));
        yield return LoadWearable(EMISSIVE_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, holder, combiner);

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_EmissiveWearable", camera);

    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator AlphaBlendWearable_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(AlphaBlendWearable()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator AlphaBlendWearable()
    {
        const string ALPHA_BLEND_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:community_contest:cw_raver_eyewear";

        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(7.5f, 1.8f, 11), new Vector3(7.5f, 1.75f, 8));

        GameObject holder = CreateTestGameObject("_Original", new Vector3(7.5f, -0.6f, 10.5f));
        yield return LoadWearable(ALPHA_BLEND_WEARABLE_ID, WearableLiterals.BodyShapes.FEMALE, holder, combiner);

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_AlphaBlendWearable", camera);
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator AlphaTestWearable_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(AlphaTestWearable()); }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator AlphaTestWearable()
    {
        const string ALPHA_TEST_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:tech_tribal_marc0matic:techtribal_beast_mask";

        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));

        //Act
        GameObject holder = CreateTestGameObject("_Original", new Vector3(8f, -0.75f, 8f));
        yield return LoadWearable(ALPHA_TEST_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, holder, combiner);

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_AlphaTestWearable", camera);
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator AlphaBlendWearableWithTransparentBaseColor_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(AlphaBlendWearableWithTransparentBaseColor()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator AlphaBlendWearableWithTransparentBaseColor()
    {
        const string WEARABLE_ID = "urn:decentraland:matic:collections-v2:0x7c688630370a2900960f5ffd7573d2f66f179733:0";

        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));

        //Act
        GameObject holder = CreateTestGameObject("_Original", new Vector3(8f, -0.6f, 8.3f));
        holder.transform.localEulerAngles = Vector3.up * 20f;
        yield return LoadWearable(WEARABLE_ID, WearableLiterals.BodyShapes.MALE, holder, combiner);

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_AlphaBlendWearableWithTransparentBaseColor", camera);
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator EmissiveWearableWithNoEmissionMap_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(EmissiveWearableWithNoEmissionMap()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator EmissiveWearableWithNoEmissionMap()
    {
        const string WEARABLE_ID = "urn:decentraland:matic:collections-v2:0x3bb75349bfd21176b4e41f8b9afe96b4b86059db:0";

        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(7.75f, 1.8f, 8.75f), new Vector3(8, 1.75f, 8));

        //Act
        GameObject holder = CreateTestGameObject("_Original", new Vector3(8, -0.5f, 7.5f));
        yield return LoadWearable(WEARABLE_ID, WearableLiterals.BodyShapes.MALE, holder, combiner);

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_EmissiveWearableWithNoEmissionMap", camera);
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator SkinAndHairMaterialsAreNotReplacedIncorrectly_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(SkinAndHairMaterialsAreNotReplacedIncorrectly()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator SkinAndHairMaterialsAreNotReplacedIncorrectly()
    {
        const string JACKET_ID = "urn:decentraland:ethereum:collections-v1:wonderzone_steampunk:steampunk_jacket";
        const string HAT_ID = "urn:decentraland:ethereum:collections-v1:wonderzone_steampunk:steampunk_hat";

        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8f, 1.8f, 9.3f), new Vector3(8, 1.75f, 8));
        GameObject holderHat = CreateTestGameObject(HAT_ID, new Vector3(8, -0.75f, 8));
        GameObject holderJacket = CreateTestGameObject(JACKET_ID, new Vector3(8, -0.75f, 8));

        var extraCombiner = new AvatarMeshCombinerHelper();
        extraCombiner.uploadMeshToGpu = false;
        extraCombiner.prepareMeshForGpuSkinning = false;

        //Act
        yield return LoadWearable(JACKET_ID, WearableLiterals.BodyShapes.MALE, holderJacket, combiner);
        yield return LoadWearable(HAT_ID, WearableLiterals.BodyShapes.MALE, holderHat, extraCombiner);

        Assert.IsFalse(holderHat.GetComponentsInChildren<SkinnedMeshRenderer>().Any(renderer => renderer.materials.Any(material => material.name.ToLower().Contains("skin"))));
        Assert.IsFalse(holderJacket.GetComponentsInChildren<SkinnedMeshRenderer>().Any(renderer => renderer.materials.Any(material => material.name.ToLower().Contains("hair"))));

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_SkinAndHairMaterialsAreNotReplacedIncorrectly", camera);

        extraCombiner.Dispose();
    }

    private IEnumerator LoadWearable(string wearableId, string bodyShapeId, GameObject container, AvatarMeshCombinerHelper combiner)
    {
        wearablesCatalogService.WearablesCatalog.TryGetValue(wearableId, out WearableItem wearableItem);
        Assert.NotNull(wearableItem);

        WearableLoader wearableLoader = new WearableLoader(new WearableRetriever(), wearableItem);

        wearableLoader.Load(container, new AvatarSettings
        {
            bodyshapeId = bodyShapeId,
            skinColor = skinColor,
            hairColor = hairColor
        });

        yield return new WaitUntil(() => wearableLoader.status == IWearableLoader.Status.Succeeded);

        List<SkinnedMeshRenderer> rends = wearableLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>().ToList();
        for ( int i = 0; i < rends.Count; i++ )
        {
            rends[i].enabled = true;
        }

        Vector3 cachedPos = container.transform.position;
        //We need to reset the holder position for the mesh combiner
        container.transform.position = Vector3.up * -0.75f;
        combiner.Combine(rends[0], rends.ToArray(), avatarMaterial, false);

        combiner.container.transform.SetParent(rends[0].transform.parent);
        combiner.container.transform.localPosition = rends[0].transform.localPosition;
        container.transform.position = cachedPos;
    }

    protected override IEnumerator TearDown()
    {
        wearablesCatalogService.Dispose();
        combiner.Dispose();

        yield return base.TearDown();
    }
}
