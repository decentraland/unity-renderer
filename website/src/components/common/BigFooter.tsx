import React from "react";
import { Container } from "./Container";
import { Reddit } from "./Icon.tsx/Reddit";
import { Github } from "./Icon.tsx/Github";
import { Twitter } from "./Icon.tsx/Twitter";
import { Discord } from "./Icon.tsx/Discord";

import "./BigFooter.css";

const year = Math.max(new Date().getFullYear(), 2020);

export const BigFooter = () => (
  <footer className="big-footer">
    <Container>
      <div>
        <div>
          <h4>NEED SUPPORT?</h4>
        </div>
        <div>
          <a
            className="big-footer-button"
            href="https://discord.gg/k5ydeZp"
            target="about:blank"
          >
            <Discord /> Join our Discord
          </a>
        </div>
      </div>
      <div>
        <div>
          <h4>FOLLOW US</h4>
        </div>
        <div>
          <a
            className="big-footer-icon"
            href="https://www.reddit.com/r/decentraland/"
            target="about:blank"
          >
            <Reddit />
          </a>
          <a
            className="big-footer-icon"
            href="http://github.com/decentraland"
            target="about:blank"
          >
            <Github />
          </a>
          <a
            className="big-footer-icon"
            href="https://twitter.com/decentraland"
            target="about:blank"
          >
            <Twitter />
          </a>
        </div>
      </div>
    </Container>
    <Container>
      <p className="copyright">© {year} Decentraland</p>
    </Container>
  </footer>
);
