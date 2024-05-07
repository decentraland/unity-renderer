
/* Why is this commented?
 * Good question! To get ride of the nasty bug `too many instant steps` from unity we have to avoid having new asmdefs with test related stuff
 * This is the only way to have both the test (although commented),
 * because due to the dependencies with the test environment either we have an asmdef with the references or we have compilation errors.
 */

// using DCL.Helpers;
// using NSubstitute;
// using NUnit.Framework;
// using System;

// [Category("EditModeCI")]
// [Explicit(TestUtils.EXPLICIT_INSTANT_STEPS)]
// public class ToSPopupControllerShould
// {
//     private IToSPopupView mockView;
//     private BaseVariable<bool> tosPopupVisible;
//     private IToSPopupHandler mockHandler;
//     private ToSPopupController controller;
//
//     [SetUp]
//     public void Setup()
//     {
//         mockView = Substitute.For<IToSPopupView>();
//         tosPopupVisible = new BaseVariable<bool>();
//         mockHandler = Substitute.For<IToSPopupHandler>();
//         controller = new ToSPopupController(mockView, tosPopupVisible, mockHandler);
//     }
//
//     [Test]
//     public void Constructor_ShouldSubscribeToEvents()
//     {
//         mockView.Received().OnAccept += Arg.Any<Action>();
//         mockView.Received().OnCancel += Arg.Any<Action>();
//     }
//
//     [Test]
//     public void OnToSPopupVisible_ShouldShowView_WhenCurrentIsTrue()
//     {
//         controller.OnToSPopupVisible(true, false);
//         mockView.Received().Show();
//     }
//
//     [Test]
//     public void OnToSPopupVisible_ShouldHideView_WhenCurrentIsFalse()
//     {
//         controller.OnToSPopupVisible(false, true);
//         mockView.Received().Hide();
//     }
//
//     [Test]
//     public void HandleCancel_ShouldCallHandlerCancel()
//     {
//         controller.HandleCancel();
//         mockHandler.Received().Cancel();
//     }
//
//     [Test]
//     public void HandleAccept_ShouldCallHandlerAccept()
//     {
//         controller.HandleAccept();
//         mockHandler.Received().Accept();
//     }
//
//     [Test]
//     public void Dispose_ShouldUnsubscribeFromEventsAndDisposeView()
//     {
//         controller.Dispose();
//         mockView.Received().OnAccept -= Arg.Any<Action>();
//         mockView.Received().Dispose();
//     }
// }
