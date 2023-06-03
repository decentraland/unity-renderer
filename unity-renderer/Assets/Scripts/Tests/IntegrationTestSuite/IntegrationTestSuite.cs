using System;
using System.Collections;
using DCL;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

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
            Debug.Log($"Setting up {DateTime.Now}");
            CommonScriptableObjects.rendererState.Set(true);
            DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;
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
            AssetPromiseKeeper_GLTFast_Instance.i?.Cleanup();
            AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
            AssetPromiseKeeper_AB.i?.Cleanup();
            AssetPromiseKeeper_Texture.i?.Cleanup();
            AssetPromiseKeeper_AudioClip.i?.Cleanup();
            AssetPromiseKeeper_Gif.i?.Cleanup();
            DataStore.Clear();

            yield return null;
            Environment.Dispose();
        }
    }
}
