import React, { useState } from "react";
import { filterInvalidNameCharacters, isBadWord } from "../../../utils";
import "./PassportForm.css";

// eslint-disable-next-line
const emailPattern = /^((([a-z]|\d|[!#$%&'*+\-/=?^_`{|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#$%&'*+\-/=?^_`{|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/;

export interface PassportFormProps {
  name?: string;
  email?: string;
  onSubmit: (name: string, email: string) => void;
}

export interface PassportFormState {
  name: string;
  hasNameError: boolean;
  email: string;
  hasEmailError: boolean;
}

const MAX_NAME_LENGTH = 15;

export const PassportForm: React.FC<PassportFormProps> = (props) => {
  const [state, setState] = useState<PassportFormState>({
    name: props.name || "",
    hasNameError: false,
    email: props.email || "",
    hasEmailError: false,
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const hasNameError =
      state.name.trim().length === 0 ||
      state.name.length > MAX_NAME_LENGTH ||
      isBadWord(state.name);
    const hasEmailError =
      state.email.length > 0 && !emailPattern.test(state.email);

    if (hasNameError || hasEmailError) {
      setState((current) => ({ ...current, hasNameError, hasEmailError }));
    } else if (!!props.onSubmit) {
      props.onSubmit(state.name, state.email);
    }
  };

  const onChangeName = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    let name = (target.value || "").trim();
    if (name.length > MAX_NAME_LENGTH) {
      setState((current) => ({
        ...current,
        hasNameError: true,
      }));
      return;
    }
    try {
      name = filterInvalidNameCharacters(name);
    } catch (err) {
      // ignore
    }

    setState((current) => ({
      ...current,
      name,
      hasNameError: name.length > MAX_NAME_LENGTH || name.length === 0,
    }));
  };

  const onChangeEmail = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    const email = (target.value || "").trim();
    setState((current) => ({
      ...current,
      email,
      hasEmailError: false,
    }));
  };

  const disabled = state.name.length === 0 || state.name.length > MAX_NAME_LENGTH

  return (
    <div className="passportForm">
      <form method="POST" onSubmit={handleSubmit}>
        <div className="inputGroup inputGroupName">
          <label>Name your avatar</label>
          <em className="required">non-alphanumeric characters or spaces allowed</em>
          <input
            type="text"
            name="name"
            className={state.hasNameError ? "hasError" : ""}
            placeholder="Your avatar name"
            autoComplete="0"
            value={state.name}
            onChange={onChangeName}
          />
          <em className={"hint" + (state.hasNameError ? " hasError" : "")}>
            {Math.max(state.name.length, 0)}/{MAX_NAME_LENGTH}
          </em>
        </div>
        <div className="inputGroup inputGroupEmail">
          <label>Let's stay in touch</label>
          <em className="required">susbscribe to our newsletter (optional)</em>
          <input
            type="text"
            name="email"
            className={state.hasEmailError ? "hasError" : ""}
            placeholder="Enter your email"
            value={state.email}
            onChange={onChangeEmail}
          />
          <em className="hint hasError">
            {state.hasEmailError ? "Enter a valid email" : ""}
          </em>
        </div>
        <div className="actions">
          <button type="submit" className="btnSubmit" disabled={disabled}>
            NEXT
          </button>
        </div>
      </form>
    </div>
  );
};
