import { log } from 'engine/logger'
import { Context, SocketReadyState } from './client'

class TrackAvgDuration {
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

class PkgStats {
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

class WebRtcPkgStats {
  public recv: number = 0

  public incrementRecv(n: number) {
    this.recv += n
  }

  public reset() {
    this.recv = 0
  }
}

export class NetworkStats {
  public chat = new PkgStats()
  public position = new PkgStats()
  public profile = new PkgStats()
  public others = new PkgStats()
  public ping = new PkgStats()
  public webRtcSession = new PkgStats()
  public processConnectionDuration = new TrackAvgDuration()
  public collectInfoDuration = new TrackAvgDuration()
  public positionWebRtcPackages = new WebRtcPkgStats()

  private reportInterval: any

  constructor(public context: Context) {
    const reportPkgStats = (name: string, stats: PkgStats) => {
      log(`${name}: sent: ${stats.sent} (${stats.sentBytes} bytes), recv: ${stats.recv} (${stats.recvBytes} bytes)`)
      stats.reset()
    }

    const reportWebRtcPkgStats = (name: string, stats: WebRtcPkgStats) => {
      log(`${name}: recv: ${stats.recv}`)
      stats.reset()
    }

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
      reportPkgStats('position (websocket)', this.position)
      reportWebRtcPkgStats('position (webrtc)', this.positionWebRtcPackages)
      reportPkgStats('chat', this.chat)
      reportPkgStats('profile', this.profile)
      reportPkgStats('ping', this.ping)
      reportPkgStats('webrtc session', this.webRtcSession)
      reportPkgStats('others', this.others)

      reportDuration('processConnection', this.processConnectionDuration)
      reportDuration('collectInfo', this.collectInfoDuration)

      for (let connection of context.connections) {
        const url = connection.url
        if (connection.ws && connection.ws.readyState === SocketReadyState.OPEN) {
          if (connection.ping >= 0) {
            log(`connected to ${url}, ping: ${connection.ping} ms`)
          } else {
            log(`connected to ${url}, no ping info available`)
          }
        } else {
          log(`non active connection to ${url}`)
        }
      }
      log('-------')
    }, 10000)
  }

  public close() {
    clearInterval(this.reportInterval)
  }
}
