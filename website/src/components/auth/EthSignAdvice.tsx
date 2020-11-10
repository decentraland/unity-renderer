import React from "react";

import "./EthLogin.css";

export const EthSignAdvice: React.FC = () => (
  <React.Fragment>
    <div className="eth-login-description">
      Please, follow the instructions provided by your Ethereum wallet provider
      to complete login.
    </div>
    <div className="eth-login-description" style={{ marginTop: "10px" }}>
      To proceed, confirm <strong>signing</strong> the following message in your
      wallet extension.
    </div>
  </React.Fragment>
);
