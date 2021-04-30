using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelTask_Numeric : MonoBehaviour, IQuestsPanelTask
    {
        [SerializeField] internal TextMeshProUGUI taskName;
        [SerializeField] internal TextMeshProUGUI progressText;
        [SerializeField] internal Image ongoingProgress;
        [SerializeField] internal Button jumpInButton;
        [SerializeField] internal Toggle status;
        [SerializeField] internal Color ongoinColor;
        [SerializeField] internal Color completedcolor;

        internal TaskPayload_Numeric payload;
        private Action jumpInDelegate;

        public void Awake() { jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke(); }); }

        public void Populate(QuestTask task)
        {
            payload = JsonUtility.FromJson<TaskPayload_Numeric>(task.payload);

            jumpInButton.gameObject.SetActive(task.progress < 1 && !string.IsNullOrEmpty(task.coordinates));
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });

            taskName.text = task.name;
            progressText.text = $"{payload.current}/{payload.end}";
            status.isOn = task.progress >= 1;
            ongoingProgress.transform.localScale = new Vector3(task.progress, 1, 1);
            ongoingProgress.color = task.progress < 1 ? ongoinColor : completedcolor;
        }
    }
}