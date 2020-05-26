# Common Patterns

The following is a list of common patterns used to solve frequent problems, they are the recommended way to do things but are not by any means the only ones. Custom solutions are encouraged and so are Pull Requests!

## Subscriber Scripts

Following the ideas presented on the [Subscribable API](../apis/common-patterns.md) section, the example below will demostrate how to add listeners from within a Script:

```ts
export class AudioPlayer extends Script {
  @inject('experimentalAudio') audio: AudioController | null = null
  @inject('experimentalScene') scene: SceneController | null = null

  async systemDidEnable() {
    const eventSubscriber = new EventSubscriber(this.scene!)

    eventSubscriber.addEventListener('click', (evt: any) => {
      this.audio!.playSound('sounds/sound.ogg')
    })
  }
}
```

The `EventSubscriber` helper class receives a reference to SubscribableComponent and exposes the `addEventListener` and `removeEventListener` methods. The `addEventListener` methods returns a binding, which can be used to remove the listener in the future:

```ts
const binding = eventSubscriber.addEventListener('customEvent', (evt: any) => {
  counter++

  if (counter === 10) {
    gotFirstEvent.resolve(evt)
    eventSubscriber.removeEventListener('customEvent', binding)
  }
})
```

By design, each call to `addEventListener` for the same event will only create a single subscription on the Component's side. Multiple listeners can be added for a single event, this means that calling `removeEventListener` will stop the Script from receiving updates for a given event **only when all listeners are removed**. Lastly, removing all event listeners belonging to a single Script will keep the rest of the listeners intact, this means that multiple Scripts (or modules) can safely subscribe to the same event.
