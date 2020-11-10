import React from "react";
import { Meta, Story } from "@storybook/react";
import { Overlay, OverlayProps } from "./Overlay";

export default {
  title: "Explorer/base/Overlay",
  args: {
    show: true,
  } as OverlayProps,
  component: Overlay,
} as Meta;

export const Template: Story<OverlayProps> = (args) => <Overlay {...args} />;
