using System.Linq;
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
            var orderedTasks = section.tasks.OrderBy(x => x.progress >= 1).ThenBy(x => x.completionTime).ToArray();
            foreach (QuestTask task in orderedTasks)
            {
                CreateTask(task);
            }
        }

        internal void CreateTask(QuestTask task)
        {
            if (task.status == QuestsLiterals.Status.BLOCKED)
                return;

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