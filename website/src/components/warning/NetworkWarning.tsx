import React from "react";
import "./NetworkWarning.css";

export interface NetworkWarningProps {
  onClose: () => void;
}

export const NetworkWarning: React.FC<NetworkWarningProps> = ({ onClose }) => (
  <div id="network-warning" className="network-warning-bar">
    <div className="network-warning-title">
      <strong>Warning:</strong> you’re running on the Ethereum Mainnet.
    </div>
    <div className="network-warning-description">
      Blockchain transactions in this network have a cost and real consequences.
      We recommend you use the <strong>Ropsten</strong> test network instead.
    </div>
    <button className="network-warning-button" onClick={onClose}>
      ⨯
    </button>
  </div>
);
