import { defaultLogger } from './logger'

export function getStack(verbose: boolean = true) {
  try {
    throw new Error()
  } catch (e: any) {
    if (verbose) {
      defaultLogger.log(e.stack)
    }
    return e.stack
  }
}
