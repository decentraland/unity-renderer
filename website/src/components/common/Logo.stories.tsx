import React from "react";
import { Meta, Story } from "@storybook/react";
import { Logo } from "./Logo";

export default {
  title: "Explorer/base/Logo",
  component: Logo,
} as Meta;

const Template: Story = (args) => <Logo style={{ width: 'auto', height: 256 }} />;
export const logo = Template.bind({});