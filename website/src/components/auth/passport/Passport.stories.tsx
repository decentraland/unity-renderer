import React from "react";
import { Meta, Story } from "@storybook/react";
import { Passport, PassportProps } from "./Passport";

export default {
  title: "Explorer/auth/Passport",
  args: {
    name: "",
    email: "",
  },
  component: Passport,
  argTypes: {
    handleSubmit: { action: "submit..." },
    handleCancel: { action: "canceling..." },
    handleEditAvatar: { action: "Edit Avatar..." },
  },
} as Meta;

export const Template: Story<PassportProps> = (args) => <Passport {...args} />;
