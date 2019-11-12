export type UserId = string
export type LastSeenTimestamp = number
export type Timestamp = number
export type TopicId = string
export type TargetLocation = string
export type UserSeen = { type: 'seen'; who: UserId; when: Timestamp }
export type TopicSubscribe = { type: 'topic sub'; what: TopicId; when: Timestamp }
export type TopicUnsubscribe = { type: 'topic del'; what: TopicId; when: Timestamp }
export type Teleport = { type: 'teleport'; where: TargetLocation; when: Timestamp }

export type Action = UserSeen | TopicSubscribe | TopicUnsubscribe | Teleport

function teleport(where: TargetLocation, when: Timestamp): Teleport {
  return { type: 'teleport', where, when }
}
function subscribe(what: TopicId, when: Timestamp): TopicSubscribe {
  return { type: 'topic sub', what, when }
}
function unsubscribe(what: TopicId, when: Timestamp): TopicUnsubscribe {
  return { type: 'topic del', what, when }
}
function userSeen(who: UserId, when: Timestamp): UserSeen {
  return { type: 'seen', who, when }
}

export class PresenceReport {
  inSight: Record<UserId, LastSeenTimestamp> = {}
  subscriptions = new Set<TopicId>()
  registry: Record<Timestamp, Action[]> = {}

  toString() {
    return JSON.stringify({ presenceReport: this.registry }, null, 2)
  }

  register(action: Action) {
    if (!this.registry[action.when]) {
      this.registry[action.when] = []
    }
    this.registry[action.when].push(action)
  }

  reportSeen(userId: UserId) {
    this.inSight[userId] = this.now()
    this.register(userSeen(userId, this.now()))
  }

  reportTeleport(where: TargetLocation) {
    this.register(teleport(where, this.now()))
  }

  subscribe(what: TopicId) {
    this.subscriptions.add(what)
    this.register(subscribe(what, this.now()))
  }

  unsubscribe(what: TopicId) {
    this.subscriptions.delete(what)
    this.register(unsubscribe(what, this.now()))
  }

  clear() {
    this.registry = {}
    this.inSight = {}
  }

  now() {
    return new Date().getTime()
  }
}

export const Reporter = new PresenceReport()
;(global as any)['reporter'] = Reporter
