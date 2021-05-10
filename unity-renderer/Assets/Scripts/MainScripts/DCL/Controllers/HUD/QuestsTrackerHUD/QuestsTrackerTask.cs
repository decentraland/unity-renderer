using DCL.Interface;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerTask : MonoBehaviour
    {
        private const string TASK_COMPLETED_ANIMATION_NAME = "QuestTrackerTaskCompleted";
        private static readonly int EXPAND_ANIMATOR_TRIGGER = Animator.StringToHash("Expand");
        private static readonly int COLLAPSE_ANIMATOR_TRIGGER = Animator.StringToHash("Collapse");
        private static readonly int COMPLETED_ANIMATOR_TRIGGER = Animator.StringToHash("Completed");
        private static readonly int NEW_ANIMATOR_TRIGGER = Animator.StringToHash("New");

        public event Action<string> OnDestroyed;

        [SerializeField] internal TextMeshProUGUI taskTitle;
        [SerializeField] internal Image progress;
        [SerializeField] internal TextMeshProUGUI progressText;
        [SerializeField] internal Button jumpInButton;
        [SerializeField] internal Animator animator;

        [Header("Audio Events")]
        [SerializeField] internal AudioEvent progressBarIncrementAudioEvent;
        [SerializeField] internal AudioEvent taskCompleteAudioEvent;

        private QuestTask task = null;
        private float progressTarget = 0;
        private Action jumpInDelegate;

        public void Awake() { jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke(); }); }

        public void SetIsNew(bool isNew) { animator.SetBool(NEW_ANIMATOR_TRIGGER, isNew); }

        public void Populate(QuestTask newTask)
        {
            StopAllCoroutines();
            task = newTask;
            taskTitle.text = task.name;
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });
            Vector3 scale = progress.transform.localScale;
            scale.x =  newTask.oldProgress;
            progress.transform.localScale = scale;
            jumpInButton.gameObject.SetActive(task.progress < 1 && !string.IsNullOrEmpty(task.coordinates));
            progressTarget = task.progress;
            switch (task.type)
            {
                case "single":
                    SetProgressText(task.progress, 1);
                    break;
                case "numeric":
                case "step-based":
                    var payload = JsonUtility.FromJson<TaskPayload_Numeric>(task.payload);
                    SetProgressText(payload.current, payload.end);
                    break;
            }
        }

        public IEnumerator ProgressAndCompleteSequence()
        {
            Vector3 scale = progress.transform.localScale;
            if (Math.Abs(scale.x - progressTarget) > Mathf.Epsilon)
                progressBarIncrementAudioEvent.Play();
            while (Math.Abs(scale.x - progressTarget) > Mathf.Epsilon)
            {
                scale.x = Mathf.MoveTowards(scale.x, progressTarget, Time.deltaTime);
                progress.transform.localScale = scale;
                yield return null;
            }
            if (progressTarget < 1)
                yield break;

            //Dont play completed animation if is already playing
            if (animator.GetCurrentAnimatorClipInfo(0).All(x => x.clip.name != TASK_COMPLETED_ANIMATION_NAME))
            {
                yield return WaitForSecondsCache.Get(0.5f);
                animator.SetTrigger(COMPLETED_ANIMATOR_TRIGGER);
                taskCompleteAudioEvent.Play();
                yield return null; // Wait for the animator to update its clipInfo
            }

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f);

            OnDestroyed?.Invoke(task.id);
            Destroy(gameObject);
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