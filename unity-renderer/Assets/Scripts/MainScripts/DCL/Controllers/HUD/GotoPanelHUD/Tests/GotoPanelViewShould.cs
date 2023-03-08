using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCL.GoToPanel
{
    public class GotoPanelViewShould
    {
        private GotoPanelHUDView view;

        [SetUp]
        public void SetUp()
        {
            view = GotoPanelHUDView.CreateView();
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityProperly(bool visible)
        {
            view.SetVisible(visible);
            Assert.AreEqual(visible, view.container.gameObject.activeSelf);
        }

        [TestCase(1, 2)]
        [TestCase(76, 24)]
        [TestCase(11, 48)]
        public void SetParcelCoordinatesProperly(int x, int y)
        {
            ParcelCoordinates coords = new ParcelCoordinates(x, y);

            view.SetPanelInfo(coords, new MinimapMetadata.MinimapSceneInfo
            {
                description = "desc",
                name = "name",
                owner = "owner",
                parcels = new List<Vector2Int> { new (x, y) },
                type = MinimapMetadata.TileType.Plaza,
                previewImageUrl = null,
                isPOI = false,
            });

            Assert.AreEqual("name", view.sceneTitleText.text);
            Assert.AreEqual("owner", view.sceneOwnerText.text);
            Assert.AreEqual($"{x},{y}", view.panelText.text);
            Assert.AreEqual(view.scenePreviewImage.texture, view.scenePreviewImage.texture);
        }

        [Test]
        public void SetPanelInfoWhenInvalidSceneInfo()
        {
            ParcelCoordinates coords = new ParcelCoordinates(74, 34);

            view.SetPanelInfo(coords, null);

            Assert.AreEqual("Untitled Scene", view.sceneTitleText.text);
            Assert.AreEqual("Unknown", view.sceneOwnerText.text);
            Assert.AreEqual("74,34", view.panelText.text);
            Assert.AreEqual(view.scenePreviewImage.texture, view.scenePreviewImage.texture);
        }

        [Test]
        public void ShowLoading()
        {
            view.ShowLoading();

            Assert.IsTrue(view.sceneMetadataLoadingContainer.activeSelf);
        }

        [Test]
        public void HideLoading()
        {
            view.HideLoading();

            Assert.IsFalse(view.sceneMetadataLoadingContainer.activeSelf);
        }

        [Test]
        public void TriggerTeleportEventWhenButtonIsClicked()
        {
            ParcelCoordinates triggeredCoordinates = null;
            view.OnTeleportPressed += coordinates => triggeredCoordinates = coordinates;

            view.SetPanelInfo(new ParcelCoordinates(8, 6), null);

            view.teleportButton.onClick.Invoke();

            Assert.IsTrue(triggeredCoordinates.x == 8 && triggeredCoordinates.y == 6);
        }

        [Test]
        public void TriggerCloseEventWhenButtonIsClicked()
        {
            var called = false;
            view.OnClosePressed += () => called = true;

            view.closeButton.onClick.Invoke();

            Assert.IsTrue(called);
        }

        [Test]
        public void TriggerCloseEventWhenCancelButtonIsClicked()
        {
            var called = false;
            view.OnClosePressed += () => called = true;

            view.cancelButton.onClick.Invoke();

            Assert.IsTrue(called);
        }
    }
}
