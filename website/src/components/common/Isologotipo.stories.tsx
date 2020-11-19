import React from "react";
import { Meta, Story } from "@storybook/react";
import { Isologotipo } from "./Isologotipo";

export default {
  title: "Explorer/base/Isologotipo",
  component: Isologotipo,
} as Meta;

const Template: Story = (args) => <Isologotipo style={{ width: 'auto', height: 256 }} />;
export const logo = Template.bind({});