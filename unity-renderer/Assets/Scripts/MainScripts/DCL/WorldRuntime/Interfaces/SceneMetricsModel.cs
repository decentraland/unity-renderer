namespace DCL
{
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

        public static bool operator >(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            int lhsValue = lhs.meshes + lhs.bodies + lhs.materials + lhs.textures + lhs.triangles + lhs.entities;
            int rhsValue = rhs.meshes + rhs.bodies + rhs.materials + rhs.textures + rhs.triangles + rhs.entities;

            return lhsValue > rhsValue;
        }

        public static bool operator <(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            int lhsValue = lhs.meshes + lhs.bodies + lhs.materials + lhs.textures + lhs.triangles + lhs.entities;
            int rhsValue = rhs.meshes + rhs.bodies + rhs.materials + rhs.textures + rhs.triangles + rhs.entities;

            return lhsValue < rhsValue;
        }

        public static SceneMetricsModel operator -(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            SceneMetricsModel result = lhs.Clone();
            result.bodies -= rhs.bodies;
            result.entities -= rhs.entities;
            result.materials -= rhs.materials;
            result.textures -= rhs.textures;
            result.meshes -= rhs.meshes;
            result.triangles -= rhs.triangles;
            return result;
        }

        public static SceneMetricsModel operator +(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            SceneMetricsModel result = lhs.Clone();
            result.bodies += rhs.bodies;
            result.entities += rhs.entities;
            result.materials += rhs.materials;
            result.textures += rhs.textures;
            result.meshes += rhs.meshes;
            result.triangles += rhs.triangles;
            return result;
        }

        public override string ToString()
        {
            return $"Textures: {textures} - Triangles: {triangles} - Materials: {materials} - Meshes: {meshes} - Bodies: {bodies} - Entities: {entities} - Scene Height: {sceneHeight}";
        }
    }
}