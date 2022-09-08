using UnityEngine;
using SocialFeaturesAnalytics;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

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
        private UserProfileWebInterfaceBridge userProfileWebInterfaceBridge;
        private double voiceMessageStartTime = 0;
        private bool isVoiceChatToggledOn = false;

        void Awake()
        {
            userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();

            voiceChatStartedDelegate = (action) => DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(true, true));
            voiceChatFinishedDelegate = (action) => DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(false, true));
            voiceChatToggleDelegate = (action) => ToggleVoiceChatRecording();
            voiceChatAction.OnStarted += voiceChatStartedDelegate;
            voiceChatAction.OnFinished += voiceChatFinishedDelegate;
            voiceChatToggleAction.OnTriggered += voiceChatToggleDelegate;

            KernelConfig.i.EnsureConfigInitialized().Then(config => EnableVoiceChat(config.comms.voiceChatEnabled));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
            DataStore.i.voiceChat.isRecording.OnChange += IsVoiceChatRecordingChanged;
        }

        void OnDestroy()
        {
            voiceChatAction.OnStarted -= voiceChatStartedDelegate;
            voiceChatAction.OnFinished -= voiceChatFinishedDelegate;
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            DataStore.i.voiceChat.isRecording.OnChange -= IsVoiceChatRecordingChanged;
        }

        void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { EnableVoiceChat(current.comms.voiceChatEnabled); }

        void EnableVoiceChat(bool enable) { CommonScriptableObjects.voiceChatDisabled.Set(!enable); }

        public void VoiceChatStatus(string voiceChatStatusPayload)
        {
            VoiceChatStatusPayload voiceChatStatus = JsonConvert.DeserializeObject<VoiceChatStatusPayload>(voiceChatStatusPayload);
            DataStore.i.voiceChat.isJoinedToVoiceChat.Set(voiceChatStatus.isConnected);
        }

        private void IsVoiceChatRecordingChanged(KeyValuePair<bool, bool> current, KeyValuePair<bool, bool> previous)
        {
            if (!DataStore.i.voiceChat.isJoinedToVoiceChat.Get())
                return;

            CreateSocialAnalyticsIfNeeded();

            if (current.Key)
            {
                if (!isVoiceChatToggledOn)
                {
                    Interface.WebInterface.SendSetVoiceChatRecording(true);
                    SendFirstTimeMetricIfNeeded();
                    voiceMessageStartTime = Time.realtimeSinceStartup;
                }
            }
            else
            {
                Interface.WebInterface.SendSetVoiceChatRecording(false);

                socialAnalytics.SendVoiceMessage(
                    Time.realtimeSinceStartup - voiceMessageStartTime, 
                    (current.Value || isVoiceChatToggledOn) ? VoiceMessageSource.Shortcut : VoiceMessageSource.Button, 
                    userProfileWebInterfaceBridge.GetOwn().userId);

                isVoiceChatToggledOn = false;
            }
        }

        private void ToggleVoiceChatRecording()
        {
            if (!DataStore.i.voiceChat.isJoinedToVoiceChat.Get())
                return;

            Interface.WebInterface.ToggleVoiceChatRecording();
            isVoiceChatToggledOn = !isVoiceChatToggledOn;

            if (isVoiceChatToggledOn)
            {
                SendFirstTimeMetricIfNeeded();
                voiceMessageStartTime = Time.realtimeSinceStartup;
            }
            else
            {
                socialAnalytics.SendVoiceMessage(
                    Time.realtimeSinceStartup - voiceMessageStartTime,
                    VoiceMessageSource.Shortcut,
                    userProfileWebInterfaceBridge.GetOwn().userId);
            }
        }

        private void CreateSocialAnalyticsIfNeeded()
        {
            if (socialAnalytics != null)
                return;

            socialAnalytics = new SocialAnalytics(
                Environment.i.platform.serviceProviders.analytics,
                userProfileWebInterfaceBridge);
        }

        private void SendFirstTimeMetricIfNeeded()
        {
            if (firstTimeVoiceRecorded)
            {
                CreateSocialAnalyticsIfNeeded();
                socialAnalytics.SendVoiceMessageStartedByFirstTime();
                firstTimeVoiceRecorded = false;
            }
        }
    }

    [Serializable]
    public class VoiceChatStatusPayload
    {
        public bool isConnected;
    }
}