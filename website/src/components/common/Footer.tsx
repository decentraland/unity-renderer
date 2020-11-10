import React from "react";
import "./Footer.css";
import discord from "../../images/footer/Discord.png";
import reddit from "../../images/footer/Reddit.png";
import github from "../../images/footer/Git.png";
import twitter from "../../images/footer/Twitter.png";

export const Footer: React.FC = () => (
  <footer className="footer-bar">
    <div className="footer-bar-content">
      <a href="https://discord.gg/k5ydeZp" target="about:blank">
        <img alt="" src={discord} />
      </a>
      <a href="https://www.reddit.com/r/decentraland/" target="about:blank">
        <img alt="" src={reddit} />
      </a>
      <a href="http://github.com/decentraland" target="about:blank">
        <img alt="" src={github} />
      </a>
      <a href="https://twitter.com/decentraland" target="about:blank">
        <img alt="" src={twitter} />
      </a>
      <span className="footer-text">© 2020 Decentraland</span>
    </div>
  </footer>
);
