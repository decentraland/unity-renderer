namespace DCL.ECSRuntime
{
    public class ECSValueTypeComponentPool<ModelType> : IECSComponentPool<ModelType> where ModelType : struct
    {
        public ModelType Get()
        {
            return default;
        }

        public void Release(ModelType item) { }
    }
}
