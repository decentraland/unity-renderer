using System.Collections;
using DCL;
using NSubstitute.ClearExtensions;
using UnityEngine.TestTools;

namespace Tests
{
    public class IntegrationTestSuite
    {
        protected virtual WorldRuntimeContext CreateRuntimeContext() { return DCL.Tests.WorldRuntimeContextFactory.CreateMocked(); }

        protected virtual PlatformContext CreatePlatformContext() { return DCL.Tests.PlatformContextFactory.CreateMocked(); }

        protected virtual MessagingContext CreateMessagingContext()
        {
            return DCL.Tests.MessagingContextFactory.CreateMocked();
        }

        [UnitySetUp]
        protected virtual IEnumerator SetUp()
        {
            Environment.SetupWithBuilders(
                messagingBuilder: CreateMessagingContext,
                platformBuilder: CreatePlatformContext,
                worldRuntimeBuilder: CreateRuntimeContext,
                hudBuilder: HUDContextFactory.CreateDefault
            );

            AssetPromiseKeeper_GLTF.i.throttlingCounter.budgetPerFrameInMilliseconds = double.MaxValue;

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