/**
 * Simple function to compare versions, like 0.1 vs 0.2, or 0.2.3 vs 0.2.4.
 * Returns -1 if the first version is previous to the second, 0 if they are the same, 1 otherwise
 */
// Based on https://github.com/substack/semver-compare (MIT)
export function compareVersions(v1: string, v2: string) {
  const pa = v1.split('.')
  const pb = v2.split('.')
  for (let i = 0; i < 3; i++) {
    const na = Number(pa[i])
    const nb = Number(pb[i])
    if (na > nb) return 1
    if (nb > na) return -1
    if (!isNaN(na) && isNaN(nb)) return 1
    if (isNaN(na) && !isNaN(nb)) return -1
  }
  return 0
}
