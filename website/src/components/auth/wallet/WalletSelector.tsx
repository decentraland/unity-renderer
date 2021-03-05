import React, { useMemo } from "react";
import { ProviderType } from "decentraland-connect";
import { isCucumberProvider, isDapperProvider } from "decentraland-dapps/dist/lib/eth";
import { Modal } from "../../common/Modal";
import { WalletButton, WalletButtonLogo } from "./WalletButton";
import { Spinner } from "../../common/Spinner";
import "./WalletSelector.css";

export interface WalletSelectorProps {
  open: boolean;
  loading: boolean;
  availableProviders: ProviderType[];
  onLogin: (provider: ProviderType | null) => void;
  onCancel: () => void;
}

export const WalletSelector: React.FC<WalletSelectorProps> = ({
  open,
  loading,
  availableProviders,
  onLogin,
  onCancel,
}) => {
  const hasWallet = availableProviders.includes(ProviderType.INJECTED)
  function handleLogin(provider: ProviderType) {
    if (provider === ProviderType.INJECTED && !hasWallet) {
      return;
    }
    if (onLogin) {
      onLogin(provider);
    }
  }

  const wallets = useMemo(() => {
    const result: WalletButtonLogo[] = []
    if (hasWallet) {
      if (isCucumberProvider()) {
        result.push(WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET)
      } else if (isDapperProvider()) {
        result.push(WalletButtonLogo.DAPPER)
      }
    }

    if (result.length === 0) {
      result.push(WalletButtonLogo.METAMASK)
    }

    if (availableProviders.includes(ProviderType.FORTMATIC)) {
      result.push(WalletButtonLogo.FORTMATIC)
    }

    if (availableProviders.includes(ProviderType.WALLET_CONNECT)) {
      result.push(WalletButtonLogo.WALLET_CONNECT)
    }

    return result
  }, [availableProviders, hasWallet])

  function isActive(wallet: WalletButtonLogo) {
    switch (wallet) {
      case WalletButtonLogo.METAMASK:
      case WalletButtonLogo.DAPPER:
      case WalletButtonLogo.SAMSUNG_BLOCKCHAIN_WALLET:
        return hasWallet
      default:
        return true
    }
  }

  return open ? (
    <Modal
      className="walletSelectorPopup"
      onClose={onCancel}
      withFlatBackground
      withOverlay
    >
      <div className="walletSelector">
        <h2 className="walletSelectorTitle">Sign In or Create an Account</h2>
        <div className="walletButtonContainer">
          {loading && <Spinner />}
          {!loading && wallets.map(wallet => (
            <WalletButton
              key={wallet}
              type={wallet}
              active={isActive(wallet)}
              onClick={handleLogin}
            />
          ))}
        </div>
      </div>
    </Modal>
  ) : null;
};
