// using DCL.Helpers;
// using UnityEngine;
// using NUnit.Framework;
// using UnityEditor;

// [Category("EditModeCI")]
// [Explicit(TestUtils.EXPLICIT_INSTANT_STEPS)]
// public class ToSPopupViewShould
// {
//     private ToSPopupView view;
//
//     [SetUp]
//     public void SetUp()
//     {
//         view = Object.Instantiate(AssetDatabase
//                          .LoadAssetAtPath<GameObject>("Assets/Scripts/MainScripts/DCL/Controllers/HUD/ToSPopupHUD/ToSPopupHUD.prefab")
//                       )
//                      .GetComponentInChildren<ToSPopupView>();
//         view.Initialize();
//     }
//
//     [TearDown]
//     public void TearDown()
//     {
//         Object.DestroyImmediate(view.gameObject);
//     }
//
//     [Test]
//     public void TestAcceptButtonPress()
//     {
//         bool wasAcceptButtonPressed = false;
//         view.OnAccept += () => wasAcceptButtonPressed = true;
//
//         view.agreeButton.onClick.Invoke();
//
//         Assert.IsTrue(wasAcceptButtonPressed);
//     }
//
//     [Test]
//     public void TestCancelButtonPress()
//     {
//         bool wasCancelButtonPressed = false;
//         view.OnCancel += () => wasCancelButtonPressed = true;
//
//         view.cancelButton.onClick.Invoke();
//
//         Assert.IsTrue(wasCancelButtonPressed);
//     }
//
//     [Test]
//     public void TestShow()
//     {
//         view.Show();
//
//         Assert.IsTrue(view.gameObject.activeSelf);
//     }
// }
