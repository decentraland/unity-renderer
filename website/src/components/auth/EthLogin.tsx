import React, { useState } from "react";
import { WalletSelector } from "./wallet/WalletSelector";
import { LoginHeader } from "./LoginHeader";
import "./EthLogin.css";
import { Spinner } from "../common/Spinner";
import { Avatars } from "../common/Avatars";

export interface EthLoginProps {
  loading: boolean;
  provider: string | null | undefined;
  showWallet?: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [wallet, setWallet] = useState(props.showWallet || false);
  const onClick = () => {
    if (props.provider) {
      return props.onLogin(props.provider);
    }
    setWallet(true);
  };
  const walletLoading = props.loading && wallet;
  const showSignIn = !props.loading || walletLoading;
  return (
    <div className="eth-login">
      <LoginHeader />
      <Avatars />
      <div id="eth-login-confirmation-wrapper">
        {!showSignIn && <Spinner />}
        {showSignIn && (
          <button className="eth-login-confirm-button" onClick={onClick}>
            Sign In
          </button>
        )}
        {showSignIn && (
          <button className="eth-login-guest-button" onClick={props.onGuest}>
            Play as Guest
          </button>
        )}
      </div>
      <WalletSelector
        show={wallet}
        loading={walletLoading}
        onClick={props.onLogin}
        onCancel={() => setWallet(false)}
      />
    </div>
  );
};
