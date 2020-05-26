import { testToFail } from './support/ClientHelpers'

testToFail(async ScriptingClient => {
  await ScriptingClient.loadAPIs([Math.random().toString()])
})
