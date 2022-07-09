using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController : IHUD
    {
        internal AvatarModifierAreaFeedbackView view;
        private BaseVariable<int> InAvatarModifierAreaForSelfCounter => DataStore.i.HUDs.inAvatarModifierAreaForSelfCounter;
        private BaseCollection<string> InAvatarModifierAreaWarningDescription => DataStore.i.HUDs.inAvatarModifierAreaWarningDescription;
        
        public AvatarModifierAreaFeedbackController()
        {
            view = AvatarModifierAreaFeedbackView.Create();

            InAvatarModifierAreaForSelfCounter.OnChange += OnAvatarModifierValueChanged;
            InAvatarModifierAreaWarningDescription.OnAdded += OnAvatarModifierWarningAdded;
            InAvatarModifierAreaWarningDescription.OnSet += OnAvatarModifierWarningReset;
        }
    
        private void OnAvatarModifierValueChanged(int current, int previous)
        {
            if (current == 0)
            {
                view.Hide();
            }
            if (current >= 1 && !view.isVisible)
            {
                view.Show();
            }
        }
    
        private void OnAvatarModifierWarningAdded(string newWarning)
        {
            view.AddNewWarningMessage(newWarning);
        }
    
        private void OnAvatarModifierWarningReset(IEnumerable<string> resetList)
        {
            view.ResetWarningMessage();
        }

        public void Dispose()
        {
            InAvatarModifierAreaForSelfCounter.OnChange -= OnAvatarModifierValueChanged;
            InAvatarModifierAreaWarningDescription.OnAdded -= OnAvatarModifierWarningAdded;
            InAvatarModifierAreaWarningDescription.OnSet -= OnAvatarModifierWarningReset;
        }
        public void SetVisibility(bool visible)
        {
        
        }
    }
}

