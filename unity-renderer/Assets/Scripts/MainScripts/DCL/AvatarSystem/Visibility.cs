using UnityEngine;

namespace AvatarSystem
{
    public class Visibility : IVisibility
    {
        public bool composedVisibility { get; private set; }
        private readonly GameObject container;

        public Visibility(GameObject container) { this.container = container; }
        public void SetVisible(bool visible)
        {
            composedVisibility = visible;
            container.SetActive(visible);
        }
    }
}