using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using UnityEngine;
using Event = Decentraland.Quests.Event;

namespace DCLServices.QuestsService.Tests.TestScene
{
    public class QuestsServiceMock : MonoBehaviour, IClientQuestsService
    {
        [SerializeField] TextAsset questsJson;

        private Queue<QuestStateUpdate> questStateUpdates = new Queue<QuestStateUpdate>();

        public UniTask<StartQuestResponse> StartQuest(StartQuestRequest request) =>
            throw new NotImplementedException();

        public UniTask<AbortQuestResponse> AbortQuest(AbortQuestRequest request) =>
            throw new NotImplementedException();

        public UniTask<EventResponse> SendEvent(Event request) =>
            throw new NotImplementedException();

        public IUniTaskAsyncEnumerable<UserUpdate> Subscribe(UserAddress request)
        {
            return UniTaskAsyncEnumerable.Create<UserUpdate>(async (writer, token) =>
            {
                while (true)
                {
                    while(questStateUpdates.Count > 0)
                    {
                        writer.YieldAsync(new UserUpdate { QuestState = questStateUpdates.Dequeue() });
                    }
                    await UniTask.NextFrame(token);
                }
            });
        }

        public void EnqueueChanges(int amount)
        {
            Debug.Log(amount);
        }

        public UniTask<QuestDefinition> GetDefinition(string questId) =>
            throw new NotImplementedException();

        public UniTask<QuestState> GetState(string questId) =>
            throw new NotImplementedException();

    }
}
