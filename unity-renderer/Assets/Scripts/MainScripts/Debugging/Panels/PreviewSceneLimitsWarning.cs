using System.Collections;
using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    public class PreviewSceneLimitsWarning : MonoBehaviour
    {
        [SerializeField] private GameObject warningView;

        private string sceneId;
        private Coroutine updateRoutine;
        private IWorldState worldState;

        private void Awake()
        {
            worldState = Environment.i.world.state;
        }

        private void OnEnable()
        {
            sceneId = KernelConfig.i.Get().debugConfig.sceneLimitsWarningSceneId;
            KernelConfig.i.OnChange += OnKernelConfigChanged;
            updateRoutine = CoroutineStarter.Start(UpdateRoutine());
        }

        private void OnDisable()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            CoroutineStarter.Stop(updateRoutine);
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            sceneId = current.debugConfig.sceneLimitsWarningSceneId;
        }

        private IEnumerator UpdateRoutine()
        {
            while (true)
            {
                bool isLimitReached = false;

                if (!string.IsNullOrEmpty(sceneId))
                {
                    IParcelScene scene = worldState.loadedScenes[sceneId];
                    ISceneMetricsController metricsController = scene?.metricsController;
                    SceneMetricsModel currentMetrics = metricsController?.GetModel();
                    SceneMetricsModel limit = metricsController?.GetLimits();

                    isLimitReached = currentMetrics != null && limit != null
                                                            && (currentMetrics.bodies > limit.bodies
                                                                || currentMetrics.entities > limit.entities
                                                                || currentMetrics.materials > limit.materials
                                                                || currentMetrics.meshes > limit.meshes
                                                                || currentMetrics.triangles > limit.triangles
                                                                || currentMetrics.sceneHeight > limit.sceneHeight
                                                            );
                }

                if (warningView.activeSelf && !isLimitReached)
                {
                    warningView.SetActive(false);
                }
                else if (!warningView.activeSelf && isLimitReached)
                {
                    warningView.SetActive(true);
                }
                yield return WaitForSecondsCache.Get(0.2f);
            }
        }
    }
}