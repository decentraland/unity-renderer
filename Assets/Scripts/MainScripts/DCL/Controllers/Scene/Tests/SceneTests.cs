using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SceneTests
    {
        [UnityTest]
        public IEnumerator PerformanceLimitControllerTests()
        {
            var sceneController = TestHelpers.InitializeSceneController(true);
            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

            yield return new WaitForSeconds(.1f);

            sceneController.UnloadAllScenes();

            yield return new WaitForSeconds(.1f);

            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForSeconds(.1f);

            var scene = sceneController.loadedScenes["0,0"];

            TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-3, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(0, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "3", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(2, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(4, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.CYLINDER_SHAPE, new Vector3(6, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 6), "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb");
            TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.OBJ_SHAPE, new Vector3(10, 1, 0), "http://127.0.0.1:9991/OBJ/teapot.obj");
            TestHelpers.InstantiateEntityWithShape(scene, "8", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 12), "http://127.0.0.1:9991/GLB/CesiumMan/CesiumMan.glb");

            yield return new WaitForSeconds(8f);

            AssertMetricsModel(scene,
                triangles:11189,
                materials:3,
                entities:8,
                meshes:9,
                bodies:9,
                textures:0 );
                
            TestHelpers.RemoveSceneEntity(scene, "8");

            yield return new WaitForSeconds(.1f);

            AssertMetricsModel(scene,
                triangles: 6517,
                materials: 3,
                entities: 7,
                meshes: 8,
                bodies: 8,
                textures: 0);

            yield return null;
        }

        void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies, int textures)
        {
            SceneMetricsController.Model inputModel = scene.metricsController.GetModel();

            Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: "+triangles );
            Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
            Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
            Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
            Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
            Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
        }
    }
}
