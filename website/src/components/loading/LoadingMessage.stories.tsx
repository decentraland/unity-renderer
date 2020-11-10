import React from "react";
import { Meta, Story } from "@storybook/react";
import { LoadingMessage, LoadingMessageProps } from "./LoadingMessage";

export default {
  title: "Explorer/loading/LoadingMessage",
  args: {
    image: "images/decentraland-connect/loadingtips/Mana.png",
    message: `MANA is Decentraland’s virtual currency. Use it to buy LAND and other premium items, vote on key policies and pay platform fees.`,
    subMessage: "Loading scenes...",
    showWalletPrompt: false,
  },
  component: LoadingMessage,
} as Meta;

export const Template: Story<LoadingMessageProps> = (args) => (
  <LoadingMessage {...args} />
);
