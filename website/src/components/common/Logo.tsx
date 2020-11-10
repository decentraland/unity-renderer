import React from "react";
import dclIcon from "../../images/logo.svg";
import dclLogo from "../../images/logo-dcl.png";
import "./Logo.css";

export interface LogoProps {
  icon?: boolean;
}

export const Logo: React.FC<LogoProps> = ({ icon = false }) => (
  <img alt="Decentraland" className="dcl-logo" src={icon ? dclIcon : dclLogo} />
);
