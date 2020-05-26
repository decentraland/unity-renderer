import { test } from './support/ClientHelpers'

test(async ScriptingClient => {
  const { xRuntime, xDebugger, xProfiler } = await ScriptingClient.loadAPIs(['xRuntime', 'xDebugger', 'xProfiler'])

  await Promise.all([xRuntime.enable(), xDebugger.enable(), xProfiler.enable(), xRuntime.run()])

  await xProfiler.start()
  await new Promise(resolve => xRuntime.onExecutionContextDestroyed(resolve))
  await xProfiler.stop()
})
