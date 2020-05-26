[![CircleCI](https://circleci.com/gh/decentraland/metaverse-rpc.svg?style=svg&circle-token=33a7ab6330a3c900c456c0367c118d912e48f484)](https://circleci.com/gh/decentraland/metaverse-rpc).
[![Build status](https://ci.appveyor.com/api/projects/status/v2ql8549rfh311go/branch/master?svg=true)](https://ci.appveyor.com/project/decentraland/metaverse-rpc/branch/master)

# `decentraland-rpc`

This repository contains the low-level API that allows us to run sandboxed (and even remote) code for Decentralands LANDs and other systems like Physics.

## Scripting

Scripts are pieces of logic that run inside the context of a Web Worker or remotely in a server. They are meant to provide the user a way to run custom logic inside the player's client, allowing the creation of rich experiences inside Decentraland. To achieve this, low level hooks are exposed from the scripting host and consumed by the scripting client.

## Transports

The scripts communicate with the host application thru a JSON-RPC2 based protocol using a defined transport. We have 3 built in transports.

- [WebWorker](src/common/transports/WebWorker.ts): Used to load a sandboxed script locally, inside a WebWorker
- [WebSocket](src/common/transports/WebSocket.ts): Used to run scripts in remote servers
- [Memory](src/common/transports/Memory.ts): Used to run tests, mainly. The script runs in the same execution context as the host.

## Scripting host

The [ScriptingHost](src/host/ScriptingHost.ts) is a core piece that instanciates APIs and handles incoming/outgoing messages from the scripts.

## APIs

APIs work as a bridge between user-created scripts and the lower level APIs of the client (communication, 3D entity management, etc). It provides a set of exposed methods that can be accessed from the script context. These methods are `async` by default and Promises are used as hooks for events that may be triggered in the future (HTTP Responses, entity collisions, etc).

The `@exposeMethod` decorator is provided as means of exposing API methods to the Script.

An example implementation can be found at [3.Class.spec.ts](test/scenarios/3.Class.spec.ts)

### See also

1.  [APIs introduction](docs/apis/introduction.md)
2.  [Common patterns](docs/apis/common-patterns.md)
3.  [Scripting host](docs/apis/scripting-host.md)

## Scripts

The term "script" or sometimes "system" refers to the instance of a user-created script, normally running inside a Web Worker. To access an API instance the decorator `@inject(apiName: string)` function is used. From then on, the user will be able to call all exposed methods and `await` the promises returned by them.

An example implementation can be found at [7.0.MethodsInjection.ts](test/fixtures/7.0.MethodsInjection.ts)

### See also

1.  [Scripts introduction](docs/scripts/introduction.md)
2.  [Common patterns](docs/scripts/common-patterns.md)

# Related documents

[The Entity-Component-System - An awesome gamedesign pattern in C Part 1](https://www.gamasutra.com/blogs/TobiasStein/20171122/310172/The_EntityComponentSystem__An_awesome_gamedesign_pattern_in_C_Part_1.php)

Why do we create a component based system? [Components](http://gameprogrammingpatterns.com/component.html)

# Decentraland Compiler

The Decentraland Compiler is used to build all sort of TypeScript related projects. Both DCL's client all all of the SDK's dynamic scenes use it. You can think about it as an scoped task runner which only does a few things but it does them well.

To get started create a build.json file:

```json
[
  {
    "name": "Compile systems",
    "kind": "Webpack",
    "file": "./scene.tsx",
    "target": "webworker"
  }
]
```

Then run the following command:

`decentraland-compiler build.json`

To run in watch mode:

`decentraland-compiler build.json --watch`

To use custom loaders (Webpack builds only) refer to https://webpack.js.org/concepts/loaders/#inline



## Copyright info

This repository is protected with a standard Apache 2 licence. See the terms and conditions in the [LICENCE](https://github.com/decentraland/decentraland-rpc/blob/master/LICENSE) file.
