import React from "react";
import { Navbar } from "../common/Navbar";
import { EthLogin } from "./EthLogin";
import { EthConnectAdvice } from "./EthConnectAdvice";
import { EthSignAdvice } from "./EthSignAdvice";
import { InitialLoading } from "./InitialLoading";
import { connect } from "react-redux";
import SignUpContainer from "./SignUpContainer";
import { Container } from "../common/Container";
import { BeginnersGuide } from "./BeginnersGuide";
import { BigFooter } from "../common/BigFooter";
import "./LoginContainer.css";

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
  const params = new URLSearchParams(window.location.search)
  return {
    stage: state.session.loginStage,
    signing: state.session.signing,
    subStage: state.session.signup.stage,
    provider: state.session.currentProvider,
    showWalletSelector: params.has('show_wallet'),
    hasWallet: !!window.ethereum,
    hasMetamask: !!(window.ethereum && window.ethereum.isMetaMask),
  }
};

const mapDispatchToProps = (dispatch: any) => ({
  onLogin: (provider: string) =>
    dispatch({ type: "[Authenticate]", payload: { provider } }),
  onGuest: () =>
    dispatch({ type: "[Authenticate]", payload: { provider: "Guest" } }),
});

export interface LoginContainerProps {
  stage: LoginStage;
  signing: boolean;
  subStage: string;
  provider?: string | null;
  showWallet?: boolean;
  hasWallet?: boolean;
  hasMetamask?: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const LoginContainer: React.FC<LoginContainerProps> = (props) => {
  const fullPage = props.stage === LoginStage.SIGN_IN
  const shouldShow =
    LoginStage.COMPLETED !== props.stage && props.subStage !== "avatar";
  return (
    <React.Fragment>
      {shouldShow && (
        <div className={'LoginContainer' + (fullPage ? ' FullPage' : '')}>
          <Navbar full={fullPage} />

          {/* Footer */}
          <main>
            <Container className="eth-login-popup">
              {props.stage === LoginStage.LOADING && <InitialLoading />}
              {props.stage === LoginStage.SIGN_IN && (
                <EthLogin
                  hasWallet={props.hasWallet}
                  hasMetamask={props.hasMetamask}
                  loading={props.signing}
                  onLogin={props.onLogin}
                  onGuest={props.onGuest}
                  provider={props.provider}
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
          {fullPage && <BeginnersGuide />}

          {/* Footer */}
          {fullPage && <BigFooter />}
        </div>
      )}
    </React.Fragment>
  );
};
export default connect(mapStateToProps, mapDispatchToProps)(LoginContainer);
