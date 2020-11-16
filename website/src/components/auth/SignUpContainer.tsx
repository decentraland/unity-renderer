import React, { useState } from "react";
import { connect } from "react-redux";
import { Passport } from "./passport/Passport";
import { TermsOfServices } from "./terms/TermsOfServices";

export interface SignUpContainerProps {
  face: string;
  name: string;
  email: string;
  stage: string;
  handleForm: any;
  handleAgree: any;
  handleCancel: any;
  handleBack: any;
}

export const SignUpContainer: React.FC<SignUpContainerProps> = (props) => {
  const [loading, setLoading] = useState(false);
  const signUp = () => {
    setLoading(true);
    props.handleAgree();
  };

  return (
    <React.Fragment>
      {props.stage === "passport" && (
        <Passport
          face={props.face}
          name={props.name}
          email={props.email}
          handleSubmit={props.handleForm}
          handleCancel={props.handleCancel}
          handleEditAvatar={() => props.handleBack("passport")}
        />
      )}
      {props.stage === "terms" && (
        <TermsOfServices
          loading={loading}
          handleBack={() => props.handleBack("terms")}
          handleCancel={props.handleCancel}
          handleAgree={signUp}
        />
      )}
    </React.Fragment>
  );
};

const mapStateToProps = (state: any, props: any) => {
  const signup = state?.session?.signup;
  const face256 = signup?.profile?.avatar?.snapshots?.face256;

  return {
    face: props?.face ?? (face256 && `data:image/jpg;base64,${face256}`),
    name: signup?.profile?.unclaimedName || "",
    email: signup?.profile?.email || "",
    stage: props?.stage ?? signup?.stage,
  };
};

const mapDispatchToProps = (dispatch: any) => ({
  handleForm: (name: string, email: string) => {
    dispatch({ type: "[SIGNUP_FORM]", payload: { name, email } });
    dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "terms" } });
  },
  handleAgree: () => dispatch({ type: "[SIGNUP]" }),
  handleCancel: () => {
    dispatch({ type: "[SIGN-UP-CANCEL]" });
  },
  handleBack: (from: "passport" | "terms") => {
    if (from === "terms") {
      dispatch({
        type: "[SIGNUP_STAGE]",
        payload: { stage: "passport" },
      });
    }
    if (from === "passport") {
      dispatch({ type: "[SIGNUP_COME_BACK_TO_AVATAR_EDITOR]" });
    }
  },
});

export default connect(mapStateToProps, mapDispatchToProps)(SignUpContainer);
