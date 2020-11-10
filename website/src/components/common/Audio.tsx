import React from "react";

export interface AudioProps {
  play: boolean;
  track: string;
}

export class Audio extends React.Component<AudioProps> {
  private player: HTMLAudioElement | null = null;

  componentDidMount() {
    this.updatePlayer();
  }

  componentDidUpdate() {
    this.updatePlayer();
  }

  updatePlayer() {
    if (!this.player) {
      return;
    }
    !this.props.play
      ? this.player.pause()
      : this.player.play().catch((e) => {});
  }

  render() {
    return (
      <audio ref={(audio) => (this.player = audio)} autoPlay loop>
        <source src={this.props.track} type="audio/mpeg" />
      </audio>
    );
  }
}
