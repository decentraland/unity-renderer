# Introduction

Scripts contain custom logic that is executed outside of the context of the [ScriptingHost](../apis/scripting-host.md). They can run either locally using the Web Worker transport, or in another server through HTTP Requests/Web Sockets.Scripts communicate with APIs through JSON-RPC and several abstractions are provided to make loading and messaging APIs a simple task.

## A basic Script

```ts
class ExampleSystems extends Script {
  @inject('Pinger') pinger: Pinger

  systemDidEnable() {
    await this.pinger.getLastPing()
  }
}
```

Following the example started in the [APIs](../api/introduction.md), the Script above emits a Ping message and request a value from an exposed method.

Scripts can load APIs using the `@inject` decorator by specifying the name registered in the ScriptingHost. Additional types for that API must be created and exported separately.

## Sending/Receiving notifications

A component subscribe to notifications by passing a callback to a method related to that specific notification. Te `on` prefix is used to identify methods that provide subscriptions. These methods are not defined in the API, but instead are processed on runtime and are automatically associated to the corresponding notification:

```ts
class ExampleSystems extends Script {
  @inject('Pinger') pinger: Pinger

  async systemDidEnable() {
    this.pinger.onPong(() => {
      console.log('Pong!')
    })
  }
}
```

In the example above, the `onPong` method refers to the `ping` notification emitted by the Pinger API.

A Script can send notifications to the client in a similar way by calling a method beggining with the `emit` prefix followed by the notification identifier:

```ts
class ExampleSystems extends Script {
  @inject('Pinger') pinger: Pinger

  async systemDidEnable() {
    setInterval(() => {
      this.emitPing()
    }, 1000)

    this.pinger.onPong(() => {
      console.log('Pong!')
    })
  }
}
```
