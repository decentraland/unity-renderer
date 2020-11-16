import React from "react";
import { Meta, Story } from "@storybook/react";
import { WalletSelector, WalletSelectorProps } from "./WalletSelector";

export default {
  title: "Explorer/auth/WalletSelector",
  args: {
    show: true,
    loading: false,
    metamask: true,
  },
  component: WalletSelector,
  argTypes: {
    onClick: { action: "clicked" },
    onCancel: { action: "Canceled" },
  },
} as Meta;

const Template: Story<WalletSelectorProps> = (args) => (
  <WalletSelector {...args} />
);

export const Default = Template.bind({});
Default.args = {
  ...Template.args,
};

export const DisabledButton = Default.bind({});
DisabledButton.args = {
  ...Default.args,
  metamask: false,
};

export const Loading = Template.bind({});
Loading.args = {
  ...Template.args,
  loading: true,
};
