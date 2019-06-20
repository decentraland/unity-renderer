export type UnityLoaderType = {
  instantiate(divId: string, manifest: string): UnityGame
}

export type UnityGame = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
  SetFullscreen(): void
}
