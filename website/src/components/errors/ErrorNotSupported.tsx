import React from "react";

import "./errors.css";
import errorImage from "../../images/errors/error-robotmobile.png";

export const ErrorNotSupported: React.FC = () => (
  <div id="error-notsupported" className="error-container">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Error</div>
        <div className="communicationslink">
          Your browser or device is not supported
        </div>
        <div className="givesomedetailof">
          The Explorer only works on Chrome or Firefox for Windows, Linux and
          macOS.
        </div>
      </div>
      <div className="errorimage col">
        <img alt="" className="error-image" src={errorImage} />
      </div>
    </div>
  </div>
);
