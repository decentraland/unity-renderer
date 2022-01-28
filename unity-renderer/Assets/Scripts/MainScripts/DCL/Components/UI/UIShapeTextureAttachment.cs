using System;
using UnityEngine;

namespace DCL.Components
{
    public class UIShapeTextureAttachment : ITextureAttachment
    {
        UIShape shape;

        public event Action<ITextureAttachment> OnUpdate;
        public event Action<ITextureAttachment> OnDetach;
        public event Action<ITextureAttachment> OnAttach;
        float ITextureAttachment.GetClosestDistanceSqr(Vector3 fromPosition) { return 0; }

        bool ITextureAttachment.IsVisible()
        {
            if (!((UIShape.Model) shape.GetModel()).visible)
                return false;
            return IsParentVisible(shape);
        }

        public string GetId()
        {
            return shape.id;
        }

        bool IsParentVisible(UIShape shape)
        {
            UIShape parent = shape.parentUIComponent;
            if (parent == null)
                return true;
            if (parent.referencesContainer.canvasGroup.alpha == 0)
            {
                return false;
            }

            return IsParentVisible(parent);
        }

        public UIShapeTextureAttachment(UIShape shape)
        {
            this.shape = shape;
        }

        public void Dispose()
        {
        }
    }
}