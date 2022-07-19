using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController : IHUD
    {
        internal IAvatarModifierAreaFeedbackView view;

        private BaseStack<List<IAvatarModifier>> avatarModifierStack;
        public AvatarModifierAreaFeedbackController(BaseStack<List<IAvatarModifier>> stack, IAvatarModifierAreaFeedbackView view)
        {
            this.view = view;

            this.avatarModifierStack = stack;
            
            avatarModifierStack.OnAdded += OnAvatarModifierValueChanged;
            avatarModifierStack.OnRemoved += OnAvatarModifierWarningReset;
        }

        private void OnAvatarModifierValueChanged(List<IAvatarModifier> avatarModifiersActivated)
        {
            view.SetWarningMessage(avatarModifiersActivated);
            view.SetVisibility(true);
        }
        
        private void OnAvatarModifierWarningReset(List<IAvatarModifier> obj)
        {
            view.ResetWarningMessage();
            view.SetVisibility(false);
        }

        public void Dispose()
        {
            avatarModifierStack.OnAdded -= OnAvatarModifierValueChanged;
            avatarModifierStack.OnRemoved -= OnAvatarModifierWarningReset;
            view.Dispose();
        }
        public void SetVisibility(bool visible)
        {
        
        }
    }
}

