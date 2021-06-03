namespace DCL.Interface
{
    public static class SceneMetricsModelExtensions
    {
        public static WebInterface.MetricsModel ToMetricsModel(this SceneMetricsModel model)
        {
            return new WebInterface.MetricsModel()
            {
                meshes = model.meshes,
                bodies = model.bodies,
                materials = model.materials,
                textures = model.textures,
                triangles = model.triangles,
                entities = model.entities
            };
        }
    }
}