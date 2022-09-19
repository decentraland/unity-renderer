using DCL.ECS7;
using NUnit.Framework;

namespace Tests
{
    public class ECS7PluginShould : IntegrationTestSuite
    {
        [Test]
        public void CreateAndDisposeCorrectly()
        {
            var plugin = new ECS7Plugin();
            plugin.Dispose();
        }
    }
}