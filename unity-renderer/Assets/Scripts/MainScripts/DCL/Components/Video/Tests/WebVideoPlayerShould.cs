using System.Collections;
using DCL.Components.Video.Plugin;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WebVideoPlayerShould
    {
        private const string ID = "id";
        private const string ERROR_MESSAGE = "Error Message";
        private WebVideoPlayer webVideoPlayer;
        private IVideoPluginWrapper plugin;

        [SetUp]
        public void Setup()
        {
            plugin = Substitute.For<IVideoPluginWrapper>();
            webVideoPlayer = new WebVideoPlayer(ID, "url", true, plugin);
            plugin.GetError(ID).Returns(ERROR_MESSAGE);
        }

        [Test]
        public void NotPlayVideoIfVideoIsNotReady()
        {
            plugin.GetState(ID).Returns(VideoState.LOADING);

            webVideoPlayer.Play();

            plugin.DidNotReceive().Play(ID, -1);
        }

        [Test]
        public void PlayVideoIfSetBeforeIsReadyWhenIsReady()
        {
            plugin.GetState(ID).Returns(VideoState.LOADING);

            webVideoPlayer.Play();

            plugin.DidNotReceive().Play(ID, -1);

            plugin.GetState(ID).Returns(VideoState.READY);

            webVideoPlayer.Update();

            plugin.Received(1).Play(ID, -1);
        }

        [Test]
        public void PlayVideo()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            webVideoPlayer.Update();
            webVideoPlayer.Play();

            plugin.Received(1).Play(ID, -1);
        }

        [Test]
        public void PlayVideoAtPausedPosition()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            webVideoPlayer.Update();
            webVideoPlayer.SetTime(100);
            webVideoPlayer.Play();

            plugin.Received(1).Play(ID, 100);
        }

        [Test]
        public void ReturnProperValueForGetTime()
        {
            plugin.GetTime(ID).Returns(7);

            var time = webVideoPlayer.GetTime();

            plugin.Received(1).GetTime(ID);
            Assert.AreEqual(7, time);
        }

        [Test]
        public void PauseVideo()
        {
            webVideoPlayer.Play();
            webVideoPlayer.Pause();

            plugin.Received(1).Pause(ID);
            Assert.IsFalse(webVideoPlayer.playing);
        }

        [Test]
        public void ResumeVideoAtCorrectTimeAfterPaused()
        {
            plugin.GetTime(ID).Returns(80);
            plugin.GetState(ID).Returns(VideoState.READY);

            webVideoPlayer.Update();

            webVideoPlayer.Play();
            webVideoPlayer.Pause();
            webVideoPlayer.Play();

            plugin.Received(1).Play(ID, -1);
            plugin.Received(1).Play(ID, 80);
        }

        [Test]
        public void SetVolume()
        {
            webVideoPlayer.SetVolume(77);

            plugin.Received(1).SetVolume(ID, 77);
        }

        [Test]
        public void SetVideoLoop()
        {
            webVideoPlayer.SetLoop(true);
            webVideoPlayer.SetLoop(false);

            plugin.Received(1).SetLoop(ID, true);
            plugin.Received(1).SetLoop(ID, false);
        }

        [Test]
        public void SetPlaybackRate()
        {
            webVideoPlayer.SetPlaybackRate(55);

            plugin.Received(1).SetPlaybackRate(ID, 55);
        }

        [Test]
        public void ReturnVideoDurationWhenGetDurationIsCalled()
        {
            plugin.GetDuration(ID).Returns(200);

            var duration = webVideoPlayer.GetDuration();

            Assert.AreEqual(200, duration);
        }

        [Test]
        public void DisposeVideo()
        {
            webVideoPlayer.Dispose();

            plugin.Received(1).Remove(ID);
        }

        [Test]
        public void SetErrorStateWhenUpdateIsCalled()
        {
            plugin.GetState(ID).Returns(VideoState.ERROR);

            webVideoPlayer.Update();

            plugin.Received(1).GetError(ID);
            Assert.IsTrue(webVideoPlayer.isError);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, ERROR_MESSAGE);
        }

        [Test]
        public void DontDoAnythingWhenErrorStateIsSet()
        {
            plugin.GetState(ID).Returns(VideoState.ERROR);

            webVideoPlayer.Update();
            webVideoPlayer.Play();
            webVideoPlayer.Pause();
            webVideoPlayer.SetVolume(10);
            webVideoPlayer.SetTime(99);
            webVideoPlayer.SetLoop(false);
            webVideoPlayer.SetPlaybackRate(8);
            webVideoPlayer.GetTime();
            webVideoPlayer.GetDuration();

            plugin.Received(0).Play(ID, Arg.Any<float>());
            plugin.Received(0).Pause(ID);
            plugin.Received(0).SetVolume(ID, Arg.Any<float>());
            plugin.Received(0).SetTime(ID, Arg.Any<float>());
            plugin.Received(0).SetLoop(ID, Arg.Any<bool>());
            plugin.Received(0).SetPlaybackRate(ID, Arg.Any<float>());
            plugin.Received(0).GetTime(ID);
            plugin.Received(0).GetDuration(ID);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, ERROR_MESSAGE);
        }

        [Test]
        public void ReturnVideoStateWhenGetStateIsCalled()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            VideoState state = webVideoPlayer.GetState();

            Assert.AreEqual(VideoState.READY, state);
        }

        [Test]
        public void ReturnValidNumberWhenVideoDurationIsNaN()
        {
            plugin.GetDuration(ID).Returns(float.NaN);

            var duration = webVideoPlayer.GetDuration();

            Assert.AreEqual(-1, duration);
        }
    }
}