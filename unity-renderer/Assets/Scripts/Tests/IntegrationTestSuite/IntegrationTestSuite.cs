using System.Collections;
using DCL;
using DCL.Tests;
using NSubstitute;
using NSubstitute.ClearExtensions;
using UnityEngine.TestTools;

namespace Tests
{
    public class IntegrationTestSuite
    {
        protected virtual void InitializeServices(ServiceLocator serviceLocator)
        {
        }

        [UnitySetUp]
        protected virtual IEnumerator SetUp()
        {
            ServiceLocator serviceLocator = DCL.Tests.ServiceLocatorFactory.CreateMocked();
            InitializeServices(serviceLocator);
            Environment.Setup(serviceLocator);

            AssetPromiseKeeper_GLTF.i.throttlingCounter.enabled = false;
            DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
            DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;

            var sceneController = serviceLocator.Get<ISceneController>();
            sceneController.prewarmSceneMessagesPool = false;
            sceneController.prewarmEntitiesPool = false;
            sceneController.prewarmShaders = false;

            yield break;
        }

        [UnityTearDown]
        protected virtual IEnumerator TearDown()
        {
            PoolManager.i?.Dispose();
            DataStore.Clear();

            yield return null;
            Environment.Dispose();
        }
    }
}