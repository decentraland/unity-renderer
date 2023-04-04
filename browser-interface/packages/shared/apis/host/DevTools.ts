import type { RpcServerPort } from '@dcl/rpc/dist/types'
import * as codegen from '@dcl/rpc/dist/codegen'
import { DevToolsServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/dev_tools.gen'
import type { ProtocolMapping } from 'devtools-protocol/types/protocol-mapping'
import type { PortContext } from './context'

export function registerDevToolsServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, DevToolsServiceDefinition, async () => ({
    async event(req, context) {
      const params = JSON.parse(req.jsonPayload)
      switch (req.type) {
        case 'Runtime.consoleAPICalled': {
          const [event] = params as ProtocolMapping.Events['Runtime.consoleAPICalled']

          context.logger.log('', ...event.args.map(($) => ('value' in $ ? $.value : $.unserializableValue)))

          break
        }

        case 'Runtime.exceptionThrown': {
          const [payload] = params as ProtocolMapping.Events['Runtime.exceptionThrown']

          if (payload.exceptionDetails.exception) {
            context.logger.error(
              payload.exceptionDetails.text,
              payload.exceptionDetails.exception.value || payload.exceptionDetails.exception.unserializableValue
            )
          } else {
            context.logger.error(payload.exceptionDetails.text)
          }
          break
        }
      }

      return {}
    }
  }))
}
