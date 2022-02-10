using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvatarSystem;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewArmatureTests : VisualTestsBase
{
    static IReadOnlyDictionary<string, string[]> WEARABLES_BY_BODYSHAPE = new Dictionary<string, string[]>
    {
        {
            "urn:decentraland:off-chain:base-avatars:BaseMale", new []
            {
                "urn:decentraland:off-chain:base-avatars:green_hoodie",
                "urn:decentraland:off-chain:base-avatars:casual_hair_01",
                "urn:decentraland:off-chain:base-avatars:beard",
                "urn:decentraland:off-chain:base-avatars:sneakers",
                "urn:decentraland:off-chain:base-avatars:brown_pants"
            }
        },
        {
            "urn:decentraland:off-chain:base-avatars:BaseFemale", new []
            {
                "urn:decentraland:off-chain:base-avatars:f_sweater",
                "urn:decentraland:off-chain:base-avatars:standard_hair",
                "urn:decentraland:off-chain:base-avatars:sneakers",
                "urn:decentraland:off-chain:base-avatars:f_jeans"
            }
        }
    };

    private BaseDictionary<string, WearableItem> catalog;
    private Material avatarMaterial;
    private Color skinColor;
    private Color hairColor;
    private GameObject newCatalog;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        EnsureCatalog();
        catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();

        avatarMaterial = Resources.Load<Material>("Avatar Material");
        Assert.IsTrue(ColorUtility.TryParseHtmlString("#F2C2A5", out skinColor));
        Assert.IsTrue(ColorUtility.TryParseHtmlString("#1C1C1C", out hairColor));
        Assert.NotNull(avatarMaterial);
    }

    void EnsureCatalog()
    {
        if (CatalogController.i == null)
            newCatalog = TestUtils.CreateComponentWithGameObject<CatalogController>("Catalog Controller").gameObject;
    }

    [UnityTest]
    public IEnumerator BodyshapesNotSkinned()
    {
        //Prepare the catalog
        catalog.TryGetValue("urn:decentraland:off-chain:base-avatars:BaseMale", out WearableItem maleItem);
        catalog.TryGetValue("urn:decentraland:off-chain:base-avatars:BaseFemale", out WearableItem femaleItem);

        //Load the male bodyshape
        GameObject containerMale = CreateTestGameObject("_Male", new Vector3(0, 0, 0));
        WearableLoader maleLoader = new WearableLoader(new WearableRetriever(), maleItem);
        maleLoader.Load(containerMale, new AvatarSettings { bodyshapeId = maleItem.id, skinColor = skinColor, hairColor = hairColor });
        yield return new WaitUntil(() => maleLoader.status == IWearableLoader.Status.Succeeded);
        for (int i = 0; i < maleLoader.rendereable.renderers.Count; i++)
        {
            if (maleLoader.rendereable.renderers[i] is SkinnedMeshRenderer skr)
                ResetBones(skr.sharedMesh.bindposes, skr.bones);
            MeshRenderer mr = maleLoader.rendereable.renderers[i].gameObject.AddComponent<MeshRenderer>();
            mr.materials = maleLoader.rendereable.renderers[i].materials;
            Object.Destroy(maleLoader.rendereable.renderers[i]);
        }

        //Load the female bodyshape
        GameObject containerFemale = CreateTestGameObject("_Female", new Vector3(0, 0, 0));
        WearableLoader femaleLoader = new WearableLoader(new WearableRetriever(), femaleItem);
        femaleLoader.Load(containerFemale, new AvatarSettings { bodyshapeId = femaleItem.id, skinColor = skinColor, hairColor = hairColor });
        yield return new WaitUntil(() => femaleLoader.status == IWearableLoader.Status.Succeeded);
        for (int i = 0; i < femaleLoader.rendereable.renderers.Count; i++)
        {
            MeshRenderer mr = femaleLoader.rendereable.renderers[i].gameObject.AddComponent<MeshRenderer>();
            mr.materials = femaleLoader.rendereable.renderers[i].materials;
            Object.Destroy(femaleLoader.rendereable.renderers[i]);
        }

        //To allow manual inspection in the editor
        yield return new WaitForSeconds(30);
    }

    [UnityTest]
    public IEnumerator BodyShapeCombinedWithWearables()
    {
        string bodyshapeId = "urn:decentraland:off-chain:base-avatars:BaseMale";
        //Prepare the catalog and the animation 
        AvatarSettings settings = new AvatarSettings
        {
            bodyshapeId = bodyshapeId,
            skinColor = skinColor,
            hairColor = hairColor
        };
        AnimationClip animationClip = Resources.Load<AnimationClip>("WalkNueva");
        catalog.TryGetValue(bodyshapeId, out WearableItem bodyshapeItem);

        //Load the bodyshape
        GameObject containerBodyshape = CreateTestGameObject("_Bodyshape", new Vector3(0, 0, 0));
        WearableLoader bodyshapeLoader = new WearableLoader(new WearableRetriever(), bodyshapeItem);
        bodyshapeLoader.Load(containerBodyshape, settings);
        yield return new WaitUntil(() => bodyshapeLoader.status == IWearableLoader.Status.Succeeded);

        //Load the wearables
        List<SkinnedMeshRenderer> rendsFromWearables = new List<SkinnedMeshRenderer>();
        foreach (string wearableId in WEARABLES_BY_BODYSHAPE[bodyshapeId])
        {
            catalog.TryGetValue(wearableId, out WearableItem wearableItem);
            GameObject container = CreateTestGameObject(wearableId, new Vector3(0, 0, 0));
            WearableLoader wearableLoader = new WearableLoader(new WearableRetriever(), wearableItem);
            wearableLoader.Load(container, settings);
            yield return new WaitUntil(() => wearableLoader.status == IWearableLoader.Status.Succeeded);
            rendsFromWearables.AddRange(wearableLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>());
        }

        //Combiner
        List<SkinnedMeshRenderer> rends = bodyshapeLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>()
                                                         .Union(rendsFromWearables)
                                                         .ToList();
        AvatarMeshCombinerHelper combiner = new AvatarMeshCombinerHelper();
        combiner.uploadMeshToGpu = false;
        combiner.prepareMeshForGpuSkinning = false;
        combiner.Combine(rends[0], rends.ToArray(), new Material(avatarMaterial));
        combiner.container.transform.SetParent(rends[0].transform.parent);
        combiner.container.transform.localPosition = rends[0].transform.localPosition;

        //Prepare and set animation
        if (!TryFindChildRecursively(containerBodyshape.transform, "Armature", out Transform armature))
        {
            Debug.LogError($"Couldn't find Armature for AnimatorLegacy in path:");
            yield break;
        }
        Transform armatureParent = armature.parent;
        var animation = armatureParent.gameObject.AddComponent<Animation>();
        animation.AddClip(animationClip, animationClip.name);
        animation.Play(animationClip.name);

        //To allow manual inspection in the editor
        yield return new WaitForSeconds(30);
    }

    [UnityTest]
    public IEnumerator BodyShapedWithModifiedWearable()
    {
        //Prepare Catalog and animation
        string bodyshapeId = "urn:decentraland:off-chain:base-avatars:BaseMale";
        AvatarSettings settings = new AvatarSettings
        {
            bodyshapeId = bodyshapeId,
            skinColor = skinColor,
            hairColor = hairColor
        };
        AnimationClip animationClip = Resources.Load<AnimationClip>("WalkNueva");
        catalog.TryGetValue(bodyshapeId, out WearableItem bodyshapeItem);
        catalog.TryGetValue("urn:decentraland:off-chain:base-avatars:casual_hair_01", out WearableItem hairItem);

        //Load the bodyshape
        GameObject containerBodyshape = CreateTestGameObject("_Bodyshape", new Vector3(0, 0, 0));
        WearableLoader bodyshapeLoader = new WearableLoader(new WearableRetriever(), bodyshapeItem);
        bodyshapeLoader.Load(containerBodyshape, settings);
        yield return new WaitUntil(() => bodyshapeLoader.status == IWearableLoader.Status.Succeeded);
        bodyshapeLoader.rendereable.renderers.ForEach(x => x.enabled = true);
        SkinnedMeshRenderer bodyshapeSKR = bodyshapeLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>().First();

        //Load the hair
        WearableLoader hairLoader = new WearableLoader(new WearableRetriever(), hairItem);
        hairLoader.Load(containerBodyshape, settings);
        yield return new WaitUntil(() => hairLoader.status == IWearableLoader.Status.Succeeded);
        hairLoader.rendereable.renderers.ForEach(x => x.enabled = true);
        SkinnedMeshRenderer hairSKR = hairLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>().First();

        //Recalculate bindposes
        ResetBones(hairSKR.sharedMesh.bindposes, hairSKR.bones);
        Matrix4x4[] bindposes = new Matrix4x4[hairSKR.sharedMesh.bindposes.Length];
        Vector3 eulerDiff = bodyshapeSKR.rootBone.localEulerAngles - hairSKR.rootBone.localEulerAngles;
        Vector3 scaleDiff = Divide(Vector3.one, Divide(bodyshapeSKR.rootBone.localScale, hairSKR.rootBone.localScale));
        Matrix4x4 transformation = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(eulerDiff), scaleDiff);
        for (int i = 0; i < bindposes.Length; i++)
        {
            bindposes[i] = (transformation * bodyshapeSKR.sharedMesh.bindposes[i].inverse).inverse;
        }

        //Reassing new bindposes along "director" bones
        hairSKR.bones = bodyshapeSKR.bones;
        hairSKR.sharedMesh.bindposes = bindposes;

        // Animation
        if (!TryFindChildRecursively(containerBodyshape.transform, "Armature", out Transform armature))
        {
            Debug.LogError($"Couldn't find Armature for AnimatorLegacy in path:");
            yield break;
        }
        Transform armatureParent = armature.parent;
        var animation = armatureParent.gameObject.AddComponent<Animation>();
        animation.AddClip(animationClip, animationClip.name);
        animation.Play(animationClip.name);

        //To allow manual inspection in the editor
        yield return new WaitForSeconds(30);
    }

    [UnityTest]
    public IEnumerator BodyShapedWithWearableCopyingTheBindposes()
    {
        //Prepare Catalog and animation
        string bodyshapeId = "urn:decentraland:off-chain:base-avatars:BaseMale";
        AvatarSettings settings = new AvatarSettings
        {
            bodyshapeId = bodyshapeId,
            skinColor = skinColor,
            hairColor = hairColor
        };
        AnimationClip animationClip = Resources.Load<AnimationClip>("WalkNueva");
        catalog.TryGetValue(bodyshapeId, out WearableItem bodyshapeItem);
        catalog.TryGetValue("urn:decentraland:off-chain:base-avatars:casual_hair_01", out WearableItem hairItem);

        //Load the bodyshape
        GameObject containerBodyshape = CreateTestGameObject("_Bodyshape", new Vector3(0, 0, 0));
        WearableLoader bodyshapeLoader = new WearableLoader(new WearableRetriever(), bodyshapeItem);
        bodyshapeLoader.Load(containerBodyshape, settings);
        yield return new WaitUntil(() => bodyshapeLoader.status == IWearableLoader.Status.Succeeded);
        bodyshapeLoader.rendereable.renderers.ForEach(x => x.enabled = true);
        SkinnedMeshRenderer bodyshapeSKR = bodyshapeLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>().First();

        //Load the hair
        WearableLoader hairLoader = new WearableLoader(new WearableRetriever(), hairItem);
        hairLoader.Load(containerBodyshape, settings);
        yield return new WaitUntil(() => hairLoader.status == IWearableLoader.Status.Succeeded);
        hairLoader.rendereable.renderers.ForEach(x => x.enabled = true);
        SkinnedMeshRenderer hairSKR = hairLoader.rendereable.renderers.OfType<SkinnedMeshRenderer>().First();

        //Recalculate bindposes
        // ResetBones(hairSKR.sharedMesh.bindposes, hairSKR.bones);
        // Matrix4x4[] bindposes = new Matrix4x4[hairSKR.sharedMesh.bindposes.Length];
        // Vector3 eulerDiff = bodyshapeSKR.rootBone.localEulerAngles - hairSKR.rootBone.localEulerAngles;
        // Vector3 scaleDiff = Divide(Vector3.one, Divide(bodyshapeSKR.rootBone.localScale, hairSKR.rootBone.localScale));
        // Matrix4x4 transformation = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(eulerDiff), scaleDiff);
        // for (int i = 0; i < bindposes.Length; i++)
        // {
        //     bindposes[i] = (transformation * bodyshapeSKR.sharedMesh.bindposes[i].inverse).inverse;
        // }

        //Reassing new bindposes along "director" bones
        hairSKR.bones = bodyshapeSKR.bones;
        hairSKR.sharedMesh.bindposes = bodyshapeSKR.sharedMesh.bindposes;

        // Animation
        if (!TryFindChildRecursively(containerBodyshape.transform, "Armature", out Transform armature))
        {
            Debug.LogError($"Couldn't find Armature for AnimatorLegacy in path:");
            yield break;
        }
        Transform armatureParent = armature.parent;
        var animation = armatureParent.gameObject.AddComponent<Animation>();
        animation.AddClip(animationClip, animationClip.name);
        animation.Play(animationClip.name);

        //To allow manual inspection in the editor
        yield return new WaitForSeconds(30);
    }

    public static bool TryFindChildRecursively(Transform transform, string name, out Transform foundChild)
    {
        foundChild = transform.Find(name);
        if (foundChild != null)
            return true;

        foreach (Transform child in transform)
        {
            if (TryFindChildRecursively(child, name, out foundChild))
                return true;
        }
        return false;
    }

    //Same method as used in AvatarMeshCombiner
    internal static void ResetBones(Matrix4x4[] bindPoses, Transform[] bones)
    {
        for ( int i = 0 ; i < bones.Length; i++ )
        {
            Transform bone = bones[i];
            Matrix4x4 bindPose = bindPoses[i].inverse;
            bone.position = bindPose.MultiplyPoint3x4(Vector3.zero);
            bone.rotation = bindPose.rotation;

            Vector3 bindPoseScale = bindPose.lossyScale;
            Vector3 boneScale = bone.lossyScale;

            bone.localScale = new Vector3(bindPoseScale.x / boneScale.x,
                bindPoseScale.y / boneScale.y,
                bindPoseScale.z / boneScale.z);
        }
    }

    protected override IEnumerator TearDown()
    {
        if (newCatalog == null)
            Object.Destroy(newCatalog);
        yield return base.TearDown();
    }

    public Vector3 Divide(Vector3 v1, Vector3 v2) { return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z); }
}