import React from "react";
import { Modal } from "../../common/Modal";
import { WalletButton } from "./WalletButton";
import { Spinner } from "../../common/Spinner";

import MetamaskLogo from "../../../images/metamask.svg";
import FortmaticLogo from "../../../images/fortmatic.svg";
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
  return show ? (
    <Modal handleClose={onCancel}>
      <div className="walletSelector">
        <h2 className="walletSelectorTitle">Sign In or Create an Account</h2>
        <div className="walletButtonContainer">
          {loading && <Spinner />}
          {!loading && (
            <React.Fragment>
              {metamask && (
                <WalletButton
                  title="Metamask"
                  logo={MetamaskLogo}
                  description="Using a browser extension"
                  onClick={() => onClick("Metamask")}
                />
              )}
              {!metamask && (
                <a
                  href="https://metamask.io/"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="linkMetamask"
                >
                  <WalletButton
                    title="Metamask"
                    logo={MetamaskLogo}
                    description="Using a browser extension"
                    onClick={null}
                  />
                </a>
              )}
              <WalletButton
                title="Fortmatic"
                logo={FortmaticLogo}
                description="Using your email account"
                onClick={() => onClick("Fortmatic")}
              />
            </React.Fragment>
          )}
        </div>
      </div>
    </Modal>
  ) : null;
};
