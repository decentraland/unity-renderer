import React from "react";
import { Meta, Story } from "@storybook/react";
import { ProgressBar, ProgressBarProps } from "./ProgressBar";

export default {
  title: "Explorer/loading/ProgressBar",
  args: {
    percentage: 10,
  },
  component: ProgressBar,
} as Meta;

export const Template: Story<ProgressBarProps> = (args) => (
  <ProgressBar {...args} />
);
