export function getExplorerVersion() {
  return ((globalThis as any).ROLLOUTS || {})['@dcl/explorer']?.version
}
