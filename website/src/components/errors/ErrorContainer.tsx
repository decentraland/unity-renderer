import React from "react";
import { connect } from "react-redux";
import { ErrorComms } from "./ErrorComms";
import { ErrorFatal } from "./ErrorFatal";
import { ErrorNoMobile } from "./ErrorNoMobile";
import { ErrorNewLogin } from "./ErrorNewLogin";
import { ErrorNetworkMismatch } from "./ErrorNetworkMismatch";
import { ErrorNotInvited } from "./ErrorNotInvited";
import { ErrorNotSupported } from "./ErrorNotSupported";

import "./errors.css";

export enum Error {
  FATAL = "fatal",
  COMMS = "comms",
  NEW_LOGIN = "newlogin",
  NOT_MOBILE = "nomobile",
  NOT_INVITED = "notinvited",
  NOT_SUPPORTED = "notsupported",
  NET_MISMATCH = "networkmismatch",
}

const mapStateToProps = (state: any) => {
  return {
    error: state.loading.error || null,
    details: state.loading.tldError || null,
  };
};

export interface ErrorContainerProps {
  error: string | null;
  details: { tld: string; web3Net: string; tldNet: string } | null;
}

export const ErrorContainer: React.FC<ErrorContainerProps> = (props) => {
  return (
    <React.Fragment>
      {props.error === Error.FATAL && <ErrorFatal />}
      {props.error === Error.COMMS && <ErrorComms />}
      {props.error === Error.NEW_LOGIN && <ErrorNewLogin />}
      {props.error === Error.NOT_MOBILE && <ErrorNoMobile />}
      {props.error === Error.NOT_INVITED && <ErrorNotInvited />}
      {props.error === Error.NOT_SUPPORTED && <ErrorNotSupported />}
      {props.error === Error.NET_MISMATCH && (
        <ErrorNetworkMismatch details={props.details} />
      )}
    </React.Fragment>
  );
};

export default connect(mapStateToProps)(ErrorContainer);
