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

            AssetPromiseKeeper_GLTF.i.throttlingCounter.budgetPerFrameInMilliseconds = double.MaxValue;

            var sceneController = serviceLocator.Get<ISceneController>();
            sceneController.prewarmSceneMessagesPool = false;
            sceneController.prewarmEntitiesPool = false;
            sceneController.prewarmShaders = false;

            yield break;
        }

        [UnityTearDown]
        protected virtual IEnumerator TearDown()
        {
            Environment.Dispose();
            PoolManager.i?.Dispose();
            DataStore.Clear();
            yield break;
        }
    }
}