import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import { TestingServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/testing.gen'
import type { PortContextService } from './context'

declare var __DCL_TESTING_EXTENSION__: any

export function registerTestingServiceServerImplementation(port: RpcServerPort<PortContextService<'logger'>>) {
  codegen.registerService(port, TestingServiceDefinition, async () => ({
    async logTestResult(result) {
      if (typeof __DCL_TESTING_EXTENSION__ !== 'undefined') return __DCL_TESTING_EXTENSION__.logTestResult(result)
      return {}
    },
    async plan(plan) {
      if (typeof __DCL_TESTING_EXTENSION__ !== 'undefined') return __DCL_TESTING_EXTENSION__.plan(plan)
      return {}
    },
    async setCameraTransform(transform) {
      if (typeof __DCL_TESTING_EXTENSION__ !== 'undefined') return __DCL_TESTING_EXTENSION__.setCameraTransform(transform)
      return {}
    }
  }))
}