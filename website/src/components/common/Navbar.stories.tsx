import React from "react";

import { Meta, Story } from "@storybook/react";
import { Navbar } from "./Navbar";

export default {
  title: "Explorer/base/Navbar",
  component: Navbar,
} as Meta;

export const Template: Story = () => <Navbar />;
