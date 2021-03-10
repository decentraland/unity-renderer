using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestNotification_SectionCompleted : MonoBehaviour
    {
        [SerializeField] internal TextMeshProUGUI sectionName;

        public void Populate(QuestSection section)
        {
            sectionName.text = section.name;
        }
    }
}