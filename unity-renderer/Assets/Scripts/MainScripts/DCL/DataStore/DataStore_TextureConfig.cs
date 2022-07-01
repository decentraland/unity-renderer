namespace DCL
{
    public class DataStore_TextureConfig
    {
        public readonly BaseVariable<int> generalMaxSize = new BaseVariable<int>(512);
        public readonly BaseVariable<int> gltfMaxSize = new BaseVariable<int>(1024);
        public readonly BaseVariable<bool> runCompression = new BaseVariable<bool>(false);
    }
}