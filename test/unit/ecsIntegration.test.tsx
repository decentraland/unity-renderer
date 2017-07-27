import { loadTestParcel, wait } from '../testHelpers'
import { DevTools } from 'shared/apis/DevTools'
import { expect } from 'chai'

describe('ecs integration', () => {
  loadTestParcel('Test thrown error on loading', -200, 238, function(_root, _futureParcelScene, futureWorker) {
    wait(1000)

    it('must have failed', async () => {
      const worker = await futureWorker
      const system = await worker.system
      const tools = system.getAPIInstance(DevTools)
      expect(tools.exceptions.size).to.eq(1)
      expect(tools.exceptions.get(0).text).to.include('bad things may happen')
    })
  })

  loadTestParcel('Check environment', -200, 237, function(_root, _futureParcelScene, futureWorker) {
    wait(1000)

    it('must not fail', async () => {
      const worker = await futureWorker
      const system = await worker.system
      const tools = system.getAPIInstance(DevTools)
      expect(tools.exceptions.size).to.eq(0)
      expect(tools.logs.length).to.eq(1)
      expect(tools.logs[0].args[0].value).to.eq('daThing')
      expect(tools.logs[0].args[1].type).to.eq('object')
    })
  })
})
