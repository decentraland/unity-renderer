import React from "react";
import { connect } from "react-redux";
import { NetworkWarning } from "./NetworkWarning";

export enum WARNINGS {
  NETWORK_WARNING = "network_warning",
}

export interface WarningContainerProps {
  type: WARNINGS | null;
  onClose: () => void;
}

const mapStateToProps = (state: any) => ({
  type: state.loading.warning || null,
});

const mapDispatchToProps = (dispatch: any) => ({
  onClose: () => dispatch({ type: "CLEAR_WARNING" }),
});

export const WarningContainer: React.FC<WarningContainerProps> = ({
  type,
  onClose,
}) => (
  <React.Fragment>
    {type === WARNINGS.NETWORK_WARNING && <NetworkWarning onClose={onClose} />}
  </React.Fragment>
);

export default connect(mapStateToProps, mapDispatchToProps)(WarningContainer);
