namespace DCL
{
    public enum AvatarMaterialProfile
    {
        InWorld,
        InEditor
    }

    public class DataStore_SkyboxConfig
    {
        public BaseVariable<bool> objectUpdated = new BaseVariable<bool>(false);
        public BaseVariable<string> configToLoad = new BaseVariable<string>("Generic Skybox");
        public BaseVariable<float> lifecycleDuration = new BaseVariable<float>(60);
        public BaseVariable<float> jumpToTime = new BaseVariable<float>(-1);
        public BaseVariable<float> updateReflectionTime = new BaseVariable<float>(-1);
        public BaseVariable<bool> disableReflection = new BaseVariable<bool>(false);
        public BaseVariable<float> currentVirtualTime = new BaseVariable<float>();
        /// <summary>
        /// Use Fixed or Dynamic skybox
        /// </summary>
        public BaseVariable<bool> useDynamicSkybox = new BaseVariable<bool>(true);
        public BaseVariable<float> fixedTime = new BaseVariable<float>(0);
        public BaseVariable<int> reflectionResolution = new BaseVariable<int>(256);
        public BaseVariable<AvatarMaterialProfile> avatarMatProfile = new BaseVariable<AvatarMaterialProfile>(AvatarMaterialProfile.InWorld);
    }
}