using DCL.Components;
using DCL.Models;

namespace MainScripts.DCL.WorldRuntime
{
    public interface ISceneMessageProcessor
    {
        IDCLEntity CreateEntity(long id);
        void RemoveEntity(long id, bool removeImmediatelyFromEntitiesList = true);
        void SetEntityParent(long entityId, long parentId);
        void SharedComponentAttach(long entityId, string id);
        IEntityComponent EntityComponentCreateOrUpdateWithModel(long entityId, CLASS_ID_COMPONENT classId, object data);
        IEntityComponent EntityComponentCreateOrUpdate(long entityId, CLASS_ID_COMPONENT classId, string data) ;
        IEntityComponent EntityComponentUpdate(IDCLEntity entity, CLASS_ID_COMPONENT classId,
            string componentJson);
        ISharedComponent SharedComponentCreate(string id, int classId);
        void SharedComponentDispose(string id);
        void EntityComponentRemove(long entityId, string name);
        ISharedComponent SharedComponentUpdate(string id, string json);
    }
}