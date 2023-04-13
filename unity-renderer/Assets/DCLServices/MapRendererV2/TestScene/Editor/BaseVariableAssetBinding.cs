using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    internal class BaseVariableAssetBinding<T> : IBinding
    {
        private readonly BaseVariableAsset<T> baseVariable;
        private readonly INotifyValueChanged<T> element;

        public BaseVariableAssetBinding(BaseVariableAsset<T> baseVariable, INotifyValueChanged<T> element)
        {
            this.baseVariable = baseVariable;
            this.element = element;

            element.value = baseVariable.Get();
            baseVariable.OnChange += OnVariableChange;
        }

        private void OnVariableChange(T current, T previous)
        {
            element.value = current;
        }

        public void PreUpdate()
        {

        }

        public void Update()
        {
            baseVariable.Set(element.value);
        }

        public void Release()
        {
            baseVariable.OnChange -= OnVariableChange;
        }
    }
}
