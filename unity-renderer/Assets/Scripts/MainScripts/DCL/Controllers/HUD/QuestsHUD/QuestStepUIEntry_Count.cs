using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds
{
    public class QuestStepUIEntry_Count : MonoBehaviour, IQuestStepUIEntry
    {
        public class Model
        {
            public string status;
            public string description;
            public int start;
            public int current;
            public int target;
        }

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Toggle status;
        [SerializeField] private TextMeshProUGUI start;
        [SerializeField] private TextMeshProUGUI current;
        [SerializeField] private TextMeshProUGUI target;
        [SerializeField] private Image ongoingProgress;

        internal Model model;

        public void Populate(string payload)
        {
            model = JsonUtility.FromJson<Model>(payload);
            description.text = model.description;
            status.isOn = model.status == "completed";
            start.text = model.start.ToString();
            current.text = model.current.ToString();
            target.text = model.target.ToString();

            ongoingProgress.fillAmount = (float)model.current / model.target;
        }
    }
}