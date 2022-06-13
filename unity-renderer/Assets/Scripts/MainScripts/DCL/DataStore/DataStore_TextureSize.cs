namespace DCL
{
    public class DataStore_TextureSize
    {
        public readonly BaseVariable<int> generalMaxSize = new BaseVariable<int>(512);
        public readonly BaseVariable<int> gltfMaxSize = new BaseVariable<int>(1024);
    }
}