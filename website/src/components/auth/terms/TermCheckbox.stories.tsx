import React from "react";

import { Meta, Story } from "@storybook/react";
import { TermCheckbox, TermCheckboxProps } from "./TermCheckbox";

export default {
  title: "Explorer/auth/TermCheckbox",
  args: {
    checked: false,
  },
  component: TermCheckbox,
} as Meta;

export const Template: Story<TermCheckboxProps> = (args) => (
  <TermCheckbox {...args} />
);
