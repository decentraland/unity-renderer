using System.Collections;
using DCL.Controllers;
using DCL.NotificationModel;
using UnityEngine;

namespace DCL
{
    public class PreviewSceneLimitsWarning
    {
        private const string NOTIFICATION_GROUP = "SceneLimitationReached";
        private const string NOTIFICATION_MESSAGE = "Scene limitations reached";

        private string sceneId;
        private Coroutine updateRoutine;
        private IWorldState worldState;
        private bool isActive = false;

        private static readonly Model limitReachedNotification = new Model()
        {
            type = Type.WARNING,
            groupID = NOTIFICATION_GROUP,
            message = NOTIFICATION_MESSAGE
        };

        public PreviewSceneLimitsWarning(IWorldState worldState)
        {
            this.worldState = worldState;
        }

        public void SetActive(bool active)
        {
            if (active && !isActive)
            {
                sceneId = KernelConfig.i.Get().debugConfig.sceneLimitsWarningSceneId;
                KernelConfig.i.OnChange += OnKernelConfigChanged;
                updateRoutine = CoroutineStarter.Start(UpdateRoutine());
            }
            if (!active && isActive)
            {
                KernelConfig.i.OnChange -= OnKernelConfigChanged;
                CoroutineStarter.Stop(updateRoutine);
            }
            isActive = active;
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
                    worldState.loadedScenes.TryGetValue(sceneId, out IParcelScene scene);
                    ISceneMetricsController metricsController = scene?.metricsController;
                    SceneMetricsModel currentMetrics = metricsController?.GetModel();
                    SceneMetricsModel limit = metricsController?.GetLimits();

                    isLimitReached = IsLimitReached(currentMetrics, limit);
                }

                ShowNotification(isLimitReached);
                yield return WaitForSecondsCache.Get(0.2f);
            }
        }

        private static bool IsLimitReached(SceneMetricsModel currentMetrics, SceneMetricsModel limit)
        {
            return currentMetrics != null && limit != null
                                          && (currentMetrics.bodies > limit.bodies
                                              || currentMetrics.entities > limit.entities
                                              || currentMetrics.materials > limit.materials
                                              || currentMetrics.meshes > limit.meshes
                                              || currentMetrics.triangles > limit.triangles
                                              || currentMetrics.sceneHeight > limit.sceneHeight
                                          );
        }

        private void ShowNotification(bool show)
        {
            if (show)
            {
                NotificationsController.i.ShowNotification(limitReachedNotification);
            }
            NotificationsController.i.DismissAllNotifications(limitReachedNotification.groupID);
        }
    }
}