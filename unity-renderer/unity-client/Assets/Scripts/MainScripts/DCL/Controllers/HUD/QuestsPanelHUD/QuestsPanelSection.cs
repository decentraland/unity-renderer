using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelSection : MonoBehaviour
    {
        [SerializeField] internal TextMeshProUGUI sectionName;
        [SerializeField] internal RectTransform titleContainer;
        [SerializeField] internal RectTransform tasksContainer;
        [SerializeField] internal QuestsPanelTaskFactory factory;

        public void Populate(QuestSection section)
        {
            CleanUpTasksList();
            titleContainer.gameObject.SetActive(!string.IsNullOrEmpty(section.name));
            sectionName.text = section.name;
            foreach (QuestTask task in section.tasks)
            {
                CreateTask(task);
            }
        }

        internal void CreateTask(QuestTask task)
        {
            GameObject prefab = factory.GetPrefab(task.type);
            if (prefab == null)
            {
                Debug.LogError($"Type: {task.type} was not found in QuestTaskFactory");
                return;
            }

            var taskUIEntry = Instantiate(prefab, tasksContainer).GetComponent<IQuestsPanelTask>();
            taskUIEntry.Populate(task);
        }

        internal void CleanUpTasksList()
        {
            for (int i = tasksContainer.childCount - 1; i >= 0; i--)
                Destroy(tasksContainer.GetChild(i).gameObject);
        }
    }
}