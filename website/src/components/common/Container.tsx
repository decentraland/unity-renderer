import React from "react";
import "./Container.css";

export const Container: React.FC<React.HTMLAttributes<HTMLDivElement>> = (
  props
) => <div {...props} className={"eth-container " + (props.className || "")} />;

export const Content: React.FC<React.HTMLAttributes<HTMLDivElement>> = (
  props
) => <div {...props} className={"eth-content " + (props.className || "")} />;
