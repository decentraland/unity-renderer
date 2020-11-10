import React from "react";
import { Meta, Story } from "@storybook/react";
import { PassportForm, PassportFormProps } from "./PassportForm";

export default {
  title: "Explorer/auth/PassportForm",
  args: {
    name: "ezequiel",
    email: "",
  },
  component: PassportForm,
  argTypes: {
    onSubmit: { action: "Submit..." },
  },
} as Meta;

export const Template: Story<PassportFormProps> = (args) => (
  <PassportForm {...args} />
);
