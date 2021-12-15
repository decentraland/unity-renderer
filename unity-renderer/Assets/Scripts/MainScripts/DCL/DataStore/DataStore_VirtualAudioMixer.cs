namespace DCL
{
    /// <summary>
    /// An "audio mixer" that handles muting/fading when entering special states like Avatar Editor, Tutorial, Builder In-World, etc.
    /// </summary>
    public class DataStore_VirtualAudioMixer
    {
        public readonly BaseVariable<float> musicVolume = new BaseVariable<float>(1f);
        public readonly BaseVariable<float> sceneSFXVolume = new BaseVariable<float>(1f);
        public readonly BaseVariable<float> voiceChatVolume = new BaseVariable<float>(1f);
        public readonly BaseVariable<float> uiSFXVolume = new BaseVariable<float>(1f);
        public readonly BaseVariable<float> avatarSFXVolume = new BaseVariable<float>(1f);
    }
}