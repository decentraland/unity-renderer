using System.Collections;
using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    public class PreviewSceneLimitsWarning : MonoBehaviour
    {
        private const string NOTIFICATION_GROUP = "SceneLimitationReached";
        private const string NOTIFICATION_MESSAGE = "Scene limitations reached";
        private const int NOTIFICATION_TYPE = 8;
        private readonly string NOTIFICATION = $"{{\"type\":{NOTIFICATION_TYPE},\"message\":{NOTIFICATION_MESSAGE},\"groupID\":{NOTIFICATION_GROUP}}}";

        private string sceneId;
        private Coroutine updateRoutine;
        private IWorldState worldState;

        private readonly Notification.Model limitReachedNotification = new Notification.Model()
        {
            type = NotificationFactory.Type.WARNING,
            groupID = "limitReachedNotification",
            message = "Scene limitations reached"
        };

        public PreviewSceneLimitsWarning(IWorldState worldState)
        {
            this.worldState = worldState;
        }

        private void Awake()
        {
            Notification.Model ll = new Notification.Model()
            {
                type = NotificationFactory.Type.WARNING,
                groupID = "limitReachedNotification",
                message = "Scene limitations reached"
            };

            string value = JsonUtility.ToJson(ll);
            Debug.Log(value);

            string N = "{{\"type\":{0},\"message\":{1},\"groupID\":{2}}}";
            var m = JsonUtility.FromJson<Notification.Model>(string.Format(N, NOTIFICATION_TYPE, NOTIFICATION_MESSAGE, NOTIFICATION_GROUP));
            Debug.Log(m);
        }

        // private void OnEnable()
        // {
        //     sceneId = KernelConfig.i.Get().debugConfig.sceneLimitsWarningSceneId;
        //     KernelConfig.i.OnChange += OnKernelConfigChanged;
        //     updateRoutine = CoroutineStarter.Start(UpdateRoutine());
        // }
        //
        // private void OnDisable()
        // {
        //     KernelConfig.i.OnChange -= OnKernelConfigChanged;
        //     CoroutineStarter.Stop(updateRoutine);
        // }

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

        private bool IsLimitReached(SceneMetricsModel currentMetrics, SceneMetricsModel limit)
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