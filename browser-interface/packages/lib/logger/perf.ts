import { now } from 'lib/javascript/now'
import { humanReadableMillis } from 'lib/javascript/humanReadableMillis'
import { spaces } from 'lib/javascript/spaces'

let record: [string, number][] = [['start', now()]]

export function reset() {
  record = [['start', now()]]
}
export function tick(name: string, withReport?: boolean): string | null {
  record.push([name, now()])
  if (withReport) {
    return report()
  }
  return null
}
export function report(): string {
  const maxLength = record.reduce((prev, next) => Math.max(prev, next[0].length), 0)
  return (
    `Timing report:\n` +
    record.reduce((prev, next, index) => {
      const name = `${next[0]}: ${spaces(maxLength - next[0].length)}`
      const time = next[1]
      if (!index) {
        return ` - start: ${spaces(maxLength - 'start'.length)} 0.000s (+0.00s)\n`
      }
      return (
        prev +
        ` - ${name} ${humanReadableMillis(time - record[0][1])} (+${humanReadableMillis(
          time - record[index - 1][1]
        )})\n`
      )
    }, '')
  )
}
