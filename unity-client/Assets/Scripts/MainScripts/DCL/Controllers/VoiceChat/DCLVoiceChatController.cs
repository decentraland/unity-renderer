using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class DCLVoiceChatController : MonoBehaviour
    {
        [Header("InputActions")] public InputAction_Hold voiceChatAction;

        private InputAction_Hold.Started voiceChatStartedDelegate;
        private InputAction_Hold.Finished voiceChatFinishedDelegate;
        void Awake()
        {
            voiceChatStartedDelegate = (action) => DCL.Interface.WebInterface.SendSetVoiceChatRecording(true);
            voiceChatFinishedDelegate = (action) => DCL.Interface.WebInterface.SendSetVoiceChatRecording(false);
            voiceChatAction.OnStarted += voiceChatStartedDelegate;
            voiceChatAction.OnFinished += voiceChatFinishedDelegate;
        }
        void OnDestroy()
        {
            voiceChatAction.OnStarted -= voiceChatStartedDelegate;
            voiceChatAction.OnFinished -= voiceChatFinishedDelegate;
        }
    }
}
