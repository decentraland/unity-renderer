import React from "react";
import { Meta, Story } from "@storybook/react";
import { Audio, AudioProps } from "./Audio";

export default {
  title: "Explorer/base/Audio",
  args: {
    play: true,
    track: "https://play.decentraland.org/tone4.mp3",
  },
  component: Audio,
} as Meta;

export const Template: Story<AudioProps> = (args) => <Audio {...args} />;
