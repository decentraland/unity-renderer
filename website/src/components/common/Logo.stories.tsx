import React from "react";

import { Meta, Story } from "@storybook/react";
import { Logo, LogoProps } from "./Logo";

export default {
  title: "Explorer/base/Logo",
  component: Logo,
} as Meta;

const Template: Story<LogoProps> = (args) => <Logo {...args} />;
export const large = Template.bind({});
large.args = {
  ...Template.args,
  icon: false,
};
export const icon = Template.bind({});
icon.args = {
  ...Template.args,
  icon: true,
};
