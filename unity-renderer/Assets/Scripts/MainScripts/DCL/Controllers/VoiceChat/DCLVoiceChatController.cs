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

        private InputAction_Hold.Started voiceChatStartedDelegate;
        private InputAction_Hold.Finished voiceChatFinishedDelegate;

        private bool firstTimeVoiceRecorded = true;
        private ISocialAnalytics socialAnalytics;
        private UserProfileWebInterfaceBridge userProfileWebInterfaceBridge;
        private double voiceMessageStartTime = 0;

        private bool isRecording;

        void Awake()
        {
            userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();

            voiceChatStartedDelegate = (action) => DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(true, true));
            voiceChatFinishedDelegate = (action) => DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(false, true));
            voiceChatAction.OnStarted += voiceChatStartedDelegate;
            voiceChatAction.OnFinished += voiceChatFinishedDelegate;

            KernelConfig.i.EnsureConfigInitialized().Then(config => EnableVoiceChat(config.comms.voiceChatEnabled));
            KernelConfig.i.OnChange += OnKernelConfigChanged;
            DataStore.i.voiceChat.isRecording.OnChange += IsVoiceChatRecordingChanged;
        }


        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                StopRecording(true);
                DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(false, true));
            }
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

            if (current.Key)
                StartRecording();
            else
                StopRecording(current.Value);
        }

        private void StartRecording()
        {
            if (isRecording) return;

            WebInterface.SendSetVoiceChatRecording(true);

            CreateSocialAnalyticsIfNeeded();
            SendFirstTimeMetricIfNeeded();
            voiceMessageStartTime = Time.realtimeSinceStartup;

            isRecording = true;
        }

        private void StopRecording(bool usedShortcut)
        {
            if (!isRecording) return;

            WebInterface.SendSetVoiceChatRecording(false);

            CreateSocialAnalyticsIfNeeded();
            socialAnalytics.SendVoiceMessage(
                Time.realtimeSinceStartup - voiceMessageStartTime,
                usedShortcut ? VoiceMessageSource.Shortcut : VoiceMessageSource.Button,
                userProfileWebInterfaceBridge.GetOwn().userId);

            isRecording = false;
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
