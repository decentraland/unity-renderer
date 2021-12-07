namespace DCL
{
    public class DataStore_AvatarsLOD
    {
        public const int DEFAULT_MAX_AVATAR = 50;
        public const int DEFAULT_MAX_IMPOSTORS = 70;

        public readonly BaseVariable<float> simpleAvatarDistance = new BaseVariable<float>(15f);
        public readonly BaseVariable<float> LODDistance = new BaseVariable<float>(30f);
        public readonly BaseVariable<int> maxAvatars = new BaseVariable<int>(DEFAULT_MAX_AVATAR);
        public readonly BaseVariable<int> maxImpostors = new BaseVariable<int>(DEFAULT_MAX_IMPOSTORS);
    }
}