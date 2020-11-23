using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using DCL.Components;
using DCL.Models;

namespace Builder
{
    public class DCLBuilderWebInterface
    {
        static bool LOG_MESSAGES = false;

        [System.Serializable]
        private class EntityLoadingPayload
        {
            public string type;
            public string entityId;
        };

        [System.Serializable]
        private class OnEntityLoadingEvent : DCL.Interface.WebInterface.UUIDEvent<EntityLoadingPayload>
        {
        };

        [System.Serializable]
        private class EntitiesOutOfBoundariesEventPayload
        {
            public string[] entities;
        };

        [System.Serializable]
        public class BuilderSceneStartEvent
        {
            public string sceneId;
            public string eventType = "builderSceneStart";
        };

        [System.Serializable]
        private class ReportCameraTargetPosition
        {
            public Vector3 cameraTarget;
            public string id;
        };

        [System.Serializable]
        private class GizmosEventPayload
        {
            [System.Serializable]
            public class TransformPayload
            {
                public string entityId = string.Empty;
                public Vector3 position = Vector3.zero;
                public Quaternion rotation = Quaternion.identity;
                public Vector3 scale = Vector3.one;
            }
            public string[] entities = null;
            public TransformPayload[] transforms = null;
            public string gizmoType = DCLGizmos.Gizmo.NONE;
            public string type = string.Empty;
        };

        private static OnEntityLoadingEvent onGetLoadingEntity = new OnEntityLoadingEvent();
        private static EntitiesOutOfBoundariesEventPayload outOfBoundariesEventPayload = new EntitiesOutOfBoundariesEventPayload();
        private static ReportCameraTargetPosition onReportCameraTarget = new ReportCameraTargetPosition();
        private static GizmosEventPayload onGizmoEventPayload = new GizmosEventPayload();

        public void SendEntityStartLoad(DecentralandEntity entity)
        {
            onGetLoadingEntity.uuid = entity.entityId;
            onGetLoadingEntity.payload.entityId = entity.entityId;
            onGetLoadingEntity.payload.type = "onEntityLoading";
            if (LOG_MESSAGES) Debug.Log($"SEND: OnEntityLoadingEvent {entity.entityId}");
            WebInterface.SendSceneEvent(entity.scene.sceneData.id, "uuidEvent", onGetLoadingEntity);
        }

        public void SendEntityFinishLoad(DecentralandEntity entity)
        {
            onGetLoadingEntity.uuid = entity.entityId;
            onGetLoadingEntity.payload.entityId = entity.entityId;
            onGetLoadingEntity.payload.type = "onEntityFinishLoading";
            if (LOG_MESSAGES) Debug.Log($"SEND: onEntityFinishLoading {entity.entityId}");
            WebInterface.SendSceneEvent(entity.scene.sceneData.id, "uuidEvent", onGetLoadingEntity);
        }

        public void SendEntitiesOutOfBoundaries(string[] entitiesId, string sceneId)
        {
            outOfBoundariesEventPayload.entities = entitiesId;
            if (LOG_MESSAGES) Debug.Log($"SEND: entitiesOutOfBoundaries {outOfBoundariesEventPayload.entities.Length}");
            WebInterface.SendSceneEvent<EntitiesOutOfBoundariesEventPayload>(sceneId, "entitiesOutOfBoundaries", outOfBoundariesEventPayload);
        }

        public void SendBuilderSceneStart(string sceneId)
        {
            if (LOG_MESSAGES) Debug.Log($"SEND: BuilderSceneStartEvent {sceneId}");
            WebInterface.SendMessage("SceneEvent", new BuilderSceneStartEvent() { sceneId = sceneId });
        }

        public void SendCameraTargetPosition(Vector3 targetPosition, string promiseId)
        {
            onReportCameraTarget.cameraTarget = targetPosition;
            onReportCameraTarget.id = promiseId;
            if (LOG_MESSAGES) Debug.Log($"SEND: ReportBuilderCameraTarget {targetPosition}");
            WebInterface.SendMessage("ReportBuilderCameraTarget", onReportCameraTarget);
        }

        public void SendEntitySelected(EditableEntity entity, string gizmoType, string sceneId)
        {
            onGizmoEventPayload.type = "gizmoSelected";
            onGizmoEventPayload.entities = entity ? new string[] { entity.rootEntity.entityId } : null;
            onGizmoEventPayload.gizmoType = gizmoType != null ? gizmoType : DCLGizmos.Gizmo.NONE;
            onGizmoEventPayload.transforms = null;

            if (LOG_MESSAGES) Debug.Log($"SEND: NotifyGizmosSelectedEvent {JsonUtility.ToJson(onGizmoEventPayload)}");
            WebInterface.SendSceneEvent(sceneId, "gizmoEvent", onGizmoEventPayload);
        }

        public void SendEntitiesTransform(List<EditableEntity> entities, string gizmoType, string sceneId)
        {
            onGizmoEventPayload.type = "gizmoDragEnded";
            onGizmoEventPayload.entities = null;
            onGizmoEventPayload.gizmoType = gizmoType != null ? gizmoType : DCLGizmos.Gizmo.NONE;
            onGizmoEventPayload.transforms = new GizmosEventPayload.TransformPayload[entities.Count];

            for (int i = 0; i < entities.Count; i++)
            {
                onGizmoEventPayload.transforms[i] = new GizmosEventPayload.TransformPayload()
                {
                    entityId = entities[i].rootEntity.entityId,
                    position = entities[i].transform.position,
                    rotation = entities[i].transform.rotation,
                    scale = entities[i].transform.lossyScale
                };
            }
            if (LOG_MESSAGES) Debug.Log($"SEND: NotifyGizmosTransformEvent {JsonUtility.ToJson(onGizmoEventPayload)}");
            WebInterface.SendSceneEvent(sceneId, "gizmoEvent", onGizmoEventPayload);
        }

    }
}