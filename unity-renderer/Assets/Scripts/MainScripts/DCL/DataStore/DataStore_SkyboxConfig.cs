namespace DCL
{
    public class DataStore_SkyboxConfig
    {
        public BaseVariable<bool> objectUpdated = new BaseVariable<bool>(false);
        public BaseVariable<bool> useProceduralSkybox = new BaseVariable<bool>(false);
        public BaseVariable<string> configToLoad = new BaseVariable<string>("Generic Skybox");
        public BaseVariable<float> lifecycleDuration = new BaseVariable<float>(60);
        public BaseVariable<float> jumpToTime = new BaseVariable<float>(-1);
        public BaseVariable<float> updateReflectionTime = new BaseVariable<float>(-1);
        public BaseVariable<bool> disableReflection = new BaseVariable<bool>(false);
        public BaseVariable<float> currentVirtualTime = new BaseVariable<float>();
        public BaseVariable<bool> useDynamicSkybox = new BaseVariable<bool>(true);
        public BaseVariable<float> fixedTime = new BaseVariable<float>(0);
    }
}