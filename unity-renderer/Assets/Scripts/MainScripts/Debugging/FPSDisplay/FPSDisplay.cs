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
        [SerializeField] private Vector2 labelPadding = new Vector2(12, 12);

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private RectTransform background;
        [SerializeField] private PerformanceMetricsDataVariable performanceData;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private int lastPlayerCount;
        private CurrentRealmVariable currentRealm => DataStore.i.realm.playerRealm;

        private Promise<KernelConfigModel> kernelConfigPromise;
        private string currentNetwork = string.Empty;
        private string currentRealmValue = string.Empty;

        private Vector2 minSize = Vector2.zero;

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

        private void SetupKernelConfig()
        {
            kernelConfigPromise = KernelConfig.i.EnsureConfigInitialized();
            kernelConfigPromise.Catch(Debug.Log);
            kernelConfigPromise.Then(OnKernelConfigChanged);
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }

        private void UpdateRealm(CurrentRealmModel current, CurrentRealmModel previous)
        {
            if (current == null) return;
            currentRealmValue = current.serverName ?? string.Empty;
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { OnKernelConfigChanged(current); }
        private void OnKernelConfigChanged(KernelConfigModel kernelConfig) { currentNetwork = kernelConfig.network ?? string.Empty; }

        private void OnOtherPlayersModified(string playerName, Player player) { lastPlayerCount = otherPlayers.Count(); }

        private void OnDisable()
        {
            otherPlayers.OnAdded -= OnOtherPlayersModified;
            otherPlayers.OnRemoved -= OnOtherPlayersModified;
            currentRealm.OnChange -= UpdateRealm;
            kernelConfigPromise.Dispose();
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

            StopAllCoroutines();
        }

        private IEnumerator UpdateLabelLoop()
        {
            while (true)
            {
                UpdateLabel();
                UpdateBackgroundSize();

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

            Color fpsColor = FPSColoring.GetDisplayColor(fps);

            targetText += GetColor(GetHexColor(Color.white));
            targetText += GetTitle("Skybox");
            targetText += GetLine($"Config: {DataStore.i.skyboxConfig.configToLoad.Get()}");
            targetText += GetLine($"Duration: {DataStore.i.skyboxConfig.lifecycleDuration.Get()}");
            targetText += GetLine($"Game Time: {DataStore.i.skyboxConfig.currentVirtualTime.Get().ToString(TWO_DECIMALS)}");
            targetText += GetLine($"UTC Time: {DataStore.i.worldTimer.GetCurrentTime().ToString()}");
            targetText += GetEmptyLine();

            targetText += GetTitle("General");
            targetText += GetLine($"Network: {currentNetwork.ToUpper()}");
            targetText += GetLine($"Realm: {currentRealmValue.ToUpper()}");
            targetText += GetLine($"Nearby players: {lastPlayerCount}");
            targetText += GetEmptyLine();

            targetText += GetColor(GetHexColor(fpsColor));
            targetText += GetTitle("FPS");
            targetText += GetLine($"Hiccups in the last 1000 frames: {performanceData.Get().hiccupCount}");
            targetText += GetLine($"Hiccup loss: {(100.0f * performanceData.Get().hiccupSum / performanceData.Get().totalSeconds).ToString(TWO_DECIMALS)}% ({performanceData.Get().hiccupSum.ToString(TWO_DECIMALS)} in {performanceData.Get().totalSeconds.ToString(TWO_DECIMALS)} secs)");
            targetText += GetLine($"Bad Frames Percentile: {((performanceData.Get().hiccupCount) / 10.0f).ToString(NO_DECIMALS)}%");
            targetText += GetLine($"Current {msFormatted} ms (fps: {fpsFormatted})");

            if (label.text != targetText)
            {
                label.text = targetText;
            }
        }
        private void UpdateBackgroundSize()
        {
            var labelSize = label.GetPreferredValues();
            var tempMinSize = labelSize + labelPadding;
            tempMinSize.x = Mathf.Max(tempMinSize.x, minSize.x);
            tempMinSize.y = Mathf.Max(tempMinSize.y, minSize.y);

            if (tempMinSize != minSize)
            {
                background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tempMinSize.x);
                background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tempMinSize.y);

                minSize = tempMinSize;
            }
        }
        private static string GetHexColor(Color color) { return $"#{ColorUtility.ToHtmlStringRGB(color)}"; }

        private string GetColor(string color) { return $"<color={color}>"; }

        private string GetTitle(string text) { return $"<voffset=0.1em><u><size=110%>{text}<size=100%></u></voffset><br>"; }

        private string GetLine(string text) { return $"{text}<br>"; }

        private string GetEmptyLine() { return "<br>"; }
    }
}