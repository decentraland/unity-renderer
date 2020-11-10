import React from "react";
import { Meta, Story } from "@storybook/react";
import { PassportAvatar, PassportAvatarProps } from "./PassportAvatar";

export default {
  title: "Explorer/auth/PassportAvatar",
  args: {},
  component: PassportAvatar,
  argTypes: {
    onEditAvatar: { action: "Go to Avatar Editor..." },
  },
} as Meta;

export const Template: Story<PassportAvatarProps> = (args) => (
  <PassportAvatar {...args} />
);
