import React from "react";
import "./Footer.css";
import avatars1x from "../../images/login/avatars.png";
import avatars2x from "../../images/login/avatars@2x.png";
import avatars3x from "../../images/login/avatars@3x.png";

export const Avatars: React.FC = () => (
  <div className="eth-avatars">
    <img
      src={avatars1x}
      srcSet={`${avatars1x} 1x, ${avatars2x} 2x, ${avatars3x} 3x`}
      alt="Avatars"
      width="480"
      height="600"
    />
  </div>
);
