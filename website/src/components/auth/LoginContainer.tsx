import React from "react";
import { Navbar } from "../common/Navbar";
import { EthLogin } from "./EthLogin";
import { EthConnectAdvice } from "./EthConnectAdvice";
import { EthSignAdvice } from "./EthSignAdvice";
import { connect } from "react-redux";
import SignUpContainer from "./SignUpContainer";
import { Container } from "../common/Container";
import { BeginnersGuide } from "./BeginnersGuide";
import { BigFooter } from "../common/BigFooter";
import "./LoginContainer.css";
import { AuthType } from "../../utils";

declare var window: Window & {
  ethereum: any;
};

export enum LoginStage {
  LOADING = "loading",
  SIGN_IN = "signIn",
  SIGN_UP = "signUp",
  CONNECT_ADVICE = "connect_advice",
  SIGN_ADVICE = "sign_advice",
  COMPLETED = "completed",
}

const mapStateToProps = (state: any) => {
  const params = new URLSearchParams(window.location.search);
  return {
    stage: state.session.loginStage,
    signing: state.session.signing,
    subStage: state.session.signup.stage,
    provider: state.session.currentProvider,
    showWalletSelector: params.has("show_wallet"),
    hasWallet: !!window.ethereum,
  };
};

const mapDispatchToProps = (dispatch: any) => ({
  onLogin: (provider: string) =>
    dispatch({ type: "[Authenticate]", payload: { provider } }),
  onGuest: () =>
    dispatch({ type: "[Authenticate]", payload: { provider: AuthType.GUEST } }),
});

export interface LoginContainerProps {
  stage: LoginStage;
  signing: boolean;
  subStage: string;
  provider?: string | null;
  showWallet?: boolean;
  hasWallet?: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const LoginContainer: React.FC<LoginContainerProps> = (props) => {
  const loading = props.stage === LoginStage.LOADING;
  const full = loading || props.stage === LoginStage.SIGN_IN;
  const shouldShow =
    LoginStage.COMPLETED !== props.stage && props.subStage !== "avatar";
  const provider = props.hasWallet
    ? AuthType.INJECTED
    : props.provider === AuthType.FORTMATIC
    ? props.provider
    : null;
  return (
    <React.Fragment>
      {shouldShow && (
        <div className={"LoginContainer" + (full ? " FullPage" : "")}>
          {/* Nabvar */}
          <Navbar full={full} />

          {/* Main */}
          <main>
            <Container className="eth-login-popup">
              {full && (
                <EthLogin
                  hasWallet={props.hasWallet}
                  loading={loading || props.signing}
                  onLogin={props.onLogin}
                  onGuest={props.onGuest}
                  provider={provider}
                  showWallet={props.showWallet}
                />
              )}
              {props.stage === LoginStage.CONNECT_ADVICE && (
                <EthConnectAdvice onLogin={props.onLogin} />
              )}
              {props.stage === LoginStage.SIGN_ADVICE && <EthSignAdvice />}
              {props.stage === LoginStage.SIGN_UP && <SignUpContainer />}
            </Container>
          </main>

          {/* Beginner Guide */}
          {full && <BeginnersGuide />}

          {/* Footer */}
          {full && <BigFooter />}
        </div>
      )}
    </React.Fragment>
  );
};
export default connect(mapStateToProps, mapDispatchToProps)(LoginContainer);
