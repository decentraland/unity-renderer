using NSubstitute;
using NUnit.Framework;
using Variables.SpawnPoints;

namespace Tests
{
    public class SpawnPointsDisplayerPluginShould
    {
        private SpawnPointsDisplayerPlugin plugin;
        private BaseDictionary<string, SceneSpawnPointsData> spawnPointsVariable;
        private ISpawnPointsDataHandler spawnPointsDataHandler;

        [SetUp]
        public void SetUp()
        {
            spawnPointsDataHandler = Substitute.For<ISpawnPointsDataHandler>();
            spawnPointsVariable = new BaseDictionary<string, SceneSpawnPointsData>();
            plugin = new SpawnPointsDisplayerPlugin(spawnPointsVariable, spawnPointsDataHandler);
        }

        [TearDown]
        public void TearDown()
        {
            plugin.Dispose();
        }

        [Test]
        public void CreateSpawnPoints()
        {
            SceneSpawnPoint[] spawnPoints = new[] { new SceneSpawnPoint() };
            SceneSpawnPointsData spawnPointsInfo = new SceneSpawnPointsData() { enabled = true, spawnPoints = spawnPoints };

            spawnPointsVariable.AddOrSet("temptation", spawnPointsInfo);
            spawnPointsDataHandler.Received(1).RemoveSpawnPoints("temptation");
            spawnPointsDataHandler.Received(1).CreateSpawnPoints("temptation", spawnPoints);
        }

        [Test]
        public void RemoveSpawnPoints()
        {
            SceneSpawnPoint[] spawnPoints = new[] { new SceneSpawnPoint() };
            SceneSpawnPointsData spawnPointsInfo = new SceneSpawnPointsData() { enabled = true, spawnPoints = spawnPoints };

            spawnPointsVariable.AddOrSet("temptation", spawnPointsInfo);
            spawnPointsVariable.Remove("temptation");
            spawnPointsDataHandler.Received(2).RemoveSpawnPoints("temptation");
        }

        [Test]
        public void DisableSpawnPoints()
        {
            SceneSpawnPoint[] spawnPoints = new[] { new SceneSpawnPoint() };
            SceneSpawnPointsData spawnPointsInfo = new SceneSpawnPointsData() { enabled = false, spawnPoints = spawnPoints };

            spawnPointsVariable.AddOrSet("temptation", spawnPointsInfo);
            spawnPointsDataHandler.Received(1).RemoveSpawnPoints("temptation");
        }

        [Test]
        public void DisposeCorrectly()
        {
            plugin.Dispose();
            spawnPointsDataHandler.Received(1).Dispose();
        }
    }
}