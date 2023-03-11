const occurrencesCounter = new Map<Counters, number>()

type Counters =
  | 'failed:sendPositionMessage'
  | `commMessage:${string}`
  | `setThrew:${string}`
  | 'voiceChatHandlerError'
  | 'voiceChatRequestMediaDeviceFail'
  | 'pong_duplicated_response_counter' // duplicated pong responses (nonce*address)
  | 'pong_expected_counter' // expected amount of responses of the current pong
  | 'pong_given_counter' // total amount of responses of the current ping
  | 'ping_sent_counter' // amount of ping requests sent by myself
  | 'pong_sent_counter' // amount of pong responses sent by peeds
  | 'pong_received_counter' // amount of pong responses received by myself
  | 'ping_received_twice_counter' // amount of times the same ping reaches myself (nonce)
  | 'profile-over-comms-succesful'
  | 'profile-over-comms-failed'

export function incrementCounter(counter: Counters, by = 1) {
  occurrencesCounter.set(counter, (occurrencesCounter.get(counter) || 0) + by)
}

export function getAndClearOccurenceCounters() {
  const metrics: Record<string, number> = {}
  let hasMetrics = false
  for (const [key, value] of occurrencesCounter) {
    metrics[key] = value
    hasMetrics = true
  }
  if (hasMetrics) {
    occurrencesCounter.clear()
  }
  return metrics
}
