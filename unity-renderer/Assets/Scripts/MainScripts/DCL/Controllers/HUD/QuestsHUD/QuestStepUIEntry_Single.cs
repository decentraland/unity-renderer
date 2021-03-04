using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds
{
    public class QuestStepUIEntry_Single : MonoBehaviour, IQuestStepUIEntry
    {
        public class Model
        {
            public string status;
            public string description;
        }

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Toggle status;

        internal Model model;

        public void Populate(string payload)
        {
            model = JsonUtility.FromJson<Model>(payload);
            description.text = model.description;
            status.isOn = model.status == "completed";
        }
    }
}