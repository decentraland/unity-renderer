using TMPro;
using UnityEngine;

namespace DCL.Huds
{
    public class QuestTaskUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private RectTransform stepsContainer;
        [SerializeField] private QuestStepUIFactory factory;

        public void Populate(QuestTask task)
        {
            CleanUpStepsList(); //TODO: Reuse already instantiated steps
            description.text = task.description;
            foreach (QuestStep step in task.steps)
            {
                CreateStep(step);
            }
        }

        internal void CreateStep(QuestStep step)
        {
            GameObject prefab = factory.GetPrefab(step.type);
            if (prefab == null)
            {
                Debug.LogError($"Type: {step.type} was not found in QuestStepFactory");
                return;
            }

            var stepUIEntry = Instantiate(prefab, stepsContainer).GetComponent<IQuestStepUIEntry>();
            stepUIEntry.Populate(step.payload);
        }

        internal void CleanUpStepsList()
        {
            while (stepsContainer.childCount > 0)
            {
                Destroy(stepsContainer.GetChild(0).gameObject);
            }
        }
    }
}