using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Rendering;
using NSubstitute;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace CullingControllerTests
{
    public class CullingControllerAndScenesShould : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();
            coreComponentsPlugin = new CoreComponentsPlugin();

            Environment.i.platform.cullingController.Stop();
            Assert.IsFalse(Environment.i.platform.cullingController.IsRunning());

            // If we get the settings copy here, it can be overriden with unwanted values
            // by QualitySettingsController, breaking this test!!. 
            var settings = new CullingControllerSettings();
            Environment.i.platform.cullingController.SetSettings(settings);
            Environment.i.platform.cullingController.Start();

            Assert.IsTrue(Environment.i.platform.cullingController.IsRunning());
            Assert.IsTrue(settings.enableObjectCulling);
        }

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            yield return base.TearDown();
        }

        protected override ServiceLocator InitializeServiceLocator()
        {
            var result = base.InitializeServiceLocator();
            result.Register<ICullingController>(CullingController.Create);
            return result;
        }


        [UnityTest]
        public IEnumerator CullMovingEntities()
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, Vector3.one * 1000, true);
            var entity = boxShape.attachedEntities.First();

            Assert.IsTrue(Environment.i.platform.cullingController.IsDirty(), "culling controller not dirty");
            Assert.IsTrue(Environment.i.platform.cullingController.objectsTracker.IsDirty(), "object tracker not dirty");

            yield return boxShape.routine;

            Environment.i.platform.cullingController.objectsTracker.ForcePopulateRenderersList();
            Environment.i.platform.cullingController.Restart();
            yield return new DCL.WaitUntil(() => entity.meshesInfo.renderers[0].forceRenderingOff, 0.3f);
            Assert.IsTrue(entity.meshesInfo.renderers[0].forceRenderingOff, "renderer wasn't hidden!");

            TestUtils.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.identity, Vector3.one);

            Environment.i.platform.cullingController.objectsTracker.ForcePopulateRenderersList();
            Environment.i.platform.cullingController.Restart();
            yield return new DCL.WaitUntil(() => !entity.meshesInfo.renderers[0].forceRenderingOff, 0.3f);
            Assert.IsFalse(entity.meshesInfo.renderers[0].forceRenderingOff, "renderer wasn't brought back!");
        }

        [UnityTest]
        public IEnumerator TogglingOnAndOffSetRenderersCorrectly()
        {
            DCL.Components.BoxShape boxShape = TestUtils.CreateEntityWithBoxShape(scene, Vector3.one * 1000, true);
            DCL.Models.IDCLEntity entity = boxShape.attachedEntities.First();

            Assert.IsTrue(Environment.i.platform.cullingController.IsDirty(), "culling controller not dirty");
            Assert.IsTrue(Environment.i.platform.cullingController.objectsTracker.IsDirty(), "object tracker not dirty");

            yield return boxShape.routine;

            Environment.i.platform.cullingController.objectsTracker.ForcePopulateRenderersList();
            Environment.i.platform.cullingController.Restart();
            yield return new DCL.WaitUntil(() => entity.meshesInfo.renderers[0].forceRenderingOff, 0.3f);
            Assert.IsTrue(entity.meshesInfo.renderers[0].forceRenderingOff, "renderer wasn't hidden!");

            TestUtils.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.identity, Vector3.one);

            CullingControllerSettings settings = Environment.i.platform.cullingController.GetSettingsCopy();
            settings.enableObjectCulling = false;
            Environment.i.platform.cullingController.SetSettings(settings);
            Environment.i.platform.cullingController.objectsTracker.ForcePopulateRenderersList();
            Environment.i.platform.cullingController.Restart();
            yield return new DCL.WaitUntil(() => entity.meshesInfo.renderers[0].forceRenderingOff, 0.3f);
            Assert.IsFalse(entity.meshesInfo.renderers[0].forceRenderingOff, "renderer wasn't brought back!");
        }
    }
}
