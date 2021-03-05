import React from "react";
import "./errors.css";
import errorImage from "../../images/errors/error-robotdead.png";

export const ErrorComms: React.FC = () => (
  <div id="error-comms" className="error-container">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Error</div>
        <div className="communicationslink">
          A communication link could not be
          <br />
          established with other peers
        </div>
        <div className="givesomedetailof">
          This might be because you are behind a restrictive network firewall,
          or a temporary problem with our coordinator server. <br />
          <br />
          Please try again later, or reach out to us at{" "}
          <a href="mailto:hello@decentraland.org">
            hello@decentraland.org
          </a>
          .<br />
          <br />
          Thank you for helping us improve!
        </div>
        <div className="cta">
          <button
            className="retry"
            onClick={() => {
              window.location.reload();
            }}
          >
            Try again
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
