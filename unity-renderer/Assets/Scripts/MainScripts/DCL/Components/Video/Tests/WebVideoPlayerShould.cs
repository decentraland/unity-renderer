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
        private VideoPlayer videoPlayer;
        private IVideoPluginWrapper plugin;

        [SetUp]
        public void Setup()
        {
            plugin = Substitute.For<IVideoPluginWrapper>();
            videoPlayer = new VideoPlayer(ID, "url", true, plugin);
            plugin.GetError(ID).Returns(ERROR_MESSAGE);
        }

        [Test]
        public void NotPlayVideoIfVideoIsNotReady()
        {
            plugin.GetState(ID).Returns(VideoState.LOADING);

            videoPlayer.Play();

            plugin.DidNotReceive().Play(ID, -1);
        }

        [Test]
        public void PlayVideoIfSetBeforeIsReadyWhenIsReady()
        {
            plugin.GetState(ID).Returns(VideoState.LOADING);

            videoPlayer.Play();

            plugin.DidNotReceive().Play(ID, -1);

            plugin.GetState(ID).Returns(VideoState.READY);

            videoPlayer.Update();

            plugin.Received(1).Play(ID, -1);
        }

        [Test]
        public void PlayVideo()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            videoPlayer.Update();
            videoPlayer.Play();

            plugin.Received(1).Play(ID, -1);
        }

        [Test]
        public void PlayVideoAtPausedPosition()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            videoPlayer.Update();
            videoPlayer.SetTime(100);
            videoPlayer.Play();

            plugin.Received(1).Play(ID, 100);
        }

        [Test]
        public void ReturnProperValueForGetTime()
        {
            plugin.GetTime(ID).Returns(7);

            var time = videoPlayer.GetTime();

            plugin.Received(1).GetTime(ID);
            Assert.AreEqual(7, time);
        }

        [Test]
        public void PauseVideo()
        {
            videoPlayer.Play();
            videoPlayer.Pause();

            plugin.Received(1).Pause(ID);
            Assert.IsFalse(videoPlayer.playing);
        }

        [Test]
        public void ResumeVideoAtCorrectTimeAfterPaused()
        {
            plugin.GetTime(ID).Returns(80);
            plugin.GetState(ID).Returns(VideoState.READY);

            videoPlayer.Update();

            videoPlayer.Play();
            videoPlayer.Pause();
            videoPlayer.Play();

            plugin.Received(1).Play(ID, -1);
            plugin.Received(1).Play(ID, 80);
        }

        [Test]
        public void SetVolume()
        {
            videoPlayer.SetVolume(77);

            plugin.Received(1).SetVolume(ID, 77);
        }

        [Test]
        public void SetVideoLoop()
        {
            videoPlayer.SetLoop(true);
            videoPlayer.SetLoop(false);

            plugin.Received(1).SetLoop(ID, true);
            plugin.Received(1).SetLoop(ID, false);
        }

        [Test]
        public void SetPlaybackRate()
        {
            videoPlayer.SetPlaybackRate(55);

            plugin.Received(1).SetPlaybackRate(ID, 55);
        }

        [Test]
        public void ReturnVideoDurationWhenGetDurationIsCalled()
        {
            plugin.GetDuration(ID).Returns(200);

            var duration = videoPlayer.GetDuration();

            Assert.AreEqual(200, duration);
        }

        [Test]
        public void DisposeVideo()
        {
            videoPlayer.Dispose();

            plugin.Received(1).Remove(ID);
        }

        [Test]
        public void SetErrorStateWhenUpdateIsCalled()
        {
            plugin.GetState(ID).Returns(VideoState.ERROR);

            videoPlayer.Update();

            plugin.Received(1).GetError(ID);
            Assert.IsTrue(videoPlayer.isError);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, ERROR_MESSAGE);
        }

        [Test]
        public void DontDoAnythingWhenErrorStateIsSet()
        {
            plugin.GetState(ID).Returns(VideoState.ERROR);

            videoPlayer.Update();
            videoPlayer.Play();
            videoPlayer.Pause();
            videoPlayer.SetVolume(10);
            videoPlayer.SetTime(99);
            videoPlayer.SetLoop(false);
            videoPlayer.SetPlaybackRate(8);
            videoPlayer.GetTime();
            videoPlayer.GetDuration();

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

            VideoState state = videoPlayer.GetState();

            Assert.AreEqual(VideoState.READY, state);
        }

        [Test]
        public void ReturnValidNumberWhenVideoDurationIsNaN()
        {
            plugin.GetDuration(ID).Returns(float.NaN);

            var duration = videoPlayer.GetDuration();

            Assert.AreEqual(-1, duration);
        }
    }
}