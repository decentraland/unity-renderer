using System.Collections;
using DCL;
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
            DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;
            DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
            AssetPromiseKeeper_GLTF.i.throttlingCounter.enabled = false;
            PoolManager.enablePrewarm = false;

            ServiceLocator serviceLocator = DCL.ServiceLocatorTestFactory.CreateMocked();
            InitializeServices(serviceLocator);
            Environment.Setup(serviceLocator);
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