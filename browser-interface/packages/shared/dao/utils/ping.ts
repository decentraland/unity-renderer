import defaultLogger from '../../logger'
import { PingResult, AskResult, ServerConnectionStatus } from '../types'

export async function ask(url: string, timeoutMs: number = 5000): Promise<AskResult> {
  try {
    return await new Promise<AskResult>((resolve) => {
      const http = new XMLHttpRequest()

      const started = new Date()

      http.timeout = timeoutMs
      http.onreadystatechange = () => {
        if (http.readyState === XMLHttpRequest.DONE) {
          try {
            if (http.status !== 200) {
              resolve({
                httpStatus: http.status,
                status: ServerConnectionStatus.UNREACHABLE
              })
            } else {
              resolve({
                httpStatus: http.status,
                status: ServerConnectionStatus.OK,
                elapsed: new Date().getTime() - started.getTime(),
                result: JSON.parse(http.responseText)
              })
            }
          } catch (e) {
            defaultLogger.error('Error fetching status of Catalyst server', e)
            resolve({
              status: ServerConnectionStatus.UNREACHABLE
            })
          }
        }
      }

      http.open('GET', url, true)

      try {
        http.send(null)
      } catch (exception) {
        resolve({
          status: ServerConnectionStatus.UNREACHABLE
        })
      }
    })
  } catch {
    return {
      status: ServerConnectionStatus.UNREACHABLE
    }
  }
}

export async function ping(url: string, timeoutMs: number = 5000): Promise<PingResult> {
  return ask(url, timeoutMs)
}
