using System;
using System.Collections;
using DCL.Controllers;
using DCL.NotificationModel;
using UnityEngine;
using Type = DCL.NotificationModel.Type;

namespace DCL
{
    public class PreviewSceneLimitsWarning : IDisposable
    {
        private const string NOTIFICATION_GROUP = "SceneLimitationExceeded";
        private const string NOTIFICATION_MESSAGE = "Scene's limits exceeded";

        internal const float CHECK_INTERVAL = 0.2f;

        private string sceneId;
        private Coroutine updateRoutine;
        private bool isActive = false;

        internal bool isShowingNotification = false;

        private readonly IWorldState worldState;

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

        public void Dispose()
        {
            StopChecking();
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
                StopChecking();
            }
            isActive = active;
        }

        private void StopChecking()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            CoroutineStarter.Stop(updateRoutine);
            ShowNotification(false);
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            sceneId = current.debugConfig.sceneLimitsWarningSceneId;
        }

        private IEnumerator UpdateRoutine()
        {
            while (true)
            {
                HandleWarningNotification();
                yield return WaitForSecondsCache.Get(CHECK_INTERVAL);
            }
        }

        internal void HandleWarningNotification()
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

            if (isShowingNotification != isLimitReached)
            {
                ShowNotification(isLimitReached);
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
            isShowingNotification = show;

            var notificationsController = NotificationsController.i;

            if (notificationsController == null)
                return;

            if (show)
            {
                notificationsController.ShowNotification(limitReachedNotification);
                return;
            }
            notificationsController.DismissAllNotifications(limitReachedNotification.groupID);
        }
    }
}