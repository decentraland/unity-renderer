using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController : IHUD
    {
        internal IAvatarModifierAreaFeedbackView view;

        private BaseCollection<string> avatarModifiersWarnings;
        public AvatarModifierAreaFeedbackController(BaseCollection<string> avatarModifiersWarnings, IAvatarModifierAreaFeedbackView view)
        {
            this.view = view;

            this.avatarModifiersWarnings = avatarModifiersWarnings;
            
            avatarModifiersWarnings.OnAdded += OnAvatarModifierAdded;
            avatarModifiersWarnings.OnRemoved += OnAvatarModifierRemoved;
        }
        private void OnAvatarModifierAdded(string obj)
        {
            view.SetWarningMessage(avatarModifiersWarnings.Get());
            view.SetVisibility(true);
        }
        
        private void OnAvatarModifierRemoved(string obj)
        {
            if (avatarModifiersWarnings.Count() == 0)
            {
                view.SetVisibility(false);
            }
        }
        
        public void Dispose()
        {
            avatarModifiersWarnings.OnAdded -= OnAvatarModifierAdded;
            avatarModifiersWarnings.OnRemoved -= OnAvatarModifierRemoved;
            view.Dispose();
        }
   
        public void SetVisibility(bool visible)
        {
        
        }
    }
}

