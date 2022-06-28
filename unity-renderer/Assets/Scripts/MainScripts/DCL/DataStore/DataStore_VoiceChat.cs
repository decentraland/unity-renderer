using System.Collections.Generic;

namespace DCL
{
    public class DataStore_VoiceChat
    {
        public readonly BaseVariable<bool> isJoinedToVoiceChat = new BaseVariable<bool>(false);
        public readonly BaseVariable<KeyValuePair<bool, bool>> isRecording = new BaseVariable<KeyValuePair<bool, bool>>(new KeyValuePair<bool, bool>(false, false));
    }
}