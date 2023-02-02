const hasOwn = Object.prototype.hasOwnProperty

export function shallowEqual(objA: any, objB: any) {
  if (objA === objB) {
    return true
  }

  const typeA = typeof objA
  const typeB = typeof objB

  if (typeA !== typeB) return false

  if (typeA !== 'object' || typeB !== 'object') return objA === objB

  if ((objA === null) !== (objB === null)) return false

  const keysA = Object.keys(objA)
  const keysB = Object.keys(objB)

  if (keysA.length !== keysB.length) {
    return false
  }

  // Test for A's keys different from B.
  for (let i = 0; i < keysA.length; i++) {
    if (!hasOwn.call(objB, keysA[i]) || objA[keysA[i]] !== objB[keysA[i]]) {
      return false
    }
  }

  return true
}
