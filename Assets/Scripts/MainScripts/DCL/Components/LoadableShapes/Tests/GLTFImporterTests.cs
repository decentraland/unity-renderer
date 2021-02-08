using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using DCL;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;
using UnityGLTF.Cache;
using UnityGLTF.Cache;

public class GLTFImporterTests : IntegrationTestSuite_Legacy
{
    public IEnumerator LoadModel(string path, System.Action<DecentralandEntity, InstantiatedGLTFObject> OnFinishLoading)
    {
        string src = Utils.GetTestsAssetsPath() + path;

        DecentralandEntity entity = null;

        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, src, out entity);

        yield return gltfShape.routine;

        yield return new WaitForAllMessagesProcessed();
        yield return new WaitUntil(() => GLTFComponent.downloadingCount == 0);

        if (OnFinishLoading != null)
        {
            OnFinishLoading.Invoke(entity, entity.meshRootGameObject.GetComponentInChildren<InstantiatedGLTFObject>());
        }
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator TrevorModelHasProperScaling()
    {
        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Trevor/Trevor.glb", (entity, m) => trevorModel = m);

        Transform child = trevorModel.transform.GetChild(0).GetChild(0);
        Vector3 scale = child.lossyScale;
        Assert.AreEqual(new Vector3(0.049f, 0.049f, 0.049f).ToString(), scale.ToString());
        yield return null;
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator TrevorModelHasProperTopology()
    {
        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Trevor/Trevor.glb", (entity, m) => trevorModel = m);

        Assert.IsTrue(trevorModel.transform.childCount == 1);
        Assert.IsTrue(trevorModel.transform.GetChild(0).childCount == 2);
        Assert.IsTrue(trevorModel.transform.GetChild(0).GetChild(0).name.Contains("Character_Avatar"));
        Assert.IsTrue(trevorModel.transform.GetChild(0).GetChild(1).name.Contains("mixamorig"));
        yield return null;
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator GLTFWithoutSkeletonIdIsLoadingCorrectly()
    {
        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Avatar/Avatar_Idle.glb", (entity, m) => trevorModel = m);
    }

    [UnityTest]
    public IEnumerator TwoGLTFsWithSameExternalTexturePathDontCollide()
    {
        InstantiatedGLTFObject trunk1 = null;
        InstantiatedGLTFObject trunk2 = null;

        PersistentAssetCache.ImageCacheByUri.Clear();

        yield return LoadModel("/GLTF/Trunk/Trunk.gltf", (entity, m) => trunk1 = m);
        yield return LoadModel("/GLTF/Trunk2/Trunk.gltf", (entity, m) => trunk2 = m);
        UnityEngine.Assertions.Assert.AreEqual(2, PersistentAssetCache.ImageCacheByUri.Count, "Image cache is colliding!");
        UnityEngine.Assertions.Assert.AreEqual(2, PersistentAssetCache.StreamCacheByUri.Count, "Buffer cache is colliding!");
    }


    [UnityTest]
    public IEnumerator CurvesAreOptimizedCorrectly()
    {
        var curvesSource = Resources.Load<AnimationCurveContainer>("CurveOptimizedCorrectlySource");
        //NOTE(Brian): We are going to output the optimization result in this SO, so it can be debugged more easily
        var curvesResult = Resources.Load<AnimationCurveContainer>("CurveOptimizedCorrectlyResult");

        curvesResult.curves = new AnimationCurve[curvesSource.curves.Length];

        for (int i = 0; i < curvesSource.curves.Length; i++)
        {
            var curve = curvesSource.curves[i];

            List<Keyframe> keys = new List<Keyframe>();

            for (int i1 = 0; i1 < curve.length; i1++)
            {
                keys.Add(curve[i1]);
            }

            var result = GLTFSceneImporter.OptimizeKeyFrames(keys.ToArray());
            var modifiedCurve = new AnimationCurve(result);

            curvesResult.curves[i] = modifiedCurve;

            for (float time = 0; time < 1.0f; time += 0.032f)
            {
                var v1 = curve.Evaluate(time);
                var v2 = modifiedCurve.Evaluate(time);

                UnityEngine.Assertions.Assert.AreApproximatelyEqual(v1, v2, 0.01f);
            }
        }

        yield break;
    }

    [UnityTest]
    public IEnumerator TexturesCacheWorksProperly()
    {
        DecentralandEntity entity = null;
        PersistentAssetCache.ImageCacheByUri.Clear();
        yield return LoadModel("/GLTF/Trunk/Trunk.gltf", (e, model) => entity = e);

        UnityEngine.Assertions.Assert.AreEqual(1, PersistentAssetCache.ImageCacheByUri.Count);
        scene.RemoveEntity(entity.entityId);
        PoolManager.i.Cleanup();

        yield return null;

        UnityEngine.Assertions.Assert.AreEqual(0, PersistentAssetCache.ImageCacheByUri.Count);
    }

    [UnityTest]
    public IEnumerator TexturesOffsetAndScaleWorkProperly()
    {
        DecentralandEntity entity = null;
        PersistentAssetCache.ImageCacheByUri.Clear();
        yield return LoadModel("/GLB/PlaneUVsOffset/planeUVsOffset.glb", (e, model) => entity = e);

        MeshRenderer meshRenderer = entity.gameObject.GetComponentInChildren<MeshRenderer>();

        var unityOffset = GLTFSceneImporter.GLTFOffsetToUnitySpace(new Vector2(0.35f, 0.35f), 2.5f);
        Assert.AreEqual( unityOffset, meshRenderer.material.GetTextureOffset("_BaseMap"));
        Assert.AreEqual( Vector2.one * 2.5f, meshRenderer.material.GetTextureScale("_BaseMap"));
    }

    [UnityTest]
    public IEnumerator TexturesProcessTexCoords()
    {
        DecentralandEntity entity = null;
        PersistentAssetCache.ImageCacheByUri.Clear();
        yield return LoadModel("/GLB/PlaneUVsMultichannel/PlaneUVsMultichannel.glb", (e, model) => entity = e);

        MeshRenderer meshRenderer = entity.gameObject.GetComponentInChildren<MeshRenderer>();

        Assert.AreEqual( 1, meshRenderer.material.GetInt("_BaseMapUVs"));
        Assert.AreEqual( 1, meshRenderer.material.GetInt("_NormalMapUVs"));
        Assert.AreEqual( 1, meshRenderer.material.GetInt("_MetallicMapUVs"));
        Assert.AreEqual( 1, meshRenderer.material.GetInt("_EmissiveMapUVs"));
    }
}