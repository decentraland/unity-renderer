using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    internal class BaseVariableBinding<T> : IBinding
    {
        private readonly BaseVariable<T> baseVariable;
        private readonly INotifyValueChanged<T> element;

        public BaseVariableBinding(BaseVariable<T> baseVariable, INotifyValueChanged<T> element)
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
