using System.Collections;
using DCL;
using UnityEngine.TestTools;

namespace Tests
{
    public class IntegrationTestSuite
    {
        protected virtual WorldRuntimeContext CreateRuntimeContext()
        {
            return DCL.Tests.WorldRuntimeContextFactory.CreateMocked();
        }

        protected virtual PlatformContext CreatePlatformContext()
        {
            return DCL.Tests.PlatformContextFactory.CreateMocked();
        }

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
                worldRuntimeBuilder: CreateRuntimeContext
            );
            yield break;
        }

        [UnityTearDown]
        protected virtual IEnumerator TearDown()
        {
            Environment.Dispose();
            yield break;
        }
    }
}