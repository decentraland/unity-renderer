using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using DCL.Controllers;
using DCL.Shaders;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TexturesTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;
        private UIComponentsPlugin uiComponentsPlugin;

        const string BASE64_TEXTURE = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAApgAAAKYB3X3/OAAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAANCSURBVEiJtZZPbBtFFMZ/M7ubXdtdb1xSFyeilBapySVU8h8OoFaooFSqiihIVIpQBKci6KEg9Q6H9kovIHoCIVQJJCKE1ENFjnAgcaSGC6rEnxBwA04Tx43t2FnvDAfjkNibxgHxnWb2e/u992bee7tCa00YFsffekFY+nUzFtjW0LrvjRXrCDIAaPLlW0nHL0SsZtVoaF98mLrx3pdhOqLtYPHChahZcYYO7KvPFxvRl5XPp1sN3adWiD1ZAqD6XYK1b/dvE5IWryTt2udLFedwc1+9kLp+vbbpoDh+6TklxBeAi9TL0taeWpdmZzQDry0AcO+jQ12RyohqqoYoo8RDwJrU+qXkjWtfi8Xxt58BdQuwQs9qC/afLwCw8tnQbqYAPsgxE1S6F3EAIXux2oQFKm0ihMsOF71dHYx+f3NND68ghCu1YIoePPQN1pGRABkJ6Bus96CutRZMydTl+TvuiRW1m3n0eDl0vRPcEysqdXn+jsQPsrHMquGeXEaY4Yk4wxWcY5V/9scqOMOVUFthatyTy8QyqwZ+kDURKoMWxNKr2EeqVKcTNOajqKoBgOE28U4tdQl5p5bwCw7BWquaZSzAPlwjlithJtp3pTImSqQRrb2Z8PHGigD4RZuNX6JYj6wj7O4TFLbCO/Mn/m8R+h6rYSUb3ekokRY6f/YukArN979jcW+V/S8g0eT/N3VN3kTqWbQ428m9/8k0P/1aIhF36PccEl6EhOcAUCrXKZXXWS3XKd2vc/TRBG9O5ELC17MmWubD2nKhUKZa26Ba2+D3P+4/MNCFwg59oWVeYhkzgN/JDR8deKBoD7Y+ljEjGZ0sosXVTvbc6RHirr2reNy1OXd6pJsQ+gqjk8VWFYmHrwBzW/n+uMPFiRwHB2I7ih8ciHFxIkd/3Omk5tCDV1t+2nNu5sxxpDFNx+huNhVT3/zMDz8usXC3ddaHBj1GHj/As08fwTS7Kt1HBTmyN29vdwAw+/wbwLVOJ3uAD1wi/dUH7Qei66PfyuRj4Ik9is+hglfbkbfR3cnZm7chlUWLdwmprtCohX4HUtlOcQjLYCu+fzGJH2QRKvP3UNz8bWk1qMxjGTOMThZ3kvgLI5AzFfo379UAAAAASUVORK5CYII=";

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            coreComponentsPlugin = new CoreComponentsPlugin();
            uiComponentsPlugin = new UIComponentsPlugin();
            scene = TestUtils.CreateTestScene();
        }

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            uiComponentsPlugin.Dispose();
            yield return base.TearDown();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        [Explicit("Broke with Legacy suite refactor, fix later")]
        [Category("Explicit")]
        public IEnumerator TextureCreateAndLoadTest()
        {
            DCLTexture dclTexture = TestUtils.CreateDCLTexture(scene,
                TestAssetsUtils.GetPath() + "/Images/avatar.png",
                DCLTexture.BabylonWrapMode.CLAMP,
                FilterMode.Bilinear);

            yield return dclTexture.routine;

            Assert.IsTrue(dclTexture.texture != null, "Texture didn't load correctly?");
            Assert.IsTrue(dclTexture.unityWrap == TextureWrapMode.Clamp, "Bad wrap mode!");
            Assert.IsTrue(dclTexture.unitySamplingMode == FilterMode.Bilinear, "Bad sampling mode!");

            dclTexture.Dispose();

            yield return null;
            Assert.IsTrue(dclTexture.texture == null, "Texture didn't dispose correctly?");
        }

        [UnityTest]
        public IEnumerator TextureAttachedGetsReplacedOnNewAttachment()
        {
            yield return TestUtils.TestAttachedSharedComponentOfSameTypeIsReplaced<DCLTexture.Model, DCLTexture>(
                scene, CLASS_ID.TEXTURE);
        }

        [Test]
        public void Texture_OnReadyBeforeLoading()
        {
            DCLTexture dclTexture = TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/avatar.png");
            bool isOnReady = false;
            dclTexture.CallWhenReady((x) => { isOnReady = true; });

            Assert.IsTrue(isOnReady); //DCLTexture is ready on creation
        }

        [UnityTest]
        public IEnumerator Texture_OnReadyWaitLoading()
        {
            DCLTexture dclTexture = TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/avatar.png");
            bool isOnReady = false;
            dclTexture.CallWhenReady((x) => { isOnReady = true; });
            yield return dclTexture.routine;

            Assert.IsTrue(isOnReady);
        }

        [UnityTest]
        public IEnumerator Texture_OnReadyAfterLoadingInstantlyCalled()
        {
            DCLTexture dclTexture = TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/avatar.png");
            yield return dclTexture.routine;

            bool isOnReady = false;
            dclTexture.CallWhenReady((x) => { isOnReady = true; });
            Assert.IsTrue(isOnReady);
        }

        [UnityTest]
        public IEnumerator Texture_Disposed_BasicMaterialUpdated()
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
            DCLTexture texture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);

            BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
                scene,
                DCL.Models.CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model()
                {
                    texture = texture.id
                });

            TestUtils.SharedComponentAttach(basicMaterial, entity);
            TestUtils.SharedComponentAttach(texture, entity);

            yield return basicMaterial.routine;

            Texture mainTex = basicMaterial.material.GetTexture(ShaderUtils.BaseMap);

            // texture should be created
            Assert.IsTrue(mainTex);

            TestUtils.SharedComponentUpdate(basicMaterial, new BasicMaterial.Model()
            {
                texture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id
            });

            yield return basicMaterial.routine;
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(mainTex);
        }

        [UnityTest]
        public IEnumerator Texture_Disposed_PBRMaterialUpdated()
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
            DCLTexture dclTextureA = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);
            DCLTexture dclTextureB = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);
            DCLTexture dclTextureC = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);
            DCLTexture dclTextureD = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);

            PBRMaterial pbrMaterial = TestUtils.SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(
                scene,
                DCL.Models.CLASS_ID.PBR_MATERIAL,
                new PBRMaterial.Model()
                {
                    albedoTexture = dclTextureA.id,
                    alphaTexture = dclTextureB.id,
                    bumpTexture = dclTextureC.id,
                    emissiveTexture = dclTextureD.id
                });

            TestUtils.SharedComponentAttach(pbrMaterial, entity);
            TestUtils.SharedComponentAttach(dclTextureA, entity);
            TestUtils.SharedComponentAttach(dclTextureB, entity);
            TestUtils.SharedComponentAttach(dclTextureC, entity);
            TestUtils.SharedComponentAttach(dclTextureD, entity);

            yield return pbrMaterial.routine;

            Texture textureA = pbrMaterial.material.GetTexture(ShaderUtils.EmissionMap);
            Texture textureB = pbrMaterial.material.GetTexture(ShaderUtils.BaseMap);
            Texture textureC = pbrMaterial.material.GetTexture(ShaderUtils.AlphaTexture);
            Texture textureD = pbrMaterial.material.GetTexture(ShaderUtils.BumpMap);

            // textures should be created
            Assert.IsTrue(textureA);
            Assert.IsTrue(textureB);
            Assert.IsTrue(textureC);
            Assert.IsTrue(textureD);

            TestUtils.SharedComponentUpdate(pbrMaterial, new PBRMaterial.Model()
            {
                albedoTexture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id,
                alphaTexture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id,
                bumpTexture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id,
                emissiveTexture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id
            });

            yield return pbrMaterial.routine;
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(textureA);
            Assert.IsFalse(textureB);
            Assert.IsFalse(textureC);
            Assert.IsFalse(textureD);
        }

        [UnityTest]
        public IEnumerator Texture_Disposed_UIImageUpdated()
        {
            // UIScreenSpace needed for UIImage
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            UIImage uiImage = TestUtils.SharedComponentCreate<UIImage, UIImage.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_IMAGE_SHAPE,
                new UIImage.Model()
                {
                    source = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id
                });

            // we cant `yield return uiImage.routine` since UIImage does not yield
            // so we just try to guess the number of frames to skip
            yield return null;
            yield return null;

            Texture imageTexture = uiImage.referencesContainer.image.texture;

            // texture should be created
            Assert.IsTrue(imageTexture);

            TestUtils.SharedComponentUpdate(uiImage, new UIImage.Model()
            {
                source = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id
            });

            // we cant `yield return uiImage.routine` since UIImage does not yield
            // so we just try to guess the number of frames to skip
            yield return null;
            yield return null;
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(imageTexture);
        }

        [UnityTest]
        public IEnumerator Texture_Disposed_BasicMaterialRemoved()
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
            DCLTexture texture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);

            BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
                scene,
                DCL.Models.CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model()
                {
                    texture = texture.id
                });

            TestUtils.SharedComponentAttach(basicMaterial, entity);
            TestUtils.SharedComponentAttach(texture, entity);

            yield return basicMaterial.routine;

            Texture mainTex = basicMaterial.material.GetTexture(ShaderUtils.BaseMap);

            // texture should be created
            Assert.IsTrue(mainTex);

            TestUtils.SharedComponentDispose(basicMaterial);

            // frame skip
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(mainTex);
        }

        [UnityTest]
        public IEnumerator Texture_Disposed_PBRMaterialRemoved()
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
            DCLTexture dclTextureA = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);
            DCLTexture dclTextureB = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);
            DCLTexture dclTextureC = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);
            DCLTexture dclTextureD = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);

            PBRMaterial pbrMaterial = TestUtils.SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(
                scene,
                DCL.Models.CLASS_ID.PBR_MATERIAL,
                new PBRMaterial.Model()
                {
                    albedoTexture = dclTextureA.id,
                    alphaTexture = dclTextureB.id,
                    bumpTexture = dclTextureC.id,
                    emissiveTexture = dclTextureD.id
                });

            TestUtils.SharedComponentAttach(pbrMaterial, entity);
            TestUtils.SharedComponentAttach(dclTextureA, entity);
            TestUtils.SharedComponentAttach(dclTextureB, entity);
            TestUtils.SharedComponentAttach(dclTextureC, entity);
            TestUtils.SharedComponentAttach(dclTextureD, entity);

            yield return pbrMaterial.routine;

            Texture textureA = pbrMaterial.material.GetTexture(ShaderUtils.EmissionMap);
            Texture textureB = pbrMaterial.material.GetTexture(ShaderUtils.BaseMap);
            Texture textureC = pbrMaterial.material.GetTexture(ShaderUtils.AlphaTexture);
            Texture textureD = pbrMaterial.material.GetTexture(ShaderUtils.BumpMap);

            // textures should be created
            Assert.IsTrue(textureA);
            Assert.IsTrue(textureB);
            Assert.IsTrue(textureC);
            Assert.IsTrue(textureD);

            TestUtils.SharedComponentDispose(pbrMaterial);

            // frame skip
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(textureA);
            Assert.IsFalse(textureB);
            Assert.IsFalse(textureC);
            Assert.IsFalse(textureD);
        }

        [UnityTest]
        public IEnumerator Texture_Disposed_UIImageRemoved()
        {
            // UIScreenSpace needed for UIImage
            UIScreenSpace screenSpaceShape =
                TestUtils.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene,
                    CLASS_ID.UI_SCREEN_SPACE_SHAPE);
            yield return screenSpaceShape.routine;

            UIImage uiImage = TestUtils.SharedComponentCreate<UIImage, UIImage.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_IMAGE_SHAPE,
                new UIImage.Model()
                {
                    source = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id
                });

            // we cant `yield return uiImage.routine` since UIImage does not yield
            // so we just try to guess the number of frames to skip
            yield return null;
            yield return null;

            Texture imageTexture = uiImage.referencesContainer.image.texture;

            // texture should be created
            Assert.IsTrue(imageTexture);

            TestUtils.SharedComponentDispose(uiImage);

            // frame skip
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(imageTexture);
        }

        [UnityTest]
        public IEnumerator Texture_ReloadsCorrectly()
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
            DCLTexture texture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE);

            BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
                scene,
                DCL.Models.CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model()
                {
                    texture = texture.id
                });

            TestUtils.SharedComponentAttach(basicMaterial, entity);
            TestUtils.SharedComponentAttach(texture, entity);

            yield return basicMaterial.routine;

            Texture mainTex = basicMaterial.material.GetTexture(ShaderUtils.BaseMap);

            // texture should be created
            Assert.IsTrue(mainTex);

            TestUtils.SharedComponentUpdate(basicMaterial, new BasicMaterial.Model()
            {
                texture = TestUtils.CreateDCLTexture(scene, BASE64_TEXTURE).id
            });

            yield return basicMaterial.routine;
            yield return null;

            // texture should have being disposed
            Assert.IsFalse(mainTex);

            TestUtils.SharedComponentUpdate(basicMaterial, new BasicMaterial.Model()
            {
                texture = texture.id
            });

            yield return basicMaterial.routine;

            // texture should have being reloaded
            Assert.IsTrue(basicMaterial.material.GetTexture(ShaderUtils.BaseMap));
        }
    }
}
