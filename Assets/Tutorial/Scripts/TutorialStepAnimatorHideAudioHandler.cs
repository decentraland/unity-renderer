using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStepAnimatorHideAudioHandler : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioScriptableObjects.fadeOut.Play(true);
    }
}
