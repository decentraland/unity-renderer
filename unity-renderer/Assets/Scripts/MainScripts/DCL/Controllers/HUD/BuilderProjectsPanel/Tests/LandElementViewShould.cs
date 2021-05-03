using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public class LandElementViewShould
    {
        private LandElementView view;

        [SetUp]
        public void SetUp()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<LandElementView>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/BuilderProjectsPanel/Prefabs/LandElementView.prefab");
            view = Object.Instantiate(prefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(view.gameObject);
        }

        [Test]
        public void TriggerCallbacks()
        {
            bool triggered = false;

            void OnCallback(string value) { triggered = true; }
            void OnCallbackVector2(Vector2Int value) { triggered = true; }

            view.OnEditorPressed += OnCallbackVector2;
            view.OnSettingsPressed += OnCallback;
            view.OnOpenInDappPressed += OnCallback;
            view.OnJumpInPressed += OnCallbackVector2;

            view.buttonEditor.onClick.Invoke();
            Assert.IsTrue(triggered, "OnEditorPressed not triggered");

            triggered = false;
            view.buttonSettings.onClick.Invoke();
            Assert.IsTrue(triggered, "OnSettingsPressed not triggered");

            triggered = false;
            view.buttonJumpIn.onClick.Invoke();
            Assert.IsTrue(triggered, "OnJumpInPressed not triggered");
            
            triggered = false;
            view.buttonOpenInBuilderDapp.onClick.Invoke();
            Assert.IsTrue(triggered, "buttonOpenInBuilderDapp not triggered");
        }
        
        [Test]
        public void SetNameCorrectly()
        {
            const string name = "Temptation";
            view.SetName(name);
            Assert.AreEqual(name,view.landName.text);
        }
        
        [Test]
        public void SetSizeCorrectly()
        {
            view.SetSize(1);
            Assert.AreEqual(string.Format(LandElementView.SIZE_TEXT_FORMAT, 1),view.landSize.text);
            Assert.IsFalse(view.landSizeGO.activeSelf);
            
            view.SetSize(2);
            Assert.AreEqual(string.Format(LandElementView.SIZE_TEXT_FORMAT, 2),view.landSize.text);
            Assert.IsTrue(view.landSizeGO.activeSelf);
        }
        
        [Test]
        public void SetRoleCorrectly()
        {
            view.SetRole(true);
            Assert.IsTrue(view.roleOwner.activeSelf);
            Assert.IsFalse(view.roleOperator.activeSelf);
            
            view.SetRole(false);
            Assert.IsFalse(view.roleOwner.activeSelf);
            Assert.IsTrue(view.roleOperator.activeSelf);
        }
    }
}