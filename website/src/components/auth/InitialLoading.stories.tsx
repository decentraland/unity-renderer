import React from "react";
import { Meta, Story } from "@storybook/react";
import { InitialLoading } from "./InitialLoading";

export default {
  title: "Explorer/auth/InitialLoading",
  component: InitialLoading,
} as Meta;

export const Template: Story = () => <InitialLoading />;
