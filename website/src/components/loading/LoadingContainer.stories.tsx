import React from "react";

import { Meta, Story } from "@storybook/react";
import { LoadingContainer, LoadingContainerProps } from "./LoadingContainer";

export default {
  title: "Explorer/LoadingContainer",
  args: {
    state: {
      status: "Getting things ready...",
      helpText: 0,
      message: "",
      pendingScenes: 4,
      loadPercentage: 30,
      subsystemsLoad: 0,
      initialLoad: true,
      showLoadingScreen: true,
    },
    showWalletPrompt: false,
  } as LoadingContainerProps,
  component: LoadingContainer,
} as Meta;

export const Template: Story<LoadingContainerProps> = (args) => (
  <LoadingContainer {...args} />
);
