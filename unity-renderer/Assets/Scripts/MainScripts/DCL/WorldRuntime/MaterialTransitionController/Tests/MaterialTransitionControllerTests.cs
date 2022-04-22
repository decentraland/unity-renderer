using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = UnityEngine.WaitUntil;

namespace Tests
{
    public class MaterialTransitionControllerTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            yield return base.TearDown();
        }

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();
            coreComponentsPlugin = new CoreComponentsPlugin();
            CommonScriptableObjects.rendererState.Set(true);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test is too slow")]
        public IEnumerator MaterialTransitionWithGLTF()
        {
            var entity1 = TestUtils.CreateSceneEntity(scene);

            ParcelSettings.VISUAL_LOADING_ENABLED = true;

            var prevLoadingType = RendereableAssetLoadHelper.defaultLoadingType;
            RendereableAssetLoadHelper.defaultLoadingType = RendereableAssetLoadHelper.LoadingType.GLTF_ONLY;

            Shader hologramShader = Shader.Find("DCL/FX/Hologram");

            Assert.IsTrue(hologramShader != null, "Hologram shader == null??");

            IDCLEntity entity = null;

            GLTFShape shape = TestUtils.InstantiateEntityWithShape<GLTFShape, GLTFShape.Model>
            (scene,
                DCL.Models.CLASS_ID.GLTF_SHAPE,
                Vector3.zero,
                out entity,
                new GLTFShape.Model() {src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"});

            LoadWrapper gltfShape = DCL.Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            float timeout = 0;

            while (timeout < 10)
            {
                timeout += Time.deltaTime;

                if (entity.meshRootGameObject != null)
                {
                    var c = entity.meshRootGameObject.GetComponentInChildren<MaterialTransitionController>();

                    if (c != null && c.placeholder != null) // NOTE(Brian): Wait for it
                    {
                        Assert.IsTrue(c.useHologram, "useHologram must be true");
                        Assert.IsTrue(entity.meshRootGameObject != null, "meshGameObject is null");
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

            RendereableAssetLoadHelper.defaultLoadingType = prevLoadingType;

            yield return null;
        }

        [UnityTest]
        public IEnumerator MaterialTransitionWithParametrizableMeshes()
        {
            DCL.Configuration.EnvironmentSettings.DEBUG = true;

            var entity1 = TestUtils.CreateSceneEntity(scene);

            ParcelSettings.VISUAL_LOADING_ENABLED = true;

            IDCLEntity entity = null;
            ConeShape shape = TestUtils.InstantiateEntityWithShape<ConeShape, ConeShape.Model>
            (
                scene,
                DCL.Models.CLASS_ID.CONE_SHAPE,
                new Vector3(2, 1, 3),
                out entity,
                new ConeShape.Model());

            yield return null;

            float timeout = 0f;
            float maxTime = 20f;
            while (timeout < maxTime)
            {
                timeout += Time.deltaTime;

                if (timeout > maxTime)
                    timeout = maxTime;

                if (entity.meshRootGameObject != null)
                {
                    var c = entity.meshRootGameObject.GetComponentInChildren<MaterialTransitionController>();

                    if (c != null) // NOTE(Brian): Wait for it
                    {
                        Assert.IsTrue(!c.useHologram, "useHologram must be false");
                        Assert.IsTrue(entity.meshRootGameObject != null, "meshGameObject is null");
                        Assert.IsTrue(c.placeholder == null,
                            "placeholder must be null because we're not using holograms with parametric shapes.");

                        yield return new DCL.WaitUntil(() => c == null, 5.0f);

                        Assert.IsTrue(c == null, "MaterialTransitionController should be destroyed by now!");

                        break;
                    }
                }

                yield return null;
            }

            Assert.Less(timeout, maxTime + 0.1f, "Timeout! MaterialTransitionController never appeared?");

            yield return null;
        }
    }
}