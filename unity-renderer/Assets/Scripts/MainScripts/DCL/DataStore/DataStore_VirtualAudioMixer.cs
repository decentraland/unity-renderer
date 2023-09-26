namespace DCL
{
    /// <summary>
    /// An "audio mixer" that handles muting/fading when entering special states like Avatar Editor, Tutorial, Builder In-World, etc.
    /// </summary>
    public class DataStore_VirtualAudioMixer
    {
        public readonly BaseVariable<float> musicVolume = new (1f);
        public readonly BaseVariable<float> sceneSFXVolume = new (1f);
        public readonly BaseVariable<float> voiceChatVolume = new (1f);
        public readonly BaseVariable<float> uiSFXVolume = new (1f);
        public readonly BaseVariable<float> avatarSFXVolume = new (1f);
    }
}
