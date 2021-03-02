import React from "react";
import { Meta, Story } from "@storybook/react";
import {
  WalletButton,
  WalletButtonLogo,
  WalletButtonProps,
} from "./WalletButton";

export default {
  title: "Explorer/auth/WalletButton",
  args: {
    type: "",
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
  type: WalletButtonLogo.METAMASK,
};

export const MetamaskButtonDisabled = MetamaskButton.bind({});
MetamaskButtonDisabled.args = {
  ...MetamaskButton.args,
  active: true,
};

export const DapperButton = Template.bind({});
DapperButton.args = {
  ...Template.args,
  type: WalletButtonLogo.FORTMATIC,
};

export const DapperButtonDisabled = DapperButton.bind({});
DapperButtonDisabled.args = {
  ...DapperButton.args,
  active: true,
};

export const FortmaticButton = Template.bind({});
FortmaticButton.args = {
  ...Template.args,
  type: WalletButtonLogo.FORTMATIC,
};

export const FortmaticButtonDisabled = FortmaticButton.bind({});
FortmaticButtonDisabled.args = {
  ...FortmaticButton.args,
  active: true,
};

export const SamsungButton = Template.bind({});
SamsungButton.args = {
  ...Template.args,
  type: WalletButtonLogo.FORTMATIC,
};

export const SamsungButtonDisabled = SamsungButton.bind({});
SamsungButtonDisabled.args = {
  ...SamsungButton.args,
  active: true,
};

export const WalletConnectButton = Template.bind({});
WalletConnectButton.args = {
  ...Template.args,
  type: WalletButtonLogo.FORTMATIC,
};

export const WalletConnectButtonDisabled = WalletConnectButton.bind({});
WalletConnectButtonDisabled.args = {
  ...WalletConnectButton.args,
  active: true,
};
