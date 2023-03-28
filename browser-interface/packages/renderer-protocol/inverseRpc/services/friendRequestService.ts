import { RpcServerPort } from '@dcl/rpc'
import { RendererProtocolContext } from '../context'
import * as codegen from '@dcl/rpc/dist/codegen'

import {
  acceptFriendRequest,
  cancelFriendRequest,
  getFriendRequestsProtocol,
  rejectFriendRequest,
  requestFriendship
} from 'shared/friends/sagas'
import defaultLogger from 'lib/logger'
import { FriendshipErrorCode } from 'shared/protocol/decentraland/renderer/common/friend_request_common.gen'
import { FriendRequestKernelServiceDefinition } from 'shared/protocol/decentraland/renderer/kernel_services/friend_request_kernel.gen'

export function registerFriendRequestKernelService(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, FriendRequestKernelServiceDefinition, async () => ({
    async getFriendRequests(req, _) {
      return handleRequest(getFriendRequestsProtocol, req)
    },

    async sendFriendRequest(req, _) {
      return handleRequest(requestFriendship, req)
    },

    async cancelFriendRequest(req, _) {
      return handleRequest(cancelFriendRequest, req)
    },

    async acceptFriendRequest(req, _) {
      return handleRequest(acceptFriendRequest, req)
    },

    async rejectFriendRequest(req, _) {
      return handleRequest(rejectFriendRequest, req)
    }
  }))
}

type FriendshipError = { message: { $case: 'error'; error: FriendshipErrorCode } }

type ResponseType<T> = { reply: NonNullable<T>; error: undefined } | { reply: undefined; error: FriendshipErrorCode }

/**
 * Build friend requests error message to send to renderer.
 * @param error - an int representing an error code.
 */
function buildErrorResponse(error?: FriendshipErrorCode): FriendshipError {
  return {
    message: {
      $case: 'error' as const,
      error: error ?? FriendshipErrorCode.FEC_UNKNOWN
    }
  }
}

/**
 * Build friend requests success message to send to renderer.
 * @param reply - a FriendRequestReplyOk kind of type.
 */
function wrapReply<T>(reply: NonNullable<T>) {
  return { message: { $case: 'reply' as const, reply } }
}

/**
 * Build friend requests message to send to renderer.
 * If the friendRequest object is truthy, the function returns the result of calling `wrapReply` on the `friendRequest` object.
 * If the friendRequest object is falsy, the function returns the result of calling `buildErrorResponse` with the `error` value.
 * @param friendRequest - it can represent any kind of `FriendRequestReplyOk` object.
 * @param error - an int representing an error code.
 */
function buildResponse<T>(friendRequest: ResponseType<T>) {
  if (friendRequest.reply) {
    return wrapReply(friendRequest.reply)
  } else {
    return buildErrorResponse(friendRequest.error)
  }
}

/**
 * Abstract the flow of request handling of friend requests.
 * @param handler - a function that takes in a request object and returns a Promise of a ResponseType object.
 * @param req - a request object.
 */
async function handleRequest<T, U>(handler: (r: T) => Promise<ResponseType<U>>, req: T) {
  try {
    // Handle request
    const internalResponse = await handler(req)

    // Build friend request reply
    const response = buildResponse(internalResponse)

    // Send response back to renderer
    return response
  } catch (err) {
    defaultLogger.error('Error while processing friend request via rpc', err)

    // Send response back to renderer
    return buildErrorResponse()
  }
}
