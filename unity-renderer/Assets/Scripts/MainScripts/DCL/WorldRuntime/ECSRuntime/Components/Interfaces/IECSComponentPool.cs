namespace DCL.ECSRuntime
{
    public interface IECSComponentPool<ModelType>
    {
        ModelType Get();
        void Release(ModelType item);
    }
}
