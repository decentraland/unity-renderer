let lastGeneratedId = 0

declare var console: any
declare var dcl: any

/**
 * Log function. Only works in debug mode, otherwise it does nothing.
 * @param args - any loggable parameter
 * @public
 */
export function log(...args: any[]) {
  if (typeof dcl !== 'undefined') {
    dcl.log(...args)
  } else {
    // tslint:disable-next-line:no-console
    console.log('DEBUG:', ...args)
  }
}

/**
 * Error function. Prints a console error. Only works in debug mode, otherwise it does nothing.
 * @param error - string or Error object.
 * @param data - any debug information.
 * @public
 */
export function error(error: string | Error, data?: any) {
  if (typeof dcl !== 'undefined') {
    dcl.error(error, data)
  } else {
    // tslint:disable-next-line:no-console
    console.error('ERROR:', error, data)
  }
}

/**
 * Generates a new prefixed id
 * @beta
 */
export function newId(type: string) {
  lastGeneratedId++
  if (type.length === 0) throw new Error('newId(type: string): type cannot be empty')
  return type + lastGeneratedId.toString(36)
}

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
 * @param size - the number of element to construct and put in the array
 * @param itemBuilder - a callback responsible for creating new instance of item. Called once per array entry.
 * @returns a new array filled with new objects
 * @internal
 */
export function buildArray<T>(size: number, itemBuilder: () => T): Array<T> {
  const a: T[] = []
  for (let i = 0; i < size; ++i) {
    a.push(itemBuilder())
  }
  return a
}

export function openExternalURL(url: string) {
  if (typeof dcl !== 'undefined') {
    dcl.openExternalUrl(url)
  } else {
    // tslint:disable-next-line:no-console
    console.error('ERROR: openExternalURL dcl is undefined')
  }
}

/**
 * Popup NFT info dialog
 * @param scr 'ethereum://contractAddress/tokenID'
 * @param comment optional. add a comment.
 */
export function openNFTDialog(scr: string, comment: string | null = null) {
  if (typeof dcl !== 'undefined') {
    const regex = /ethereum:\/\/(.+)\/(.+)/
    const matches = scr.match(regex)

    if (!matches || matches.length < 3) {
      return
    }

    dcl.openNFTDialog(matches[1], matches[2], comment)
  } else {
    // tslint:disable-next-line:no-console
    console.error('ERROR: openNFTDialog dcl is undefined')
  }
}
