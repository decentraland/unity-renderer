import { Script, WebWorkerTransport } from '../../lib/client'

const ScriptingClient = new Script(WebWorkerTransport(self as any))

const x = async () => {
  const data: object = await ScriptingClient.call('MethodX', ['a worker generated string'])
  await ScriptingClient.call('JumpBack', data)
}
x().catch(x => console.error(x))
