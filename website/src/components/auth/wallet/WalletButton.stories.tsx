import React from "react";
import { Meta, Story } from "@storybook/react";
import { WalletButton, WalletButtonProps } from "./WalletButton";
import MetamaskLogo from "../../../images/metamask.svg";
import FortmaticLogo from "../../../images/fortmatic.svg";

export default {
  title: "Explorer/auth/WalletButton",
  args: {
    logo: "",
    title: "",
    description: "",
  },
  component: WalletButton,
  argTypes: {
    onClick: { action: "clicked" },
  },
} as Meta;

const Template: Story<WalletButtonProps> = (args) => <WalletButton {...args} />;

export const MetamaskButton = Template.bind({});
MetamaskButton.args = {
  ...Template.args,
  logo: MetamaskLogo,
  title: "Metamask",
  description: "Using a browser extension",
};

export const MetamaskDisabledButton = MetamaskButton.bind({});
MetamaskDisabledButton.args = {
  ...MetamaskButton.args,
  disabled: true,
};

export const FortmaticButton = Template.bind({});
FortmaticButton.args = {
  ...Template.args,
  logo: FortmaticLogo,
  title: "Metamask",
  description: "Using your Email account",
};
