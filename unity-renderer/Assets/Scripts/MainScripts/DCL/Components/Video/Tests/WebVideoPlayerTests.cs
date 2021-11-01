using DCL.Components.Video.Plugin;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class WebVideoPlayerShould
    {
        private const string ID = "id";
        private const string ERROR_MESSAGE = "Error Message";
        private WebVideoPlayer webVideoPlayer;
        private IWebVideoPlayerPlugin plugin;

        [SetUp]
        public void Setup()
        {
            plugin = Substitute.For<IWebVideoPlayerPlugin>();
            webVideoPlayer = new WebVideoPlayer(ID, "url", true, plugin);
            plugin.GetError(ID).Returns(ERROR_MESSAGE);
        }
        
        [Test]
        public void VideoPlays()
        {
            webVideoPlayer.Play();
            
            plugin.Received(1).Play(ID, -1);
        }
        
        [Test]
        public void VideoPlaysAtPausedPosition()
        {
            webVideoPlayer.SetTime(100);
            
            webVideoPlayer.Play();
            
            plugin.Received(1).Play(ID, 100);
        }

        [Test]
        public void VideoGetTimeReturnsNativeTime()
        {
            plugin.GetTime(ID).Returns(7);
            
            var time = webVideoPlayer.GetTime();
            
            plugin.Received(1).GetTime(ID);
            Assert.AreEqual(7, time);
        }

        [Test]
        public void VideoIsPausedCorrectly()
        {
            webVideoPlayer.Play();
            webVideoPlayer.Pause();
            
            plugin.Received(1).Pause(ID);
            Assert.IsTrue(webVideoPlayer.IsPaused());
        }

        [Test]
        public void VideoIsResumedAtPausedTime()
        {
            plugin.GetTime(ID).Returns(80);
            
            webVideoPlayer.Play();
            webVideoPlayer.Pause();
            webVideoPlayer.Play();
            
            plugin.Received(1).Play(ID, -1);
            plugin.Received(1).Play(ID, 80);
        }

        [Test]
        public void VideoVolumeIsSet()
        {
            webVideoPlayer.SetVolume(77);
            
            plugin.Received(1).SetVolume(ID, 77);
        }

        [Test]
        public void VideoLoopIsSet()
        {
            webVideoPlayer.SetLoop(true);
            webVideoPlayer.SetLoop(false);
            
            plugin.Received(1).SetLoop(ID, true);
            plugin.Received(1).SetLoop(ID, false);
        }

        [Test]
        public void VideoPlaybackRateIsSet()
        {
            webVideoPlayer.SetPlaybackRate(55);
            
            plugin.Received(1).SetPlaybackRate(ID, 55);
        }

        [Test]
        public void ReturnsVideoDuration()
        {
            plugin.GetDuration(ID).Returns(200);

            var duration = webVideoPlayer.GetDuration();
            
            Assert.AreEqual(200, duration);
        }

        [Test]
        public void VideoIsDisposed()
        {
            webVideoPlayer.Dispose();
            
            plugin.Received(1).Remove(ID);
        }

        [Test]
        public void VideoErrorStateOnUpdate()
        {
            plugin.GetState(ID).Returns((int)VideoState.ERROR);
            
            webVideoPlayer.UpdateWebVideoTexture();

            plugin.Received(1).GetError(ID);
            Assert.IsTrue(webVideoPlayer.isError);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, ERROR_MESSAGE);
        }

        [Test]
        public void OnVideoErrorPlayerWontDoAnything()
        {
            plugin.GetState(ID).Returns((int)VideoState.ERROR);
            
            webVideoPlayer.UpdateWebVideoTexture();
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
        public void OnVideoReadyCreateTextureOnce()
        {
            Texture texture = new Texture2D(0, 0);
            int timesTextureWasReady = 0;
            plugin.GetWidth(ID).Returns(32);
            plugin.GetHeight(ID).Returns(64);
            plugin.GetState(ID).Returns((int)VideoState.READY);
            webVideoPlayer.OnTextureReady += t =>
            {
                timesTextureWasReady++;
                texture = t;
            };
            
            webVideoPlayer.UpdateWebVideoTexture();
            webVideoPlayer.UpdateWebVideoTexture();

            Assert.IsTrue(timesTextureWasReady == 1);
            Assert.AreEqual(32, texture.width);
            Assert.AreEqual(64, texture.height);
        }

        [Test]
        public void OnVideoPlayingResizeTextureAndUpdate()
        {
            Texture texture = new Texture2D(0, 0);
            plugin.GetWidth(ID).Returns(5);
            plugin.GetHeight(ID).Returns(5);
            plugin.GetState(ID).Returns((int)VideoState.READY);
            
            webVideoPlayer.OnTextureReady += t =>
            {
                texture = t;
            };
            webVideoPlayer.Play();
            webVideoPlayer.visible = true;
            webVideoPlayer.UpdateWebVideoTexture();
            
            plugin.GetWidth(ID).Returns(64);
            plugin.GetHeight(ID).Returns(64);
            plugin.GetState(ID).Returns((int)VideoState.PLAYING);
            
            webVideoPlayer.UpdateWebVideoTexture();
            plugin.Received(1).TextureUpdate(ID, texture.GetNativeTexturePtr(), Arg.Any<bool>() );
        }

        [Test]
        public void VideoStateIsReturned()
        {
            plugin.GetState(ID).Returns(3);

            VideoState state = webVideoPlayer.GetState();
            
            Assert.AreEqual(VideoState.READY, state);
        }
        
        [Test]
        public void WhenVideoDurationIsNaNReturnAValidNumber()
        {
            plugin.GetDuration(ID).Returns(float.NaN);

            var duration = webVideoPlayer.GetDuration();
            
            Assert.AreEqual(-1, duration);
        }
    }
}