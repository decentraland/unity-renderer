using System.Collections;
using System.Collections.Generic;
using DCL.Rendering;
using NSubstitute;
using NSubstitute.Exceptions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace CullingControllerTests
{
    public class CullingControllerShould
    {
        public CullingController cullingController;

        [SetUp]
        public void SetUp()
        {
            cullingController = CreateMockedCulledController(null, null, null);
        }

        private static CullingController CreateMockedCulledController(UniversalRenderPipelineAsset urpAsset, CullingControllerSettings settings, ICullingObjectsTracker cullingObjectsTracker = null)
        {
            var result = Substitute.ForPartsOf<CullingController>(
                urpAsset == null ? GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset : urpAsset,
                settings ?? new CullingControllerSettings(),
                cullingObjectsTracker ?? new CullingObjectsTracker());

            result.SetSettings(new CullingControllerSettings() { maxTimeBudget = float.MaxValue });

            return result;
        }

        [TearDown]
        public void TearDown()
        {
            cullingController.Dispose();
        }

        [Test]
        public void ReturnCopyWhenGetSettingsIsCalled()
        {
            // Ensure settings never return null
            var settings = cullingController.GetSettingsCopy();
            Assert.IsNotNull(settings, "Settings should never be null!");

            var prevAnimationCulling = settings.enableAnimationCulling;

            // This is needed because SetSettings sets dirty flag and other stuff. We don't want direct access.
            // Settings is not a property because to make the SetSetting performance hit more obvious.
            settings.enableAnimationCulling = !settings.enableAnimationCulling;
            settings = cullingController.GetSettingsCopy();
            Assert.IsTrue(settings.enableAnimationCulling == prevAnimationCulling, "GetSettings should return a copy!");
        }

        [Test]
        public void SetSettingsProperly()
        {
            // Ensure settings never return null
            var settings = cullingController.GetSettingsCopy();
            Assert.IsNotNull(settings, "Settings should never be null!");

            var prevAnimationCulling = settings.enableAnimationCulling;

            // Ensure SetSettings works as intended.
            settings.enableAnimationCulling = !settings.enableAnimationCulling;
            cullingController.SetSettings(settings);
            settings = cullingController.GetSettingsCopy();
            Assert.IsTrue(settings.enableAnimationCulling != prevAnimationCulling, "SetSettings should assign the settings!");
        }

        [Test]
        public void EvaluateRendererVisibility()
        {
            //Arrange
            var profile = new CullingControllerProfile();
            profile.emissiveSizeThreshold = 10;
            profile.opaqueSizeThreshold = 20;
            profile.visibleDistanceThreshold = 5;

            // Act
            // Truth tests
            var farAndBigTest = CullingControllerUtils.TestRendererVisibleRule(profile, 30, 20, false, true, true);
            var smallAndNearTest = CullingControllerUtils.TestRendererVisibleRule(profile, 5, 2, false, true, true);
            var cameraInBoundsTest = CullingControllerUtils.TestRendererVisibleRule(profile, 1, 100, true, true, true);
            var emissiveTest = CullingControllerUtils.TestRendererVisibleRule(profile, 15, 20, false, false, true);

            // Falsehood tests
            var farAndSmallTest = CullingControllerUtils.TestRendererVisibleRule(profile, 5, 20, false, true, true);
            var emissiveAndFarTest = CullingControllerUtils.TestRendererVisibleRule(profile, 5, 20, false, false, true);
            var farAndTransparentTest = CullingControllerUtils.TestRendererVisibleRule(profile, 1, 50, false, false, false);

            // Assert
            Assert.IsTrue(farAndBigTest);
            Assert.IsTrue(smallAndNearTest);
            Assert.IsTrue(cameraInBoundsTest);
            Assert.IsTrue(emissiveTest);

            Assert.IsFalse(farAndSmallTest);
            Assert.IsFalse(emissiveAndFarTest);
            Assert.IsFalse(farAndTransparentTest);
        }

        [Test]
        public void EvaluateShadowVisibility()
        {
            // Arrange
            var profile = new CullingControllerProfile();
            profile.shadowMapProjectionSizeThreshold = 6;
            profile.shadowRendererSizeThreshold = 20;
            profile.shadowDistanceThreshold = 15;

            // Act
            var nearTest = CullingControllerUtils.TestRendererShadowRule(profile, 1, 5, 10);
            var nearButSmallTexel = CullingControllerUtils.TestRendererShadowRule(profile, 1, 5, 1);
            var farAndBigEnough = CullingControllerUtils.TestRendererShadowRule(profile, 30, 30, 30);
            var farAndSmall = CullingControllerUtils.TestRendererShadowRule(profile, 10, 30, 30);
            var farAndSmallTexel = CullingControllerUtils.TestRendererShadowRule(profile, 10, 30, 1);

            // Assert
            Assert.IsTrue(nearTest);
            Assert.IsTrue(farAndBigEnough);
            Assert.IsFalse(nearButSmallTexel);
            Assert.IsFalse(farAndSmall);
            Assert.IsFalse(farAndSmallTexel);
        }

        [Test]
        public void EvaluateSkinnedMeshesOffscreenUpdate()
        {
            // Arrange
            var profile = new CullingControllerProfile();
            profile.shadowMapProjectionSizeThreshold = 6;
            profile.shadowRendererSizeThreshold = 20;
            profile.shadowDistanceThreshold = 15;

            var settings = new CullingControllerSettings();
            settings.enableAnimationCullingDistance = 20;

            // Act
            var farTest = CullingControllerUtils.TestSkinnedRendererOffscreenRule(settings, 30);
            var nearTest = CullingControllerUtils.TestSkinnedRendererOffscreenRule(settings, 10);

            settings.enableAnimationCulling = false;
            var farTestWithCullingDisabled = CullingControllerUtils.TestSkinnedRendererOffscreenRule(settings, 30);

            // Assert
            Assert.IsTrue(nearTest);
            Assert.IsFalse(farTest);
            Assert.IsTrue(farTestWithCullingDisabled);
        }

        [Test]
        public void ResetObjects()
        {
            // Arrange
            GameObject go1 = new GameObject();
            GameObject go2 = new GameObject();

            var r = go1.AddComponent<MeshRenderer>();
            var skr = go2.AddComponent<SkinnedMeshRenderer>();
            var anim = go2.AddComponent<Animation>();

            r.forceRenderingOff = true;
            skr.updateWhenOffscreen = false;
            anim.cullingType = AnimationCullingType.BasedOnRenderers;

            var mockTracker = Substitute.For<ICullingObjectsTracker>();
            cullingController = CreateMockedCulledController(null, null, mockTracker);

            mockTracker.GetRenderers().Returns(info => go1.GetComponentsInChildren<Renderer>());
            mockTracker.GetSkinnedRenderers().Returns(info => go2.GetComponentsInChildren<SkinnedMeshRenderer>());
            mockTracker.GetAnimations().Returns(info => go2.GetComponentsInChildren<Animation>());

            // Act
            cullingController.ResetObjects();

            // Assert
            Assert.IsFalse(r.forceRenderingOff);
            Assert.IsTrue(skr.updateWhenOffscreen);
            Assert.IsTrue(anim.cullingType == AnimationCullingType.AlwaysAnimate);

            // Annihilate
            Object.Destroy(go1);
            Object.Destroy(go2);
        }

        [UnityTest]
        public IEnumerator ProcessAnimationCulling()
        {
            // Pre-arrange
            const int GO_COUNT = 3;
            GameObject[] gos = new GameObject[GO_COUNT];
            Animation[] anims = new Animation[GO_COUNT];

            for (int i = 0; i < GO_COUNT; i++)
            {
                gos[i] = new GameObject("Test " + i);
                anims[i] = gos[i].AddComponent<Animation>();
                anims[i].cullingType = AnimationCullingType.AlwaysAnimate;
            }

            var settings = cullingController.GetSettingsCopy();

            settings.enableAnimationCulling = true;
            settings.enableAnimationCullingDistance = 15;

            cullingController.SetSettings(settings);

            cullingController.objectsTracker.MarkDirty();
            yield return cullingController.objectsTracker.PopulateRenderersList();

            // Test #1
            gos[0].transform.position = Vector3.zero;
            gos[1].transform.position = new Vector3(0, 0, 30);
            gos[2].transform.position = new Vector3(0, 0, 10);
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);

            yield return cullingController.ProcessAnimations();

            Assert.AreEqual(AnimationCullingType.AlwaysAnimate, anims[0].cullingType);
            Assert.AreEqual(AnimationCullingType.BasedOnRenderers, anims[1].cullingType);
            Assert.AreEqual(AnimationCullingType.AlwaysAnimate, anims[2].cullingType);

            // Test #2
            gos[2].transform.position = new Vector3(0, 0, 30);
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);

            yield return cullingController.ProcessAnimations();

            Assert.AreEqual(AnimationCullingType.BasedOnRenderers, anims[2].cullingType);

            // Test #3
            gos[2].transform.position = new Vector3(0, 0, 10);
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);

            yield return cullingController.ProcessAnimations();

            // Test #4
            settings.enableAnimationCulling = false;
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.one * 10000);

            for (int i = 0; i < GO_COUNT; i++)
            {
                anims[i].cullingType = AnimationCullingType.AlwaysAnimate;
            }

            cullingController.SetSettings(settings);

            yield return cullingController.ProcessAnimations();

            for (int i = 0; i < GO_COUNT; i++)
            {
                Assert.AreEqual(AnimationCullingType.AlwaysAnimate, anims[i].cullingType);
            }

            // Annihilate
            for (int i = 0; i < GO_COUNT; i++)
            {
                Object.Destroy(gos[i]);
            }
        }

        [UnityTest]
        public IEnumerator ReactToRendererState()
        {
            cullingController.Start();
            yield return null;
            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.rendererState.Set(false);
            Assert.IsFalse(cullingController.IsRunning());
            CommonScriptableObjects.rendererState.Set(true);
            Assert.IsTrue(cullingController.IsRunning());
            CommonScriptableObjects.rendererState.Set(false);
            Assert.IsFalse(cullingController.IsRunning());
            cullingController.Stop();
            yield return null;
            CommonScriptableObjects.rendererState.Set(true);
            Assert.IsFalse(cullingController.IsRunning());
        }

        [UnityTest]
        public IEnumerator ProcessProfile()
        {
            // Pre-arrange
            const int GO_COUNT = 10;
            GameObject[] gos = new GameObject[GO_COUNT];
            Renderer[] rends = new Renderer[GO_COUNT];

            for (int i = 0; i < GO_COUNT; i++)
            {
                gos[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);

                if (i >= GO_COUNT >> 1)
                {
                    Object.DestroyImmediate(gos[i].GetComponent<Renderer>());
                    gos[i].AddComponent<SkinnedMeshRenderer>();
                }

                rends[i] = gos[i].GetComponent<Renderer>();
            }

            var settings = cullingController.GetSettingsCopy();

            settings.skinnedRendererProfile.visibleDistanceThreshold = 0;
            settings.skinnedRendererProfile.emissiveSizeThreshold = 0;
            settings.skinnedRendererProfile.opaqueSizeThreshold = 0;
            settings.skinnedRendererProfile.shadowDistanceThreshold = 0;
            settings.skinnedRendererProfile.shadowRendererSizeThreshold = 0;
            settings.skinnedRendererProfile.shadowMapProjectionSizeThreshold = 0;

            settings.rendererProfile.visibleDistanceThreshold = 0;
            settings.rendererProfile.emissiveSizeThreshold = 0;
            settings.rendererProfile.opaqueSizeThreshold = 0;
            settings.rendererProfile.shadowDistanceThreshold = 0;
            settings.rendererProfile.shadowRendererSizeThreshold = 0;
            settings.rendererProfile.shadowMapProjectionSizeThreshold = 0;

            settings.enableObjectCulling = true;

            cullingController.SetSettings(settings);
            cullingController.objectsTracker.MarkDirty();

            yield return cullingController.objectsTracker.PopulateRenderersList();

            yield return cullingController.ProcessProfile(settings.rendererProfile);

            for (int i = 0; i < 5; i++)
            {
                cullingController.Received().SetCullingForRenderer(rends[i], true, true);
            }

            yield return cullingController.ProcessProfile(settings.skinnedRendererProfile);

            for (int i = 5; i < GO_COUNT; i++)
            {
                cullingController.Received().SetCullingForRenderer(rends[i], true, true);
            }

            // Annihilate
            for (int i = 0; i < GO_COUNT; i++)
            {
                Object.Destroy(gos[i]);
            }
        }
    }
}