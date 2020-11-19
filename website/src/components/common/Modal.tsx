import React from "react";
import { Avatars } from "./Avatars";
import "./Modal.css";

export interface ModalProps {
  withAvatars?: boolean;
  withOverlay?: boolean;
  withFlatBackground?: boolean;
  className?: string;
  onClose?: () => void;
}

export const Modal: React.FC<ModalProps> = ({
  onClose,
  withAvatars,
  withFlatBackground,
  withOverlay,
  className,
  children,
}) => {
  let containerClassName = 'popup-container';

  if (withAvatars) {
    containerClassName += ' with-avatars';
  }

  if (withOverlay) {
    containerClassName += ' with-overlay';
  }

  if (withFlatBackground) {
    containerClassName += ' with-flat-background';
  }

  let popupClassName = 'popup'

  if (className) {
    popupClassName += ' ' + className;
  }

  function handleClose() {
    if (onClose) {
      onClose()
    }
  }

  return (
    <div className={containerClassName} onClick={handleClose}>
      <div className={popupClassName} onClick={(e) => e.stopPropagation()}>
        {onClose && <div className="close" onClick={handleClose} />}
        {children}
      </div>
      {withAvatars && <Avatars />}
    </div>
  );
};
