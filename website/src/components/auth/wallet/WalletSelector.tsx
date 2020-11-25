import React from "react";
import { Modal } from "../../common/Modal";
import { WalletButton, WalletButtonLogo } from "./WalletButton";
import { Spinner } from "../../common/Spinner";
import "./WalletSelector.css";

export interface WalletSelectorProps {
  show: boolean;
  loading: boolean;
  metamask: boolean;
  onClick: (provider: string) => void;
  onCancel: () => void;
}

export const WalletSelector: React.FC<WalletSelectorProps> = ({
  show,
  metamask,
  loading,
  onClick,
  onCancel,
}) => {
  function handleClick(_: React.MouseEvent, provider: WalletButtonLogo) {
    if (provider === "Metamask" && !metamask) {
      return;
    }
    if (onClick) {
      onClick(provider);
    }
  }

  return show ? (
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
          {!loading && (
            <React.Fragment>
              <WalletButton
                logo="Metamask"
                active={!!metamask}
                href="https://metamask.io/"
                onClick={handleClick}
              />
            </React.Fragment>
          )}
        </div>
      </div>
    </Modal>
  ) : null;
};
