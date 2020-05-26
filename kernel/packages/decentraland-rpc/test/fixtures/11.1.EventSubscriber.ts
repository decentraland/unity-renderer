import { inject, EventSubscriber } from '../../lib/client'
import { TestableScript, future, wait } from './support/ClientHelpers'
import * as assert from 'assert'

export default class SomeSystem extends TestableScript {
  @inject('eventController') eventController: any | null = null

  async doTest() {
    this.eventController.setCount(0)
    const eventSubscriber = new EventSubscriber(this.eventController)

    const gotFirstEvent = future()

    let counter = 0

    const binding = eventSubscriber.on('customEvent', (evt: any) => {
      // Make sure we receive the correct payload from the event
      assert.equal(evt.data.message, 'test')

      counter++

      if (counter === 10) {
        gotFirstEvent.resolve(evt)
        eventSubscriber.off(binding)
      }
    })

    await gotFirstEvent
    await wait(500)

    // If after one second we are still at the same count,
    // it means we have stopped listening to events
    assert.equal(counter, 10)

    // We also need to validate that unrelated event bindings are kept intact
    this.eventController.emitValidate({ value: 10 }) // will be handled by the EventListener class
  }
}
