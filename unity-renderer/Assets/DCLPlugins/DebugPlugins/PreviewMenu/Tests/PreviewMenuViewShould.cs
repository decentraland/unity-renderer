using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class PreviewMenuViewShould
    {
        [Test]
        public void VisibilityToggleCorrectly()
        {
            var resource = Resources.Load<PreviewMenuVisibilityToggleView>(PreviewMenuController.TOGGLEVISIBILITY_VIEW_RES_PATH);
            var toggleView = Object.Instantiate(resource);

            bool isEnabled = true;

            bool IsEnabled() => isEnabled;
            void ToggleEnable(bool enable) => isEnabled = enable;

            const string label = "temptation";
            toggleView.SetUp(label, IsEnabled, ToggleEnable);

            Assert.AreEqual(label, toggleView.textReference.text);

            // start toggled on
            Assert.AreEqual(toggleView.colorON, toggleView.textReference.color);
            Assert.AreEqual(toggleView.colorON, toggleView.imageReference.color);
            Assert.AreEqual(toggleView.imageON, toggleView.imageReference.sprite);

            // click to toggle off
            toggleView.buttonReference.onClick.Invoke();

            // should be off
            Assert.AreEqual(toggleView.colorOFF, toggleView.textReference.color);
            Assert.AreEqual(toggleView.colorOFF, toggleView.imageReference.color);
            Assert.AreEqual(toggleView.imageOFF, toggleView.imageReference.sprite);

            Object.Destroy(toggleView.gameObject);
        }

        [Test]
        public void PositionUpdatedCorrectly()
        {
            // setup environment
            var scene = Substitute.For<IParcelScene>();
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
            {
                basePosition = new Vector2Int(0, 0)
            });

            var worldState = Substitute.For<IWorldState>();
            worldState.GetCurrentSceneId().Returns("temptation");
            worldState.GetLoadedScenes().Returns(new Dictionary<string, IParcelScene>() { { "temptation", scene } });

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register<IWorldState>(() => worldState);

            Environment.Setup(serviceLocator);

            // setup view
            var resource = Resources.Load<PreviewMenuPositionView>(PreviewMenuController.POSITION_VIEW_RES_PATH);
            var positionView = Object.Instantiate(resource);

            // move player
            var firstPosition = new Vector3(10, 6, 3);
            CommonScriptableObjects.playerUnityPosition.Set(firstPosition);
            positionView.LateUpdate();

            Assert.AreEqual(PreviewMenuPositionView.FormatFloatValue(firstPosition.x), positionView.xValueInputField.text);
            Assert.AreEqual(PreviewMenuPositionView.FormatFloatValue(firstPosition.y), positionView.yValueInputField.text);
            Assert.AreEqual(PreviewMenuPositionView.FormatFloatValue(firstPosition.z), positionView.zValueInputField.text);

            // move player again
            var secondPosition = new Vector3(1, 7, 3);
            CommonScriptableObjects.playerUnityPosition.Set(secondPosition);
            positionView.LateUpdate();

            Assert.AreEqual(PreviewMenuPositionView.FormatFloatValue(secondPosition.x), positionView.xValueInputField.text);
            Assert.AreEqual(PreviewMenuPositionView.FormatFloatValue(secondPosition.y), positionView.yValueInputField.text);
            Assert.AreEqual(PreviewMenuPositionView.FormatFloatValue(secondPosition.z), positionView.zValueInputField.text);

            Object.Destroy(positionView.gameObject);
        }

        [Test]
        public void CopyPositionToClipboardOnClick()
        {
            var resource = Resources.Load<PreviewMenuPositionView>(PreviewMenuController.POSITION_VIEW_RES_PATH);
            var positionView = Object.Instantiate(resource);

            positionView.xValueInputField.text = "temp";
            positionView.yValueInputField.text = "ta";
            positionView.zValueInputField.text = "tion";

            var clipboard = Substitute.For<IClipboard>();
            var serviceLocator = new ServiceLocator();
            serviceLocator.Register<IClipboard>(() => clipboard);
            Environment.Setup(serviceLocator);

            positionView.buttonReference.onClick.Invoke();
            clipboard.Received(1).WriteText("temp,ta,tion");

            Object.Destroy(positionView.gameObject);
        }

        [Test]
        public void ButtonTogglesMenuCorrectly()
        {
            var resource = Resources.Load<PreviewMenuView>(PreviewMenuController.MENU_VIEW_RES_PATH);
            var menuView = Object.Instantiate(resource);

            bool initialVisibilityState = menuView.contentContainer.activeSelf;
            menuView.menuButton.onClick.Invoke();
            Assert.AreNotEqual(initialVisibilityState, menuView.contentContainer.activeSelf);

            menuView.menuButton.onClick.Invoke();
            Assert.AreEqual(initialVisibilityState, menuView.contentContainer.activeSelf);

            Object.Destroy(menuView.gameObject);
        }

        [Test]
        public void MenuAddsItemsCorrectly()
        {
            var resource = Resources.Load<PreviewMenuView>(PreviewMenuController.MENU_VIEW_RES_PATH);
            var menuView = Object.Instantiate(resource);

            Assert.AreEqual(0, menuView.menuList.childCount);

            var item = new GameObject();
            item.transform.localScale = new Vector3(100, 0, 3);

            menuView.AddMenuItem(item.transform);
            Assert.AreEqual(1, menuView.menuList.childCount);
            Assert.AreEqual(Vector3.one, item.transform.localScale);

            Object.Destroy(item);
            Object.Destroy(menuView.gameObject);
        }
    }
}