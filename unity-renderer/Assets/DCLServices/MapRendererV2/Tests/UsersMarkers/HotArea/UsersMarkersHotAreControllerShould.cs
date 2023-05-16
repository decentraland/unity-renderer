using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea;
using MainScripts.DCL.Helpers.Utils;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.Tests.UsersMarkers.HotArea
{
    [Category("EditModeCI")]
    [TestFixture]
    public class UsersMarkersHotAreControllerShould
    {
        private const int PREWARM_COUNT = 50;

        private IUnityObjectPool<HotUserMarkerObject> objectsPool;
        private IObjectPool<IHotUserMarker> wrapsPool;
        private UsersMarkersHotAreaController controller;
        private IMapCullingController cullingController;

        private BaseDictionary<string, Player> otherPlayers;

        [SetUp]
        public async Task Setup()
        {
            controller = new UsersMarkersHotAreaController(
                otherPlayers = new BaseDictionary<string, Player>(),
                objectsPool = Substitute.For<IUnityObjectPool<HotUserMarkerObject>>(),
                wrapsPool = Substitute.For<IObjectPool<IHotUserMarker>>(),
                PREWARM_COUNT,
                null,
                Substitute.For<ICoordsUtils>(),
                cullingController = Substitute.For<IMapCullingController>());

            wrapsPool.Get().Returns(Substitute.For<IHotUserMarker>());

            await controller.Initialize(CancellationToken.None);
        }

        [Test]
        public async Task AddPlayer()
        {
            wrapsPool.ClearReceivedCalls();
            await controller.Enable(CancellationToken.None);

            var player = new Player { id = "1" };
            otherPlayers.Add(player.id, player);

            wrapsPool.Received(1).Get();

            AssertPlayerAdded(player);
        }

        [Test]
        public async Task RemovePlayer()
        {
            wrapsPool.ClearReceivedCalls();
            await controller.Enable(CancellationToken.None);

            var player = new Player { id = "1" };
            otherPlayers.Add(player.id, player);

            var marker = controller.Markers["1"];

            otherPlayers.Remove("1");

            wrapsPool.Received(1).Release(marker);
            Assert.IsFalse(controller.Markers.ContainsKey("1"));
        }

        [Test]
        public async Task NotListenIfDisabled()
        {
            await controller.Enable(CancellationToken.None);
            await UniTask.DelayFrame(1);
            await controller.Disable(CancellationToken.None);

            otherPlayers.Add("TEST", new Player {id = "TEST"});

            Assert.IsFalse(controller.Markers.ContainsKey("TEST"));
        }

        [Test]
        public async Task RestorePlayersOnEnable([NUnit.Framework.Range(1, 100, 5)] int playersCount)
        {
            wrapsPool.ClearReceivedCalls();
            cullingController.ClearReceivedCalls();
            var players = CreatePlayers(playersCount);

            foreach (Player player in players)
                otherPlayers.Add(player.id, player);

            await controller.Enable(CancellationToken.None);

            wrapsPool.Received(playersCount).Get();

            foreach (Player player in players)
                AssertPlayerAdded(player);
        }

        [Test]
        public async Task ReleasePlayersOnDisable([NUnit.Framework.Range(1, 100, 5)] int playersCount)
        {
            var players = CreatePlayers(playersCount);

            await controller.Enable(CancellationToken.None);

            foreach (Player player in players)
                otherPlayers.Add(player.id, player);

            var markers = controller.Markers.Values.ToArray();

            await controller.Disable(CancellationToken.None);

            Assert.AreEqual(0, controller.Markers.Count);

            foreach (var marker in markers)
                wrapsPool.Received().Release(marker);
        }

        [Test]
        public void PrewarmWrapsPool()
        {
            wrapsPool.Received().Prewarm(PREWARM_COUNT);
        }

        [Test]
        public void PrewarmObjectsPool()
        {
            objectsPool.Received().PrewarmAsync(PREWARM_COUNT, UsersMarkersHotAreaController.PREWARM_PER_FRAME, Arg.Any<CancellationToken>());
        }

        [Test]
        public void CleanUp()
        {
            controller.Dispose();

            objectsPool.Received(1).Clear();
            wrapsPool.Received(1).Clear();
        }

        private void AssertPlayerAdded(Player player)
        {
            controller.Markers[player.id].Received(1).TrackPlayer(player);
        }

        private IReadOnlyList<Player> CreatePlayers(int count)
        {
            return Enumerable.Range(1, count).Select(id => new Player { id = id.ToString() }).ToList();
        }
    }
}
