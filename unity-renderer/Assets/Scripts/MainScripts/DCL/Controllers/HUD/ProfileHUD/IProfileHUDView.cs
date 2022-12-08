
using System;

public interface IProfileHUDView
{
    event EventHandler ClaimNamePressed;
    event EventHandler SignedUp;
    event EventHandler LogedOut;

    event EventHandler WalletTermsAndServicesPressed;
    event EventHandler WalletPrivacyPolicyPressed;
    event EventHandler NonWalletTermsAndServicesPressed;
    event EventHandler NonWalletPrivacyPolicyPressed;

    protected void CopyAdress();
}
