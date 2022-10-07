using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
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

        [SerializeField] private PerformanceMetricsDataVariable performanceData;
        [SerializeField] internal List<DebugValue> valuesToUpdate;
        [SerializeField] private Button closeButton;
        [SerializeField] private CopyToClipboardButton copySceneToClipboard;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private int lastPlayerCount;
        private CurrentRealmVariable currentRealm => DataStore.i.realm.playerRealm;

        private Promise<KernelConfigModel> kernelConfigPromise;
        private string currentNetwork = string.Empty;
        private string currentRealmValue = string.Empty;

        private Vector2 minSize = Vector2.zero;
        private Dictionary<DebugValueEnum, Func<string>> updateValueDictionary;
        private float fps;
        private string fpsColor;
        private SceneMetricsModel metrics;
        private SceneMetricsModel limits;
        private IParcelScene activeScene;
        private int totalMessagesCurrent;
        private int totalMessagesGlobal;

        private void Start()
        {
            closeButton.onClick.AddListener(() =>DataStore.i.debugConfig.isFPSPanelVisible.Set(false));
            copySceneToClipboard.SetFuncToCopy(() =>  activeScene.sceneData.id);
        }

        private void OnEnable()
        {
            lastPlayerCount = otherPlayers.Count();
            otherPlayers.OnAdded += OnOtherPlayersModified;
            otherPlayers.OnRemoved += OnOtherPlayersModified;

            SetupKernelConfig();
            SetupRealm();

            SetupDictionary();
            
            ProfilingEvents.OnMessageWillQueue += OnMessageWillQueue;
            ProfilingEvents.OnMessageWillDequeue += OnMessageWillDequeue;

            StartCoroutine(UpdateLabelLoop());
        }
        
        private void OnDisable()
        {
            otherPlayers.OnAdded -= OnOtherPlayersModified;
            otherPlayers.OnRemoved -= OnOtherPlayersModified;
            currentRealm.OnChange -= UpdateRealm;
            kernelConfigPromise.Dispose();
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            
            ProfilingEvents.OnMessageWillQueue += OnMessageWillQueue;
            ProfilingEvents.OnMessageWillDequeue += OnMessageWillDequeue;

            StopAllCoroutines();
        }

        private void SetupDictionary()
        {
            if (updateValueDictionary != null)
                return;
            
            updateValueDictionary = new Dictionary<DebugValueEnum, Func<string>>();
            
            updateValueDictionary.Add(DebugValueEnum.General_Network, () => currentNetwork.ToUpper());
            updateValueDictionary.Add(DebugValueEnum.General_Realm, () => currentRealmValue.ToUpper());
            updateValueDictionary.Add(DebugValueEnum.General_NearbyPlayers, () => lastPlayerCount.ToString());
            
            updateValueDictionary.Add(DebugValueEnum.FPS, GetFPSCount);
            updateValueDictionary.Add(DebugValueEnum.FPS_HiccupsInTheLast1000, () => $"{fpsColor}{performanceData.Get().hiccupCount.ToString()}</color>");
            updateValueDictionary.Add(DebugValueEnum.FPS_HiccupsLoss, GetHiccupsLoss);
            updateValueDictionary.Add(DebugValueEnum.FPS_BadFramesPercentiles, () => $"{fpsColor}{((performanceData.Get().hiccupCount) / 10.0f).ToString(NO_DECIMALS)}%</color>");
            
            updateValueDictionary.Add(DebugValueEnum.Scene_Name, GetSceneID);
            updateValueDictionary.Add(DebugValueEnum.Scene_ProcessedMessages, () =>  $"{(float)totalMessagesCurrent / totalMessagesGlobal * 100}%");
            updateValueDictionary.Add(DebugValueEnum.Scene_PendingOnQueue, () =>  $"{totalMessagesGlobal - totalMessagesCurrent}");
            updateValueDictionary.Add(DebugValueEnum.Scene_Poly, () => GetSceneMetric(metrics.triangles, limits.triangles));
            updateValueDictionary.Add(DebugValueEnum.Scene_Textures, () => GetSceneMetric(metrics.textures,limits.textures));
            updateValueDictionary.Add(DebugValueEnum.Scene_Materials, () => GetSceneMetric(metrics.materials,limits.materials));
            updateValueDictionary.Add(DebugValueEnum.Scene_Entities, () =>GetSceneMetric(metrics.entities,limits.entities));
            updateValueDictionary.Add(DebugValueEnum.Scene_Meshes, () => GetSceneMetric(metrics.meshes, limits.meshes));
            updateValueDictionary.Add(DebugValueEnum.Scene_Bodies, () => GetSceneMetric(metrics.bodies,limits.bodies));
            updateValueDictionary.Add(DebugValueEnum.Scene_Components, () => (activeScene.componentsManagerLegacy.GetSceneSharedComponentsDictionary().Count + activeScene.componentsManagerLegacy.GetComponentsCount()).ToString());
            
            updateValueDictionary.Add(DebugValueEnum.Memory_Used_JS_Heap_Size, () =>  $"{DataStore.i.debugConfig.usedJSHeapSize.Get().ToString(TWO_DECIMALS)} M");
            updateValueDictionary.Add(DebugValueEnum.Memory_Limit_JS_Heap_Size, () => $"{DataStore.i.debugConfig.jsHeapSizeLimit.Get().ToString(TWO_DECIMALS)} M");
            updateValueDictionary.Add(DebugValueEnum.Memory_Total_JS_Heap_Size, () => $"{DataStore.i.debugConfig.totalJSHeapSize.Get().ToString(TWO_DECIMALS)} M");

        }
        
        private string GetFPSCount()
        {
            float dt = Time.unscaledDeltaTime;
            string fpsFormatted = fps.ToString("##");
            string msFormatted = (dt * 1000).ToString("##");
            return $"<b>FPS</b> {fpsColor}{fpsFormatted}</color> {msFormatted} ms";
        }

        private string GetSceneMetric(int value, int limit)
        {
            return $"{GetColor(GetHexColor(FPSColoring.GetPercentageColoring(value, limit)))}current: {value} max: {limit}</color>";
        }
        
        private string GetSceneID()
        {
            string activeSceneID = activeScene.sceneData.id;
            if (activeSceneID.Length >= 11)
            {
                return $"{activeSceneID.Substring(0, 5)}...{activeSceneID.Substring(activeSceneID.Length - 5, 5)}";
            }
            else
            {
                return activeSceneID;
            }
                
        }

        private string GetHiccupsLoss()
        {
            return $"{fpsColor}{(100.0f * performanceData.Get().hiccupSum / performanceData.Get().totalSeconds).ToString(TWO_DECIMALS)}% {performanceData.Get().hiccupSum.ToString(TWO_DECIMALS)} in {performanceData.Get().totalSeconds.ToString(TWO_DECIMALS)} secs</color>";
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

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            OnKernelConfigChanged(current);
        }

        private void OnKernelConfigChanged(KernelConfigModel kernelConfig)
        {
            currentNetwork = kernelConfig.network ?? string.Empty;
        }

        private void OnOtherPlayersModified(string playerName, Player player)
        {
            lastPlayerCount = otherPlayers.Count();
        }

        private IEnumerator UpdateLabelLoop()
        {
            while (performanceData.Get().totalSeconds <= 0)
            {
                yield return null;
            }
            
            while (true)
            {
                fps = performanceData.Get().fpsCount;
                fpsColor = GetColor(GetHexColor(FPSColoring.GetDisplayColor(fps)));
                activeScene = GetActiveScene();
                WebInterface.UpdateMemoryUsage();
                if (activeScene != null && activeScene.metricsCounter != null)
                {
                    metrics = activeScene.metricsCounter.currentCount;
                    limits = activeScene.metricsCounter.maxCount;
                }
                for (var i = 0; i < valuesToUpdate.Count; i++)
                {
                    if (valuesToUpdate[i].isActiveAndEnabled)
                    {
                        valuesToUpdate[i].SetValue(updateValueDictionary[valuesToUpdate[i].debugValueEnum].Invoke());
                    }
                }
                yield return new WaitForSeconds(REFRESH_SECONDS);
            }
        }
       
        private IParcelScene GetActiveScene()
        {
            IWorldState worldState = DCL.Environment.i.world.state;
            string debugSceneId = KernelConfig.i.Get().debugConfig.sceneDebugPanelTargetSceneId;

            if (!string.IsNullOrEmpty(debugSceneId))
            {
                if (worldState.TryGetScene(debugSceneId, out IParcelScene scene))
                    return scene;
            }

            var currentPos = DataStore.i.player.playerGridPosition.Get();
            worldState.TryGetScene(worldState.GetSceneIdByCoords(currentPos), out IParcelScene resultScene);

            return resultScene;
        }
        
        private void OnMessageWillDequeue(string obj)
        {
            totalMessagesCurrent = Math.Min(totalMessagesCurrent + 1, totalMessagesGlobal);
        }

        private void OnMessageWillQueue(string obj)
        {
            totalMessagesGlobal++;
        }
        
        private static string GetHexColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        private string GetColor(string color)
        {
            return $"<color={color}>";
        }
        
        private void OnDestroy()
        {
            closeButton.onClick.RemoveAllListeners();
        }

        public void AddValueToUpdate(DebugValue valueToUpdate)
        {
            valuesToUpdate.Add(valueToUpdate);
        }



    }
}