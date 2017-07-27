/**
 * @internal
 */
export function uuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    let r = (Math.random() * 16) | 0
    let v = c === 'x' ? r : (r & 0x3) | 0x8
    return v.toString(16)
  })
}

/**
 * Returns an array of the given size filled with element built from the given constructor and the paramters
 * @param size the number of element to construct and put in the array
 * @param itemBuilder a callback responsible for creating new instance of item. Called once per array entry.
 * @returns a new array filled with new objects
 * @public
 */
export function buildArray<T>(size: number, itemBuilder: () => T): Array<T> {
  const a: T[] = []
  for (let i = 0; i < size; ++i) {
    a.push(itemBuilder())
  }
  return a
}
