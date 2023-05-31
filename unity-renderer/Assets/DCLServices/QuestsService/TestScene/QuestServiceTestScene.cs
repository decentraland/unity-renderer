using UnityEngine;

namespace DCLServices.QuestsService.TestScene
{
    public class QuestServiceTestScene :  MonoBehaviour
    {
        [SerializeField] private ClientQuestsServiceMock client = null;
        private QuestsService service;

        private void Awake()
        {
            service = new QuestsService(client);
            service.SetUserId("Test");
            service.OnQuestUpdated += (questUpdate) => {Debug.Log($"QuestUdpated: {questUpdate.Name}"); };
        }
    }
}
