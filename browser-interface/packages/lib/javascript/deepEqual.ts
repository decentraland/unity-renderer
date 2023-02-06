const hasOwn = Object.prototype.hasOwnProperty

export function deepEqual<T = any>(objA: T, objB: T) {
  if (objA === objB) {
    return true
  }

  const typeA = typeof objA
  const typeB = typeof objB

  if (typeA !== typeB) return false

  if (typeA !== 'object' || typeB !== 'object') return objA === objB

  if ((objA === null) !== (objB === null)) return false

  const keysA = Object.keys(objA as any)
  const keysB = Object.keys(objB as any)

  if (keysA.length !== keysB.length) {
    return false
  }

  // Test for A's keys different from B.
  for (let i = 0; i < keysA.length; i++) {
    if (!hasOwn.call(objB as any, keysA[i]) || !deepEqual((objA as any)[keysA[i]], (objB as any)[keysA[i]])) {
      return false
    }
  }

  return true
}
