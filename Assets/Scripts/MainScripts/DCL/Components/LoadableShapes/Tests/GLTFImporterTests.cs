using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GLTFImporterTests : TestsBase
    {
        UnityGLTF.InstantiatedGLTFObject trevorModel;

        public IEnumerator LoadTrevorModel()
        {
            string src = TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb";
            DecentralandEntity entity = null;
            GLTFShape trevorGLTFShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, src, out entity);
            yield return trevorGLTFShape.routine;
            yield return new WaitForSeconds(4);

            trevorModel = entity.meshGameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>();
        }

        [UnityTest]
        public IEnumerator TrevorModelHasProperScaling()
        {
            yield return InitScene();
            yield return LoadTrevorModel();

            Transform child = trevorModel.transform.GetChild(0).GetChild(0);
            Vector3 scale = child.lossyScale;
            Assert.AreEqual(new Vector3(0.049f, 0.049f, 0.049f).ToString(), scale.ToString());
            yield return null;
        }

        [UnityTest]
        public IEnumerator TrevorModelHasProperTopology()
        {
            yield return InitScene();
            yield return LoadTrevorModel();

            Assert.IsTrue(trevorModel.transform.childCount == 1);
            Assert.IsTrue(trevorModel.transform.GetChild(0).childCount == 2);
            Assert.IsTrue(trevorModel.transform.GetChild(0).GetChild(0).name.Contains("Character_Avatar"));
            Assert.IsTrue(trevorModel.transform.GetChild(0).GetChild(1).name.Contains("mixamorig"));
            yield return null;
        }
    }
}
