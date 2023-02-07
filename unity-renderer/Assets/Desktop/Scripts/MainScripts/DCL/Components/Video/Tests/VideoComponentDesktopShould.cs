using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using DCL.Controllers;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class VideoComponentDesktopShould : IntegrationTestSuite
{
    private ParcelScene scene;

    /* Remote videos to test locally:
      "https://vimeo.com/671880962",
      "https://player.vimeo.com/external/552481870.m3u8?s=c312c8533f97e808fccc92b0510b085c8122a875",
      "https://theuniverse.club/live/genesisplaza/index.m3u8",
      "https://theuniverse.club/live/consensys/index.m3u8",
      "https://www.youtube.com/watch?v=LiE1VgWdcQM",
      "https://dcl.cubeprohost.com/live/dcl/index.m3u8?sign=4101743660-b5a7da280765e8a17c4d7209b6782872",
      "https://206-189-115-14.nip.io/live/dclgoethe/index.m3u8",
      "https://peer-lb.decentraland.org/content/contents/QmWPLo8CroLCVC57ja25RKixbXCsNM7SoTsWWnx3TPydkA",
      "https://peer-lb.decentraland.org/content/contents/QmTvAo7fTpFRgS1nY4VMxWbzzv4nEHsqs5paQ8euYCKdFi",
      "https://peer-lb.decentraland.org/content/contents/Qmf5XAr9gBs3nYEmrmuqQNZK7zpSTfSNtvk2ZmqHq341B8",
      "https://ipfs.io/ipfs/QmXkGhWzbAd4kN6xpdi793JNd7ABG9A4eL9ANxNrmDsEba?filename=2021-10-22%2015-12-55.mp4",
      "https://carls1987.cafe24.com/video/rhythmical-nft-club.mp4",
      "http://carls1987.cafe24.com/video/carls1.mp4",
      "https://5caf24a595d94.streamlock.net:1937/8094/8094/playlist.m3u8",
      "https://5dcc6a54d90e8c5dc4345c16-s-4.ssai.zype.com/5dcc6a54d90e8c5dc4345c16-s-4/manifest.m3u8"
     */

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        DCLVideoTexture.videoPluginWrapperBuilder = () => new VideoPluginWrapper_Native();
        serviceLocator.Register<ISceneController>(() => new SceneController());
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
        CommonScriptableObjects.rendererState.Set(true);
    }

    [UnityTest]
    [Category("Explicit")]
    [Explicit]
    public IEnumerator BasicVideo()
    {
        DCLVideoClip clip =
            TestUtils.SharedComponentCreate<DCLVideoClip, DCLVideoClip.Model>(scene, CLASS_ID.VIDEO_CLIP, new DCLVideoClip.Model() { url = "https://player.vimeo.com/external/552481870.m3u8?s=c312c8533f97e808fccc92b0510b085c8122a875" });

        yield return clip.routine;
        DCLVideoTexture videoTexture = TestUtils.SharedComponentCreate<DCLVideoTexture, DCLVideoTexture.Model>(scene, CLASS_ID.VIDEO_TEXTURE, new DCLVideoTexture.Model() { videoClipId = clip.id, playing = true });

        Assert.IsNotNull(videoTexture, "VideoTexture not loaded");
        yield return videoTexture.routine;
    }
}
