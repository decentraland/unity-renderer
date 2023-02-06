using System.Collections;
using DCL.Components.Video.Plugin;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

#if AV_PRO_PRESENT
public class AvProVideoShould : IntegrationTestSuite
{
    
    [UnityTest]
    [TestCase("MP3","https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3", ExpectedResult = null)]
    [TestCase("loop", "https://player.vimeo.com/external/691621058.m3u8?s=a2aa7b62cd0431537ed53cd699109e46d0de8575", ExpectedResult = null)]
    [TestCase ("HTTPS+HLS 1","https://player.vimeo.com/external/552481870.m3u8?s=c312c8533f97e808fccc92b0510b085c8122a875", ExpectedResult = null)]
    [TestCase("HTTPS+HLS 2", "https://player.vimeo.com/external/575854261.m3u8?s=d09797037b7f4f1013d337c04836d1e998ad9c80", ExpectedResult = null)]
    [TestCase("HTTP+MP4","http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4",ExpectedResult = null)]
    [TestCase("HTTP+WEBM","http://techslides.com/demos/sample-videos/small.webm", ExpectedResult = null)]
    [TestCase("HTTP+OGV","http://techslides.com/demos/sample-videos/small.ogv", ExpectedResult = null)]
    [TestCase("HTTPS+HLS","https://player.vimeo.com/external/691415562.m3u8?s=65096902279bbd8bb19bf9e2b9391c4c7e510402", ExpectedResult = null)]
    [TestCase("JPEG","https://ironapeclub.com/wp-content/uploads/2022/01/ironape-club-poster.jpg", ExpectedResult = null)]
    public IEnumerator AvProVideoTestCases(string id, string url)
    {
        if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
        {
            Assert.IsTrue(true, "AVProVideo not supported in Linux for video " + url);
        }
        else
        {
            VideoPluginWrapper_AVPro pluginWrapperAvPro = new VideoPluginWrapper_AVPro();
            pluginWrapperAvPro.Create(id,url,true);

            yield return new WaitUntil(() => pluginWrapperAvPro.GetState(id) == VideoState.READY);
           
            Texture2D movieTexture = pluginWrapperAvPro.PrepareTexture(id);
            pluginWrapperAvPro.TextureUpdate(id);
            pluginWrapperAvPro.Play(id,0);
            pluginWrapperAvPro.SetVolume(id, 1);

            Assert.IsNotNull(movieTexture);
            Assert.IsNull(pluginWrapperAvPro.GetError(id));
           
            pluginWrapperAvPro.Remove(id);
        }
    }

}
#endif
