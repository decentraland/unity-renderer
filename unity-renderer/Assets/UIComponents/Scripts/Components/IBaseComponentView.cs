namespace UIComponents.Scripts.Components
{
    public interface IBaseComponentView<in TModel> : IBaseComponentView
    {
        void SetModel(TModel newModel);
    }
}
