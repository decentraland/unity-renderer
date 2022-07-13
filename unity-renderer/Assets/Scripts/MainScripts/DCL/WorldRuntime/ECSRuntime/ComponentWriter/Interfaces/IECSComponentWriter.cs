using System;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECSRuntime
{
    [Flags]
    public enum ECSComponentWriteType
    {
        SEND_TO_SCENE = 1,
        SEND_TO_LOCAL = 2,
        DEFAULT = SEND_TO_SCENE | SEND_TO_LOCAL
    }

    public interface IECSComponentWriter : IDisposable
    {
        void AddOrReplaceComponentSerializer<T>(int componentId, Func<T, byte[]> serializer);
        void RemoveComponentSerializer(int componentId);
        void PutComponent<T>(IParcelScene scene, IDCLEntity entity, int componentId, T model, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void PutComponent<T>(string sceneId, long entityId, int componentId, T model, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(IParcelScene scene, IDCLEntity entity, int componentId, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
        void RemoveComponent(string sceneId, long entityId, int componentId, ECSComponentWriteType writeType = ECSComponentWriteType.DEFAULT);
    }
}