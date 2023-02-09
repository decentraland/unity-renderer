namespace DCLServices.MapRendererV2.Culling
{
    public interface IMapCullingListener<in T> where T : IMapPositionProvider
    {
        void OnMapObjectBecameVisible(T obj);

        void OnMapObjectCulled(T obj);
    }
}
