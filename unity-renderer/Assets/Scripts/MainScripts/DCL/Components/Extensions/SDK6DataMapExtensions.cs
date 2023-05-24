using DCL;
using DCL.Components;
using DCL.Models;
using Decentraland.Common;
using Decentraland.Sdk.Ecs6;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Ray = DCL.Models.Ray;
using Vector3 = UnityEngine.Vector3;

namespace MainScripts.DCL.Components
{
    public static class SDK6DataMapExtensions
    {
        public static Color AsUnityColor(this ECS6Color4 color4) =>
            new (color4.R, color4.G, color4.B, color4.HasA ? color4.A : 1f);

        public static Color AsUnityColor(this Color3 color3) =>
            new (color3.R, color3.G, color3.B);

        public static Vector3 AsUnityVector3(this Decentraland.Common.Vector3 vector3) =>
            new (vector3.X, vector3.Y, vector3.Z);

        public static Quaternion AsUnityQuaternion(this Decentraland.Common.Quaternion quaternion) =>
            new (quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

        public static UIValue FromProtobuf(UIValue defaultValue, UiValue uiValue) =>
            new ()
            {
                value = uiValue.HasValue ? uiValue.Value : defaultValue.value,
                type = uiValue.HasType ? (UIValue.Unit)uiValue.Type : defaultValue.type,
            };

        public static QueuedSceneMessage_Scene SceneMessageFromSdk6Message(EntityAction action, int sceneNumber) =>
            new ()
            {
                type = QueuedSceneMessage.Type.SCENE_MESSAGE,
                method = MapMessagingMethodType(action),
                sceneNumber = sceneNumber,
                payload = ExtractPayload(from: action, sceneNumber),
                tag = action.Tag,
            };

        private static object ExtractPayload(EntityAction from, int sceneNumber)
        {
            return from.Payload.PayloadCase switch
                   {
                       EntityActionPayload.PayloadOneofCase.InitMessagesFinished => new Protocol.SceneReady(),
                       EntityActionPayload.PayloadOneofCase.OpenExternalUrl => new Protocol.OpenExternalUrl { url = from.Payload.OpenExternalUrl.Url },
                       EntityActionPayload.PayloadOneofCase.OpenNftDialog => new Protocol.OpenNftDialog
                       {
                           contactAddress = from.Payload.OpenNftDialog.AssetContractAddress,
                           comment = from.Payload.OpenNftDialog.Comment,
                           tokenId = from.Payload.OpenNftDialog.TokenId,
                       },
                       EntityActionPayload.PayloadOneofCase.CreateEntity => new Protocol.CreateEntity { entityId = from.Payload.CreateEntity.Id },
                       EntityActionPayload.PayloadOneofCase.RemoveEntity => new Protocol.RemoveEntity { entityId = from.Payload.RemoveEntity.Id },
                       EntityActionPayload.PayloadOneofCase.AttachEntityComponent => new Protocol.SharedComponentAttach
                       {
                           entityId = from.Payload.AttachEntityComponent.EntityId,
                           id = from.Payload.AttachEntityComponent.Id,
                           name = from.Payload.AttachEntityComponent.Name,
                       },
                       EntityActionPayload.PayloadOneofCase.ComponentRemoved => new Protocol.EntityComponentDestroy
                       {
                           entityId = from.Payload.ComponentRemoved.EntityId,
                           name = from.Payload.ComponentRemoved.Name,
                       },
                       EntityActionPayload.PayloadOneofCase.SetEntityParent => new Protocol.SetEntityParent
                       {
                           entityId = from.Payload.SetEntityParent.EntityId,
                           parentId = from.Payload.SetEntityParent.ParentId,
                       },
                       EntityActionPayload.PayloadOneofCase.Query => new QueryMessage { payload = CreateRaycastPayload(from, sceneNumber) },
                       EntityActionPayload.PayloadOneofCase.ComponentCreated => new Protocol.SharedComponentCreate
                       {
                           id = from.Payload.ComponentCreated.Id,
                           classId = from.Payload.ComponentCreated.ClassId,
                           name = from.Payload.ComponentCreated.Name,
                       },
                       EntityActionPayload.PayloadOneofCase.ComponentDisposed => new Protocol.SharedComponentDispose { id = from.Payload.ComponentDisposed.Id },

                       EntityActionPayload.PayloadOneofCase.ComponentUpdated => from.Payload.ComponentUpdated,
                       EntityActionPayload.PayloadOneofCase.UpdateEntityComponent => from.Payload.UpdateEntityComponent,

                       EntityActionPayload.PayloadOneofCase.None => null,
                       _ => throw new SwitchExpressionException($"Unknown payload type for sdk6 protobuf message {from.Payload.PayloadCase}"),
                   };
        }

        private static RaycastQuery CreateRaycastPayload(EntityAction action, int sceneNumber)
        {
            RaycastType raycastType = action.Payload.Query.Payload.QueryType switch
                                      {
                                          "HitFirst" => RaycastType.HIT_FIRST,
                                          "HitAll" => RaycastType.HIT_ALL,
                                          "HitFirstAvatar" => RaycastType.HIT_FIRST_AVATAR,
                                          "HitAllAvatars" => RaycastType.HIT_ALL_AVATARS,
                                          _ => RaycastType.NONE,
                                      };

            var ray = new Ray
            {
                origin = action.Payload.Query.Payload.Ray.Origin.AsUnityVector3(),
                direction = action.Payload.Query.Payload.Ray.Direction.AsUnityVector3(),
                distance = action.Payload.Query.Payload.Ray.Distance,
            };

            return new RaycastQuery
            {
                id = action.Payload.Query.Payload.QueryId,
                raycastType = raycastType,
                ray = ray,
                sceneNumber = sceneNumber,
            };
        }

        private static string MapMessagingMethodType(EntityAction action) =>
            action.Payload.PayloadCase switch
            {
                EntityActionPayload.PayloadOneofCase.InitMessagesFinished => MessagingTypes.INIT_DONE,
                EntityActionPayload.PayloadOneofCase.OpenExternalUrl => MessagingTypes.OPEN_EXTERNAL_URL,
                EntityActionPayload.PayloadOneofCase.OpenNftDialog => MessagingTypes.OPEN_NFT_DIALOG,
                EntityActionPayload.PayloadOneofCase.CreateEntity => MessagingTypes.ENTITY_CREATE,
                EntityActionPayload.PayloadOneofCase.RemoveEntity => MessagingTypes.ENTITY_DESTROY,
                EntityActionPayload.PayloadOneofCase.AttachEntityComponent => MessagingTypes.SHARED_COMPONENT_ATTACH,
                EntityActionPayload.PayloadOneofCase.ComponentRemoved => MessagingTypes.ENTITY_COMPONENT_DESTROY,
                EntityActionPayload.PayloadOneofCase.SetEntityParent => MessagingTypes.ENTITY_REPARENT,
                EntityActionPayload.PayloadOneofCase.Query => MessagingTypes.QUERY,
                EntityActionPayload.PayloadOneofCase.ComponentCreated => MessagingTypes.SHARED_COMPONENT_CREATE,
                EntityActionPayload.PayloadOneofCase.ComponentDisposed => MessagingTypes.SHARED_COMPONENT_DISPOSE,
                EntityActionPayload.PayloadOneofCase.UpdateEntityComponent => MessagingTypes.PB_ENTITY_COMPONENT_CREATE_OR_UPDATE,
                EntityActionPayload.PayloadOneofCase.ComponentUpdated => MessagingTypes.PB_SHARED_COMPONENT_UPDATE,
                EntityActionPayload.PayloadOneofCase.None => null,
                _ => throw new ArgumentOutOfRangeException(),
            };
    }
}
