import { AsyncQueue } from '@dcl/rpc/dist/push-channel'
import { WsPacket } from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc5/ws_comms.gen'

export function wsAsAsyncChannel(socket: WebSocket) {
  // Wire the socket to a pushable channel
  const channel = new AsyncQueue<WsPacket>((queue, action) => {
    if (action === 'close') {
      socket.removeEventListener('message', processMessage)
      socket.removeEventListener('close', closeChannel)
    }
  })
  function processMessage(event: MessageEvent) {
    try {
      const msg = new Uint8Array(event.data)
      channel.enqueue(WsPacket.decode(msg))
    } catch (error: any) {
      socket.close(undefined, 'Error: ' + error)
    }
  }
  function closeChannel() {
    channel.close()
  }
  socket.addEventListener('message', processMessage)
  socket.addEventListener('close', closeChannel)
  return Object.assign(channel, {
    async yield(timeoutMs: number, error?: string): Promise<WsPacket> {
      if (timeoutMs) {
        const next: any = (await Promise.race([channel.next(), timeout(timeoutMs, error)])) as any
        if (next.done) throw new Error('Cannot consume message from closed AsyncQueue. ' + error)
        return next.value
      } else {
        const next = await channel.next()
        if (next.done) throw new Error('Cannot consume message from closed AsyncQueue.' + error)
        return next.value
      }
    }
  })
}

function timeout(ms: number, error = 'Timed out') {
  return new Promise((_, reject) => {
    setTimeout(() => reject(new Error(error)), ms)
  })
}
