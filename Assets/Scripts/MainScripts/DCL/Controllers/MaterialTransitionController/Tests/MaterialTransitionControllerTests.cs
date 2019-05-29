using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MaterialTransitionControllerTests : TestsBase
    {
        ParcelScene scene;
        SceneController sceneController;

        IEnumerator InitScene()
        {
            yield return TestHelpers.UnloadAllUnityScenes();

            sceneController = TestHelpers.InitializeSceneController();
            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

            yield return new WaitForSeconds(.1f);

            sceneController.UnloadAllScenes();

            yield return new WaitForSeconds(.1f);

            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForAllMessagesProcessed();

            scene = sceneController.loadedScenes["0,0"];
        }

        [UnityTest]
        public IEnumerator MaterialTransitionWithGLTF()
        {
            yield return InitScene();

            var entity1 = TestHelpers.CreateSceneEntity(scene);

            ParcelSettings.VISUAL_LOADING_ENABLED = true;

            Shader hologramShader = Shader.Find("DCL/FX/Hologram");

            Assert.IsTrue(hologramShader != null, "Hologram shader == null??");

            DecentralandEntity entity = null;

            GLTFShape shape = TestHelpers.InstantiateEntityWithShape<GLTFShape, GLTFShape.Model>
            (scene,
                DCL.Models.CLASS_ID.GLTF_SHAPE,
                Vector3.zero,
                out entity,
                new GLTFShape.Model() { src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb" });

            GLTFLoader gltfShape = entity.gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            float timeout = 0;

            while (timeout < 10)
            {
                timeout += Time.deltaTime;

                if (entity.meshGameObject != null)
                {
                    var c = entity.meshGameObject.GetComponentInChildren<MaterialTransitionController>();

                    if (c != null && c.placeholder != null) // NOTE(Brian): Wait for it
                    {
                        Assert.IsTrue(c.useHologram, "useHologram must be true");
                        Assert.IsTrue(entity.meshGameObject != null, "meshGameObject is null");
                        Assert.AreEqual(c.gameObject.transform, c.placeholder.transform.parent,
                            "MaterialTransitionController is not parented correctly");
                        Assert.IsTrue(c.placeholder.GetComponent<MeshFilter>() != null,
                            "MeshFilter missing from placeholder object");
                        Assert.IsTrue(c.placeholder.GetComponent<Renderer>() != null,
                            "Renderer missing from placeholder object");
                        Assert.IsTrue(c.placeholder.GetComponent<Renderer>().sharedMaterial != null,
                            "SharedMaterial missing from placeholder's renderer");
                        Assert.AreEqual(hologramShader, c.placeholder.GetComponent<Renderer>().sharedMaterial.shader,
                            "Placeholder is not hologram");

                        yield return new WaitForSeconds(2f);

                        Assert.IsTrue(c == null, "MaterialTransitionController should be destroyed by now!");

                        break;
                    }
                }

                yield return null;
            }

            Assert.Less(timeout, 10.1f, "Timeout! MaterialTransitionController never appeared?");

            yield return null;
        }

        [UnityTest]
        public IEnumerator MaterialTransitionWithParametrizableMeshes()
        {
            yield return InitScene();

            var entity1 = TestHelpers.CreateSceneEntity(scene);

            ParcelSettings.VISUAL_LOADING_ENABLED = true;

            DecentralandEntity entity = null;
            ConeShape shape = TestHelpers.InstantiateEntityWithShape<ConeShape, ConeShape.Model>
            (
                scene,
                DCL.Models.CLASS_ID.CONE_SHAPE,
                new Vector3(2, 1, 3),
                out entity,
                new ConeShape.Model());

            yield return null;

            float timeout = 0;
            while (timeout < 10)
            {
                timeout += Time.deltaTime;

                if (entity.meshGameObject != null)
                {
                    var c = entity.meshGameObject.GetComponentInChildren<MaterialTransitionController>();

                    if (c != null) // NOTE(Brian): Wait for it
                    {
                        Assert.IsTrue(!c.useHologram, "useHologram must be false");
                        Assert.IsTrue(entity.meshGameObject != null, "meshGameObject is null");
                        Assert.IsTrue(c.placeholder == null,
                            "placeholder must be null because we're not using holograms with parametric shapes.");

                        yield return new WaitForSeconds(2f);

                        Assert.IsTrue(c == null, "MaterialTransitionController should be destroyed by now!");

                        break;
                    }
                }

                yield return null;
            }

            Assert.Less(timeout, 10.1f, "Timeout! MaterialTransitionController never appeared?");

            yield return null;
        }
    }
}