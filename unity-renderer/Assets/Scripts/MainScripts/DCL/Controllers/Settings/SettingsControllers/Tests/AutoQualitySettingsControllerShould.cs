using System.Collections;
using DCL;
using DCL.SettingsData;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace Tests
{
    public class AutoQualitySettingsControllerShould : TestsBase
    {
        private AutoQualitySettingsController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = CreateTestGameObject("controllerHolder").AddComponent<AutoQualitySettingsController>();
            Settings.i.autoqualitySettings = ScriptableObject.CreateInstance<QualitySettingsData>();
            controller.qualitySettings.Set(new []
            {
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
            });
        }

        [Test]
        public void LowerTheQualityOnPerformanceDrop()
        {
            controller.currentQualityIndex = 2;
            controller.evaluator = Substitute.For<IAutoQualitySettingsEvaluator>();
            controller.evaluator.Evaluate(null).ReturnsForAnyArgs( -1);

            controller.EvaluateQuality();

            Assert.AreEqual(1, controller.currentQualityIndex);
        }

        [Test]
        public void MaintainTheQualityOnAcceptablePerformance()
        {
            controller.currentQualityIndex = 2;
            controller.evaluator = Substitute.For<IAutoQualitySettingsEvaluator>();
            controller.evaluator.Evaluate(null).ReturnsForAnyArgs( 0);

            controller.EvaluateQuality();

            Assert.AreEqual(2, controller.currentQualityIndex);
        }

        [Test]
        public void IncreaseTheQualityOnAcceptablePerformance()
        {
            controller.currentQualityIndex = 2;
            controller.evaluator = Substitute.For<IAutoQualitySettingsEvaluator>();
            controller.evaluator.Evaluate(null).ReturnsForAnyArgs( 1);

            controller.EvaluateQuality();

            Assert.AreEqual(3, controller.currentQualityIndex);
        }
    }
}