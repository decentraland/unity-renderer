using DCL.Components;
using DCL.Models;

namespace MainScripts.DCL.WorldRuntime
{
    public interface ISceneMessageProcessor
    {
        IDCLEntity CreateEntity(string id);
        void RemoveEntity(string id, bool removeImmediatelyFromEntitiesList = true);
        void SetEntityParent(string entityId, string parentId);
        void SharedComponentAttach(string entityId, string id);
        IEntityComponent EntityComponentCreateOrUpdateWithModel(string entityId, CLASS_ID_COMPONENT classId, object data);
        IEntityComponent EntityComponentCreateOrUpdate(string entityId, CLASS_ID_COMPONENT classId, string data) ;
        IEntityComponent EntityComponentUpdate(IDCLEntity entity, CLASS_ID_COMPONENT classId,
            string componentJson);
        ISharedComponent SharedComponentCreate(string id, int classId);
        void SharedComponentDispose(string id);
        void EntityComponentRemove(string entityId, string name);
        ISharedComponent SharedComponentUpdate(string id, string json);
    }
}