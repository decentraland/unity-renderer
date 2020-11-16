import React, { useState } from "react";
import { filterInvalidNameCharacters } from "../../../utils";
import "./PassportForm.css";

// eslint-disable-next-line
const emailPattern = /^((([a-z]|\d|[!#$%&'*+\-/=?^_`{|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#$%&'*+\-/=?^_`{|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/;

export interface PassportFormProps {
  name?: string;
  email?: string;
  onSubmit: (name: string, email: string) => void;
}

export const PassportForm: React.FC<PassportFormProps> = (props) => {
  const [chars, setChars] = useState(props.name ? props.name.length : null);
  const [name, setName] = useState(props.name || "");
  const [email, setEmail] = useState(props.email || "");
  const [hasNameError, setNameError] = useState(false);
  const [hasEmailError, setEmailError] = useState(false);
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!name || name.trim().length > 10) {
      setNameError(true);
      return;
    }
    if (email.trim().length > 0 && !emailPattern.test(email)) {
      setEmailError(true);
      return;
    }
    props.onSubmit(name.trim(), email.trim());
  };

  const onChangeName = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    if (target.value.length <= 15) {
      const value = filterInvalidNameCharacters(target.value);
      setNameError(false);
      setName(value);
      setChars(value.length);
    }
  };

  const onChangeEmail = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    setEmailError(false);
    setEmail(target.value);
  };

  return (
    <div className="passportForm">
      <form method="POST" onSubmit={handleSubmit}>
        <div className="inputGroup">
          {hasNameError && (
            <em className="error">*Required field (you can edit it later)</em>
          )}
          <label>Name your avatar</label>
          <input
            type="text"
            name="name"
            className={hasNameError ? "hasError" : ""}
            placeholder="Your avatar name"
            value={name}
            onChange={onChangeName}
          />
          <em className={"warningLength " + (chars === 0 ? "error" : "")}>
            {chars || 0}/10
          </em>
        </div>
        <div className="inputGroup">
          <label>Let's stay in touch</label>
          <input
            type="text"
            name="email"
            className={hasEmailError ? "hasError" : ""}
            placeholder="Enter your email"
            value={email}
            onChange={onChangeEmail}
          />
          {hasEmailError && <em className="hasError">Email not valid</em>}
        </div>
        <div className="actions">
          <button type="submit" className="btnSubmit">
            NEXT
          </button>
        </div>
      </form>
    </div>
  );
};
