import React from "react";
import { Logo } from "./Logo";
import "./Navbar.css";
import imgDiscord from "../../images/discord.png";

export const Navbar: React.FC = () => (
  <nav className="nav-bar">
    <a
      href="https://decentraland.org/"
      target="_blank"
      rel="noopener noreferrer"
    >
      <Logo icon={true} />
    </a>
    <div className="nav-bar-content">
      <div className="nav-text nav-need-support">
        <span>Need support?</span>
      </div>
      <a
        className="nav-discord"
        href="https://dcl.gg/discord"
        target="about:blank"
      >
        <img alt="Discord" className="nav-discord-img" src={imgDiscord} />
        <span className="nav-text nav-discord-text">JOIN OUR DISCORD</span>
      </a>
    </div>
  </nav>
);
