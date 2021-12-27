using System.Collections;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCL.FPSDisplay
{

    public class FPSDisplay : MonoBehaviour
    {
        private const float REFRESH_SECONDS = 0.1f;

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private PerformanceMetricsDataVariable performanceData;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private int lastPlayerCount = 0;
        private CurrentRealmVariable currentRealm => DataStore.i.realm.playerRealm;

        private Promise<KernelConfigModel> kernelConfigPromise;
        private string currentNetwork;
        private string currentRealmValue;

        private void OnEnable()
        {
            if (label == null || performanceData == null)
                return;

            lastPlayerCount = otherPlayers.Count();
            otherPlayers.OnAdded += OnOtherPlayersModified;
            otherPlayers.OnRemoved += OnOtherPlayersModified;

            SetupKernelConfig();
            SetupRealm();
            StartCoroutine(UpdateLabelLoop());
        }
        private void SetupRealm()
        {
            currentRealm.OnChange += UpdateRealm;
            UpdateRealm(currentRealm.Get(), null);
        }
        private void UpdateRealm(CurrentRealmModel current, CurrentRealmModel previous)
        {
            if (current != null)
            {
                currentRealmValue = current.serverName;
            }
        }

        private void SetupKernelConfig()
        {
            kernelConfigPromise = KernelConfig.i.EnsureConfigInitialized();
            kernelConfigPromise.Catch(Debug.Log);
            kernelConfigPromise.Then(OnKernelConfigChanged);
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { OnKernelConfigChanged(current); }
        private void OnKernelConfigChanged(KernelConfigModel kernelConfig) { currentNetwork = kernelConfig.network; }

        private void OnOtherPlayersModified(string playerName, Player player) { lastPlayerCount = otherPlayers.Count(); }

        private void OnDisable()
        {
            otherPlayers.OnAdded -= OnOtherPlayersModified;
            otherPlayers.OnRemoved -= OnOtherPlayersModified;
            StopAllCoroutines();
        }

        private IEnumerator UpdateLabelLoop()
        {
            while (true)
            {
                UpdateLabel();

                yield return new WaitForSeconds(REFRESH_SECONDS);
            }
        }

        private void UpdateLabel()
        {
            float dt = Time.unscaledDeltaTime;

            float fps = performanceData.Get().fpsCount;

            string fpsFormatted = fps.ToString("##");
            string msFormatted = (dt * 1000).ToString("##");

            string targetText = string.Empty;

            string NO_DECIMALS = "##";
            string TWO_DECIMALS = "##.00";

            string fpsColor = GetHexColor(FPSColoring.GetDisplayColor(fps));

            targetText += SetColor(GetHexColor(Color.gray));
            
            targetText += AddTitle("Skybox");
            targetText += AddLine($"Config: {DataStore.i.skyboxConfig.configToLoad.Get()}");
            targetText += AddLine($"Duration: {DataStore.i.skyboxConfig.lifecycleDuration.Get()}");
            targetText += AddLine($"Game Time: {DataStore.i.skyboxConfig.currentVirtualTime.Get()}");
            targetText += AddLine($"UTC Time: {DataStore.i.worldTimer.GetCurrentTime().ToString()}");
            targetText += AddEmptyLine();
            
            targetText += AddTitle("General");
            targetText += AddLine($"Network: {currentNetwork}");
            targetText += AddLine($"Realm: {currentRealmValue}");
            targetText += AddLine($"Nearby players: {lastPlayerCount}");
            targetText += AddEmptyLine();
            
            targetText += AddTitle("Fps");
            targetText += AddColorLine($"Hiccups in the last 1000 frames: {performanceData.Get().hiccupCount}", fpsColor);
            targetText += AddColorLine($"Hiccup loss: {(100.0f * performanceData.Get().hiccupSum / performanceData.Get().totalSeconds).ToString(TWO_DECIMALS)}% ({performanceData.Get().hiccupSum.ToString(TWO_DECIMALS)} in {performanceData.Get().totalSeconds.ToString(TWO_DECIMALS)} secs)", fpsColor);
            targetText += AddColorLine($"Bad Frames Percentile: {((performanceData.Get().hiccupCount) / 10.0f).ToString(NO_DECIMALS)}%", fpsColor);
            targetText += AddColorLine($"Current {msFormatted} ms (fps: {fpsFormatted})", fpsColor);

            if (label.text != targetText)
            {
                label.text = targetText;
            }

        }
        private static string GetHexColor(Color color) { return $"#{ColorUtility.ToHtmlStringRGB(color)}"; }

        private string SetColor(string color) { return $"<color={color}>"; }

        private string AddTitle(string text) { return $"<size=110%>{text}<size=100%><br>"; }

        private string AddLine(string text) { return $"{text}<br>"; }

        private string AddColorLine(string text, string hex) { return $"<color={hex}>{text}</color><br>"; }

        private string AddEmptyLine() { return "<br>"; }
    }
}