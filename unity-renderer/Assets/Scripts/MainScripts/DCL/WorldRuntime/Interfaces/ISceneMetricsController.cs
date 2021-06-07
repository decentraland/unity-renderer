namespace DCL
{
    public interface ISceneMetricsController
    {
        SceneMetricsModel GetLimits();
        SceneMetricsModel GetModel();

        void Enable();

        void Disable();

        void SendEvent();

        bool IsInsideTheLimits();
    }

    [System.Serializable]
    public class SceneMetricsModel
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entities;
        public float sceneHeight;

        public SceneMetricsModel Clone() { return (SceneMetricsModel) MemberwiseClone(); }
    }
}