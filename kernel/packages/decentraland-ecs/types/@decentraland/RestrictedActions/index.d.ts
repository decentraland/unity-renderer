declare module '@decentraland/RestrictedActions' {
  /**
   * move player to a position inside the scene
   *
   * @param position PositionType
   * @param cameraTarget PositionType
   */
  export function movePlayerTo(newPosition: PositionType, cameraTarget?: PositionType): Promise<void>

  export type PositionType = { x: number; y: number; z: number }

  /**
   * trigger an emote on the current player
   *
   * @param emote the emote to perform
   */
  export function triggerEmote(emote: Emote): Promise<void>

  export type Emote = {
    predefined: PredefinedEmote
  }

  export const enum PredefinedEmote {
    WAVE = 'wave',
    FIST_PUMP = 'fistpump',
    ROBOT = 'robot',
    RAISE_HAND = 'raiseHand',
    CLAP = 'clap',
    MONEY = 'money',
    KISS = 'kiss',
  }
}