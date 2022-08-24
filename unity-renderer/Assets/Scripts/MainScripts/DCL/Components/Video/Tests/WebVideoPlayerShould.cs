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
        private DCLVideoPlayer dclVideoPlayer;
        private IVideoPluginWrapper plugin;

        [SetUp]
        public void Setup()
        {
            plugin = Substitute.For<IVideoPluginWrapper>();
            dclVideoPlayer = new DCLVideoPlayer(ID, "url", true, plugin);
            plugin.GetError(ID).Returns(ERROR_MESSAGE);
        }

        [Test]
        public void NotPlayVideoIfVideoIsNotReady()
        {
            plugin.GetState(ID).Returns(VideoState.LOADING);

            dclVideoPlayer.Play();

            plugin.DidNotReceive().Play(ID, -1);
        }

        [Test]
        public void PlayVideoIfSetBeforeIsReadyWhenIsReady()
        {
            plugin.GetState(ID).Returns(VideoState.LOADING);

            dclVideoPlayer.Play();

            plugin.DidNotReceive().Play(ID, -1);

            plugin.GetState(ID).Returns(VideoState.READY);

            dclVideoPlayer.Update();

            plugin.Received(1).Play(ID, -1);
        }

        [Test]
        public void PlayVideo()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            dclVideoPlayer.Update();
            dclVideoPlayer.Play();

            plugin.Received(1).Play(ID, -1);
        }

        [Test]
        public void PlayVideoAtPausedPosition()
        {
            plugin.GetState(ID).Returns(VideoState.READY);

            dclVideoPlayer.Update();
            dclVideoPlayer.SetTime(100);
            dclVideoPlayer.Play();

            plugin.Received(1).Play(ID, 100);
        }

        [Test]
        public void ReturnProperValueForGetTime()
        {
            plugin.GetTime(ID).Returns(7);

            var time = dclVideoPlayer.GetTime();

            plugin.Received(1).GetTime(ID);
            Assert.AreEqual(7, time);
        }

        [Test]
        public void PauseVideo()
        {
            dclVideoPlayer.Play();
            dclVideoPlayer.Pause();

            plugin.Received(1).Pause(ID);
            Assert.IsFalse(dclVideoPlayer.playing);
        }

        [Test]
        public void ResumeVideoAtCorrectTimeAfterPaused()
        {
            plugin.GetTime(ID).Returns(80);
            plugin.GetState(ID).Returns(VideoState.READY);

            dclVideoPlayer.Update();

            dclVideoPlayer.Play();
            dclVideoPlayer.Pause();
            dclVideoPlayer.Play();

            plugin.Received(1).Play(ID, -1);
            plugin.Received(1).Play(ID, 80);
        }

        [Test]
        public void SetVolume()
        {
            dclVideoPlayer.SetVolume(77);

            plugin.Received(1).SetVolume(ID, 77);
        }

        [Test]
        public void SetVideoLoop()
        {
            dclVideoPlayer.SetLoop(true);
            dclVideoPlayer.SetLoop(false);

            plugin.Received(1).SetLoop(ID, true);
            plugin.Received(1).SetLoop(ID, false);
        }

        [Test]
        public void SetPlaybackRate()
        {
            dclVideoPlayer.SetPlaybackRate(55);

            plugin.Received(1).SetPlaybackRate(ID, 55);
        }

        [Test]
        public void ReturnVideoDurationWhenGetDurationIsCalled()
        {
            plugin.GetDuration(ID).Returns(200);

            var duration = dclVideoPlayer.GetDuration();

            Assert.AreEqual(200, duration);
        }

        [Test]
        public void DisposeVideo()
        {
            dclVideoPlayer.Dispose();

            plugin.Received(1).Remove(ID);
        }

        [Test]
        public void SetErrorStateWhenUpdateIsCalled()
        {
            plugin.GetState(ID).Returns(VideoState.ERROR);

            dclVideoPlayer.Update();

            plugin.Received(1).GetError(ID);
            Assert.IsTrue(dclVideoPlayer.isError);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, ERROR_MESSAGE);
        }

        [Test]
        public void DontDoAnythingWhenErrorStateIsSet()
        {
            plugin.GetState(ID).Returns(VideoState.ERROR);

            dclVideoPlayer.Update();
            dclVideoPlayer.Play();
            dclVideoPlayer.Pause();
            dclVideoPlayer.SetVolume(10);
            dclVideoPlayer.SetTime(99);
            dclVideoPlayer.SetLoop(false);
            dclVideoPlayer.SetPlaybackRate(8);
            dclVideoPlayer.GetTime();
            dclVideoPlayer.GetDuration();

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

            VideoState state = dclVideoPlayer.GetState();

            Assert.AreEqual(VideoState.READY, state);
        }

        [Test]
        public void ReturnValidNumberWhenVideoDurationIsNaN()
        {
            plugin.GetDuration(ID).Returns(float.NaN);

            var duration = dclVideoPlayer.GetDuration();

            Assert.AreEqual(-1, duration);
        }
    }
}