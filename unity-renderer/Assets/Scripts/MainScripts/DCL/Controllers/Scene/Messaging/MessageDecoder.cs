using System.Net;
using UnityEngine;
using DCL.Interface;
using DCL.Models;
using DCL.Components;

namespace DCL
{
    public static class MessageDecoder
    {
#if UNITY_EDITOR
        public const string DUMP_PATH = "TestResources/SceneMessages";
        public const string MESSAGES_FILENAME = "messages.txt";
        public const string TRANSFORM_FILENAME = "transform.txt";
        public const string QUERY_FILENAME = "query.txt";

        static bool DUMP_MESSAGES_FOR_PERFORMANCE_TESTS = false;
        const int MESSAGES_COUNT = 1000;
        static int messagesCount = 0;
        static int transformCount = 0;
        static int queryCount = 0;
        static string messageDumpStr;
        static string transformDumpStr;
        static string queryDumpStr;
#endif

        public static bool DecodePayloadChunk(string payload, out string sceneId, out string message, out string tag, out PB_SendSceneMessage sendSceneMessage)
        {
#if UNITY_EDITOR
            DumpMessage(payload, MESSAGES_FILENAME, ref messageDumpStr, ref messagesCount);
#endif
            byte[] bytes = System.Convert.FromBase64String(payload);

            sendSceneMessage = DCL.Interface.PB_SendSceneMessage.Parser.ParseFrom(bytes);
            sceneId = sendSceneMessage.SceneId;
            tag = sendSceneMessage.Tag;

            message = null;

            switch (sendSceneMessage.PayloadCase)
            {
                case PB_SendSceneMessage.PayloadOneofCase.CreateEntity:
                    message = MessagingTypes.ENTITY_CREATE;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.RemoveEntity:
                    message = MessagingTypes.ENTITY_DESTROY;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.UpdateEntityComponent:
                    message = MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.AttachEntityComponent:
                    message = MessagingTypes.SHARED_COMPONENT_ATTACH;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.ComponentCreated:
                    message = MessagingTypes.SHARED_COMPONENT_CREATE;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.ComponentDisposed:
                    message = MessagingTypes.SHARED_COMPONENT_DISPOSE;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.ComponentRemoved:
                    message = MessagingTypes.ENTITY_COMPONENT_DESTROY;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.ComponentUpdated:
                    message = MessagingTypes.SHARED_COMPONENT_UPDATE;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.SceneStarted:
                    message = MessagingTypes.INIT_DONE;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.SetEntityParent:
                    message = MessagingTypes.ENTITY_REPARENT;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.Query:
                    message = MessagingTypes.QUERY;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.OpenExternalUrl:
                    message = MessagingTypes.OPEN_EXTERNAL_URL;
                    break;
                case PB_SendSceneMessage.PayloadOneofCase.OpenNFTDialog:
                    message = MessagingTypes.OPEN_NFT_DIALOG;
                    break;
                default:
                    Debug.Log("Error: " + payload);
                    break;
            }

            return true;
        }

