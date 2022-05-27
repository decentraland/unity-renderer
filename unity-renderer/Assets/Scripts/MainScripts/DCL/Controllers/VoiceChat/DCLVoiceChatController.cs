using UnityEngine;
using SocialFeaturesAnalytics;

namespace DCL
{
    public class DCLVoiceChatController : MonoBehaviour
    {
        [Header("InputActions")]
        public InputAction_Hold voiceChatAction;
        public InputAction_Trigger voiceChatToggleAction;

        private InputAction_Hold.Started voiceChatStartedDelegate;
        private InputAction_Hold.Finished voiceChatFinishedDelegate;
        private InputAction_Trigger.Triggered voiceChatToggleDelegate;

        private bool firstTimeVoiceRecorded = true;
        private ISocialAnalytics socialAnalytics;

        void Awake()
        {
            voiceChatStartedDelegate = (action) => StartVoiceChatRecording();
            voiceChatFinishedDelegate = (action) => StopVoiceChatRecording();
            voiceChatToggleDelegate = (action) => ToggleVoiceChatRecording();
            voiceChatAction.OnStarted += voiceChatStartedDelegate;
            voiceChatAction.OnFinished += voiceChatFinishedDelegate;
            voiceChatToggleAction.OnTriggered += voiceChatToggleDelegate;

            KernelConfig.i.EnsureConfigInitialized().Then(config => EnableVoiceChat(config.comms.voiceChatEnabled));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
        }
        void OnDestroy()
        {
            voiceChatAction.OnStarted -= voiceChatStartedDelegate;
            voiceChatAction.OnFinished -= voiceChatFinishedDelegate;
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
        }

        void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { EnableVoiceChat(current.comms.voiceChatEnabled); }

        void EnableVoiceChat(bool enable) { CommonScriptableObjects.voiceChatDisabled.Set(!enable); }

        private void StartVoiceChatRecording()
        {
            if (!DataStore.i.player.isJoinedToVoiceChat.Get())
                return;

            Interface.WebInterface.SendSetVoiceChatRecording(true);

            if (firstTimeVoiceRecorded)
            {
                if (socialAnalytics == null)
                    socialAnalytics = new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge());
                
                socialAnalytics.SendVoiceMessageStartedByFirstTime();
                firstTimeVoiceRecorded = false;
            }
        }

        private void StopVoiceChatRecording()
        {
            if (!DataStore.i.player.isJoinedToVoiceChat.Get())
                return;

            Interface.WebInterface.SendSetVoiceChatRecording(false);
        }

        private void ToggleVoiceChatRecording()
        {
            if (!DataStore.i.player.isJoinedToVoiceChat.Get())
                return;

            Interface.WebInterface.ToggleVoiceChatRecording();
        }
    }
}