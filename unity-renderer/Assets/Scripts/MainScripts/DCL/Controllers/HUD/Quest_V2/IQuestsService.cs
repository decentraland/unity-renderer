using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

public interface IQuestsService
{
    public event Action<QuestStateUpdate> OnQuestUpdated;

    void SetUserId(string userId);
    public UniTask<StartQuestResponse> StartQuest(string questId);
    public UniTask<AbortQuestResponse> AbortQuest(string questInstanceId);
    public UniTask<ProtoQuest> GetDefinition(string questId, CancellationToken cancellationToken = default);
}
