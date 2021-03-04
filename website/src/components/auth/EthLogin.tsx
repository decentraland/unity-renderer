import React, { useState } from "react";
import { ProviderType } from "decentraland-connect/dist/types"
import { WalletSelector } from "./wallet/WalletSelector";
import { LoginHeader } from "./LoginHeader";
import { Spinner } from "../common/Spinner";
import { Avatars } from "../common/Avatars";
import "./EthLogin.css";

export interface EthLoginProps {
  loading: boolean;
  availableProviders: ProviderType[];
  onLogin: (provider: ProviderType | null) => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [showWalletSelector, setShowWalletSelector] = useState(false);
  const hasWallet = !!(props.availableProviders && props.availableProviders.includes(ProviderType.INJECTED))
  const isLoading = props.loading || showWalletSelector;

  function handlePlay() {
    setShowWalletSelector(true);
  }

  function handlePlayAsGuest() {
    if (props.onLogin) {
      props.onLogin(null);
    }
  }

  return (
    <div className="eth-login">
      <LoginHeader />
      <Avatars />
      <div id="eth-login-confirmation-wrapper">
        {isLoading && <Spinner />}
        {!isLoading && (
          <React.Fragment>
            <button className="eth-login-confirm-button" onClick={handlePlay}>
              Play
            </button>
            {!hasWallet && (
              <button
                className="eth-login-guest-button"
                onClick={handlePlayAsGuest}
              >
                Enter as Guest
              </button>
            )}
          </React.Fragment>
        )}
      </div>
      <WalletSelector
        open={showWalletSelector}
        loading={props.loading}
        onLogin={props.onLogin}
        availableProviders={props.availableProviders}
        onCancel={() => setShowWalletSelector(false)}
      />
    </div>
  );
};
