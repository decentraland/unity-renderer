using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.FPSDisplay
{
    public class FPSDisplay : MonoBehaviour
    {
        private const float REFRESH_SECONDS = 0.1f;

        [SerializeField] internal List<DebugValue> valuesToUpdate;
        [SerializeField] private Button closeButton;
        [SerializeField] private CopyToClipboardButton copySceneToClipboard;
        [SerializeField] private DebugSection memorySection;
        [SerializeField] private RectTransform container;
        [SerializeField] private PerformanceMetricsDataVariable performanceData;

        private Dictionary<DebugValueEnum, Func<string>> updateValueDictionary;
        private Vector3 containerStartAnchoredPosition;

        private List<IDebugMetricModule> debugMetricsModules;
        private SceneDebugMetricModule sceneDebugMetricModule;

        private void Awake()
        {
            containerStartAnchoredPosition = container.anchoredPosition;

            sceneDebugMetricModule = new SceneDebugMetricModule();
            debugMetricsModules = new List<IDebugMetricModule>
            {
                new FPSDebugMetricModule(performanceData),
                new SkyboxDebugMetricModule(),
                new MemoryJSDebugMetricModule(),
                new MemoryDesktopDebugMetricModule(),
                new GeneralDebugMetricModule(),
                sceneDebugMetricModule
            };

            updateValueDictionary = new Dictionary<DebugValueEnum, Func<string>>();
            foreach (IDebugMetricModule debugMetricsModule in debugMetricsModules)
            {
                debugMetricsModule.SetUpModule(updateValueDictionary);
            }
        }

        private void Start()
        {
            closeButton.onClick.AddListener(() =>DataStore.i.debugConfig.isFPSPanelVisible.Set(false));
            copySceneToClipboard.SetFuncToCopy(() =>  sceneDebugMetricModule.GetSceneID());
        }

        private void OnEnable()
        {
            container.anchoredPosition = containerStartAnchoredPosition;
            foreach (IDebugMetricModule debugMetricsModule in debugMetricsModules)
            {
                debugMetricsModule.EnableModule();
            }

            StartCoroutine(UpdateLabelLoop());
        }

        private void OnDisable()
        {
            foreach (IDebugMetricModule debugMetricsModule in debugMetricsModules)
            {
                debugMetricsModule.DisableModule();
            }
            StopAllCoroutines();
        }

        private IEnumerator UpdateLabelLoop()
        {
            while (performanceData.Get().totalSeconds <= 0)
            {
                yield return null;
            }

            while (true)
            {
                foreach (IDebugMetricModule debugMetricsModule in debugMetricsModules)
                {
                    debugMetricsModule.UpdateModule();
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

        private void OnDestroy()
        {
            closeButton.onClick.RemoveAllListeners();
            foreach (IDebugMetricModule debugMetricsModule in debugMetricsModules)
            {
                debugMetricsModule.Dispose();
            }
        }

        public void AddValueToUpdate(DebugValue valueToUpdate)
        {
            valuesToUpdate.Add(valueToUpdate);
        }

    }
}
