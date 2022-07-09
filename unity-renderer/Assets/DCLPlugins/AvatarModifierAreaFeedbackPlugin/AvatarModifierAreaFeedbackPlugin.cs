using System.Collections;
using System.Collections.Generic;
using DCL.AvatarModifierAreaFeedback;
using UnityEngine;

public class AvatarModifierAreaFeedbackPlugin : IPlugin
{
    
    private AvatarModifierAreaFeedbackController avatarModifierAreaFeedbackController;
    
    public AvatarModifierAreaFeedbackPlugin() { avatarModifierAreaFeedbackController = new AvatarModifierAreaFeedbackController(); }

    public void Dispose()
    {
        avatarModifierAreaFeedbackController.Dispose();
    }
}
