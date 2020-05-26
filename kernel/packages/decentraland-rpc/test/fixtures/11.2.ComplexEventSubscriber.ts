import { inject, EventSubscriber } from '../../lib/client'
import { TestableScript, future, wait } from './support/ClientHelpers'
import * as assert from 'assert'

export default class SomeSystem extends TestableScript {
  @inject('eventController') eventController: any | null = null

  async doTest() {
    this.eventController.setCount(0)
    const eventSubscriber = new EventSubscriber(this.eventController)

    const doneListening1 = future()
    const doneListening2 = future()
    let counters = [0, 0]

    const binding = eventSubscriber.on('customEvent', (evt: any) => {
      counters[0]++

      if (counters[0] === 10) {
        doneListening1.resolve(evt)
        eventSubscriber.off(binding)
      }
    })

    const binding2 = eventSubscriber.on('customEvent', (evt: any) => {
      counters[1]++

      if (counters[1] === 15) {
        doneListening2.resolve(evt)
        eventSubscriber.off(binding2)
      }
    })

    await doneListening1
    await doneListening2
    await wait(500)

    assert.equal(counters[0], 10)
    assert.equal(counters[1], 15) // we validate that each event listener is removed atomically

    // We also need to validate that unrelated event bindings are kept intact
    this.eventController.emitValidate({ value: 10 }) // will be handled by the EventListener class
  }
}
