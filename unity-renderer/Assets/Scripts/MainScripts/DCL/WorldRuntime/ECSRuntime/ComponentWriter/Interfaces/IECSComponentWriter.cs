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
        void PutComponent(Type componentType, int sceneNumber, long entityId, int componentId, object model,
            int minTimeStamp, ECSComponentWriteType writeType);
        void PutComponent<T>(IParcelScene scene, IDCLEntity entity, int componentId, T model, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void PutComponent<T>(int sceneNumber, long entityId, int componentId, T model, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void PutComponent<T>(int sceneNumber, long entityId, int componentId, T model, int minTimeStamp, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(int sceneNumber, long entityId, int componentId, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(int sceneNumber, long entityId, int componentId, int minTimeStamp, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);

        void PutComponent(Type componentType, int sceneNumber, long entityId, int componentId, object model, ECSComponentWriteType writeType);

        void AppendComponent(Type componentType, int sceneNumber, long entityId, int componentId, object model,
            ECSComponentWriteType writeType);

        void AppendComponent<T>(int sceneNumber, long entityId, int componentId, T model, ECSComponentWriteType writeType);

    }
}
