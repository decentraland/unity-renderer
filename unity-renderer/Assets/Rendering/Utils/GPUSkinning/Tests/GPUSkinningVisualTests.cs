using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using GPUSkinning;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GPUSkinningVisualTests : VisualTestsBase
{
    private CatalogController catalogController;
    private BaseDictionary<string, WearableItem> catalog;
    private readonly HashSet<WearableController> toCleanUp = new HashSet<WearableController>();
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
    public IEnumerator Basic_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(Basic()); }

    [UnityTest, VisualTest]
    [Category("Visual Tests")]
    public IEnumerator Basic()
    {
        //Arrange
        VisualTestUtils.RepositionVisualTestsCamera(camera, new Vector3(7.5f, 1.8f, 11), new Vector3(7.5f, 1.75f, 8));

        AnimationClip animationClip = Resources.Load<AnimationClip>("Male/dab");

        // Loading the wearable twice is far from ideal,
        // but our loading process is so convoluted that it made impossible to reuse the same wearableController.

        AvatarMeshCombinerHelper originalCombiner = new AvatarMeshCombinerHelper();
        originalCombiner.uploadMeshToGpu = false;

        GameObject originalGO = CreateTestGameObject("_Original", new Vector3(8, 0, 8));
        yield return LoadWearable("urn:decentraland:off-chain:base-avatars:bee_t_shirt", WearableLiterals.BodyShapes.FEMALE, originalGO, originalCombiner);
        Animation originalAnim = originalGO.transform.GetChild(0).gameObject.AddComponent<Animation>();
        originalAnim.AddClip(animationClip, animationClip.name);
        AnimationState originalState = originalAnim.PlayQueued(animationClip.name);
        originalState.normalizedSpeed = 0;
        originalState.time = 0.5f;
        originalAnim.Sample();

        AvatarMeshCombinerHelper gpuSkinningCombiner = new AvatarMeshCombinerHelper();
        gpuSkinningCombiner.uploadMeshToGpu = false;

        GameObject gpuSkinningGO = CreateTestGameObject("_Original", new Vector3(7, 0, 8));
        yield return LoadWearable("urn:decentraland:off-chain:base-avatars:bee_t_shirt", WearableLiterals.BodyShapes.FEMALE, gpuSkinningGO, gpuSkinningCombiner);
        Animation gpuSkinningAnim = gpuSkinningGO.transform.GetChild(0).gameObject.AddComponent<Animation>();
        gpuSkinningAnim.AddClip(animationClip, animationClip.name);
        AnimationState gpuSkinningState = gpuSkinningAnim.PlayQueued(animationClip.name);
        gpuSkinningState.normalizedSpeed = 0;
        gpuSkinningState.time = 0.5f;
        gpuSkinningAnim.Sample();

        SimpleGPUSkinning gpuSkinning = new SimpleGPUSkinning(gpuSkinningCombiner.renderer);
        yield return null;
        gpuSkinning.Update();

        //Assert
        yield return VisualTestUtils.TakeSnapshot("GPUSkinningVisualTests_Basic", camera);
    }

    private IEnumerator LoadWearable(string wearableId, string bodyShapeId, GameObject container, AvatarMeshCombinerHelper combiner)
    {
        catalog.TryGetValue(wearableId, out WearableItem wearableItem);
        Assert.NotNull(wearableItem);

        WearableController wearable = new WearableController(wearableItem);
        toCleanUp.Add(wearable);

        bool succeeded = false;
        bool failed = false;

        wearable.Load(bodyShapeId, container.transform, x => succeeded = true, (x, e) => failed = true);

        yield return new WaitUntil(() => succeeded || failed);

        wearable.SetAssetRenderersEnabled(true);
        wearable.SetupHairAndSkinColors(skinColor, hairColor);

        var rends = wearable.GetRenderers();
        combiner.Combine(rends[0], rends.ToArray(), new Material(avatarMaterial));

        combiner.container.transform.SetParent(rends[0].transform.parent);
        combiner.container.transform.localPosition = rends[0].transform.localPosition;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(catalogController.gameObject);

        foreach (WearableController wearable in toCleanUp)
        {
            wearable.CleanUp();
        }

        yield return base.TearDown();
    }
}