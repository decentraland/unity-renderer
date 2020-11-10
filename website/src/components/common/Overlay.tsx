import React from "react";
import { connect } from "react-redux";
import "./Overlay.css";

const mapStateToProps = (state: any) => {
  return {
    show: (!state.loading.error && state.loading.showLoadingScreen) || false,
  };
};

export interface OverlayProps {
  show: boolean;
}

export const Overlay: React.FC<OverlayProps> = (props) => (
  <React.Fragment>{props.show && <div id="overlay" />}</React.Fragment>
);

export default connect(mapStateToProps)(Overlay);
