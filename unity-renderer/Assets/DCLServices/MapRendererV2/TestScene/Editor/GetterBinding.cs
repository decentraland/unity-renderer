using System;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    internal class GetterBinding<T> : IBinding
    {
        private readonly INotifyValueChanged<T> visualElement;
        private readonly Func<T> getter;

        public GetterBinding(INotifyValueChanged<T> visualElement, Func<T> getter)
        {
            this.visualElement = visualElement;
            this.getter = getter;
        }

        public void PreUpdate()
        {

        }

        public void Update()
        {
            visualElement.SetValueWithoutNotify(getter());
        }

        public void Release()
        {
        }
    }
}
