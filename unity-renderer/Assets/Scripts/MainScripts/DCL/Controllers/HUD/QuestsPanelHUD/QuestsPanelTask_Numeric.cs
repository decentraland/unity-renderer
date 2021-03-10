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
        [SerializeField] internal TextMeshProUGUI start;
        [SerializeField] internal TextMeshProUGUI end;
        [SerializeField] internal TextMeshProUGUI current;
        [SerializeField] internal Image ongoingProgress;
        [SerializeField] internal Button jumpInButton;

        internal TaskPayload_Numeric payload;
        private Action jumpInDelegate;

        public void Awake()
        {
            jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke();});
        }

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
            start.text = payload.start.ToString();
            current.text = payload.current.ToString();
            end.text = payload.end.ToString();

            ongoingProgress.fillAmount = task.progress;
        }
    }
}