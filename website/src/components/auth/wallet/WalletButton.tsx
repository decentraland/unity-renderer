import React, { useMemo } from "react";
import { AuthType } from "../../../utils";
import MetamaskLogo from "../../../images/login/metamask.svg";
import FortmaticLogo from "../../../images/login/fortmatic.svg";
import "./WalletButton.css";

export enum WalletButtonLogo {
  METAMASK = "Metamask",
  FORTMATIC = "Fortmatic",
}

export interface WalletButtonProps {
  logo: WalletButtonLogo;
  active?: boolean;
  href?: string;
  onClick: (
    event: React.MouseEvent<HTMLAnchorElement>,
    authType: AuthType
  ) => void;
}

export const WalletButton: React.FC<WalletButtonProps> = ({
  logo,
  href,
  active,
  onClick,
}) => {
  const provider =
    logo === WalletButtonLogo.METAMASK ? AuthType.INJECTED : AuthType.FORTMATIC;
  const src = useMemo(() => {
    switch (logo) {
      case "Fortmatic":
        return <img alt={logo} src={FortmaticLogo} className="fortmatic" />;
      case "Metamask":
      default:
        return <img alt={logo} src={MetamaskLogo} className="metamask" />;
    }
  }, [logo]);

  const title = useMemo(() => {
    switch (logo) {
      case "Fortmatic":
        return "Fortmatic";
      case "Metamask":
      default:
        return "Metamask";
    }
  }, [logo]);

  const description = useMemo(() => {
    switch (logo) {
      case "Fortmatic":
        return "Using your email account";
      case "Metamask":
      default:
        return "Using a browser extension";
    }
  }, [logo]);

  function handleClick(event: React.MouseEvent<HTMLAnchorElement>) {
    if (active !== false) {
      event.preventDefault();
    }

    if (onClick) {
      onClick(event, provider);
    }
  }

  return (
    <a
      className="walletButton"
      href={href || "/"}
      onClick={handleClick}
      target={href && "_blank"}
      rel="noopener noreferrer"
    >
      <div className="walletImage">{src}</div>
      <div className="walletTitle">
        <h3>{title}</h3>
      </div>
      <div className="walletDescription">
        <p>{description}</p>
      </div>
    </a>
  );
};
