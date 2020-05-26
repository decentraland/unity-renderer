# Introduction

APIs work as a bridge between user-created scripts and the lower level APIs of the client (communication, 3D entity management, etc). It provides a set of exposed methods that can be accessed from the Web Worker context. These methods are `async` by default and Promises are used as hooks for events that may be triggered in the future (HTTP Responses, entity collisions, etc).

## A basic API

The following is the implementation of a basic API that exposes the method `getLastPing` to be used by a external [System](../scripts/introduction.md).

```ts
@registerAPI('Pinger')
export class Pinger extends API {
  private lastPing: number = 0

  constructor(opt: APIOptions) {
    super(opt)
    opt.on('Ping', () => (this.lastPing = +new Date()))
  }

  @exposeMethod
  async getLastPing(): number {
    return this.lastPing
  }
}
```

An API typically extends from the `API` class exported by the SDK. This allows APIs to inherit the [options methods](#options-methods) which are used to communicate with other Scripts. In the example above, a listener is registered for the `Ping` notification. Once a notification is emitted from a System the callback is executed as expected.

APIs can expose methods that can be called from within a System using the `@exposeMethod` decorator. All exposed methods must be `async` but they are not required to return a Promise. This is not the case when calling the same method inside a System, where you [must always await the result](../scripts/introduction.md).

APIs can hold state of their own, as is the case of the `lastPing` property in the example above. There are no hard limits on what you can do with state, but no update hooks are provided either.

Finally, the `@registerAPI` decorator will make the class available as a API consumable from within other Scripts.

## _Options_ methods

When extending from the core `API` class a set of functions become available through the `options` property. The following is a list of them and their functionality:

* `notify(identifier: string, data?: Array | Object)`

  Sends a notification message to the System. Other than exposed methods, this is the only method of communication between the two parts.

* `on(identifier: string, handler: (data?: Array | Object) => void)`

  Works as a listener for messages emitted from another System.

* `getAPIInstance(...)`

  Allows a API to create [create an instance of another API](#instancing-API) from within itself.

* `expose(identifier: string, handler: (...args) => Promise<any>)`

  Low level function that serves the same purpose as the `@exposeMethod` decorator does.

## Rate limiting

Rate limiting allows to specify a time interval in which calls to a API's method can be accepted. This allows for more control over computationally expensive methods and can be specified by using the `@rateLimit(inteval)` decorator:

```ts
@exposeMethod
@rateLimit(1000)  
async playSound(): number {
  // some sensible logic
}
```

## Throttling

Much like Rate limiting, Throttling allows to specify a time interval in which an specific amount of calls to a API's method can be accepted. This functionality can be accessed through the `@throttle(callLimit, inteval)` decorator:

```ts
@exposeMethod
@throttle(5, 1000)
async playSound(): number {
  // some sensible logic
}
```

In the above example only five sounds will be allowed to be played per second. This method is specially useful for setting more complex limitations to a method that could be could from an external script.

## Instancing a API from within another API

It is possible to access an istance of a API from another API's context. This can be achieved with the `getAPIInstance()` method found inside the `options` property.

It can be used to create an instance based on a API's regitered name:

```ts
@registerAPI('APIOne')
class APIOne extends API {
  @exposeMethod
  async someMethod() {
    return true
  }
}

@registerAPI('APITwo')
class APITwo extends API {
  private One: APIOne

  constructor(options: APIOptions) {
    super(options)
    this.One = options.getAPIInstance('APIOne') as APIOne
  }

  otherMethod() {
    this.One.someMethod() // true
  }
}
```

A similar approach can be taken to access an instance based on the API's constructor:

```ts
@registerAPI('APIOne')
class APIOne extends API {
  @exposeMethod
  async someMethod() {
    return true
  }
}

@registerAPI('APITwo')
class APITwo extends API {
  private One: APIOne

  constructor(options: APIOptions) {
    super(options)

    // We are not using a string any longer
    // Types are also correctly resolved
    this.One = options.getAPIInstance(APIOne)
  }

  otherMethod() {
    this.One.someMethod() // true
  }
}
```
