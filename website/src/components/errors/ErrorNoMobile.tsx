import React from "react";

import "./errors.css";
import errorImage from "../../images/errors/error-robotmobile.png";

export const ErrorNoMobile: React.FC = () => (
  <div id="error-nomobile" className="hidden-error">
    <div
      className="error-background"
      style={{
        zIndex: 10,
        width: "100%",
        height: "100%",
        backgroundColor: "#0d0c0f",
      }}
    />
    <div className="iphone8916" style={{ zIndex: 11 }}>
      <div
        className="div1"
        style={{
          width: "375px",
          height: "100%",
          position: "relative",
          margin: "auto",
        }}
      >
        <div className="theclientisonlya">
          The client is only available on desktop right now.
        </div>
        <img alt="" className="robotmobilebrowsererror" src={errorImage} />
        <div className="cta">
          <div
            className="signup"
            onClick={() => {
              window.location.href = "https://decentraland.org";
            }}
          >
            Learn more about decentraland
          </div>
        </div>
      </div>
    </div>
  </div>
);
