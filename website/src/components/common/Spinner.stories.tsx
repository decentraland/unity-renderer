import React from "react";
import { Meta, Story } from "@storybook/react";
import { Spinner } from "./Spinner";

export default {
  title: "Explorer/base/Spinner",
  component: Spinner,
} as Meta;

export const Template: Story = () => <Spinner />;
