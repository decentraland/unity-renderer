using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController : IHUD
    {
        internal IAvatarModifierAreaFeedbackView view;
        private BaseRefCounter<AvatarAreaWarningID> avatarModifiersWarnings;
        
        public AvatarModifierAreaFeedbackController(BaseRefCounter<AvatarAreaWarningID> avatarAreaWarnings, IAvatarModifierAreaFeedbackView view)
        {
            this.view = view;
            view.SetUp(avatarAreaWarnings);
        }

        public void Dispose()
        {
            view.Dispose();
        }
   
        public void SetVisibility(bool visible) { }
    }
}

