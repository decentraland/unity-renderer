using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainScripts.DCL.Controllers.HUD.Profile
{
    public class ProfileHUDViewDesktop_V2 : ProfileHUDViewV2
    {
        [SerializeField]
        protected internal List<Button> exitButtons;

        protected internal Button getButtonSignUp => buttonSignUp;
    }
}
