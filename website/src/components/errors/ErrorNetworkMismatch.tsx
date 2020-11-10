import React from "react";

import "./errors.css";
import errorImage from "../../images/errors/robotsmiling.png";

export interface ErrorNetworkMismatchProps {
  details: { tld: string; web3Net: string; tldNet: string } | null;
}

export const ErrorNetworkMismatch: React.FC<ErrorNetworkMismatchProps> = (
  props: ErrorNetworkMismatchProps
) => {
  const { tld = "zone", web3Net = "mainet", tldNet = "ropsten" } =
    props.details || {};
  return (
    <div id="error-networkmismatch" className="error-container">
      <div className="error-background" />
      <div className="errormessage">
        <div className="errortext col">
          <div className="communicationslink">
            A network mismatch was detected
          </div>
          <div className="givesomedetailof">
            We detected that you are entering the{" "}
            <strong id="tld">{tld || "zone"}</strong> domain with your Ethereum
            wallet set to <strong id="web3Net">{web3Net || "mainnet"}</strong>.
          </div>
          <div className="givesomedetailof">
            To continue, please change the Ethereum network in your wallet to{" "}
            <strong id="web3NetGoal">{tldNet || "ropsten"}</strong> and click
            "Reload".
          </div>
          <div className="cta">
            <button
              className="retry"
              onClick={() => {
                window.location.reload();
              }}
            >
              Reload
            </button>
          </div>
        </div>
        <div className="errorimage col">
          <div className="imagewrapper">
            <img alt="" className="error-image" src={errorImage} />
          </div>
        </div>
      </div>
    </div>
  );
};
