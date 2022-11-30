using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Tests.ValidationTests
{
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
    }
}
