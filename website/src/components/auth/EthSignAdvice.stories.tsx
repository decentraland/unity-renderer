import React from "react";

import { Meta, Story } from "@storybook/react";
import { EthSignAdvice } from "./EthSignAdvice";

export default {
  title: "Explorer/auth/EthSignAdvice",
  component: EthSignAdvice,
} as Meta;

export const Template: Story = () => <EthSignAdvice />;