        public static void DecodeSceneMessage(string sceneId, string method, string tag, PB_SendSceneMessage sendSceneMessage, ref QueuedSceneMessage_Scene queuedMessage)
        {
            queuedMessage.type = QueuedSceneMessage.Type.SCENE_MESSAGE;
            queuedMessage.sceneId = sceneId;
            queuedMessage.method = method;
            queuedMessage.tag = tag;

            switch (method)
            {
                case MessagingTypes.INIT_DONE:
                    queuedMessage.payload = new Protocol.SceneReady();
                    break;
                case MessagingTypes.QUERY:
                    QueryMessage query = new QueryMessage();
                    DecodeQueryMessage(sendSceneMessage.Query.QueryId, sendSceneMessage.Query.Payload, ref query);
                    queuedMessage.payload = query;
                    break;
                case MessagingTypes.ENTITY_CREATE:
                    queuedMessage.payload = Protocol.CreateEntity.FromPB(sendSceneMessage.CreateEntity);
                    break;
                case MessagingTypes.ENTITY_DESTROY:
                    queuedMessage.payload = Protocol.RemoveEntity.FromPB(sendSceneMessage.RemoveEntity);
                    break;
                case MessagingTypes.ENTITY_REPARENT:
                    queuedMessage.payload = Protocol.SetEntityParent.FromPB(sendSceneMessage.SetEntityParent);
                    break;
                case MessagingTypes.SHARED_COMPONENT_CREATE:
                    queuedMessage.payload = Protocol.SharedComponentCreate.FromPB(sendSceneMessage.ComponentCreated);
                    break;
                case MessagingTypes.SHARED_COMPONENT_ATTACH:
                    queuedMessage.payload = Protocol.SharedComponentAttach.FromPB(sendSceneMessage.AttachEntityComponent);
                    break;
                case MessagingTypes.SHARED_COMPONENT_UPDATE:
                    queuedMessage.payload = Protocol.SharedComponentUpdate.FromPB(sendSceneMessage.ComponentUpdated);
                    break;
                case MessagingTypes.SHARED_COMPONENT_DISPOSE:
                    queuedMessage.payload = Protocol.SharedComponentDispose.FromPB(sendSceneMessage.ComponentDisposed);
                    break;
                case MessagingTypes.ENTITY_COMPONENT_CREATE_OR_UPDATE:
                    queuedMessage.payload = Protocol.EntityComponentCreateOrUpdate.FromPB(sendSceneMessage.UpdateEntityComponent);
                    break;
                case MessagingTypes.ENTITY_COMPONENT_DESTROY:
                    queuedMessage.payload = Protocol.EntityComponentDestroy.FromPB(sendSceneMessage.ComponentRemoved);
                    break;
                case MessagingTypes.OPEN_NFT_DIALOG:
                    queuedMessage.payload = Protocol.OpenNftDialog.FromPB(sendSceneMessage.OpenNFTDialog);
                    break;
                case MessagingTypes.OPEN_EXTERNAL_URL:
                    queuedMessage.payload = Protocol.OpenExternalUrl.FromPB(sendSceneMessage.OpenExternalUrl);
                    break;
            }
        }

        public static void DecodeTransform(string payload, ref DCLTransform.Model model)
        {
#if UNITY_EDITOR
            DumpMessage(payload, TRANSFORM_FILENAME, ref transformDumpStr, ref transformCount);
#endif
            byte[] bytes = System.Convert.FromBase64String(payload);

            DCL.Interface.PB_Transform pbTransform = DCL.Interface.PB_Transform.Parser.ParseFrom(bytes);
            model.position = new Vector3(pbTransform.Position.X, pbTransform.Position.Y, pbTransform.Position.Z);
            model.scale = new Vector3(pbTransform.Scale.X, pbTransform.Scale.Y, pbTransform.Scale.Z);
            model.rotation = new Quaternion((float)pbTransform.Rotation.X, (float)pbTransform.Rotation.Y, (float)pbTransform.Rotation.Z, (float)pbTransform.Rotation.W);
        }

        public static void DecodeQueryMessage(string queryId, string payload, ref QueryMessage query)
        {
#if UNITY_EDITOR
            DumpMessage(payload, QUERY_FILENAME, ref queryDumpStr, ref queryCount);
#endif

            byte[] bytes = System.Convert.FromBase64String(payload);
            PB_RayQuery pbRayQuery = PB_RayQuery.Parser.ParseFrom(bytes);

            query.queryType = queryId;
            query.payload = new RaycastQuery();
            query.payload.id = pbRayQuery.QueryId;
            query.payload.raycastType = Protocol.RaycastLiteralToType(pbRayQuery.QueryType);
            query.payload.ray = new DCL.Models.Ray();
            query.payload.ray.direction = new Vector3(pbRayQuery.Ray.Direction.X, pbRayQuery.Ray.Direction.Y, pbRayQuery.Ray.Direction.Z);
            query.payload.ray.distance = pbRayQuery.Ray.Distance;
            query.payload.ray.origin = new Vector3(pbRayQuery.Ray.Origin.X, pbRayQuery.Ray.Origin.Y, pbRayQuery.Ray.Origin.Z);
        }

#if UNITY_EDITOR
        private static void DumpMessage(string payload, string filename, ref string dumpString, ref int counter)
        {
            if (DUMP_MESSAGES_FOR_PERFORMANCE_TESTS && CommonScriptableObjects.rendererState.Get())
            {
                if (counter < MESSAGES_COUNT)
                {
                    dumpString += payload + "\n";

                    counter++;
                    if (counter >= MESSAGES_COUNT)
                    {
                        string fullFilename = System.IO.Path.Combine(DUMP_PATH, filename);
                        System.IO.File.WriteAllText($"{fullFilename}", dumpString);

                        Debug.Log($"{filename} file dumped!");
                    }
                }
            }
        }
#endif
    }
}