import React from "react";
import { Meta, Story } from "@storybook/react";
import { Provider } from "react-redux";
import { createStore } from "redux";
import {
  LoginContainer,
  LoginContainerProps,
  LoginStage,
} from "./LoginContainer";

export default {
  title: "Explorer/Login",
  args: {
    stage: LoginStage.LOADING,
    subStage: "",
  } as LoginContainerProps,
  component: LoginContainer,
  argTypes: {
    onLogin: { action: "signing in..." },
    onTermsChange: { action: "terms changed..." },
  },
} as Meta;

const Template: Story<LoginContainerProps & { state: any }> = ({
  state,
  ...props
}) => (
  <Provider store={createStore(() => state, state)}>
    <LoginContainer {...props} />
  </Provider>
);

export const LoadingState = Template.bind({});
LoadingState.args = {
  ...Template.args,
};

export const play = Template.bind({});
play.args = {
  ...Template.args,
  stage: LoginStage.SIGN_IN,
  hasWallet: true,
};

export const playAsGuest = Template.bind({});
playAsGuest.args = {
  ...Template.args,
  stage: LoginStage.SIGN_IN,
  hasWallet: false,
};

export const passport = Template.bind({});
passport.args = {
  ...Template.args,
  stage: LoginStage.SIGN_UP,
  state: { session: { signup: { stage: "passport" } } },
};

export const terms = Template.bind({});
terms.args = {
  ...Template.args,
  stage: LoginStage.SIGN_UP,
  state: { session: { signup: { stage: "terms" } } },
};

export const ConnectAdvice = Template.bind({});
ConnectAdvice.args = {
  ...Template.args,
  stage: LoginStage.CONNECT_ADVICE,
};

export const SignAdvice = Template.bind({});
SignAdvice.args = {
  ...Template.args,
  stage: LoginStage.SIGN_ADVICE,
};
