# Common Patterns

The following is a list of common patterns used to solve frequent problems, they are the recommended way to do things but are not by any means the only ones. Custom solutions are encouraged and so are Pull Requests!

## Subscribable APIs

Typically there is a need to subscribe to a buch on arbitrary events from within a Scripts. These events are often generated outside of the scope of the API, so creating and disposing listeners across contexts becomes complicated. In this scenario we will treat the API as a "Controller", the Script as the "Subscriber" and another module as the "Event Dispatcher".

Given the following Event Dispatcher we can allow any amount of APIs (and other modules) to subscribe to a domain-specific event:

```ts
class EventListener extends EventDispatcher {
  constructor() {
    super()
    const evt = new CustomEvent('customEvent', { detail: 'test' })

    window.addEventListener('customEvent', e => {
      this.handleEvent(e.type, (e as CustomEvent).detail)
    })

    setInterval(() => {
      window.dispatchEvent(evt)
    }, 1000)
  }

  handleEvent(type: string, detail?: string) {
    this.emit(type, { data: { message: detail } })
  }
}
```

Extending the `EventDispatchter` class is the recommended way to allow the communication between the EventListener class and any other API. The `emit()` method is used to notify the API, and the `on()` method is provided as a way to register a new event listener.

Similarly, the API can extend the `SubscribableAPI` class as it needs to implement the `subscribe()`and `unsubscribe()` methods to allow Scripts to subscribe to events via an [EventSubscriber](../scripts/common-patterns.md). In this context, APIs become intermediaries between the two parts:

```ts
@registerAPI('eventController')
export class EventController extends SubscribableAPI {
  private listener: EventListener = new EventListener()
  private bindings: EventDispatcherBinding[] = []

  @exposeMethod
  async subscribe(event: string) {
    const binding = this.listener.on(event, (e: any) => {
      this.options.notify('SubscribedEvent', { event, data: e.data })
    })

    this.bindings.push(binding)
  }

  @exposeMethod
  async unsubscribe(event: string) {
    this.bindings.filter(binding => binding.event === event).forEach(binding => binding.off())
  }
}
```

Keep in mind that the implementation of subscription related method are left entirely up to the developer. In the example above we keep an array of `EventDispatcherBinding` to remove listeners related to this specific API when the `unsubscribe()` method is called (many APIs could interact with a single EventDispatcher).
