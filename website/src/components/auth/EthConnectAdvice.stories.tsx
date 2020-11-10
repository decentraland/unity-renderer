import React from "react";
import { Meta, Story } from "@storybook/react";
import { EthConnectAdvice, EthConnectAdviceProps } from "./EthConnectAdvice";

export default {
  title: "Explorer/auth/EthConnectAdvice",
  args: {},
  component: EthConnectAdvice,
  argTypes: { onLogin: { action: "login clicked" } },
} as Meta;

export const Template: Story<EthConnectAdviceProps> = (args) => (
  <EthConnectAdvice {...args} />
);
