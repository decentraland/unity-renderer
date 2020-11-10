import React from "react";

import { Meta, Story } from "@storybook/react";
import { TermsOfServices, TermsOfServicesProps } from "./TermsOfServices";

export default {
  title: "Explorer/auth/TermsOfServices",
  args: {
    loading: false,
  },
  component: TermsOfServices,
  argTypes: {
    handleBack: { action: "back to..." },
    handleCancel: { action: "canceling..." },
    handleAgree: { action: "agree..." },
  },
} as Meta;

const Template: Story<TermsOfServicesProps> = (args) => (
  <TermsOfServices {...args} />
);
export const Default = Template.bind({});
Default.args = {
  ...Template.args,
};

export const Loading = Template.bind({});
Loading.args = {
  ...Template.args,
  loading: true,
};
