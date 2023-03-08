/**
 * Register to any change in the configuration of the wallet to reload the app and avoid wallet changes in-game.
 * TODO: move to explorer-website
 */
declare const window: { ethereum: any }
export function registerProviderNetChanges() {
  if (window.ethereum && typeof window.ethereum.on === 'function') {
    window.ethereum.on('chainChanged', () => location.reload())
  }
}
