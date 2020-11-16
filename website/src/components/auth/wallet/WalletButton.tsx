import React from "react";

import "./WalletButton.css";

export interface WalletButtonProps {
  logo: string;
  title: string;
  description: string;
  disabled?: boolean;
  onClick: any;
}

export const WalletButton: React.FC<WalletButtonProps> = ({
  logo,
  title,
  description,
  disabled = false,
  onClick,
}) => {
  const handleClick = disabled ? null : onClick;
  const walletClass = "walletButton" + (disabled ? " disabled" : "");
  return (
    <div className={walletClass} onClick={handleClick}>
      <div className="walletImage">
        <img alt={title} src={logo} />
        <span>{title}</span>
      </div>
      <p>{description}</p>
    </div>
  );
};
