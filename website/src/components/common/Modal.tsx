import React from "react";
import { Avatars } from "./Avatars";
import "./Modal.css";

export interface ModalProps {
  withAvatars?: boolean;
  handleClose?: () => void;
}

export const Modal: React.FC<ModalProps> = ({
  handleClose,
  withAvatars,
  children,
}) => {
  let className = "popup-container";
  if (withAvatars) {
    className += " with-avatars";
  }

  return (
    <div className={className}>
      <div className="popup">
        {handleClose && <div className="close" onClick={handleClose} />}
        {children}
      </div>
      {withAvatars && <Avatars />}
    </div>
  );
};
