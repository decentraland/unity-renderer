using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Variables.SpawnPoints;

namespace Tests
{
    public class SpawnPointsDataHandlerShould
    {
        [Test]
        public void CreateSpawnPointIndicatorsCorrectly()
        {
            var spawnPoints = new[]
            {
                new SceneSpawnPoint()
                {
                    name = "Temptation",
                    @default = true,
                    position = new SceneSpawnPoint.Position()
                    {
                        x = new[] { 1f },
                        y = new[] { 1f },
                        z = new[] { 1f },
                    }
                },
                new SceneSpawnPoint()
                {
                    name = "Temptation2",
                    @default = false,
                    position = new SceneSpawnPoint.Position()
                    {
                        x = new[] { 1f },
                        y = new[] { 1f },
                        z = new[] { 1f },
                    }
                },
            };

            ISpawnPointIndicatorInstantiator instantiator = Substitute.For<ISpawnPointIndicatorInstantiator>();
            ISpawnPointIndicator indicator = Substitute.For<ISpawnPointIndicator>();
            instantiator.Instantiate().Returns(indicator);

            SpawnPointsDataHandler handler = new SpawnPointsDataHandler(instantiator);
            ((ISpawnPointsDataHandler)handler).CreateSpawnPoints("temptation", spawnPoints);

            instantiator.Received(spawnPoints.Length).Instantiate();

            indicator.Received().SetName(Arg.Any<string>());
            indicator.Received().SetPosition(Arg.Any<Vector3>());
            indicator.Received().SetSize(Arg.Any<Vector3>());
            indicator.Received().SetRotation(Arg.Any<Quaternion?>());

            Assert.AreEqual(spawnPoints.Length, handler.indicatorsBySceneId["temptation"].Count);
        }

        [Test]
        public void RemoveSpawnPointIndicatorsCorrectly()
        {
            var spawnPoints = new[]
            {
                new SceneSpawnPoint()
                {
                    name = "Temptation",
                    @default = true,
                    position = new SceneSpawnPoint.Position()
                    {
                        x = new[] { 1f },
                        y = new[] { 1f },
                        z = new[] { 1f },
                    }
                },
                new SceneSpawnPoint()
                {
                    name = "Temptation2",
                    @default = false,
                    position = new SceneSpawnPoint.Position()
                    {
                        x = new[] { 1f },
                        y = new[] { 1f },
                        z = new[] { 1f },
                    }
                },
            };

            ISpawnPointIndicatorInstantiator instantiator = Substitute.For<ISpawnPointIndicatorInstantiator>();
            instantiator.Instantiate().Returns(x => Substitute.For<ISpawnPointIndicator>());

            SpawnPointsDataHandler handler = new SpawnPointsDataHandler(instantiator);
            ((ISpawnPointsDataHandler)handler).CreateSpawnPoints("temptation", spawnPoints);

            var indicators = new List<ISpawnPointIndicator>(handler.indicatorsBySceneId["temptation"]);
            ((ISpawnPointsDataHandler)handler).RemoveSpawnPoints("temptation");

            foreach (var indicator in indicators)
            {
                indicator.Received(1).Dispose();
            }

            Assert.IsFalse(handler.indicatorsBySceneId.ContainsKey("temptation"));
        }

        [Test]
        public void DisposeCorrectly()
        {
            var indicators = new List<ISpawnPointIndicator>()
            {
                Substitute.For<ISpawnPointIndicator>(),
                Substitute.For<ISpawnPointIndicator>(),
                Substitute.For<ISpawnPointIndicator>(),
                Substitute.For<ISpawnPointIndicator>()
            };

            SpawnPointsDataHandler handler = new SpawnPointsDataHandler(null);
            handler.indicatorsBySceneId.Add("temptation", indicators);
            ((IDisposable)handler).Dispose();

            foreach (var indicator in indicators)
            {
                indicator.Received(1).Dispose();
            }

            Assert.AreEqual(0, handler.indicatorsBySceneId.Count);
        }

