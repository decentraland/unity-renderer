import React from "react";

import "./EthLogin.css";

export interface EthConnectAdviceProps {
  onLogin: any;
}

export const EthConnectAdvice: React.FC<EthConnectAdviceProps> = (props) => (
  <React.Fragment>
    <div className="eth-login-description">
      Please, follow the instructions provided by your Ethereum wallet provider
      to complete login.
      <br />
      To proceed, <strong>login</strong> into your wallet and confirm with{" "}
      <strong>connecting</strong> to your Ethereum wallet extension.
    </div>
    <button className="eth-login-confirm-button" onClick={props.onLogin}>
      <img
        alt=""
        src="images/decentraland-connect/walletIcon.png"
        className="eth-login-wallet-icon"
      />
      Connect wallet
    </button>
  </React.Fragment>
);
