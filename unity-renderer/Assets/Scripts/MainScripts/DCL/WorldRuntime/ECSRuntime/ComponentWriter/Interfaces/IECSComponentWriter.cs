using System;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    [Flags]
    public enum ECSComponentWriteType
    {
        /* send component modification to scene */
        SEND_TO_SCENE = 1,
        /* write component modification to local crdt state and execute */
        SEND_TO_LOCAL = WRITE_STATE_LOCALLY | EXECUTE_LOCALLY,
        /* write component modification to local crdt state */
        WRITE_STATE_LOCALLY = 2,
        /* execute component modification in renderer */
        EXECUTE_LOCALLY = 4,
        /* send component modification to scene, write component to local crdt state and execute */
        DEFAULT = SEND_TO_SCENE | SEND_TO_LOCAL
    }

    public interface IECSComponentWriter : IDisposable
    {
        void AddOrReplaceComponentSerializer<T>(int componentId, Func<T, byte[]> serializer);
        void RemoveComponentSerializer(int componentId);
        void PutComponent<T>(IParcelScene scene, IDCLEntity entity, int componentId, T model, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void PutComponent<T>(string sceneId, long entityId, int componentId, T model, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void PutComponent<T>(string sceneId, long entityId, int componentId, T model, long minTimeStamp, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(IParcelScene scene, IDCLEntity entity, int componentId, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(string sceneId, long entityId, int componentId, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(string sceneId, long entityId, int componentId, long minTimeStamp, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
    }
}