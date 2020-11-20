import { expect } from 'chai'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'

describe('get performance info', function() {
  it(`works correctly`, () => {
    const data: string[] = []
    for (let i = 0; i < 100; i++) {
      data[i] = String.fromCharCode((i * 2 + 2348) % 120)
    }
    const strData = data.join('')
    const result = getPerformanceInfo({ samples: strData, fpsIsCapped: false})
    expect(result.len).to.eq(100)
    expect(result.min).to.eq(0)
    expect(result.max).to.eq(98)
    expect(result.fps).to.be.approximately(16.28664, 0.0001)
  })
})
