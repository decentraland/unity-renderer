declare module '@decentraland/web3-provider' {
  export type Provider = {
    send: Function
    sendAsync: Function
  }
  export function getProvider(): Provider
}
