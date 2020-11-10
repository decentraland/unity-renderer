import React from "react";
import "./PassportAvatar.css";
import defaultAvatar from "../../../images/defaultAvatar.png";

export interface PassportAvatarProps {
  face?: string;
  onEditAvatar: any;
}

export const PassportAvatar: React.FC<PassportAvatarProps> = (props) => (
  <div className="PassportAvatar">
    <img
      alt=""
      className="avatar"
      width="180"
      height="180"
      src={props.face || defaultAvatar}
    />
    {/*<em>Active since Aug 2020</em>*/}
    <button onClick={props.onEditAvatar}>Edit Avatar</button>
  </div>
);
