using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;

public interface IQuestsService
{
    public event Action<QuestStateUpdate> OnQuestUpdated;
    public event Action<QuestStateUpdate> OnQuestPopup;

    void SetUserId(string userId);
    public UniTask<StartQuestResponse> StartQuest(string questId, CancellationToken cancellationToken = default);
    public UniTask<AbortQuestResponse> AbortQuest(string questInstanceId, CancellationToken cancellationToken = default);
    public UniTask<ProtoQuest> GetDefinition(string questId, CancellationToken cancellationToken = default);
}
