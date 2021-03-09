using UnityEngine;

namespace DCL.Huds
{
    public class QuestsHUDView : MonoBehaviour
    {
        private const string VIEW_PATH = "QuestsHUD";

        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;

        internal static QuestsHUDView Create()
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuestsHUDView>();

#if UNITY_EDITOR
            view.gameObject.name = "_QuestHUDView";
#endif
            return view;
        }

        public void Populate(QuestModel[] quests)
        {
            CleanUpQuestsList(); //TODO Reuse already instantiated quests
            for (int i = 0; i < quests.Length; i++)
            {
                CreateQuest(quests[i]);
            }
        }

        internal void CreateQuest(QuestModel quest)
        {
            var questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestUIEntry>();
            questEntry.Populate(quest);
        }

        internal void CleanUpQuestsList()
        {
            while (questsContainer.childCount > 0)
            {
                Destroy(questsContainer.GetChild(0).gameObject);
            }
        }
    }
}