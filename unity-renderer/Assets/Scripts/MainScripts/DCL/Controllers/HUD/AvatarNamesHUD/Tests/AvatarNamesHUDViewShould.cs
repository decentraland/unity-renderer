using AvatarNamesHUD;
using NUnit.Framework;
using UnityEngine;

namespace Tests.AvatarNamesHUD
{
    public class AvatarNamesHUDViewShould
    {
        private AvatarNamesHUDView hudView;

        [SetUp]
        public void SetUp()
        {
            hudView = Object.Instantiate(Resources.Load<GameObject>("AvatarNamesHUD")).GetComponent<AvatarNamesHUDView>();
            hudView.Initialize();
        }

        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual( AvatarNamesHUDView.INITIAL_RESERVE_SIZE, hudView.reserveTrackers.Count);
            Assert.AreEqual(0, hudView.trackers.Count);
        }

        [Test]
        public void TrackPlayer()
        {
            Player user0 = new Player { id = "user0", name = "user0", isTalking = false };
            hudView.TrackPlayer(user0);

            Assert.AreEqual(AvatarNamesHUDView.INITIAL_RESERVE_SIZE - 1, hudView.reserveTrackers.Count);
            Assert.AreEqual(1, hudView.trackers.Count);
            Assert.IsTrue(hudView.trackers.ContainsKey("user0"));

            AvatarNamesTracker tracker = hudView.trackers["user0"];
            Assert.AreEqual(user0, tracker.player);
            Assert.NotNull(tracker);

            Assert.True(tracker.background.gameObject.activeSelf);
            Assert.NotNull(tracker.background);
            Assert.NotNull(tracker.backgroundCanvasGroup);

            Assert.True(tracker.name.gameObject.activeSelf);
            Assert.NotNull(tracker.name);
            Assert.AreEqual(user0.name, tracker.name.text);

            Assert.False(tracker.voiceChatCanvasGroup.gameObject.activeSelf);
            Assert.NotNull(tracker.voiceChatCanvasGroup);
            Assert.NotNull(tracker.voiceChatAnimator);
        }

        [Test]
        public void UntrackPlayer()
        {
            Player user0 = new Player { id = "user0", name = "user0" };
            hudView.TrackPlayer(user0);
            AvatarNamesTracker tracker = hudView.trackers["user0"];

            hudView.UntrackPlayer("user0");
            Assert.AreEqual(AvatarNamesHUDView.INITIAL_RESERVE_SIZE , hudView.reserveTrackers.Count);
            Assert.AreEqual(0, hudView.trackers.Count);
            Assert.IsFalse(hudView.trackers.ContainsKey("user0"));

            Assert.False(tracker.background.gameObject.activeSelf);
            Assert.False(tracker.name.gameObject.activeSelf);
            Assert.False(tracker.voiceChatCanvasGroup.gameObject.activeSelf);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(hudView.gameObject); }
    }
}