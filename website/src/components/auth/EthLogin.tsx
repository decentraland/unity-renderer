import React, { useState } from "react";
import { WalletSelector } from "./wallet/WalletSelector";
import { LoginHeader } from "./LoginHeader";
import { Spinner } from "../common/Spinner";
import { Avatars } from "../common/Avatars";
import "./EthLogin.css";

export interface EthLoginProps {
  loading: boolean;
  provider: string | null | undefined;
  showWallet?: boolean;
  hasWallet?: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [showWallet, setShowWallet] = useState(props.showWallet || false);
  const isLoading = props.loading || showWallet;

  function handlePlay() {
    if (props.provider) {
      return props.onLogin(props.provider);
    }
    setShowWallet(true);
  }

  function handlePlayAsGuest() {
    if (props.onGuest) {
      props.onGuest();
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
            {!props.hasWallet && (
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
        show={showWallet}
        hasWallet={!!props.hasWallet}
        loading={props.loading}
        onClick={props.onLogin}
        onCancel={() => setShowWallet(false)}
      />
    </div>
  );
};
