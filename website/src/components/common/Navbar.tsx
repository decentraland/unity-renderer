import React from "react";
import { Logo } from "./Logo";
import { Isologotipo } from "./Isologotipo";
import { Discord } from "./Icon.tsx/Discord";
import "./Navbar.css";

export type NavbarProps = {
  full?: boolean,
  onClickLogo?: (event: React.MouseEvent<SVGElement>) => void
}

export const Navbar = (props: NavbarProps) => (
  <nav className="nav-bar">
    {!props.full && <Isologotipo onClick={props.onClickLogo} />}
    {!!props.full && <a href="https://decentraland.org/" target="_blank" rel="noopener noreferrer">
      <Logo onClick={props.onClickLogo} />
    </a>}
    {!!props.full && <div className="nav-bar-content">
      <div className="nav-text">
        <span>Need support?</span>
      </div>
      <a
        className="nav-discord"
        href="https://discord.gg/k5ydeZp"
        target="about:blank"
      >
        <Discord />
        <div>Join our discord</div>
      </a>
    </div>}
  </nav>
);