        [Test]
        public void CalculateNameCorrectly()
        {
            var spawnPoint = new SceneSpawnPoint()
            {
                name = "Temptation",
                @default = true,
                position = new SceneSpawnPoint.Position()
                {
                    x = new[] { 1f },
                    y = new[] { 1f },
                    z = new[] { 1f },
                }
            };
            var name = SpawnPointsDataHandler.GetName(spawnPoint);
            var expected = string.Format(SpawnPointsDataHandler.NAME_FORMAT, SpawnPointsDataHandler.POINT_TYPE_NAME,
                "Temptation", SpawnPointsDataHandler.NAME_DEFAULT_INDICATOR);
            Assert.AreEqual(expected, name);

            spawnPoint.@default = null;
            name = SpawnPointsDataHandler.GetName(spawnPoint);
            expected = string.Format(SpawnPointsDataHandler.NAME_FORMAT, SpawnPointsDataHandler.POINT_TYPE_NAME,
                "Temptation", "");
            Assert.AreEqual(expected, name);

            spawnPoint.@default = false;
            name = SpawnPointsDataHandler.GetName(spawnPoint);
            expected = string.Format(SpawnPointsDataHandler.NAME_FORMAT, SpawnPointsDataHandler.POINT_TYPE_NAME,
                "Temptation", "");
            Assert.AreEqual(expected, name);

            spawnPoint.position.x = new[] { 1f, 2f };
            name = SpawnPointsDataHandler.GetName(spawnPoint);
            expected = string.Format(SpawnPointsDataHandler.NAME_FORMAT, SpawnPointsDataHandler.AREA_TYPE_NAME,
                "Temptation", "");
            Assert.AreEqual(expected, name);
        }

        [Test]
        public void CalculateSizeCorrectly()
        {
            var spawnPoint = new SceneSpawnPoint()
            {
                position = new SceneSpawnPoint.Position()
                {
                    x = new[] { 1f },
                    y = new[] { 1f },
                    z = new[] { 1f },
                }
            };
            var size = SpawnPointsDataHandler.GetSize(spawnPoint);
            var expected = new Vector3(SpawnPointsDataHandler.SIZE_MIN_XZ, SpawnPointsDataHandler.SIZE_MIN_Y, SpawnPointsDataHandler.SIZE_MIN_XZ);
            Assert.AreEqual(expected, size);

            spawnPoint.position.x = new[] { 1f, 3f };
            size = SpawnPointsDataHandler.GetSize(spawnPoint);
            expected = new Vector3(2, SpawnPointsDataHandler.SIZE_MIN_Y, SpawnPointsDataHandler.SIZE_MIN_XZ);
            Assert.AreEqual(expected, size);

            spawnPoint.position.y = new[] { -5f, -1f };
            size = SpawnPointsDataHandler.GetSize(spawnPoint);
            expected = new Vector3(2, 4, SpawnPointsDataHandler.SIZE_MIN_XZ);
            Assert.AreEqual(expected, size);
        }

        [Test]
        public void CalculatePositionCorrectly()
        {
            var spawnPoint = new SceneSpawnPoint()
            {
                position = new SceneSpawnPoint.Position()
                {
                    x = new[] { 0f },
                    y = new[] { 0f },
                    z = new[] { 0f },
                }
            };
            var position = SpawnPointsDataHandler.GetPosition(spawnPoint, Vector3.one);
            var expected = new Vector3(0.5f, 0.5f, 0.5f);
            Assert.AreEqual(expected, position);

            spawnPoint.position.x = new[] { 2f };
            spawnPoint.position.y = new[] { 5f };
            position = SpawnPointsDataHandler.GetPosition(spawnPoint, new Vector3(4, 3, 2));
            expected = new Vector3(4, 6.5f, 1);
            Assert.AreEqual(expected, position);
        }

        [Test]
        public void CalculateRotationCorrectly()
        {
            var spawnPoint = new SceneSpawnPoint();
            var rotation = SpawnPointsDataHandler.GetLookAtRotation(spawnPoint, Vector3.zero);
            Assert.AreEqual(null, rotation);

            spawnPoint.cameraTarget = new Vector3(0, 0, 1);
            rotation = SpawnPointsDataHandler.GetLookAtRotation(spawnPoint, Vector3.zero);
            Assert.AreEqual(new Vector3(0, 0, 0), rotation.Value.eulerAngles);

            spawnPoint.cameraTarget = new Vector3(1, 0, 0);
            rotation = SpawnPointsDataHandler.GetLookAtRotation(spawnPoint, Vector3.zero);
            Assert.AreEqual(new Vector3(0, 90, 0), rotation.Value.eulerAngles);
        }
    }
}