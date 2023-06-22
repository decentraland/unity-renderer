using DCL;
using DCL.Quests;
using UnityEngine;

namespace DCLServices.QuestsService.TestScene
{
    public class QuestServiceTestScene : MonoBehaviour
    {
        [SerializeField] private ClientQuestsServiceMock client = null;
        [SerializeField] private QuestTrackerComponentView questTrackerComponentView;
        [SerializeField] private QuestCompletedComponentView questCompletedComponentView;
        [SerializeField] private QuestStartedPopupComponentView questStartedPopupComponentView;
        [SerializeField] private QuestLogComponentView questLogComponentView;
        private QuestsService service;
        private QuestsController questController;

        private void Awake()
        {
            service = new QuestsService(client);
            service.QuestUpdated.AddListener((questUpdate) => {Debug.Log($"QuestUdpated: {questUpdate.Quest.Name}"); });
            service.QuestStarted.AddListener((questUpdate) => {Debug.Log($"QuestStarted: {questUpdate.Quest.Name}"); });
            //questController = new QuestsController(service, questTrackerComponentView, questCompletedComponentView, questStartedPopupComponentView, questLogComponentView, DataStore.i);
        }
    }
}
