using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class GLTFImporterTests : TestsBase
{
    public IEnumerator LoadModel(string path, System.Action<InstantiatedGLTFObject> OnFinishLoading)
    {
        string src = Utils.GetTestsAssetsPath() + path;
        DecentralandEntity entity = null;
        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, src, out entity);
        yield return gltfShape.routine;
        yield return new WaitForSeconds(4);

        if (OnFinishLoading != null)
        {
            OnFinishLoading.Invoke(entity.meshRootGameObject.GetComponentInChildren<InstantiatedGLTFObject>());
        }
    }

    [UnityTest]
    [Explicit("Test takes too long")]
    [Category("Explicit")]
    public IEnumerator TrevorModelHasProperScaling()
    {

        InstantiatedGLTFObject trevorModel = null;
        yield return LoadModel("/GLB/Trevor/Trevor.glb", (m) => trevorModel = m);

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
        yield return LoadModel("/GLB/Trevor/Trevor.glb", (m) => trevorModel = m);

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
        yield return LoadModel("/GLB/Avatar/Avatar_Idle.glb", (m) => trevorModel = m);
    }
}
