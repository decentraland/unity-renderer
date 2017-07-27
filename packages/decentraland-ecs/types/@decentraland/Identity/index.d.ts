declare module '@decentraland/Identity' {
  export type UserData = {
    displayName: string
    publicKey: string
  }

  /**
   * Return the Ethereum address of the user
   */
  export function getUserPublicKey(): Promise<string | null>

  /**
   * Return the user's data
   */
  export function getUserData(): Promise<UserData | null>
}
