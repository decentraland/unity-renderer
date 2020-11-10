import React from "react";
import { Meta, Story } from "@storybook/react";
import {
  WarningContainer,
  WarningContainerProps,
  WARNINGS,
} from "./WarningContainer";

export default {
  title: "Explorer/Warnings",
  args: {
    type: null,
  },
  component: WarningContainer,
  argTypes: { onClose: { action: "closed clicked" } },
} as Meta;

const Template: Story<WarningContainerProps> = (args) => (
  <WarningContainer {...args} />
);

export const none = Template.bind({});
none.args = {
  ...Template.args,
};

export const NetworkWarning = Template.bind({});
NetworkWarning.args = {
  ...Template.args,
  type: WARNINGS.NETWORK_WARNING,
};
