using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerTask : MonoBehaviour
    {
        private static readonly int EXPAND_ANIMATOR_TRIGGER = Animator.StringToHash("Expand");
        private static readonly int COLLAPSE_ANIMATOR_TRIGGER = Animator.StringToHash("Collapse");

        [SerializeField] internal TextMeshProUGUI taskTitle;
        [SerializeField] internal Image progress;
        [SerializeField] internal TextMeshProUGUI progressText;
        [SerializeField] internal Button jumpInButton;
        [SerializeField] internal Animator animator;

        private float progressTarget = 0;

        private Action jumpInDelegate;

        public void Awake() { jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke(); }); }

        public void Populate(QuestTask task)
        {
            taskTitle.text = task.name;
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });

            jumpInButton.gameObject.SetActive(task.progress < 1 && !string.IsNullOrEmpty(task.coordinates));
            progressTarget = task.progress;
            switch (task.type)
            {
                case "single":
                    SetProgressText(task.progress, 1);
                    break;
                case "numeric":
                    var payload = JsonUtility.FromJson<TaskPayload_Numeric>(task.payload);
                    SetProgressText(payload.current, payload.end);
                    break;
            }
        }

        private void Update()
        {
            Vector3 scale = progress.transform.localScale;
            if (Math.Abs(scale.x - progressTarget) < Mathf.Epsilon)
                return;
            scale.x = Mathf.MoveTowards(scale.x, progressTarget, 0.1f);
            progress.transform.localScale = scale;
        }

        internal void SetProgressText(float current, float end) { progressText.text = $"{current}/{end}"; }

        public void SetExpandedStatus(bool active)
        {
            if (active)
                animator.SetTrigger(EXPAND_ANIMATOR_TRIGGER);
            else
                animator.SetTrigger(COLLAPSE_ANIMATOR_TRIGGER);
        }
    }
}