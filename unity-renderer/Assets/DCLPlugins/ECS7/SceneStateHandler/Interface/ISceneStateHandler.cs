namespace DCL.ECS7
{
    public interface ISceneStateHandler
    {
        public uint GetSceneTick(int sceneNumber);
        public void IncreaseSceneTick(int sceneNumber);
        public bool IsSceneGltfLoadingFinished(int sceneNumber);
    }
}
