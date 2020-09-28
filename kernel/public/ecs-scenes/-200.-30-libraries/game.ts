import * as X from "eth-wrapper"
import * as test from "eth-wrapper/test"

log("does it work?" + (test.testFunction() == 1 ? ' YES IT DOES!' : 'NO, THIS IS A BUG'))

executeTask(async () => {
  log('wrapped public key:', await X.wrappedGetPublicKey())
})

