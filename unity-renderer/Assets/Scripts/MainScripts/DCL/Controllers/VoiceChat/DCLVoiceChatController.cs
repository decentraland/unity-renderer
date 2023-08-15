using DCL.Interface;
using Newtonsoft.Json;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using UnityEngine;

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

            Environment.i.serviceLocator.Get<IApplicationFocusService>().OnApplicationFocusLost += OnApplicationFocusLost;
        }

        private void OnApplicationFocusLost()
        {
            isVoiceChatToggledOn = false;
            StopRecording();
            DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(false, true));
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

            if (isVoiceChatToggledOn) return;

            CreateSocialAnalyticsIfNeeded();

            if (current.Key)
                StartRecording();
            else
                StopRecording();
        }

        private void StartRecording()
        {
            if (isVoiceChatToggledOn) return;

            Debug.LogError("STARTED RECORDING");
            WebInterface.SendSetVoiceChatRecording(true);
            SendFirstTimeMetricIfNeeded();
            voiceMessageStartTime = Time.realtimeSinceStartup;
        }

        private void StopRecording()
        {
            Debug.LogError("STOPPED RECORDING");
            WebInterface.SendSetVoiceChatRecording(false);

            //TODO: Pressing T is considered as shortcut as well?
            socialAnalytics.SendVoiceMessage(
                Time.realtimeSinceStartup - voiceMessageStartTime,
                VoiceMessageSource.Shortcut,
                userProfileWebInterfaceBridge.GetOwn().userId);
        }

        private void ToggleVoiceChatRecording()
        {
            if (!DataStore.i.voiceChat.isJoinedToVoiceChat.Get()) return;

            isVoiceChatToggledOn = !isVoiceChatToggledOn;

            Debug.LogError("USING THE TOGGLE BUTTON " + isVoiceChatToggledOn);

            if (isVoiceChatToggledOn)
                StartRecording();
            else
                StopRecording();
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
