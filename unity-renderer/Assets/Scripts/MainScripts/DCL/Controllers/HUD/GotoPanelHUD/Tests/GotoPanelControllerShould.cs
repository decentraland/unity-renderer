using Cysharp.Threading.Tasks;
using DCL.Map;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace DCL.GoToPanel
{
    public class GotoPanelControllerShould
    {
        private GotoPanelHUDController controller;
        private IGotoPanelHUDView view;
        private DataStore dataStore;
        private IMinimapApiBridge minimapApiBridge;
        private ITeleportController teleportController;
        private BaseVariable<bool> visible => dataStore.HUDs.gotoPanelVisible;
        private BaseVariable<ParcelCoordinates> coords => dataStore.HUDs.gotoPanelCoordinates;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IGotoPanelHUDView>();

            dataStore = new DataStore();

            minimapApiBridge = Substitute.For<IMinimapApiBridge>();

            teleportController = Substitute.For<ITeleportController>();

            controller = new GotoPanelHUDController(view, dataStore, teleportController,
                minimapApiBridge);
        }

        [TearDown]
        public void TearDown()
        {
            DataStore.Clear();
        }

        [Test]
        public void GetVisibleValueChangeToTrue()
        {
            visible.Set(true, true);
            view.Received(1).SetVisible(true);
        }

        [Test]
        public void GetVisibleValueChangeToFalse()
        {
            visible.Set(false, true);
            view.Received(1).SetVisible(false);
        }

        [Test]
        public void GetCoordinatesValueChange()
        {
            var sceneInfo = new MinimapMetadata.MinimapSceneInfo
            {
                description = "desc",
                name = "name",
                owner = "owner",
                parcels = new List<Vector2Int> { new (10, 30) },
                type = MinimapMetadata.TileType.Plaza,
                previewImageUrl = "url",
                isPOI = false,
            };

            minimapApiBridge.GetScenesInformationAroundParcel(Arg.Is<Vector2Int>(v => v.x == 10 && v.y == 30),
                                 2,
                                 Arg.Any<CancellationToken>())
                            .Returns(UniTask.FromResult(new[]
                             {
                                 sceneInfo,
                                 new MinimapMetadata.MinimapSceneInfo
                                 {
                                     description = "",
                                     name = "adjacent",
                                     owner = "wo",
                                     parcels = new List<Vector2Int> { new (11, 30) },
                                     type = MinimapMetadata.TileType.Background,
                                     previewImageUrl = "other",
                                     isPOI = false,
                                 },
                             }));

            ParcelCoordinates gotoCoords = new ParcelCoordinates(10, 30);
            coords.Set(gotoCoords);

            view.Received(1).SetPanelInfo(gotoCoords, sceneInfo);
        }

        [Test]
        public void ShowAndHideLoadingWhenRequestingSceneInformation()
        {
            minimapApiBridge.GetScenesInformationAroundParcel(Arg.Is<Vector2Int>(v => v.x == 5 && v.y == 7),
                                 2,
                                 Arg.Any<CancellationToken>())
                            .Returns(UniTask.FromResult(new[]
                             {
                                 new MinimapMetadata.MinimapSceneInfo
                                 {
                                     description = "desc",
                                     name = "name",
                                     owner = "owner",
                                     parcels = new List<Vector2Int> { new (5, 7) },
                                     type = MinimapMetadata.TileType.Plaza,
                                     previewImageUrl = "url",
                                     isPOI = false,
                                 }
                             }));

            ParcelCoordinates gotoCoords = new ParcelCoordinates(5, 7);
            coords.Set(gotoCoords);

            Received.InOrder(() =>
            {
                view.ShowLoading();
                view.HideLoading();
            });
        }

        [Test]
        public void SetFallbackSceneInfoWhenSceneInformationRequestFails()
        {
            LogAssert.Expect(LogType.Exception, new Regex("Failed"));

            minimapApiBridge.GetScenesInformationAroundParcel(Arg.Is<Vector2Int>(v => v.x == 8 && v.y == 19),
                                 2,
                                 Arg.Any<CancellationToken>())
                            .Returns(UniTask.FromException<MinimapMetadata.MinimapSceneInfo[]>(new Exception("Failed")));

            ParcelCoordinates gotoCoords = new ParcelCoordinates(8, 19);
            coords.Set(gotoCoords);

            view.Received(1).SetPanelInfo(gotoCoords, null);

            Received.InOrder(() =>
            {
                view.ShowLoading();
                view.HideLoading();
            });
        }

        [Test]
        public void CloseWhenViewRequestsIt()
        {
            var called = false;
            dataStore.HUDs.goToPanelConfirmed.OnChange += (confirmed, _) => called = confirmed == false;

            view.OnClosePressed += Raise.Event<Action>();

            view.Received(1).SetVisible(false);
            Assert.IsTrue(called);
        }

        [Test]
        public void TeleportWhenViewRequestsIt()
        {
            var called = false;
            dataStore.HUDs.goToPanelConfirmed.OnChange += (confirmed, _) => called = confirmed;

            view.OnTeleportPressed += Raise.Event<Action<ParcelCoordinates>>(new ParcelCoordinates(6, 87));

            teleportController.Received(1).Teleport(6, 87);
            Assert.IsTrue(called);
        }
    }
}
