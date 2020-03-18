import { defaultLogger } from 'shared/logger'
import { Context } from './index'
import { PositionData } from '../comms/v1/proto/comms'

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
  public recvTotal = 0
  public sentTotal = 0
  public recvTotalBytes = 0
  public sentTotalBytes = 0
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
    this.recvTotal += this.recv
    this.sentTotal += this.sent
    this.recvTotalBytes += this.recvBytes
    this.sentTotalBytes += this.sentBytes
    this.recv = 0
    this.sent = 0
    this.recvBytes = 0
    this.sentBytes = 0
  }
}

class AvgFrequency {
  public samples = 0
  public total = 0
  public lastTs = -1

  public seen(ts: number) {
    if (this.lastTs === -1) {
      this.lastTs = ts
    } else {
      const duration = ts - this.lastTs
      this.lastTs = ts
      this.total += duration
      this.samples++
    }
  }

  public avg() {
    if (this.samples === 0) {
      return 0
    }

    return this.total / this.samples
  }
}

class PeerStats {
  public positionAvgFrequency = new AvgFrequency()

  public onPositionMessage() {
    this.positionAvgFrequency.seen(Date.now())
  }
}

export class Stats {
  public peers = new Map<string, PeerStats>()

  public topic = new PkgStats()
  public others = new PkgStats()
  public ping = new PkgStats()
  public position = new PkgStats()
  public profile = new PkgStats()
  public chat = new PkgStats()
  public sceneComms = new PkgStats()
  public webRtcSession = new PkgStats()
  public collectInfoDuration = new TrackAvgDuration()
  public dispatchTopicDuration = new TrackAvgDuration()
  public visiblePeerIds: string[] = []
  public trackingPeersCount = 0

  private reportInterval: any

  constructor(private context: Context) {}

  public printDebugInformation() {
    const reportDuration = (name: string, duration: TrackAvgDuration) => {
      const durationsMs = duration.durationsMs
      if (durationsMs.length > 0) {
        const avg = durationsMs.reduce((total, d) => total + d) / durationsMs.length
        defaultLogger.info(`${name} took an avg of ${avg} ms`)
      }
      duration.clear()
    }

    const reportPkgStats = (name: string, stats: PkgStats) => {
      const sent = `${stats.sent}x${stats.sentBytes} bytes in this period`
      const sentTotal = `${stats.sentTotal}x${stats.sentTotalBytes} bytes in this period`
      const recv = `${stats.recv}x${stats.recvBytes} bytes in this period`
      const recvTotal = `${stats.recvTotal}x${stats.recvTotalBytes} bytes total`
      defaultLogger.info(`${name}: sent: ${sent} (${sentTotal}), recv: ${recv} (${recvTotal})`)
      stats.reset()
    }

    defaultLogger.info(`------- ${new Date()}: `)
    reportDuration('collectInfo', this.collectInfoDuration)
    reportDuration('dispatchTopic', this.dispatchTopicDuration)
    defaultLogger.info(`tracking peers: ${this.trackingPeersCount}, visible peers: ${this.visiblePeerIds.length}`)

    defaultLogger.info('World instance: ')

    const connection = this.context.worldInstanceConnection!
    connection.printDebugInformation()

    if (connection.ping >= 0) {
      defaultLogger.info(`  ping: ${connection.ping} ms`)
    } else {
      defaultLogger.info(`  ping: ? ms`)
    }

    reportPkgStats('  topic (total)', this.topic)
    reportPkgStats('    - position', this.position)
    reportPkgStats('    - profile', this.profile)
    reportPkgStats('    - sceneComms', this.sceneComms)
    reportPkgStats('    - chat', this.chat)
    reportPkgStats('  ping', this.ping)
    reportPkgStats('  webrtc session', this.webRtcSession)
    reportPkgStats('  others', this.others)

    this.peers.forEach((stat, alias) => {
      const positionAvgFreq = stat.positionAvgFrequency
      if (positionAvgFreq.samples > 1) {
        const samples = positionAvgFreq.samples
        const avg = positionAvgFreq.avg()
        defaultLogger.info(`${alias} avg duration between position messages ${avg}ms, from ${samples} samples`)
      }
    })

    defaultLogger.info('-------')
  }

  public onPositionMessage(fromAlias: string, data: PositionData) {
    let stats = this.peers.get(fromAlias)

    if (!stats) {
      stats = new PeerStats()
      this.peers.set(fromAlias, stats)
    }

    stats.onPositionMessage()
  }

  public onPeerRemoved(alias: string) {
    this.peers.delete(alias)
  }

  public close() {
    clearInterval(this.reportInterval)
  }
}
