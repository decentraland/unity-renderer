using NUnit.Framework;
using UnityEngine;

namespace Tests.ValidationTests
{
    public class SettingAssetsValidationTests
    {
        [Test]
        public void ValidateAntiAliasingInQualitySettings() =>
            Assert.That(QualitySettings.antiAliasing, Is.EqualTo(0));
    }
}
