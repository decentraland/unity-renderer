using UnityEngine;

namespace DCL.MyAccount
{
    public interface IMyAccountHUDComponentView
    {
        IMyProfileComponentView CurrentMyProfileView { get; }

        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
