export function isPromiseLike(thing: Promise<any>): true
export function isPromiseLike(thing: void): false
export function isPromiseLike(thing: any): false
export function isPromiseLike(thing: any): boolean {
  return (
    thing &&
    // it is an object
    typeof thing === 'object' &&
    // it has then
    typeof thing['then'] === 'function' &&
    // it has catch
    typeof thing['catch'] === 'function'
  )
}
