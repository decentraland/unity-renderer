using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.FPSDisplay;
using DCL.SettingsData;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace Tests
{
    public class AutoQualitySettingsComponentShould : TestsBase
    {
        private AutoQualitySettingsComponent component;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            component = CreateTestGameObject("controllerHolder").AddComponent<AutoQualitySettingsComponent>();
            Settings.i.autoqualitySettings = ScriptableObject.CreateInstance<QualitySettingsData>();
            component.qualitySettings.Set(new []
            {
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
            });
        }

        [Test]
        public void CreateCappedControllerProperly()
        {
            component.fpsCapped = false;
            component.OnQualitySettingsChanged(new QualitySettings { fpsCap = true });

            Assert.IsTrue(component.fpsCapped);
            Assert.IsInstanceOf<AutoQualityCappedFPSController>(component.controller);
        }

        [Test]
        public void CreateUncappedControllerProperly()
        {
            component.fpsCapped = true;
            component.OnQualitySettingsChanged(new QualitySettings { fpsCap = false });

            Assert.IsFalse(component.fpsCapped);
            Assert.IsInstanceOf<AutoQualityUncappedFPSController>(component.controller);
        }
    }

    public class AutoQualityCappedFPSControllerShould : TestsBase
    {
        private AutoQualityCappedFPSController controller;
        private QualitySettingsData qualities;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            qualities = ScriptableObject.CreateInstance<QualitySettingsData>();
            qualities.Set(new []
            {
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
            });
            controller = new AutoQualityCappedFPSController(30, 0, qualities);
        }

        [Test]
        public void StayIfNotEnoughData()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            int newQualityIndex;
            for (int i = 0; i < AutoQualityCappedFPSController.EVALUATIONS_SIZE; i++)
            {
                newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = 0 });
                Assert.AreEqual(initialIndex, newQualityIndex);
            }

            newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = 0 });
            Assert.AreNotEqual(initialIndex, newQualityIndex);
        }

        [Test]
        public void DecreaseIfBadPerformance()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            float belowAcceptableFPS = controller.targetFPS * Mathf.Lerp( 0,AutoQualityCappedFPSController.STAY_MARGIN, 0.5f);
            controller.fpsEvaluations.Clear();
            controller.fpsEvaluations.AddRange(Enumerable.Repeat(belowAcceptableFPS, AutoQualityCappedFPSController.EVALUATIONS_SIZE));

            int newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = belowAcceptableFPS });
            Assert.AreEqual(initialIndex - 1, newQualityIndex);
        }

        [Test]
        public void StayIfAcceptablePerformance()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            float acceptableFPS = controller.targetFPS * Mathf.Lerp( AutoQualityCappedFPSController.STAY_MARGIN, AutoQualityCappedFPSController.INCREASE_MARGIN, 0.5f);
            controller.fpsEvaluations.Clear();
            controller.fpsEvaluations.AddRange(Enumerable.Repeat(acceptableFPS, AutoQualityCappedFPSController.EVALUATIONS_SIZE));

            int newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = acceptableFPS });
            Assert.AreEqual(initialIndex, newQualityIndex);
        }

        [Test]
        public void IncreaseIfGreatPerformance()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            float greatFPS = controller.targetFPS * Mathf.Lerp( AutoQualityCappedFPSController.INCREASE_MARGIN, 1, 0.5f);
            controller.fpsEvaluations.Clear();
            controller.fpsEvaluations.AddRange(Enumerable.Repeat(greatFPS, AutoQualityCappedFPSController.EVALUATIONS_SIZE));

            int newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = greatFPS });
            Assert.AreEqual(initialIndex + 2, newQualityIndex);
        }
    }

     public class AutoQualityUncappedFPSControllerShould : TestsBase
    {
        private AutoQualityUncappedFPSController controller;
        private QualitySettingsData qualities;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            qualities = ScriptableObject.CreateInstance<QualitySettingsData>();
            qualities.Set(new []
            {
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
                Settings.i.qualitySettings,
            });
            controller = new AutoQualityUncappedFPSController(0, qualities);
        }

        [Test]
        public void StayIfNotEnoughData()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            int newQualityIndex;
            for (int i = 0; i < AutoQualityCappedFPSController.EVALUATIONS_SIZE; i++)
            {
                newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = 0 });
                Assert.AreEqual(initialIndex, newQualityIndex);
            }

            newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = 0 });
            Assert.AreNotEqual(initialIndex, newQualityIndex);
        }

        [Test]
        public void DecreaseIfBadPerformance()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            float belowAcceptableFPS = FPSEvaluation.WORSE;
            controller.fpsEvaluations.Clear();
            controller.fpsEvaluations.AddRange(Enumerable.Repeat(belowAcceptableFPS, AutoQualityCappedFPSController.EVALUATIONS_SIZE));

            int newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = belowAcceptableFPS });
            Assert.AreEqual(initialIndex - 1, newQualityIndex);
        }

        [Test]
        public void StayIfAcceptablePerformance()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            float acceptableFPS = FPSEvaluation.GOOD;
            controller.fpsEvaluations.Clear();
            controller.fpsEvaluations.AddRange(Enumerable.Repeat(acceptableFPS, AutoQualityCappedFPSController.EVALUATIONS_SIZE));

            int newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = acceptableFPS });
            Assert.AreEqual(initialIndex, newQualityIndex);
        }

        [Test]
        public void IncreaseIfGreatPerformance()
        {
            int initialIndex = qualities.Length / 2;
            controller.currentQualityIndex = initialIndex;

            float greatFPS = FPSEvaluation.GREAT;
            controller.fpsEvaluations.Clear();
            controller.fpsEvaluations.AddRange(Enumerable.Repeat(greatFPS, AutoQualityCappedFPSController.EVALUATIONS_SIZE));

            int newQualityIndex = controller.EvaluateQuality(new PerformanceMetricsData { fpsCount = greatFPS });
            Assert.AreEqual(initialIndex + 1, newQualityIndex);
        }
    }
}