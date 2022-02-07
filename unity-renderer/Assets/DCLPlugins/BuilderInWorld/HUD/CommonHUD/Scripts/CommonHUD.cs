using UnityEngine;

namespace DCL.Builder
{
    public class CommonHUD : ICommonHUD
    {
        private const string GENERIC_POP_UP_VIEW_PREFAB_PATH = "GenericPopUp/GenericPopUp";
        private IGenericPopUp genericPopUp;

        public IGenericPopUp GetPopUp() => genericPopUp;

        public void Initialize(IContext context)
        {
            genericPopUp = GameObject.Instantiate(Resources.Load<GenericPopUpView>(GENERIC_POP_UP_VIEW_PREFAB_PATH));
        }

        public void Dispose() { genericPopUp.Dispose(); }
    }
}