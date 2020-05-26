import { test } from './support/ClientHelpers'

test(async System => {
  const { Terminator } = await System.loadAPIs(['Terminate'])

  setInterval(() => Terminator.emitPing(), 16)

  // tslint:disable-next-line:no-empty
  while (true) {
    /*noop*/
  }
})
