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
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
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
                KernelConfig.i.OnChange -= OnKernelConfigChanged;
                StopChecking();
            }
            isActive = active;
        }

        private void StopChecking()
        {
            CoroutineStarter.Stop(updateRoutine);
            ShowNotification(false);
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            sceneId = current.debugConfig.sceneLimitsWarningSceneId;
            if (string.IsNullOrEmpty(sceneId))
            {
                StopChecking();
            }
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
            if (worldState == null)
                return;

            bool isLimitReached = false;

            if (!string.IsNullOrEmpty(sceneId))
            {
                worldState.loadedScenes.TryGetValue(sceneId, out IParcelScene scene);
                ISceneMetricsController metricsController = scene?.metricsController;
                SceneMetricsModel currentMetrics = metricsController?.GetModel();
                SceneMetricsModel limit = metricsController?.GetLimits();

                string warningMessage = null;
                isLimitReached = IsLimitReached(currentMetrics, limit, ref warningMessage);
                if (isLimitReached)
                {
                    limitReachedNotification.message = $"{NOTIFICATION_MESSAGE}: {warningMessage}";
                }
            }

            if (isShowingNotification != isLimitReached)
            {
                ShowNotification(isLimitReached);
            }
        }

        private static bool IsLimitReached(SceneMetricsModel currentMetrics, SceneMetricsModel limit, ref string message)
        {
            if (currentMetrics == null || limit == null)
                return false;

            if (currentMetrics.materials > limit.materials)
            {
                message = $"Materials ({currentMetrics.materials}/{limit.materials})";
                return true;
            }

            if (currentMetrics.triangles > limit.triangles)
            {
                message = $"Triangles ({currentMetrics.triangles}/{limit.triangles})";
                return true;
            }

            if (currentMetrics.meshes > limit.meshes)
            {
                message = $"Meshes ({currentMetrics.meshes}/{limit.meshes})";
                return true;
            }

            if (currentMetrics.entities > limit.entities)
            {
                message = $"Entities ({currentMetrics.entities}/{limit.entities})";
                return true;
            }

            if (currentMetrics.bodies > limit.bodies)
            {
                message = $"Bodies ({currentMetrics.bodies}/{limit.bodies})";
                return true;
            }

            if (currentMetrics.sceneHeight > limit.sceneHeight)
            {
                message = $"Height ({currentMetrics.sceneHeight}/{limit.sceneHeight})";
                return true;
            }

            return false;
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