using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = UnityEngine.WaitUntil;

public class WearablesVisualTests : VisualTestsBase
{
    private CatalogController catalogController;
    private BaseDictionary<string, WearableItem> catalog;
    private readonly HashSet<WearableController> toCleanUp = new HashSet<WearableController>();
    private readonly HashSet<AvatarMeshCombinerHelper> toCleanUpCombiners = new HashSet<AvatarMeshCombinerHelper>();
    private Material avatarMaterial;
    private Color skinColor;
    private Color hairColor;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        catalogController = TestUtils.CreateComponentWithGameObject<CatalogController>("CatalogController");
        catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        toCleanUp.Clear();

        avatarMaterial = Resources.Load<Material>("Materials/Avatar Material");
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
        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));
        const string EMISSIVE_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:dc_niftyblocksmith:blocksmith_feet";

        //Act
        yield return LoadWearable(EMISSIVE_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, CreateTestGameObject(EMISSIVE_WEARABLE_ID, new Vector3(8, 1, 8)));

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
        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));
        const string ALPHA_BLEND_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:community_contest:cw_raver_eyewear";

        //Act
        yield return LoadWearable(ALPHA_BLEND_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, CreateTestGameObject(ALPHA_BLEND_WEARABLE_ID, new Vector3(8, -0.65f, 8.5f)));

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
        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));
        const string ALPHA_TEST_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:tech_tribal_marc0matic:techtribal_beast_mask";

        //Act
        yield return LoadWearable(ALPHA_TEST_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, CreateTestGameObject(ALPHA_TEST_WEARABLE_ID, new Vector3(8, -0.75f, 8)));

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
        //Arrange
        const string WEARABLE_ID = "urn:decentraland:matic:collections-v2:0x7c688630370a2900960f5ffd7573d2f66f179733:0";

        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(7.78f, 1.68f, 8.5f), new Vector3(8, 1.75f, 8));
        camera.fieldOfView = 30;

        //Act
        yield return LoadWearable(WEARABLE_ID, WearableLiterals.BodyShapes.MALE, CreateTestGameObject(WEARABLE_ID, new Vector3(8, -0.75f, 8)));

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
        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(7.75f, 1.8f, 8.75f), new Vector3(8, 1.75f, 8));
        const string WEARABLE_ID = "urn:decentraland:matic:collections-v2:0x3bb75349bfd21176b4e41f8b9afe96b4b86059db:0";

        //Act
        yield return LoadWearable(WEARABLE_ID, WearableLiterals.BodyShapes.MALE, CreateTestGameObject(WEARABLE_ID, new Vector3(8, -0.75f, 8)));

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
        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(8f, 1.8f, 9.3f), new Vector3(8, 1.75f, 8));
        const string JACKET_ID = "urn:decentraland:ethereum:collections-v1:wonderzone_steampunk:steampunk_jacket";
        const string HAT_ID = "urn:decentraland:ethereum:collections-v1:wonderzone_steampunk:steampunk_hat";
        GameObject holderHat = CreateTestGameObject(HAT_ID, new Vector3(8, -0.75f, 8));
        GameObject holderJacket = CreateTestGameObject(JACKET_ID, new Vector3(8, -0.75f, 8));

        //Act
        yield return LoadWearable(JACKET_ID, WearableLiterals.BodyShapes.MALE, holderJacket, null);
        yield return LoadWearable(HAT_ID, WearableLiterals.BodyShapes.MALE, holderHat, null);

        Assert.IsFalse(holderHat.GetComponentsInChildren<SkinnedMeshRenderer>().Any(renderer => renderer.materials.Any(material => material.name.ToLower().Contains("skin"))));
        Assert.IsFalse(holderJacket.GetComponentsInChildren<SkinnedMeshRenderer>().Any(renderer => renderer.materials.Any(material => material.name.ToLower().Contains("hair"))));

        //Assert
        yield return VisualTestUtils.TakeSnapshot("WearableVisualTests_SkinAndHairMaterialsAreNotReplacedIncorrectly", camera);
    }

    private IEnumerator LoadWearable(string wearableId, string bodyShapeId, GameObject holder) => LoadWearable(wearableId, bodyShapeId, holder, CreateCombiner());

    private IEnumerator LoadWearable(string wearableId, string bodyShapeId, GameObject holder, AvatarMeshCombinerHelper combiner)
    {
        catalog.TryGetValue(wearableId, out WearableItem wearableItem);
        Assert.NotNull(wearableItem);

        WearableController wearable = new WearableController(wearableItem);
        toCleanUp.Add(wearable);

        bool succeeded = false;
        bool failed = false;

        wearable.Load(bodyShapeId, holder.transform, x => succeeded = true, (x, e) => failed = true);

        yield return new WaitUntil(() => succeeded || failed);

        Assert.IsTrue(succeeded);

        wearable.SetAssetRenderersEnabled(true);
        wearable.SetupHairAndSkinColors(skinColor, hairColor);

        if (combiner != null)
        {
            Vector3 cachedPos = holder.transform.position;
            //We need to reset the holder position for the mesh combiner
            holder.transform.position = Vector3.up * -0.75f;
            var rends = wearable.GetRenderers();
            combiner.Combine(rends[0], rends.ToArray(), avatarMaterial);

            combiner.container.transform.SetParent(rends[0].transform.parent);
            combiner.container.transform.localPosition = rends[0].transform.localPosition;
            holder.transform.position = cachedPos;
        }
    }

    private AvatarMeshCombinerHelper CreateCombiner()
    {
        AvatarMeshCombinerHelper combiner = new AvatarMeshCombinerHelper();
        toCleanUpCombiners.Add(combiner);
        return combiner;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(catalogController.gameObject);

        foreach (AvatarMeshCombinerHelper combiner in toCleanUpCombiners)
        {
            combiner.Dispose();
        }

        foreach (WearableController wearable in toCleanUp)
        {
            wearable.CleanUp();
        }

        yield return base.TearDown();
    }
}