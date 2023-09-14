using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.AvatarModifierAreaFeedback;
using UnityEngine;

public class AvatarModifierAreaFeedbackPlugin : IPlugin
{

    private AvatarModifierAreaFeedbackController avatarModifierAreaFeedbackController;

    public AvatarModifierAreaFeedbackPlugin()
    {
        avatarModifierAreaFeedbackController = new AvatarModifierAreaFeedbackController(DataStore.i.HUDs.avatarAreaWarnings, AvatarModifierAreaFeedbackView.Create(), DataStore.i.common);
    }

    public void Dispose()
    {
        avatarModifierAreaFeedbackController.Dispose();
    }
}
