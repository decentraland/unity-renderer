using System.Collections;
using DCL.Helpers;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Variables.RealmsInfo;

namespace DCL.FPSDisplay
{
    public class FPSDisplay : MonoBehaviour
    {
        private const float REFRESH_SECONDS = 0.1f;
        private const string NO_DECIMALS = "##";
        private const string TWO_DECIMALS = "##.00";
        [SerializeField] private Vector2 labelPadding = new Vector2(12, 12);

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private RectTransform background;
        [SerializeField] private PerformanceMetricsDataVariable performanceData;
        [SerializeField] private Button expandButton;
        [SerializeField] private Image expandButtonImage;
        [SerializeField] private Sprite expandSprite;
        [SerializeField] private Sprite contractSprite;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private int lastPlayerCount;
        private CurrentRealmVariable currentRealm => DataStore.i.realm.playerRealm;
        private DataStore_SkyboxConfig dataStoreSkyboxConfig => DataStore.i.skyboxConfig;

        private Promise<KernelConfigModel> kernelConfigPromise;
        private string currentNetwork = string.Empty;
        private string currentRealmValue = string.Empty;

        private Vector2 minSize = Vector2.zero;
        private bool isExpanded = false;
        

        private void OnEnable()
        {
            if (label == null || performanceData == null)
                return;

            Contract();

            lastPlayerCount = otherPlayers.Count();
            otherPlayers.OnAdded += OnOtherPlayersModified;
            otherPlayers.OnRemoved += OnOtherPlayersModified;

            SetupKernelConfig();
            SetupRealm();

            StartCoroutine(UpdateLabelLoop());

            expandButton.onClick.AddListener(OnExpand);
        }
        private void OnExpand()
        {
            if (isExpanded)
            {
                Contract();
            }
            else
            {
                Expand();
            }

            minSize = Vector2.zero;
        }

        private void Contract()
        {
            isExpanded = false;
            expandButtonImage.sprite = expandSprite;
        }

        private void Expand()
        {
            isExpanded = true;
            expandButtonImage.sprite = contractSprite;
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
            if (current == null)
                return;

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

            Color fpsColor = FPSColoring.GetDisplayColor(fps);

            if (isExpanded)
            {
                targetText += DrawSkyboxData();
                targetText += GetEmptyLine();

                targetText += DrawGeneralData();
                targetText += GetEmptyLine();
                
                targetText += DrawPerformanceData();
                targetText += GetEmptyLine();
            }

            targetText += GetColor(GetHexColor(fpsColor));
            targetText += GetTitle("FPS");
            targetText += GetLine($"Hiccups in the last 1000 frames: {performanceData.Get().hiccupCount}");

            targetText += GetLine(
                $"Hiccup loss: {(100.0f * performanceData.Get().hiccupSum / performanceData.Get().totalSeconds).ToString(TWO_DECIMALS)}% ({performanceData.Get().hiccupSum.ToString(TWO_DECIMALS)} in {performanceData.Get().totalSeconds.ToString(TWO_DECIMALS)} secs)");

            targetText +=
                GetLine(
                    $"Bad Frames Percentile: {((performanceData.Get().hiccupCount) / 10.0f).ToString(NO_DECIMALS)}%");

            targetText += GetLine($"Current {msFormatted} ms (fps: {fpsFormatted})");

            if (label.text != targetText)
            {
                label.text = targetText;
            }
        }
        private string DrawPerformanceData()
        {
            var result = GetTitle("GLTF");
            (int loading, int failed, int cancelled, int loaded) gltfStats = PerformanceAnalytics.GLTFTracker.GetData();
            (int loading, int failed, int cancelled, int loaded) abStats = PerformanceAnalytics.ABTracker.GetData();

            result += GetLine($"Loaded: {gltfStats.loaded} Loading: {gltfStats.loading} Failed: {gltfStats.failed} Cancelled: {gltfStats.cancelled}");
            result += GetEmptyLine();
            result += GetTitle("AB");
            result += GetLine($"Loaded: {abStats.loaded} Loading: {abStats.loading} Failed: {abStats.failed} Cancelled: {abStats.cancelled}");
            result += GetEmptyLine();
            result += GetTitle("Textures");
            result += GetLine($"GLTF: {PerformanceAnalytics.GLTFTextureTracker.Get()}, AB: {PerformanceAnalytics.ABTextureTracker.Get()}, Prom: {PerformanceAnalytics.PromiseTextureTracker.Get()}");
            return result;
        }
        private string DrawGeneralData()
        {
            var result = GetTitle("General");
            result += GetLine($"Network: {currentNetwork.ToUpper()}");
            result += GetLine($"Realm: {currentRealmValue.ToUpper()}");
            result += GetLine($"Nearby players: {lastPlayerCount}");

            return result;
        }
        private string DrawSkyboxData()
        {
            var result = GetColor(GetHexColor(Color.white));
            result += GetTitle("Skybox");
            result += GetLine($"Config: {dataStoreSkyboxConfig.configToLoad.Get()}");
            result += GetLine($"Duration: {dataStoreSkyboxConfig.lifecycleDuration.Get()}");
            result += GetLine($"Game Time: {dataStoreSkyboxConfig.currentVirtualTime.Get().ToString(TWO_DECIMALS)}");
            result += GetLine($"UTC Time: {DataStore.i.worldTimer.GetCurrentTime().ToString()}");

            return result;
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