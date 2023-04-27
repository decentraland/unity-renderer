// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.quests
// file: decentraland/renderer/quests/quests.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;
using System;

namespace Decentraland.Quests
{
    public interface IClientQuestsService
    {
        UniTask<StartQuestResponse> StartQuest(StartQuestRequest request);

        UniTask<AbortQuestResponse> AbortQuest(AbortQuestRequest request);

        UniTask<EventResponse> SendEvent(Event request);

        IUniTaskAsyncEnumerable<UserUpdate> Subscribe(UserAddress request);

        UniTask<QuestDefinition> GetDefinition(string questId);

        UniTask<QuestState> GetState(string questId);
    }

    public class ClientQuestsService : IClientQuestsService
    {
        private readonly RpcClientModule module;

        public ClientQuestsService(RpcClientModule module)
        {
            this.module = module;
        }

        public UniTask<StartQuestResponse> StartQuest(StartQuestRequest request)
        {
            return module.CallUnaryProcedure<StartQuestResponse>("StartQuest", request);
        }

        public UniTask<AbortQuestResponse> AbortQuest(AbortQuestRequest request)
        {
            return module.CallUnaryProcedure<AbortQuestResponse>("AbortQuest", request);
        }

        public UniTask<EventResponse> SendEvent(Event request)
        {
            return module.CallUnaryProcedure<EventResponse>("SendEvent", request);
        }

        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(UserAddress request)
        {
            return module.CallServerStream<UserUpdate>("Subscribe", request);
        }

        public UniTask<QuestDefinition> GetDefinition(string questId) =>
            throw new NotImplementedException();

        public UniTask<QuestState> GetState(string questId) =>
            throw new NotImplementedException();
    }
}
