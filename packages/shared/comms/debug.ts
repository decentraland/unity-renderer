import { log } from 'engine/logger'
import { WorldInstanceConnection, SocketReadyState } from './worldInstanceConnection'
import { Context } from './index'

export class TrackAvgDuration {
  public durationsMs: number[] = []
  public currentDurationStart: number = -1

  public start() {
    this.currentDurationStart = Date.now()
  }

  public stop() {
    const now = Date.now()
    if (this.currentDurationStart === -1) {
      throw new Error('stop() without start()')
    }

    this.durationsMs.push(now - this.currentDurationStart)
    this.currentDurationStart = -1
  }

  public clear() {
    this.durationsMs = []
  }
}

export class PkgStats {
  public recv: number = 0
  public sent: number = 0
  public recvBytes: number = 0
  public sentBytes: number = 0

  public incrementRecv(size: number) {
    this.recv++
    this.recvBytes += size
  }

  public incrementSent(amount: number, size: number) {
    this.sent += amount
    this.sentBytes += size
  }

  public reset() {
    this.recv = 0
    this.sent = 0
    this.recvBytes = 0
    this.sentBytes = 0
  }
}

export class NetworkStats {
  public topic = new PkgStats()
  public others = new PkgStats()
  public ping = new PkgStats()
  public position = new PkgStats()
  public profile = new PkgStats()
  public chat = new PkgStats()
  public webRtcSession = new PkgStats()

  constructor(public connection: WorldInstanceConnection) {}

  report(context: Context) {
    const reportPkgStats = (name: string, stats: PkgStats) => {
      log(`${name}: sent: ${stats.sent} (${stats.sentBytes} bytes), recv: ${stats.recv} (${stats.recvBytes} bytes)`)
      stats.reset()
    }

    const url = this.connection.url
    if (this.connection.ws && this.connection.ws.readyState === SocketReadyState.OPEN) {
      const state =
        (this.connection.authenticated ? 'authenticated' : 'not authenticated') +
        `- my alias is ${this.connection.alias}`
      if (this.connection.ping >= 0) {
        log(`  ${url}, ping: ${this.connection.ping} ms (${state})`)
      } else {
        log(`  ${url}, no ping info available (${state})`)
      }
    } else {
      log(`  non active coordinator connection to ${url}`)
    }

    reportPkgStats('  topic (total)', this.topic)
    reportPkgStats('    - position', this.position)
    reportPkgStats('    - profile', this.profile)
    reportPkgStats('    - chat', this.chat)
    reportPkgStats('  ping', this.ping)
    reportPkgStats('  webrtc session', this.webRtcSession)
    reportPkgStats('  others', this.others)
  }
}

export class Stats {
  public primaryNetworkStats: NetworkStats | null
  public collectInfoDuration = new TrackAvgDuration()
  public visiblePeersCount = 0

  private reportInterval: any

  constructor(context: Context) {
    const reportDuration = (name: string, duration: TrackAvgDuration) => {
      const durationsMs = duration.durationsMs
      if (durationsMs.length > 0) {
        const avg = durationsMs.reduce((total, d) => total + d) / durationsMs.length
        log(`${name} took an avg of ${avg} ms`)
      }
      duration.clear()
    }

    this.reportInterval = setInterval(() => {
      log(`------- ${new Date()}: `)
      reportDuration('collectInfo', this.collectInfoDuration)
      log('visible peers: ', this.visiblePeersCount)

      if (this.primaryNetworkStats) {
        log('Primary world instance: ')
        this.primaryNetworkStats.report(context)
      }

      log('-------')
    }, 10000)
  }

  public close() {
    clearInterval(this.reportInterval)
  }
}
