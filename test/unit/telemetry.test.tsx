import { wait, saveScreenshot, enableVisualTests } from '../testHelpers'
import { DebugTelemetry } from 'atomicHelpers/DebugTelemetry'
import { expect } from 'chai'

enableVisualTests('DebugTelemetry', function() {
  it('should enable telemetry', () => {
    DebugTelemetry.startTelemetry({
      test: 'telemetry'
    })
  })

  // Wait to collect telemetry data across the render cycles.
  wait(1000)

  it('stopping telemetry should return render values', () => {
    const values = DebugTelemetry.stopTelemetry()
    expect(values.length).greaterThan(0)
    expect(values.some($ => $.metric === 'render')).to.equal(true, "At least one 'render' metric should be present")
  })

  // Wait to collect telemetry data across the render cycles.
  wait(300)

  it('if DebugTelemetry is deactivated, it should contain no new metrics', () => {
    expect(DebugTelemetry._store.length).to.equal(0)
  })

  saveScreenshot('telemetry.png')
})
