import React from "react";

import { Meta, Story } from "@storybook/react";
import { EthLogin, EthLoginProps } from "./EthLogin";

export default {
  title: "Explorer/auth/EthLogin",
  args: {
    loading: false,
    provider: null,
    hasMetamask: true,
  },
  component: EthLogin,
  argTypes: {
    onLogin: { action: "signing in/up..." },
    onGuest: { action: "guest click..." },
    onTermsChange: { action: "terms changed..." },
  },
} as Meta;

const Template: Story<EthLoginProps> = (args: EthLoginProps) => (
  <EthLogin {...args} />
);

export const LoginHome = Template.bind({});
LoginHome.args = {
  ...Template.args,
};

export const LoginSigning = Template.bind({});
LoginSigning.args = {
  ...Template.args,
  loading: true,
};

export const LoginPreviousSession = Template.bind({});
LoginPreviousSession.args = {
  ...Template.args,
  loading: false,
  provider: "Metamask",
};
