import { expect } from 'chai'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'

describe('get performance info', function () {
  it(`works correctly`, () => {
    const data: string[] = []
    for (let i = 0; i < 100; i++) {
      data[i] = String.fromCharCode((i * 2 + 2348) % 120)
    }
    const strData = data.join('')
    const result = getPerformanceInfo({
      samples: strData,
      fpsIsCapped: false,
      hiccupsInThousandFrames: 4,
      hiccupsTime: 2,
      totalTime: 100,
      estimatedAllocatedMemory: 0,
      estimatedTotalMemory: 0,

      gltfInProgress: 0,
      gltfFailed: 0,
      gltfCancelled: 0,
      gltfLoaded: 42,
      abInProgress: 2,
      abFailed: 52,
      abCancelled: 6,
      abLoaded: 117,
      gltfTexturesLoaded: 44,
      abTexturesLoaded: 226,
      promiseTexturesLoaded: 80,
      enqueuedMessages: 34873,
      processedMessages: 30832,
      playerCount: 14,
      loadRadius: 1,
      sceneScores: {
        bafkreib7kmfvbxyibw6en3h2ewtryobtcegcmjh47iyrd56bh5fikfytfi: 388992155
      },
      drawCalls: 1027,
      memoryReserved: 4006592481,
      memoryUsage: 3592907388,
      totalGCAlloc: 325312318
    })
    expect(result.len).to.eq(100)
    expect(result.min).to.eq(0)
    expect(result.max).to.eq(118)
    expect(result.hiccupsInThousandFrames).to.eq(4)
    expect(result.hiccupsTime).to.eq(2)
    expect(result.totalTime).to.eq(100)
    expect(result.fps).to.be.approximately(16.28664, 0.0001)
  })
})
