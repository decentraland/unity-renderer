using DCL;
using DCL.QuestsController;
using UnityEngine;

public class TaskbarNewQuestTooltip : MonoBehaviour
{
    private static readonly int ANIM_STATE_TRIGGER = Animator.StringToHash("ShowDisabledTooltip");
    [SerializeField] private Animator animator;
    [SerializeField] private AudioEvent newQuestAudioEvent;

    private void Awake() { QuestsController.i.OnNewQuest += OnNewQuest; }

    private void OnDestroy() { QuestsController.i.OnNewQuest -= OnNewQuest; }

    private void OnNewQuest(string s)
    {
        animator?.SetTrigger(ANIM_STATE_TRIGGER);
        newQuestAudioEvent.Play();
    }
}