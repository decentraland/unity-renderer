import React from "react";
import { Meta, Story } from "@storybook/react";
import { Error, ErrorContainer, ErrorContainerProps } from "./ErrorContainer";

export default {
  title: "Explorer/Errors",
  args: {
    error: Error.FATAL,
    details: null,
  } as ErrorContainerProps,
  component: ErrorContainer,
} as Meta;

const Template: Story<ErrorContainerProps> = (args) => (
  <ErrorContainer {...args} />
);
export const FatalError = Template.bind({});
FatalError.args = {
  ...Template.args,
};
export const CommsError = Template.bind({});
CommsError.args = {
  ...Template.args,
  error: Error.COMMS,
};

export const NotSupportedError = Template.bind({});
NotSupportedError.args = {
  ...Template.args,
  error: Error.NOT_SUPPORTED,
};

export const NotInvitedError = Template.bind({});
NotInvitedError.args = {
  ...Template.args,
  error: Error.NOT_INVITED,
};

export const NetMismatchError = Template.bind({});
NetMismatchError.args = {
  ...Template.args,
  error: Error.NET_MISMATCH,
  details: { tld: "zone", web3Net: "mainet", tldNet: "ropsten" },
};

export const NewLoginError = Template.bind({});
NewLoginError.args = {
  ...Template.args,
  error: Error.NEW_LOGIN,
};

export const NotMobileError = Template.bind({});
NotMobileError.args = {
  ...Template.args,
  error: Error.NOT_MOBILE,
};
