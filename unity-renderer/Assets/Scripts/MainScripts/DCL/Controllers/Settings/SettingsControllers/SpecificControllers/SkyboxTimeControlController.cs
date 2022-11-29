using Cysharp.Threading.Tasks;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System.Threading;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Skybox Time", fileName = "SkyboxTime")]
    public class SkyboxTimeControlController : SliderSettingsControlController
    {
        private const int CHECK_DELAY = 100; // in milliseconds

        private CancellationTokenSource delayCancellation;

        public override void Initialize()
        {
            base.Initialize();
            UpdateSetting(currentGeneralSettings.skyboxTime);

            if (DataStore.i.skyboxConfig.mode.Get() == SkyboxMode.Dynamic)
            {
                Debug.Log(0);
                delayCancellation = new CancellationTokenSource();
                CheckVirtualTimeAsync(CHECK_DELAY, delayCancellation.Token).Forget();
            }

            DataStore.i.skyboxConfig.mode.OnChange += OnSkyboxModeChanged;
        }

        private void OnSkyboxModeChanged(SkyboxMode current, SkyboxMode previous)
        {
            if (current == previous) return;

            switch (current)
            {
                case SkyboxMode.Dynamic:
                    delayCancellation = new CancellationTokenSource();
                    CheckVirtualTimeAsync(CHECK_DELAY, delayCancellation.Token).Forget();
                    break;
                case SkyboxMode.HoursFixedByWorld:
                    delayCancellation.Cancel();
                    delayCancellation.Dispose();
                    RaiseSliderValueChanged(DataStore.i.skyboxConfig.fixedTime.Get());
                    UpdateSetting(DataStore.i.skyboxConfig.fixedTime.Get());
                    break;
                case SkyboxMode.HoursFixedByUser:
                    delayCancellation.Cancel();
                    delayCancellation.Dispose();
                    break;
                default: break;
            }
        }

        private async UniTask CheckVirtualTimeAsync(int delay, CancellationToken cancellationToken)
        {
            Debug.Log(1);

            while (!cancellationToken.IsCancellationRequested)
            {
                Debug.Log(2);
                Debug.Log(DataStore.i.skyboxConfig.currentVirtualTime.Get());

                RaiseSliderValueChanged(DataStore.i.skyboxConfig.fixedTime.Get());
                UpdateSetting(DataStore.i.skyboxConfig.currentVirtualTime.Get());
                await UniTask.Delay(delay, cancellationToken: cancellationToken);
            }
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.skyboxTime;

        public override void UpdateSetting(object newValue)
        {
            var valueAsFloat = (float)newValue;

            valueAsFloat = Mathf.Clamp(valueAsFloat, 0, 23.998f);

            currentGeneralSettings.skyboxTime = valueAsFloat;

            var hourSection = (int)valueAsFloat;
            var minuteSection = (int)((valueAsFloat - hourSection) * 60);

            RaiseOnIndicatorLabelChange($"{hourSection:00}:{minuteSection:00}");

            Debug.Log(3);
            Debug.Log(valueAsFloat);

            DataStore.i.skyboxConfig.fixedTime.Set(valueAsFloat);
        }
    }
}
