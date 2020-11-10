import React from "react";

import "./WalletButton.css";

export interface WalletButtonProps {
  logo: string;
  title: string;
  description: string;
  onClick: any;
}

export const WalletButton: React.FC<WalletButtonProps> = ({
  logo,
  title,
  description,
  onClick,
}) => (
  <div className="walletButton" onClick={onClick}>
    <div className="walletImage">
      <img alt={title} src={logo} />
      <span>{title}</span>
    </div>
    <p>{description}</p>
  </div>
);
