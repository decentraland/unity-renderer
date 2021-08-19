using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = UnityEngine.WaitUntil;

public class WearablesVisualTests : VisualTestsBase
{
    private AvatarMeshCombinerHelper combiner = new AvatarMeshCombinerHelper();
    private BaseDictionary<string, WearableItem> catalog;
    private readonly HashSet<WearableController> toCleanUp = new HashSet<WearableController>();
    private Material avatarMaterial;
    private Color skinColor;
    private Color hairColor;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
        toCleanUp.Clear();

        avatarMaterial = Resources.Load<Material>("Materials/Avatar Material");
        Assert.IsTrue(ColorUtility.TryParseHtmlString("#F2C2A5", out skinColor));
        Assert.IsTrue(ColorUtility.TryParseHtmlString("#1C1C1C", out hairColor));
        Assert.NotNull(avatarMaterial);
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator EmissiveWearable_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(EmissiveWearable()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator EmissiveWearable()
    {
        //Arrange
        yield return InitVisualTestsScene("WearableVisualTests_EmissiveWearable");
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));
        const string EMISSIVE_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:dc_niftyblocksmith:blocksmith_feet";

        //Act
        yield return LoadWearable(EMISSIVE_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, new Vector3(8, 1, 8));

        //Assert
        yield return VisualTestHelpers.TakeSnapshot();
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator AlphaBlendWearable_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(AlphaBlendWearable()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator AlphaBlendWearable()
    {
        //Arrange
        yield return InitVisualTestsScene("WearableVisualTests_AlphaBlendWearable");
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));
        const string ALPHA_BLEND_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:community_contest:cw_raver_eyewear";

        //Act
        yield return LoadWearable(ALPHA_BLEND_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, new Vector3(8, -0.65f, 8.5f));

        //Assert
        yield return VisualTestHelpers.TakeSnapshot();
    }

    [UnityTest, VisualTest]
    [Explicit, Category("Explicit")]
    public IEnumerator AlphaTestWearable_Generate() { yield return VisualTestHelpers.GenerateBaselineForTest(AlphaTestWearable()); }

    [UnityTest, VisualTest]
    public IEnumerator AlphaTestWearable()
    {
        //Arrange
        yield return InitVisualTestsScene("WearableVisualTests_AlphaTestWearable");
        VisualTestHelpers.RepositionVisualTestsCamera(VisualTestController.i.camera, new Vector3(8, 1.8f, 9), new Vector3(8, 1.75f, 8));
        const string ALPHA_TEST_WEARABLE_ID = "urn:decentraland:ethereum:collections-v1:tech_tribal_marc0matic:techtribal_beast_mask";

        //Act
        yield return LoadWearable(ALPHA_TEST_WEARABLE_ID, WearableLiterals.BodyShapes.MALE, new Vector3(8, -0.75f, 8));

        //Assert
        yield return VisualTestHelpers.TakeSnapshot();
    }

    private IEnumerator LoadWearable(string wearableId, string bodyShapeId, Vector3 wearablePosition)
    {
        catalog.TryGetValue(wearableId, out WearableItem wearableItem);
        Assert.NotNull(wearableItem);

        WearableController wearable = new WearableController(wearableItem);
        toCleanUp.Add(wearable);

        bool succeeded = false;
        bool failed = false;

        GameObject testGameObject = CreateTestGameObject(wearable.id, Vector3.up * -0.75f);
        wearable.Load(bodyShapeId, testGameObject.transform, x => succeeded = true, x => failed = true);

        yield return new WaitUntil(() => succeeded || failed);

        Assert.IsTrue(succeeded);

        wearable.SetAssetRenderersEnabled(true);
        wearable.SetupHairAndSkinColors(skinColor, hairColor);

        var rends = wearable.GetRenderers();
        combiner.Combine(rends[0], rends.ToArray(), avatarMaterial);
        combiner.container.transform.SetParent(rends[0].transform.parent);
        combiner.container.transform.localPosition = rends[0].transform.localPosition;

        testGameObject.transform.position = wearablePosition;
    }

    protected override IEnumerator TearDown()
    {
        combiner.Dispose();

        foreach (WearableController wearable in toCleanUp)
        {
            wearable.CleanUp();
        }

        return base.TearDown();
    }
}