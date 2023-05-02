using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Tests.ValidationTests
{
    [Category("EditModeCI")]
    public class SettingAssetsValidationTests
    {
        [Test]
        public void QualitySettingsAntiAliasingShouldBeZero()
        {
            string qualityAssetPath = Application.dataPath.Remove(Application.dataPath.Length - "Assets".Length)
                                      + "ProjectSettings/QualitySettings.asset";

            string antiAliasingValue =
                File.ReadAllLines(qualityAssetPath)
                    .First(x => x.Contains("antiAliasing"))
                    .Split(':')
                     [1]
                    .Trim();

            Assert.That(antiAliasingValue, Is.EqualTo("0"));
        }

        [Test]
        public void SplashScreenShouldBeFalse()
        {
            string projectSettingsPath = Application.dataPath.Remove(Application.dataPath.Length - "Assets".Length)
                                         + "ProjectSettings/ProjectSettings.asset";

            string splashScreenEnabled =
                File.ReadAllLines(projectSettingsPath)
                    .First(x => x.Contains("ShowUnitySplashScreen"))
                    .Split(':')
                     [1]
                    .Trim();

            Assert.That(splashScreenEnabled, Is.EqualTo("0"));
        }
    }
}
