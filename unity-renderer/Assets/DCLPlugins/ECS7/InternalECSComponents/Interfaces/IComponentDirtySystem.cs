
    using System;

    public interface IComponentDirtySystem
    {
        public event Action MarkComponentsAsDirty;
        public event Action RemoveComponentsAsDirty;
    }
