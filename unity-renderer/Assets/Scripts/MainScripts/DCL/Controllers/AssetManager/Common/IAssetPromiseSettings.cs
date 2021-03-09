namespace DCL
{
    public interface IAssetPromiseSettings<T>
    {
        void ApplyBeforeLoad(T target);
        void ApplyAfterLoad(T target);
    }
}
