import { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import {
  SignRequestKernelServiceDefinition,
  SignBodyResponse,
  requestMethodToJSON,
  GetSignedHeadersResponse
} from 'shared/protocol/decentraland/renderer/kernel_services/sign_request.gen'
import { Authenticator } from '@dcl/crypto/dist/Authenticator'
import { store } from 'shared/store/isolatedStore'
import { getCurrentIdentity } from 'shared/session/selectors'
import {getAuthChainSignature, getSignedHeaders} from 'lib/decentraland/authentication/signedFetch'
import { RendererProtocolContext } from '../context'

// eslint-disable-next-line @typescript-eslint/ban-types
export function registerSignRequestService<_ extends {}>(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, SignRequestKernelServiceDefinition, async () => ({
    async getRequestSignature(req, _) {
      const url = new URL(req.url)
      const identity = getCurrentIdentity(store.getState())
      if (!identity) {
        throw new Error(`Signature requested before the user has been initialized`)
      }

      const signature = getAuthChainSignature(requestMethodToJSON(req.method), url.pathname, req.metadata, (payload) =>
        Authenticator.signPayload(identity, payload)
      )
      // const response: SignBodyResponse = {}

      return {
        authChain: signature.authChain.map((item) => JSON.stringify(item)),
        timestamp: signature.timestamp,
        metadata: signature.metadata
      } as SignBodyResponse
    },
    async getSignedHeaders(req, ctx) {
      const identity = getCurrentIdentity(store.getState())
      if (!identity) {
        throw new Error(`Signed header requested before the user has been initialized`)
      }
      let headers = getSignedHeaders('get', req.url, req.metadata, (_payload) =>
        Authenticator.signPayload(identity, _payload)
      )
      return {
        message: JSON.stringify(headers)
      } as GetSignedHeadersResponse
    }
  }))
}
