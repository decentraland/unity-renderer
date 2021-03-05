import React from "react";
import { ProviderType } from "decentraland-connect";
import MetamaskLogo from "../../../images/login/metamask.svg";
import DapperLogo from "../../../images/login/dapper.png";
import SamsungBlockchainWalletLogo from "../../../images/login/samsung-blockchain-wallet.svg";
import FortmaticLogo from "../../../images/login/fortmatic.svg";
import WalletConnect from "../../../images/login/wallet-connect.svg";
import "./WalletButton.css";

export enum WalletButtonLogo {
  METAMASK = "Metamask",
  DAPPER = "Dapper",
  FORTMATIC = "Fortmatic",
  WALLET_CONNECT = "WalletConnect",
  SAMSUNG_BLOCKCHAIN_WALLET = "SamsungBlockChainWallet",
}

const options = {
  title: {
    [WalletButtonLogo.METAMASK]: 'Metamask',
    [WalletButtonLogo.DAPPER]: 'Dapper',
    [WalletButtonLogo.FORTMATIC]: 'Fortmatic',
    [WalletButtonLogo.WALLET_CONNECT]: 'Wallet Connect',
    [WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET]: 'Samsung Wallet'
  },

  img: {
    [WalletButtonLogo.METAMASK]: MetamaskLogo,
    [WalletButtonLogo.DAPPER]: DapperLogo,
    [WalletButtonLogo.FORTMATIC]: FortmaticLogo,
    [WalletButtonLogo.WALLET_CONNECT]: WalletConnect,
    [WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET]: SamsungBlockchainWalletLogo
  },

  target: {
    [WalletButtonLogo.METAMASK]: 'https://metamask.io/',
    [WalletButtonLogo.DAPPER]: 'https://www.meetdapper.com/',
    [WalletButtonLogo.FORTMATIC]: 'https://fortmatic.com/',
    [WalletButtonLogo.WALLET_CONNECT]: 'https://walletconnect.org/',
    [WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET]: 'https://www.samsung.com/global/galaxy/apps/samsung-blockchain/'
  },

  provider: {
    [WalletButtonLogo.METAMASK]: ProviderType.INJECTED,
    [WalletButtonLogo.DAPPER]: ProviderType.INJECTED,
    [WalletButtonLogo.FORTMATIC]: ProviderType.FORTMATIC,
    [WalletButtonLogo.WALLET_CONNECT]: ProviderType.WALLET_CONNECT,
    [WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET]: ProviderType.INJECTED
  },

  description: {
    [WalletButtonLogo.METAMASK]: "Using your browser account",
    [WalletButtonLogo.DAPPER]: "Using your browser account",
    [WalletButtonLogo.FORTMATIC]: "Using your email account",
    [WalletButtonLogo.WALLET_CONNECT]: "Using your mobile account",
    [WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET]: "Using your mobile account"
  }
}

export interface WalletButtonProps {
  type: WalletButtonLogo;
  active?: boolean;
  onClick: (providerType: ProviderType) => void;
}

export const WalletButton: React.FC<WalletButtonProps> = ({
  type,
  active,
  onClick,
}) => {

  function handleClick(event: React.MouseEvent<HTMLAnchorElement>) {
    if (active !== false) {
      event.preventDefault();
      if (onClick) {
        onClick(options.provider[type]);
      }
    }
  }

  return (
    <a
      className={`walletButton ${active ? 'active' : 'inactive'}`}
      href={options.target[type]}
      onClick={handleClick}
      target="_blank"
      rel="noopener noreferrer"
    >
      <div className="walletImage">
        <img alt={type} src={options.img[type]} className={type} />
      </div>
      <div className="walletTitle">
        <h3>{options.title[type]}</h3>
      </div>
      <div className="walletDescription">
        <p>{options.description[type]}</p>
      </div>
    </a>
  );
};
