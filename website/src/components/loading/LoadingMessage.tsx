import React from "react";
import "./LoadingMessage.css";

export interface LoadingMessageProps {
  image?: string;
  message?: string;
  subMessage: string;
  showWalletPrompt: boolean;
}

export const LoadingMessage: React.FC<LoadingMessageProps> = (props) => (
  <div id="load-messages-wrapper">
    <div className="load-images-wrapper">
      {props.image && <img id="load-images" alt="" src={props.image} />}
    </div>
    <div id="load-messages">{props.message}</div>
    <div id="subtext-messages-container">
      {props.showWalletPrompt && (
        <div id="check-wallet-prompt">
          Please check your wallet (i.e MetaMask) and look for the Signature
          Request.
        </div>
      )}
      <div id="subtext-messages">
        {props.subMessage.split("\n").map((item) => (
          <React.Fragment>
            {item}
            <br />
          </React.Fragment>
        ))}
      </div>
    </div>
  </div>
);
